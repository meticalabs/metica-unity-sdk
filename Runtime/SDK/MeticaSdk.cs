using System.Collections.Generic;
using System.Threading.Tasks;
using Metica.Network;
using Metica.Core;
using Metica.SDK.Model;
using Metica.SDK.Unity;
using Metica.ADS;

namespace Metica.SDK
{
    // public interface IMeticaSdk : IAsyncDisposable
    // {
    //     Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userData = null);
    //     [Obsolete]
    //     Task<ConfigResult> GetConfigsAsync(List<string> configKeys, Dictionary<string, object> userData, DeviceInfo deviceInfo);
    //     Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null);
    //     [Obsolete]
    //     Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData, DeviceInfo deviceInfo);
    //     void LogAdRevenueEvent(string placement, string type, string source, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
    //     void LogCustomEvent(string customEventType, Dictionary<string, object> customPayload = null);
    //     void LogInstallEvent(Dictionary<string, object> customPayload = null);
    //     void LogLoginEvent(Dictionary<string, object> customPayload = null);
    //     void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null);
    //     void LogOfferImpressionEventWithProductId(string productId, Dictionary<string, object> customPayload = null);
    //     void LogOfferInteractionEvent(string placementId, string offerId, string interactionType, Dictionary<string, object> customPayload = null);
    //     void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null);
    //     void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
    //     void LogOfferPurchaseEventWithProductId(string productId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
    //     void LogFullStateUserUpdateEvent(Dictionary<string, object> fullUserStateAttributes, Dictionary<string, object> customPayload = null);
    //     void LogPartialStateUserUpdateEvent(Dictionary<string, object> partialUserStateAttributes, Dictionary<string, object> customPayload = null);
    //     void RequestDispatchEvents();
    // }

    public class MeticaSdk
    {
        #region Fields

        public static MeticaSdk SDK { get; private set; } = null;

        public static string Version { get => "1.13.4"; }

        public static string CurrentUserId {  get; set; }
        public static string ApiKey { get; private set; }
        public static string AppId { get; private set; }
        // TODO: remove apploving key fromm here in favour of
        // more configurability considering possible additions of other ad providers.
        public static string ApplovinKey { get; private set; }
        public static string BaseEndpoint { get; private set; }

        private readonly IHttpService _http;
        internal readonly OfferManager _offerManager;
        internal readonly ConfigManager _configManager;
        internal readonly EventManager _eventManager;

        public static bool IsMeticaAdsEnabled { get; private set; }


        #endregion Fields

        /// <summary>
        ///  Utility, (Unity specific) method that should not be used directly.
        /// Resets static properties to null.
        /// </summary>
        public static void ResetStaticFields()
        {
            CurrentUserId = null;
            ApiKey = null;
            AppId = null;
            BaseEndpoint = null;
        }

        
        private static bool CheckConfig(SdkConfig config)
        {
            if (string.IsNullOrEmpty(config.apiKey) || string.IsNullOrEmpty(config.appId) || string.IsNullOrEmpty(config.baseEndpoint))
            {
                Log.Error(() => "The given SDK configuration is not valid. Please make sure all fields are filled.");
                return false;
            }
            if (config.baseEndpoint.EndsWith('/'))
            {
                Log.Error(() => "Please remove the '/' character at the end of the endpoint URL");
                return false;
            }
            return true;
        }

        /// <summary>
        /// NEW INITIALIZATION
        /// TODO: return result
        /// </summary>
        public static async Task InitializeAsync(SdkConfig config)
        {
            RegisterServices(config);
            CheckConfig(config);
            if (SDK != null)
            {
                Log.Warning(() => "Metica SDK reinitialized. This means a new initialization was done on top of a previous one.");
            }
            SDK = new MeticaSdk(config);

            // ADS
            IsMeticaAdsEnabled = await MeticaAds.InitializeAsync(new MeticaConfiguration()); // TODO: placeholder for configuration (should be SdkConfig)
        }

        /// <summary>
        /// OLD INITIALIZATION
        /// </summary>
        // public static MeticaSdk Initialize(SdkConfig sdkConfig)
        // {
        //     return new MeticaSdk(sdkConfig);
        // }

        /// <summary>
        /// Registration of implementation of services
        /// </summary>
        /// <param name="config"></param>
        public static void RegisterServices(SdkConfig config)
        {
            Registry.Register<IDeviceInfoProvider>(new DeviceInfoProvider());
            Registry.Register<ILog>(new MeticaLogger(config.logLevel));
        }

