using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    [System.Serializable]
    public struct OfferResult : IMeticaSdkResult
    {
        // TODO : fields should be readonly or with private setter

        public Dictionary<string, List<Metica.Unity.Offer>> Placements { get; set; }

        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent {  get; set; }

        public override string ToString()
        {
            string placementsString = string.Empty;
            if(Placements != null)
            {
                foreach (var p in Placements)
                {
                    placementsString = $"{placementsString}{p.Key}\n";
                }
            }
            return $"{nameof(OfferResult)}:\n{placementsString}\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
        }
    }

    public sealed class OfferManager : EndpointManager
    {
        public OfferManager(IHttpService httpService, string offersEndpoint) : base(httpService, offersEndpoint)
        {
        }

        public async Task<OfferResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { nameof(userId), userId },
                { nameof(deviceInfo), deviceInfo },
                { nameof(userData), userData }
            };

            var url = _url;
            if(placements != null && placements.Length > 0)
            {
                url = $"{url}?placements=";
                for (int i = 0; i < placements.Length; i++)
                {
                    var pl = placements[i];
                    url = $"{url}{pl}{((i<placements.Length-1)?",":"")}";
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<OfferResult>(httpResponse);
        }
    }
   
}
