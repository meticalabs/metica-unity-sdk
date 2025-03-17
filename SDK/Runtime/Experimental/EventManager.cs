using Metica.Experimental.Network;
using Metica.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    [System.Serializable]
    public class EventResult : IMeticaSdkResult
    {
        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent { get; set; }

        public override string ToString()
        {
            return $"{nameof(EventResult)}:\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
        }

    }

    internal class EventManager : EndpointManager
    {
        private readonly IMeticaAttributesProvider _meticaAttributesProvider;
        private readonly Metica.Experimental.Core.ITimeSource _timeSource = new SystemDateTimeSource();
        private readonly SdkConfig _sdkConfig;

        private List<object> _events;

        public EventManager(IHttpService httpService, string endpoint, IMeticaAttributesProvider meticaAttributesProvider) : base(httpService, endpoint)
        {
            _meticaAttributesProvider = meticaAttributesProvider;
            _events = new List<object>();
        }

        public async Task<EventResult> SendEventWithMeticaAttributesAsync(string userId, string appId, string placementId, string offerId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            if (_meticaAttributesProvider == null)
            {
                return new()
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    RawContent = string.Empty,
                    Error = $"There was an initialization problem. To use this method an {nameof(IMeticaAttributesProvider)} nmust be provided."
                };
            }
            object meticaAttributes = await _meticaAttributesProvider.GetMeticaAttributes(placementId, offerId);
            if (meticaAttributes == null)
            {
                return new()
                {
                    Status = HttpResponse.ResultStatus.Failure,
                    RawContent = string.Empty,
                    Error = $"Placement with name \"{placementId}\"or offer with name \"{offerId}\" wasn't found."
                };
            }
            
            var requestBody = new Dictionary<string, object>
            {
                { nameof(eventType), eventType },
                { "eventId", Guid.NewGuid().ToString() },
                { "eventTime", _timeSource.EpochSeconds() },
                { nameof(appId), appId },
                { nameof(userId), userId },
                { nameof(meticaAttributes), meticaAttributes },
                //{ nameof(deviceInfo), deviceInfo }, // TODO
                { nameof(customPayload), customPayload }
            };

            if(eventFields != null)
            {
                requestBody.Concat(eventFields);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            _events.Add(requestBody);

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(new Dictionary<string, object> { { "events", _events } }, settings), "application/json");

            _events.Clear();

            return ResponseToResult<EventResult>(httpResponse);
        }

        public async Task<EventResult> SendEventWithProductId(string userId, string appId, string productId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        {
            var requestBody = new Dictionary<string, object>
            {
                { nameof(userId), userId },
                { nameof(appId), appId },
                { nameof(productId), productId },
                { nameof(eventType), eventType },
                { "eventId", Guid.NewGuid().ToString() },
                { "eventTime", _timeSource.EpochSeconds() },
                //{ nameof(deviceInfo), deviceInfo }, // TODO
                { nameof(customPayload), customPayload }
            };

            if(eventFields != null)
            {
                requestBody.Concat(eventFields);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            _events.Add(requestBody);

            //UnityEngine.Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, object> { { "events", _events } }, settings));

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(new Dictionary<string, object> { { "events", _events } }, settings), "application/json");

            _events.Clear();

            return ResponseToResult<EventResult>(httpResponse);
        }

        //public async Task<EventResult> LogPurchaseEvent(string userId, string appId, string placementId, string offerId, string eventType, Dictionary<string, object> eventFields, Dictionary<string, object> customPayload)
        //{
            
        //}
    }
}
