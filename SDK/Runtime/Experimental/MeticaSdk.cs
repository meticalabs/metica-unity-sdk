using Metica.Experimental.Network;
using Metica.Experimental.Core;
using Metica.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Metica.Experimental
{
    public interface IMeticaSdk { }

    public class MeticaSdk : IMeticaSdk, IAsyncDisposable
    {
        #region Fields

        public static string CurrentUserId {  get; set; }

        private readonly SdkConfig _sdkConfig;
        private readonly IHttpService _http;
        private readonly OfferManager _offerManager;
        private readonly ConfigManager _configManager;
        private readonly EventManager _eventManager;

        private Metica.Unity.SdkConfig Config { get => _sdkConfig; } // alias for above

        #endregion Fields

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

            // In the following code we compose our SDK

            // Use the .NET based IHttpService implementation
            _http = new HttpServiceDotnet().WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", Config.apiKey } });
            // Initialize an OfferManager
            _offerManager = new OfferManager(_http, $"{Config.offersEndpoint}/offers/v1/apps/{Config.appId}");
            // Initialize a ConfigManager
            _configManager = new ConfigManager(_http, $"{Config.remoteConfigEndpoint}/config/v1/apps/{Config.appId}");
            // Initialize an EventManager with _offerManager as IMeticaAttributesProvider
            _eventManager = new EventManager(_http, $"{Config.ingestionEndpoint}/ingest/v1/events", _offerManager);
            // Set the current (mutable) CurrentUserId with the initial value given in the configuration
            CurrentUserId = Config.initialUserId;

            // Register this class as IMeticaSdk service in Registry
            Registry.Register<IMeticaSdk>(this);
        }

        public async Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null, Metica.Unity.DeviceInfo deviceInfo = null)
            => await _offerManager.GetOffersAsync(CurrentUserId, placements, userData, deviceInfo);


        public async Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
            => await _configManager.GetConfigsAsync(CurrentUserId, configKeys, userProperties, deviceInfo);


        public void LogLoginEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                "login",
                null,
                customPayload);


        public void LogInstallEvent(Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                "install",
                null,
                customPayload);


        public void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                Config.appId,
                placementId,
                offerId,
                "purchase",
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
                "purchase",
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
                "interaction",
                new() { { nameof(interactionType), interactionType } },
                customPayload);


        public void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                Config.appId,
                productId,
                "interaction",
                new() { { nameof(interactionType), interactionType} },
                customPayload);


        public void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null)
            => _ = _eventManager.QueueEventWithMeticaAttributesAsync(
                CurrentUserId,
                Config.appId,
                placementId,
                offerId,
                "impression",
                null,
                customPayload);


        public void LogOfferImpressionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventWithProductId(
                CurrentUserId,
                Config.appId,
                productId,
                "impression",
                null,
                customPayload);


        public void LogAdRevenueEvent(string placement, string type, string source, double totalAmount, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                "adRevenue",
                new() {
                    { nameof (placement), placement },
                    { nameof (type), type },
                    { nameof (source), source },
                    { nameof (totalAmount), totalAmount },
                },
                customPayload);


        public void LogFullStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                "fullStateUpdate",
                new() { { nameof (userStateAttributes), userStateAttributes }, },
                customPayload);


        public void LogPartialStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
            => _eventManager.QueueEventAsync(
                CurrentUserId,
                Config.appId,
                "partialStateUpdate",
                new() { { nameof(userStateAttributes), userStateAttributes } },
                customPayload);

        public async ValueTask DisposeAsync()
        {
            await _eventManager.DisposeAsync();
            await _offerManager.DisposeAsync();
            await _configManager.DisposeAsync();
            _http?.Dispose();
        }
    }
}
