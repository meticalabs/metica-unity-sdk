using Metica.Experimental.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public class OffersManager
    {
        private IHttpService _httpService;
        private string _url;

        public OffersManager(IHttpService httpService, string offersEndpoint)
        {
            {
                _httpService = httpService;
                _url = offersEndpoint;
            }
        }

        public struct OffersResult
        {
            public Dictionary<string, List<Metica.Unity.Offer>> placements { get; set; }

            [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
            [JsonIgnore] public string Error { get; set; }
            [JsonIgnore] public string RawContent {  get; set; }

            public override string ToString()
            {
                return $"{nameof(OffersResult)}:\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
            }
        }

        public async Task<OffersResult> GetOffersAsync(string userId, string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
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
            if (httpResponse.Status == HttpResponse.ResultStatus.Success)
            {
                string content = httpResponse.ResponseContent;
                OffersResult offersResult = JsonConvert.DeserializeObject<OffersResult>(content);
                offersResult.Status = httpResponse.Status;
                offersResult.RawContent = httpResponse.ResponseContent;
                return offersResult;
            }
            else
            {
                return new() {
                    Status = httpResponse.Status,
                    Error = httpResponse.ErrorMessage,
                    RawContent = httpResponse.ResponseContent,
                    placements = null
                };
            }
        }
    }
}
