using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public class EventManager : EndpointManager
    {
        IMeticaAttributesProvider _meticaAttributesProvider;

        public EventManager(IHttpService httpService, string endpoint) : base(httpService, endpoint)
        {
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

        private async Task<EventResult> SendEventAsync(object requestBody)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<EventResult>(httpResponse);
        }

        //public async Task<EventResult> SendEventAsync(string userId, string appId, Metica.Unity.Offer relatedToOffer, string eventType, string eventId, long eventTime, object customPayload)
        //{
        //    var requestBody = new Dictionary<string, object>
        //    {
        //        { nameof(eventType), eventType },
        //        { nameof(eventId), eventId },
        //        { nameof(appId), appId },
        //        { nameof(eventTime), eventTime },
        //        { nameof(userId), userId },
        //        //{ nameof(deviceInfo), deviceInfo },
        //        { nameof(customPayload), customPayload }
        //        // TODO : build meticaAttributes
        //    };
        //    JsonSerializerSettings settings = new JsonSerializerSettings();
        //    settings.NullValueHandling = NullValueHandling.Ignore;

        //    var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
        //    return ResponseToResult<EventResult>(httpResponse);
        //}

        //public async Task<EventResult> SendEventWithMeticaAttributes()
        //{
            
        //}

    }
}
