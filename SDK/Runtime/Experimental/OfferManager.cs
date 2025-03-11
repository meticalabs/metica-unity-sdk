using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public sealed class OfferManager : EndpointManager
    {

        public OfferManager(IHttpService httpService, string offersEndpoint) : base(httpService, offersEndpoint)
        {
        }

        public struct OfferResult : IMeticaSdkResult
        {
            public Dictionary<string, List<Metica.Unity.Offer>> placements { get; set; }

            [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
            [JsonIgnore] public string Error { get; set; }
            [JsonIgnore] public string RawContent {  get; set; }

            public override string ToString()
            {
                return $"{nameof(OfferResult)}:\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
            }
        }

        public async Task<OfferResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { nameof(userId), userId },
                { nameof(deviceInfo), deviceInfo },
                { nameof(userData), userData }
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(_url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<OfferResult>(httpResponse);
        }
    }
   
}
