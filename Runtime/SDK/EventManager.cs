using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Metica.Core;
using Metica.Network;
using Metica.Model;

namespace Metica
{
    [Serializable]
    public class EventDispatchResult : IMeticaHttpResult
    {
        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent { get; set; }
        [JsonIgnore] public string OriginalRequestBody { get; set; }

        public override string ToString()
        {
            return $"{nameof(EventDispatchResult)}:\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}\n Orig. Request Body: {OriginalRequestBody}";
        }
    }

    /// <summary>
    /// Endpoint Manager has the following responsibilities:
    /// - Expose all methods to log events
    /// - Accumulate events and dispatch them
    /// - For the events that are relative to offers, this class takes care of retrieving the necessary information from the <see cref="OfferManager"/>.
    ///   This though happens behind the scenes so it shouldn't be a concern for the calling code.
    /// - If the client-code must be informed on when the dispatch takes place, a delegate can be given to handle an <see cref="EventDispatchResult"/>.
    /// </summary>
    /// <remarks>Note that whenever a Log- method is called, the event is not immediately sent,
    /// instead, the get stored and sent in group when certain conditions are met.
    /// ROADMAP:
    /// - TODO : dispatch events based on a timer
    /// - TODO : proper finalization of the object to flush/dispatch the events and other cleanup.
    /// - TODO : use constants for eventTypes
    /// - TODO : (low priority) create an event queue processing system that processes the queue right before sending (for exampe to aggregate certain events)
    /// </remarks>
    internal class EventManager : EndpointManager
    {
        public delegate void OnEventsDispatchDelegate(EventDispatchResult eventResult);
        /// <summary>
        /// Callback to process an <see cref="EventDispatchResult"/> when the events are effectively sent.
        /// </summary>
        public event OnEventsDispatchDelegate OnEventsDispatch;

        private readonly IMeticaAttributesProvider _meticaAttributesProvider;
        private readonly ITimeSource _timeSource = new SystemDateTimeSource();

        public const uint DefaultQueueFlushCountTrigger = 32;
        private const uint DefaultQueueFlushTimeoutSecondsTrigger = 10;
        private readonly uint _eventQueueCountTrigger;
        private long _lastEventDispatchUnixTime = 0;

        private List<object> _events;

        public EventManager(
            IHttpService httpService,
            string endpoint,
            IMeticaAttributesProvider meticaAttributesProvider,
            uint eventQueueCountTrigger = DefaultQueueFlushCountTrigger
            ) : base(httpService, endpoint)
        {
            _meticaAttributesProvider = meticaAttributesProvider;
            _events = new List<object>();
            if (Log.CurrentLogLevel == LogLevel.Debug)
            {
                OnEventsDispatch += DispatchHandler;
            }
            _eventQueueCountTrigger = eventQueueCountTrigger;
            _lastEventDispatchUnixTime = _timeSource.EpochSeconds();
        }

        /// <summary>
        /// Use this method for simple events without specific fields.
        /// <see cref="QueueEventWithMeticaAttributesAsync(string, string, string, string, string, Dictionary{string, object}, Dictionary{string, object})"/>
        /// and
        /// <see cref="QueueEventWithProductId(string, string, string, string, Dictionary{string, object}, Dictionary{string, object})"/>
        /// both use this method but it's perfectly fine to use it directly.
        /// This method also calls the <see cref="DispatchEvents"/> with a fire and forget style call <code>_ = Dispatch();</code> when certain conditions are met.
        /// </summary>
        internal void QueueEventAsync(string userId, string appId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            var requestBody = new Dictionary<string, object>
            {
                { FieldNames.EventType, eventType },
                { FieldNames.EventId, Guid.NewGuid().ToString() },
                { FieldNames.EventTime, _timeSource.EpochSeconds() },
                { FieldNames.AppId, appId },
                { FieldNames.UserId, userId },
                { FieldNames.MeticaUnitySdk, MeticaSdk.Version },
                { FieldNames.CustomPayload, customPayload }
            };

            if(eventFields != null)
            {
                requestBody.AddDictionary(eventFields, overwriteExistingKeys: true);
            }

            _events.Add(requestBody);

            Log.Debug(() => $"Queueing {eventType} event, id={requestBody[FieldNames.EventId]}");

            long unixTimeSinceLastDispatch = _timeSource.EpochSeconds() - _lastEventDispatchUnixTime;

            if (_events.Count >= _eventQueueCountTrigger || unixTimeSinceLastDispatch >= DefaultQueueFlushTimeoutSecondsTrigger)
            {
                // Log.Debug(() => (_events.Count >= _eventQueueCountTrigger)? "Dispatch : reason=count" : "Dispatch : reason=time" );
                _ = DispatchEvents();
            }
        }

