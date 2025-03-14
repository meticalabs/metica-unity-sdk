using Metica.Experimental.Caching;
using Metica.Experimental.Core;
using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    [System.Serializable]
    public class OfferResult : IMeticaSdkResult
    {
        // TODO : fields should be readonly or with private setter

        public Dictionary<string, List<Metica.Unity.Offer>> Placements { get; set; }

        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent {  get; set; }

        public void Append(Dictionary<string, List<Metica.Unity.Offer>> placements)
        {
            foreach (var k in placements.Keys)
            {
                Placements.Add(k, placements[k]); // TODO : manage possible key exception?
            }
        }

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
        private readonly ITimeSource timeSource = new SystemDateTimeSource();
        private readonly Cache<string, List<Metica.Unity.Offer>> placementCache;

        public OfferManager(IHttpService httpService, string offersEndpoint) : base(httpService, offersEndpoint)
        {
            placementCache = new Cache<string, List<Metica.Unity.Offer>>(timeSource);
        }

        public async Task<OfferResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
        {
            var cachedPlacements = placementCache.GetAsDictionary(placements);
            var pendingPlacementKeys = placementCache.GetMissingKeys(placements);

            var requestBody = new Dictionary<string, object>
            {
                { nameof(userId), userId },
                { nameof(deviceInfo), deviceInfo },
                { nameof(userData), userData }
            };

            // We only retrieve via http the pending placements
            var url = _url;
            if(pendingPlacementKeys != null && pendingPlacementKeys.Length > 0)
            {
                url = $"{url}?placements=";
                for (int i = 0; i < pendingPlacementKeys.Length; i++)
                {
                    var pl = pendingPlacementKeys[i];
                    url = $"{url}{pl}{((i<pendingPlacementKeys.Length-1)?",":"")}";
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            OfferResult offerResult = ResponseToResult<OfferResult>(httpResponse);
            if (cachedPlacements != null)
            {
                offerResult.Append(cachedPlacements);
            }
            return offerResult;
        }
    }
   
}
