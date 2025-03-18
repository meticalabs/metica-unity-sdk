using Metica.Experimental.Network;
using Metica.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    [System.Serializable]
    public class EventDispatchResult : IMeticaSdkResult
    {
        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent { get; set; }

        public override string ToString()
        {
            return $"{nameof(EventDispatchResult)}:\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
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
    /// - TODO : use a model to manage parameters more consistently to reduce chances of human errors in passing all strings.
    /// - TODO : improve async/await calls and don't bubble up warnings when we fire&forget.
    /// - TODO : device info
    /// - TODO : use constants for eventTypes
    /// - TODO : (low priority) create an event queue processing system that processes the queue right before sending (for exampe to aggregate certain events)
    /// </remarks>
    internal class EventManager : EndpointManager
    {
        public delegate void OnEventsDispatchDelegate(EventDispatchResult eventResult);
        public event OnEventsDispatchDelegate OnEventsDispatch;

        private readonly IMeticaAttributesProvider _meticaAttributesProvider;
        private readonly Metica.Experimental.Core.ITimeSource _timeSource = new SystemDateTimeSource();
        private readonly SdkConfig _sdkConfig;

        private const int DISPATCH_TRIGGER_COUNT = 2; // TODO : temporarily hardcoded

        private List<object> _events;

        public EventManager(IHttpService httpService, string endpoint, IMeticaAttributesProvider meticaAttributesProvider) : base(httpService, endpoint)
        {
            _meticaAttributesProvider = meticaAttributesProvider;
            _events = new List<object>();
            OnEventsDispatch -= DispatchHandler;
            OnEventsDispatch += DispatchHandler;
        }

        internal async Task QueueEventAsync(string userId, string appId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            var requestBody = new Dictionary<string, object>
            {
                { nameof(eventType), eventType },
                { "eventId", Guid.NewGuid().ToString() },
                { "eventTime", _timeSource.EpochSeconds() },
                { nameof(appId), appId },
                { nameof(userId), userId },
                //{ nameof(deviceInfo), deviceInfo }, // TODO
                { nameof(customPayload), customPayload }
            };

            if(eventFields != null)
            {
                requestBody.AddDictionary(eventFields, overwriteExistingKeys: true);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            _events.Add(requestBody);

            if(_events.Count >= DISPATCH_TRIGGER_COUNT)
            {
                Dispatch();
            }
        }

        internal async Task QueueEventWithMeticaAttributesAsync(string userId, string appId, string placementId, string offerId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            if (_meticaAttributesProvider == null)
            {
                return; // TODO : raise error
            }
            object meticaAttributes = await _meticaAttributesProvider.GetMeticaAttributes(placementId, offerId);
            if (meticaAttributes == null)
            {
                return; // TODO : raise error
            }

            if (eventFields == null)
            {
                eventFields = new Dictionary<string, object>();
            }

            eventFields.AddDictionary(
                new()
                {
                    { nameof(meticaAttributes), meticaAttributes }
                });

            QueueEventAsync(
                userId,
                appId,
                eventType,
                eventFields,
                customPayload
                );
        }

        internal async Task QueueEventWithProductIdAsync(string userId, string appId, string productId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            if (eventFields == null)
            {
                eventFields = new Dictionary<string, object>();
            }

            eventFields.AddDictionary(
                new()
                {
                    { nameof(productId), productId }
                });

            QueueEventAsync(
                userId,
                appId,
                eventType,
                eventFields,
                customPayload
                );
        }

        private async Task Dispatch()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(new Dictionary<string, object> { { "events", _events } }, settings), "application/json");
            _events.Clear();
            EventDispatchResult result = ResponseToResult<EventDispatchResult>(httpResponse);
            OnEventsDispatch?.Invoke(result);
        }

        private void DispatchHandler(EventDispatchResult result)
        {
            UnityEngine.Debug.Log($"Events Dispatched.\n{result}");
        }
    }
}