        /// <summary>
        /// Queues an event for later sending in bulk. Note that this method is asynchronous but it doesn't need to be awaited.
        /// It's only asynchronous because it's retrieving some information before queuing the event.
        /// To avoid the "method is not awaited" warning, you can use the <code>_ =</code> discard operator.
        /// </summary>
        internal async Task QueueEventWithMeticaAttributesAsync(string userId, string appId, string placementId, string offerId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            if (_meticaAttributesProvider == null)
            {
                Log.Error(() => $"{nameof(IMeticaAttributesProvider)} is not initialized.");
                return;
            }
            object meticaAttributes = await _meticaAttributesProvider.GetMeticaAttributes(placementId, offerId);
            if (meticaAttributes == null)
            {
                Log.Error(() => $"Failed to retrieve information for placementId '{placementId}' and offerId '{offerId}'.");
                return;
            }

            if (eventFields == null)
            {
                eventFields = new Dictionary<string, object>();
            }

            eventFields.AddDictionary(
                new()
                {
                    { FieldNames.MeticaAttributes, meticaAttributes }
                });

            QueueEventAsync(
                userId,
                appId,
                eventType,
                eventFields,
                customPayload
                );
        }

        /// <summary>
        /// Queues an event for later sending in bulk. (<see cref="DispatchEvents"/>)
        /// </summary>
        /// <remarks>This doesn't return anything because it's only adding an event to the queue.</remarks>
        internal void QueueEventWithProductId(string userId, string appId, string productId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            if (eventFields == null)
            {
                eventFields = new Dictionary<string, object>();
            }

            eventFields.AddDictionary(
                new()
                {
                    { FieldNames.ProductId, productId }
                });

            QueueEventAsync(
                userId,
                appId,
                eventType,
                eventFields,
                customPayload
                );
        }

        public async Task RequestDispatchEvents()
        {
            await DispatchEvents();
        }

        /// <summary>
        /// Dispatches events in bulk and clears the list/queue.
        /// </summary>
        /// <returns></returns>
        private async Task DispatchEvents()
        {
            if( _events == null || _events.Count == 0 )
            {
                return;
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var body = JsonConvert.SerializeObject(new Dictionary<string, object> { { "events", _events } }, settings);
            try
            {
                var httpResponse = await _httpService.PostAsync(_url, body, "application/json", useCache: false);
                _events.Clear();
                EventDispatchResult result = ResponseToResult<EventDispatchResult>(httpResponse);
                if (result.Status != HttpResponse.ResultStatus.Success)
                {
                    // TODO : https://linear.app/metica/issue/MET-3515/
                    // does this case need retry logic? Queue is cleared at this stage thus events may get lost.
                    Log.Warning(() => $"EventManager.DispatchEvents: Response indicates failure: {result.Error}. Queue has been cleared.");
                }
                result.OriginalRequestBody = body;
                OnEventsDispatch?.Invoke(result);
            }
            catch (System.Net.Http.HttpRequestException exception)
                when (exception.InnerException is TimeoutException || exception.Message.Contains("timed out"))
            {
                Log.Error(() => $"EventManager.DispatchEvents: Request timed out: {exception.Message}");
                EventDispatchResult result = new EventDispatchResult
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = $"Timeout: {exception.Message}",
                    RawContent = null,
                    OriginalRequestBody = body
                };
                OnEventsDispatch?.Invoke(result);
            }
            catch (Exception exception)
            {
                Log.Error(() => $"EventManager.DispatchEvents: Exception: {exception.Message}");
                EventDispatchResult result = new EventDispatchResult
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = exception.Message,
                    RawContent = null,
                    OriginalRequestBody = body
                };
                OnEventsDispatch?.Invoke(result);
            }
            finally
            {
                _lastEventDispatchUnixTime = _timeSource.EpochSeconds();
            }
        }

        private void DispatchHandler(EventDispatchResult result)
        {
            Log.Debug(() => $"Events Dispatched.\n{result}");
        }

        public override async ValueTask DisposeAsync()
        {
            await DispatchEvents();
        }
    }
}
