using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Metica.Core;
using Metica.Network;
using Metica.Model;

namespace Metica
{
    [System.Serializable]
    public class ConfigResult : IMeticaHttpResult
    {
        // TODO : ideally fields should be readonly or with private setter

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

        /// <summary>
        /// Asynchronously retrieves configurations, automatically injecting device-specific information.
        /// </summary>
        public async Task<ConfigResult> GetConfigsAsync(string userId, List<string> configKeys = null, Dictionary<string, object> userData = null, DeviceInfo deviceInfo = null)
        {
            try
            {
                // If the incoming userData is null, create a new dictionary. Otherwise, use the existing one.
                var finalUserData = userData ?? new Dictionary<string, object>();

                // Inject the additional device information using the SystemInfo class
                // This will add the keys or update them if they already exist.
                finalUserData["deviceType"] = _deviceInfoProvider.deviceType.ToString();
                finalUserData["osVersion"] = _deviceInfoProvider.operatingSystem;
                finalUserData["deviceModel"] = _deviceInfoProvider.deviceModel;

                var requestBody = new Dictionary<string, object>
                {
                    { FieldNames.UserId, userId },
                    { FieldNames.ConfigKeys, configKeys },
                    // Use the augmented dictionary here
                    { FieldNames.UserData, finalUserData },
                    { FieldNames.DeviceInfo, _deviceInfoProvider.GetDeviceInfo() },
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
                Log.Debug(() => $"MeticaSdk ConfigManager.GetConfigsAsync: Success: {httpResponse.ResponseContent}");
                return ResponseToResult<ConfigResult>(httpResponse);
            }
            catch (System.Net.Http.HttpRequestException exception)
                when (exception.InnerException is System.TimeoutException || exception.Message.Contains("timed out"))
            {
                Log.Error(() => $"ConfigManager.GetConfigsAsync: Request timed out: {exception.Message}");
                return new ConfigResult {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = $"Timeout: {exception.Message}",
                    RawContent = null,
                    Configs = null
                };
            }
            catch (System.Exception exception)
            {
                Log.Error(() => $"ConfigManager.GetConfigsAsync: Exception: {exception.Message}");
                return new ConfigResult {
                    Status = HttpResponse.ResultStatus.Failure,
                    Error = exception.Message,
                    RawContent = null,
                    Configs = null
                };
            }
        }

        public override ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