        /// <summary>
        /// ORIGINAL SDK INITIALIZATION
        /// </summary>
        /// <param name="config">Metica SDK configuration object.</param>
        public MeticaSdk(SdkConfig config)
        {
            // 
            _http = new HttpServiceDotnet(
                requestTimeoutSeconds: 60,
                cacheGCTimeoutSeconds: 10,
                cacheTTLSeconds: 60
                ).WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", config.apiKey } });
            // Initialize an OfferManager
            _offerManager = new OfferManager(_http, $"{config.baseEndpoint}/offers/v1/apps/{config.appId}");
            // Initialize a ConfigManager
            _configManager = new ConfigManager(_http, $"{config.baseEndpoint}/configs/v1/apps/{config.appId}");
            // Initialize an EventManager with _offerManager as IMeticaAttributesProvider
            _eventManager = new EventManager(_http, $"{config.baseEndpoint}/ingest/v1/events", _offerManager);
            // Set the CurrentUserId with the initial value given in the configuration
            CurrentUserId = config.userId;
            ApiKey = config.apiKey;
            AppId = config.appId;
            BaseEndpoint = config.baseEndpoint;
        }

        public async Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null)
            => await _offerManager.GetOffersAsync(CurrentUserId, placements, userData);

        internal async Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userProperties = null)
            => await _configManager.GetConfigsAsync(CurrentUserId, configKeys, userProperties);

        public void LogLoginEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                AppId,
                EventTypes.Login,
                null,
                customPayload);


        public void LogInstallEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                AppId,
                EventTypes.Install,
                null,
                customPayload);


        public void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                AppId,
                placementId,
                offerId,
                EventTypes.OfferPurchase,
                new() {
                    { nameof(currencyCode), currencyCode },
                    { nameof(totalAmount), totalAmount },
                },
                customPayload);


        public void LogOfferPurchaseEventWithProductId(string productId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                AppId,
                productId,
                EventTypes.OfferPurchase,
                new() {
                    { nameof(currencyCode), currencyCode },
                    { nameof(totalAmount), totalAmount },
                },
                customPayload);


        public void LogOfferInteractionEvent(string placementId, string offerId, string interactionType, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                AppId,
                placementId,
                offerId,
                EventTypes.OfferInteraction,
                new() { { nameof(interactionType), interactionType } },
                customPayload);


        public void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                AppId,
                productId,
                EventTypes.OfferInteraction,
                new() { { nameof(interactionType), interactionType} },
                customPayload);


        public void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                AppId,
                placementId,
                offerId,
                EventTypes.OfferImpression,
                null,
                customPayload);


        public void LogOfferImpressionEventWithProductId(string productId, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                AppId,
                productId,
                EventTypes.OfferImpression,
                null,
                customPayload);


        public void LogAdRevenueEvent(string placement, string type, string source, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                AppId,
                EventTypes.AdRevenue,
                new() {
                    { nameof (placement), placement },
                    { nameof (type), type },
                    { nameof (source), source },
                    { nameof (currencyCode), currencyCode },
                    { nameof (totalAmount), totalAmount },
                },
                customPayload);


        public void LogFullStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                AppId,
                EventTypes.FullStateUpdate,
                new() { { nameof (userStateAttributes), userStateAttributes }, },
                customPayload);


        public void LogPartialStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                AppId,
                EventTypes.PartialStateUpdate,
                new() { { nameof(userStateAttributes), userStateAttributes } },
                customPayload);

        public void LogCustomEvent(string customEventType, Dictionary<string, object> customPayload = null)
        {
            if (EventTypes.IsEventType(customEventType))
            {
                Log.Error(() => $"{customEventType} cannot be used with {nameof(LogCustomEvent)}. Please use an event type that is not a core event. See documentation at https://docs.metica.com/integration#core-events.");
                return;
            }
            _eventManager.QueueEventAsync(
            CurrentUserId,
            AppId,
            customEventType,
            null,
            customPayload);
        }

        public void RequestDispatchEvents()
        {
            _ = _eventManager.RequestDispatchEvents();
        }

        public async ValueTask DisposeAsync()
        {
            if (_eventManager != null) await _eventManager.DisposeAsync();
            if (_offerManager != null) await _offerManager.DisposeAsync();
            if (_configManager != null) await _configManager.DisposeAsync();
            _http?.Dispose();
        }
    }
}
