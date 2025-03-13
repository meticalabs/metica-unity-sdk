using Metica.Experimental.Network;
using Metica.Experimental.Core;
using Metica.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental
{
    public interface IMeticaSdk { }

    public class MeticaSdk : IMeticaSdk
    {
        #region Fields

        public static string CurrentUserId {  get; set; }

        private readonly SdkConfig _sdkConfig;
        private readonly IHttpService _http;
        private readonly OfferManager _offerManager;
        private readonly ConfigManager _configManager;

        private Metica.Unity.SdkConfig Config { get => _sdkConfig; } // alias for above

        #endregion Fields

        public MeticaSdk(SdkConfig config)
        {
            _sdkConfig = config;
            // In the following code we compose our SDK

            // Use the .NET based IHttpService implementation
            _http = new HttpServiceDotnet().WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", Config.apiKey } });
            // Initialize an OfferManager
            _offerManager = new OfferManager(_http, $"{Config.offersEndpoint}/offers/v1/apps/{Config.appId}");
            // Initialize a ConfigManager
            _configManager = new ConfigManager(_http, $"{Config.remoteConfigEndpoint}/config/v1/apps/{Config.appId}");
            // Set the current (mutable) CurrentUserId with the initial value given in the configuration
            CurrentUserId = Config.initialUserId;

            // Register this class as IMeticaSdk service in Registry
            Registry.Register<IMeticaSdk>(this);
        }

        public async Task<OfferManager.OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
            => await _offerManager.GetOffersAsync(CurrentUserId, placements, userData, deviceInfo);

        public async Task<ConfigManager.ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
            => await _configManager.GetConfigsAsync(CurrentUserId, configKeys, userProperties, deviceInfo);

        

        public void Cleanup()
        {
            _http?.Dispose();
        }
    }
}
