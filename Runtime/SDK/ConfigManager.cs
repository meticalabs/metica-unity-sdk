using System.Collections.Generic;
using System.Threading.Tasks;
using Metica.Core;
using Metica.Network;
using Metica.SDK.Model;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Metica.SDK
{
    [Preserve]
    [System.Serializable]
    public class ConfigResult : IMeticaHttpResult
    {
        // TODO : ideally fields should be readonly or with private setter

        [Preserve]
        [JsonExtensionData] // Needed when we want the root (anonymous dictionary in this case) to go in a specific field
        public Dictionary<string, object> Configs { get; set; }

        [JsonIgnore] public HttpResponse.ResultStatus Status { get; set; }
        [JsonIgnore] public string Error { get; set; }
        [JsonIgnore] public string RawContent { get; set; }

        public override string ToString()
        {
            string configString = string.Empty;
            if(Configs != null)
            {
                foreach (var c in Configs)
                {
                    configString = $"{configString}{c.Key} : {c.Value}\n";
                }
            }
            return $"{nameof(ConfigResult)} [{Configs?.Count}]:\n{configString}\n Status: {Status}\n RawContent: {RawContent}\n Error: {Error}";
        }
    }

    public sealed class ConfigManager : EndpointManager
    {
        private readonly IDeviceInfoProvider _deviceInfoProvider;

        public ConfigManager(IHttpService httpService, string endpoint) : base(httpService, endpoint)
        {
            _deviceInfoProvider = Registry.Resolve<IDeviceInfoProvider>();
        }

        public async Task<ConfigResult> GetConfigsAsync(string userId, List<string> configKeys = null, Dictionary<string, object> userData = null, DeviceInfo deviceInfo = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { FieldNames.UserId, userId },
                { FieldNames.ConfigKeys, configKeys },
                { FieldNames.UserData, userData },
                { FieldNames.DeviceInfo, deviceInfo ?? _deviceInfoProvider.GetDeviceInfo() },
            };

            var url = _url;
            if(configKeys != null && configKeys.Count > 0)
            {
                url = $"{url}?keys=";
                for (int i = 0; i < configKeys.Count; i++)
                {
                    var ck = configKeys[i];
                    url = $"{url}{ck}{((i<configKeys.Count-1)?",":"")}";
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var httpResponse = await _httpService.PostAsync(url, JsonConvert.SerializeObject(requestBody, settings), "application/json");
            return ResponseToResult<ConfigResult>(httpResponse);
        }

        public override ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
