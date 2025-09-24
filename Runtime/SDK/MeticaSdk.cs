using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metica.Core;
using Metica.Network;
using Metica.SDK.Model;

namespace Metica.SDK
{
    public interface IMeticaSdk : IAsyncDisposable
    {
        Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userData = null);
        [Obsolete]
        Task<ConfigResult> GetConfigsAsync(List<string> configKeys, Dictionary<string, object> userData, DeviceInfo deviceInfo);
        Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null);
        [Obsolete]
        Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData, DeviceInfo deviceInfo);
        void LogAdRevenueEvent(string placement, string type, string source, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
        void LogCustomEvent(string customEventType, Dictionary<string, object> customPayload = null);
        void LogInstallEvent(Dictionary<string, object> customPayload = null);
        void LogLoginEvent(Dictionary<string, object> customPayload = null);
        void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null);
        void LogOfferImpressionEventWithProductId(string productId, Dictionary<string, object> customPayload = null);
        void LogOfferInteractionEvent(string placementId, string offerId, string interactionType, Dictionary<string, object> customPayload = null);
        void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null);
        void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
        void LogOfferPurchaseEventWithProductId(string productId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null);
        void LogFullStateUserUpdateEvent(Dictionary<string, object> fullUserStateAttributes, Dictionary<string, object> customPayload = null);
        void LogPartialStateUserUpdateEvent(Dictionary<string, object> partialUserStateAttributes, Dictionary<string, object> customPayload = null);
        void RequestDispatchEvents();
    }

    public class MeticaSdk : IMeticaSdk
    {
        #region Fields

        public static IMeticaSdk SDK {  get => Registry.Resolve<IMeticaSdk>(); }

        public static string Version { get => "1.13.4"; }

        public static string CurrentUserId {  get; set; }
        public static string ApiKey { get; private set; }
        public static string AppId { get; private set; }
        public static string BaseEndpoint { get; private set; }

        private readonly SdkConfig _sdkConfig;
        private readonly IHttpService _http;
        private readonly OfferManager _offerManager;
        private readonly ConfigManager _configManager;
        private readonly EventManager _eventManager;

        private SdkConfig Config { get => _sdkConfig; } // alias for above

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

        /// <summary>
        /// Metica SDK, engine and platform independent, control room.
        /// </summary>
        /// <param name="config">Metica SDK configuration object.</param>
        /// <remarks>
        /// <h2>ROADMAP</h2>
        /// - Swap inline strings like "purchase" with constants.
        /// </remarks>
        public MeticaSdk(SdkConfig config)
        {
            _sdkConfig = config;

            if(string.IsNullOrEmpty(_sdkConfig.apiKey) || string.IsNullOrEmpty(_sdkConfig.initialUserId) || string.IsNullOrEmpty(_sdkConfig.appId) || string.IsNullOrEmpty(config.baseEndpoint))
            {
                Log.Error(() => "The given SDK configuration is not valid. Please make sure all fields are filled.");
                return;
            }
            if (_sdkConfig.baseEndpoint.EndsWith('/'))
            {
                Log.Error(() => "Please remove the '/' character at the end of the endpoint URL");
                return;
            }

            // In the following code we compose our SDK

            _http = new HttpServiceDotnet(
                requestTimeoutSeconds: config.httpRequestTimeout,
                cacheGCTimeoutSeconds: 10,
                cacheTTLSeconds: 60
                ).WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", Config.apiKey } });
            // Initialize an OfferManager
            _offerManager = new OfferManager(_http, $"{Config.baseEndpoint}/offers/v1/apps/{Config.appId}");
            // Initialize a ConfigManager
            _configManager = new ConfigManager(_http, $"{Config.baseEndpoint}/configs/v1/apps/{Config.appId}");
            // Initialize an EventManager with _offerManager as IMeticaAttributesProvider
            _eventManager = new EventManager(_http, $"{Config.baseEndpoint}/ingest/v1/events", _offerManager, config.eventsLogDispatchMaxQueueSize);
            // Set the current (mutable) CurrentUserId with the initial value given in the configuration
            CurrentUserId = null; // Config.initialUserId;
            ApiKey = Config.apiKey;
            AppId = Config.appId;
            BaseEndpoint = Config.baseEndpoint;
                
            // Register this class as IMeticaSdk service in Registry
            Registry.Register<IMeticaSdk>(this);
        }

        public async Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null)
            => await _offerManager.GetOffersAsync(CurrentUserId, placements, userData);
        [Obsolete]
        public async Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData, DeviceInfo deviceInfo)
            => await _offerManager.GetOffersAsync(CurrentUserId, placements, userData);


        public async Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userProperties = null)
            => await _configManager.GetConfigsAsync(CurrentUserId, configKeys, userProperties);
        [Obsolete]
        public async Task<ConfigResult> GetConfigsAsync(List<string> configKeys, Dictionary<string, object> userProperties, DeviceInfo deviceInfo)
            => await _configManager.GetConfigsAsync(CurrentUserId, configKeys, userProperties);


        public void LogLoginEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                EventTypes.Login,
                null,
                customPayload);


        public void LogInstallEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                EventTypes.Install,
                null,
                customPayload);


        public void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                Config.appId,
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
                Config.appId,
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
                Config.appId,
                placementId,
                offerId,
                EventTypes.OfferInteraction,
                new() { { nameof(interactionType), interactionType } },
                customPayload);


        public void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                Config.appId,
                productId,
                EventTypes.OfferInteraction,
                new() { { nameof(interactionType), interactionType} },
                customPayload);


        public void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                Config.appId,
                placementId,
                offerId,
                EventTypes.OfferImpression,
                null,
                customPayload);


        public void LogOfferImpressionEventWithProductId(string productId, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                Config.appId,
                productId,
                EventTypes.OfferImpression,
                null,
                customPayload);


        public void LogAdRevenueEvent(string placement, string type, string source, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
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
                Config.appId,
                EventTypes.FullStateUpdate,
                new() { { nameof (userStateAttributes), userStateAttributes }, },
                customPayload);


        public void LogPartialStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
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
            Config.appId,
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
