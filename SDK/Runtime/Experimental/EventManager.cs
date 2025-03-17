using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public class EventManager : EndpointManager
    {
        IMeticaAttributesProvider _meticaAttributesProvider;

        public EventManager(IHttpService httpService, string endpoint, IMeticaAttributesProvider meticaAttributesProvider) : base(httpService, endpoint)
        {
            _meticaAttributesProvider = meticaAttributesProvider;
        }

        public struct EventResult : IMeticaSdkResult
        {
            [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
            [JsonIgnore] public string Error { get; set; }
            [JsonIgnore] public string RawContent { get; set; }
        }

        public EventManager WithMeticaAttributesProvider(IMeticaAttributesProvider meticaAttributesProvider)
        {
            _meticaAttributesProvider = meticaAttributesProvider;
            return this;
        }

        private async Task<EventResult> SendEventWithMeticaAttributesAsync(string userId, string appId, string placementId, string offerId, string eventType, string eventId, long eventTime, object customPayload)
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
            object meticaAttributes = await _meticaAttributesProvider.GetMeticaAttributes(offerId, placementId);
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
                { nameof(eventId), eventId },
                { nameof(appId), appId },
                { nameof(eventTime), eventTime },
                { nameof(userId), userId },
                { nameof(meticaAttributes), meticaAttributes },
                //{ nameof(deviceInfo), deviceInfo }, // TODO
                { nameof(customPayload), customPayload }
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<EventResult>(httpResponse);
        }

        private async Task<EventResult> SendEventWithProductId(string userId, string appId, string productId, string eventType, string eventId, long eventTime, object customPayload)
        {
            var requestBody = new Dictionary<string, object>
            {
                { nameof(eventType), eventType },
                { nameof(eventId), eventId },
                { nameof(appId), appId },
                { nameof(eventTime), eventTime },
                { nameof(userId), userId },
                { nameof(productId), productId },
                //{ nameof(deviceInfo), deviceInfo }, // TODO
                { nameof(customPayload), customPayload }
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<EventResult>(httpResponse);
        }
    }
}
