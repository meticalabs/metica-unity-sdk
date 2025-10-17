using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metica.Network;
using Metica.Core;
using Metica.SDK.Model;
using Metica.SDK.Unity;
using Metica.ADS;

namespace Metica.SDK
{
    public class MeticaSdk
    {
        static MeticaSdk() 
        {
            Registry.RegisterIfNull<IDeviceInfoProvider>(new DeviceInfoProvider());
            Registry.RegisterIfNull<ILog>(new MeticaLogger(LogLevel.Error));
        }

        #region Fields
        
        private static MeticaSdk Sdk { get; set; }

        internal static string Version { get => "2.0.0-beta1"; }

        internal static string UserId { get; set; } // TODO: set should become private and require a reinitialization to cheange user id
        public static string ApiKey { get; private set; }
        public static string AppId { get; private set; }

        private readonly IHttpService _http;
        private readonly OfferManager _offerManager;
        private readonly ConfigManager _configManager;
        private readonly EventManager _eventManager;
        private const string EndpointEnd = "https://api-gateway.dev.metica.com";
        private const string EndpointProd = "https://api-gateway.prod-eu.metica.com";

        #endregion Fields

        #region Methods

        public static void SetLogEnabled(bool logEnabled)
        {
            Registry.Register<ILog>(new MeticaLogger(LogLevel.Debug));
            MeticaAds.SetLogEnabled(logEnabled);
        }
        /// <summary>
        /// Utility, (Unity specific) method that should not be used directly.
        /// Resets static properties to null.
        /// </summary>
        public static void ResetStaticFields()
        {
            UserId = null;
            ApiKey = null;
            AppId = null;
        }

        private static void CheckConfig(MeticaInitConfig config)
        {
            if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.AppId))
            {
                Log.Error(() => "The given SDK configuration is not valid. Please make sure all fields are filled.");
                throw new InvalidOperationException("MeticaSDK cannot initialize with invalid configuration - ApiKey and AppId are required.");
            }
        }
        
        private static void CheckMediationInfo(MeticaMediationInfo mediationInfo)
        {
            if (mediationInfo == null)
            {
                Log.Error(() => "MeticaMediationInfo cannot be null.");
                throw new ArgumentNullException(nameof(mediationInfo), "Mediation configuration is required for ad initialization.");
            }

            if (string.IsNullOrEmpty(mediationInfo.Key))
            {
                Log.Error(() => $"The {mediationInfo.MediationType} mediation key is not valid. Please provide a valid SDK key.");
                throw new InvalidOperationException($"MeticaSDK cannot initialize {mediationInfo.MediationType} mediation without a valid key.");
            }
        }

        /// <summary>
        /// Registers services and initializes all SDK components.
        /// </summary>
        public static async Task<MeticaInitResponse> InitializeAsync(
            MeticaInitConfig config,
            MeticaMediationInfo mediationInfo)
        {
            CheckConfig(config);
            // Version 2.0.0 wil ship with MediationInfo parameter being required. 
            // Perhaps in a later version this may be less strict
            CheckMediationInfo(mediationInfo);
            
            if (Sdk != null)
            {
                Log.Warning(() => "Metica SDK reinitialized. This means a new initialization was done on top of a previous one.");
            }
            Sdk = new MeticaSdk(config);

            // ADS
            var result = await MeticaAds.InitializeAsync(config, mediationInfo);
            return result;
        }

        /// <summary>
        /// ORIGINAL SDK INITIALIZATION
        /// </summary>
        /// <param name="config">Metica SDK configuration object.</param>
        private MeticaSdk(MeticaInitConfig config)
        {
            _http = new HttpServiceDotnet(
                requestTimeoutSeconds: 60,
                cacheGCTimeoutSeconds: 10,
                cacheTTLSeconds: 60
                ).WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", config.ApiKey } });
            // Initialize an OfferManager
            _offerManager = new OfferManager(_http, $"{EndpointProd}/offers/v1/apps/{config.AppId}");
            // Initialize a ConfigManager
            _configManager = new ConfigManager(_http, $"{EndpointProd}/configs/v1/apps/{config.AppId}");
            // Initialize an EventManager with _offerManager as IMeticaAttributesProvider
            _eventManager = new EventManager(_http, $"{EndpointProd}/ingest/v1/events", _offerManager);
            // Set the CurrentUserId with the initial value given in the configuration

            // - - - - - - - - - - -

            UserId = config.UserId;
            ApiKey = config.ApiKey;
            AppId = config.AppId;
        }

        public static async ValueTask DisposeAsync()
            => await Sdk.SdkDisposeAsync();

        private async ValueTask SdkDisposeAsync()
        {
            if (_eventManager != null) await _eventManager.DisposeAsync();
            if (_offerManager != null) await _offerManager.DisposeAsync();
            if (_configManager != null) await _configManager.DisposeAsync();
            _http?.Dispose();
            Ads.Dispose(); // TODO
            Sdk = null;
        }

        #endregion Methods

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Offers

        public static class Offers
        {
            public static async Task<OfferResult> GetOffersAsync(string[] placements, Dictionary<string, object> userData = null)
                => await Sdk._offerManager.GetOffersAsync(UserId, placements, userData);
        }

        #endregion Offers

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region SmartConfig

        public static class SmartConfig
        {
            public static async Task<ConfigResult> GetConfigsAsync(List<string> configKeys = null, Dictionary<string, object> userProperties = null)
                => await Sdk._configManager.GetConfigsAsync(UserId, configKeys, userProperties);
        }

        #endregion SmartConfig


        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Events

        public static class Events
        {
            public static void LogLoginEvent(Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventAsync(
                    UserId,
                    AppId,
                    EventTypes.Login,
                    null,
                    customPayload);

            public static void LogInstallEvent(Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventAsync(
                    UserId,
                    AppId,
                    EventTypes.Install,
                    null,
                    customPayload);

            public static void LogOfferPurchaseEvent(string placementId, string offerId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
                => _ = Sdk._eventManager.QueueEventWithMeticaAttributesAsync(
                    UserId,
                    AppId,
                    placementId,
                    offerId,
                    EventTypes.OfferPurchase,
                    new() {
                    { nameof(currencyCode), currencyCode },
                    { nameof(totalAmount), totalAmount },
                    },
                    customPayload);

            public static void LogOfferPurchaseEventWithProductId(string productId, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventWithProductId(
                    UserId,
                    AppId,
                    productId,
                    EventTypes.OfferPurchase,
                    new() {
                    { nameof(currencyCode), currencyCode },
                    { nameof(totalAmount), totalAmount },
                    },
                    customPayload);

            public static void LogOfferInteractionEvent(string placementId, string offerId, string interactionType, Dictionary<string, object> customPayload = null)
                => _ = Sdk._eventManager.QueueEventWithMeticaAttributesAsync(
                    UserId,
                    AppId,
                    placementId,
                    offerId,
                    EventTypes.OfferInteraction,
                    new() { { nameof(interactionType), interactionType } },
                    customPayload);

            public static void LogOfferInteractionEventWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventWithProductId(
                    UserId,
                    AppId,
                    productId,
                    EventTypes.OfferInteraction,
                    new() { { nameof(interactionType), interactionType } },
                    customPayload);

            public static void LogOfferImpressionEvent(string placementId, string offerId, Dictionary<string, object> customPayload = null)
                => _ = Sdk._eventManager.QueueEventWithMeticaAttributesAsync(
                    UserId,
                    AppId,
                    placementId,
                    offerId,
                    EventTypes.OfferImpression,
                    null,
                    customPayload);

            public static void LogOfferImpressionEventWithProductId(string productId, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventWithProductId(
                    UserId,
                    AppId,
                    productId,
                    EventTypes.OfferImpression,
                    null,
                    customPayload);

            public static void LogAdRevenueEvent(string placement, string type, string source, string currencyCode, double totalAmount, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventAsync(
                    UserId,
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

            public static void LogFullStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventAsync(
                    UserId,
                    AppId,
                    EventTypes.FullStateUpdate,
                    new() { { nameof(userStateAttributes), userStateAttributes }, },
                    customPayload);

            public static void LogPartialStateUserUpdateEvent(Dictionary<string, object> userStateAttributes, Dictionary<string, object> customPayload = null)
                => Sdk._eventManager.QueueEventAsync(
                    UserId,
                    AppId,
                    EventTypes.PartialStateUpdate,
                    new() { { nameof(userStateAttributes), userStateAttributes } },
                    customPayload);

            public static void LogCustomEvent(string customEventType, Dictionary<string, object> customPayload = null)
            {
                if (EventTypes.IsEventType(customEventType))
                {
                    Log.Error(() => $"{customEventType} cannot be used with {nameof(LogCustomEvent)}. Please use an event type that is not a core event. See documentation at https://docs.metica.com/integration#core-events.");
                    return;
                }
                Sdk._eventManager.QueueEventAsync(
                UserId,
                AppId,
                customEventType,
                null,
                customPayload);
            }
            public static void RequestDispatchEvents()
            {
                _ = Sdk._eventManager.RequestDispatchEvents();
            }
        }

        #endregion Events

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Ads

        // ADS bridge
        public static class Ads
        {
            public static void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
                => MeticaAds.CreateBanner(bannerAdUnitId, position);

            public static void ShowBanner(string adUnitId)
                => MeticaAds.ShowBanner(adUnitId);

            public static void HideBanner(string adUnitId)
                => MeticaAds.HideBanner(adUnitId);

            public static void DestroyBanner(string adUnitId)
                => MeticaAds.DestroyBanner(adUnitId);

            public static void LoadInterstitial(string interstitialAdUnitId)
                => MeticaAds.LoadInterstitial(interstitialAdUnitId);

            public static void ShowInterstitial(string interstitialAdUnitId)
                => MeticaAds.ShowInterstitial(interstitialAdUnitId);

            public static bool IsInterstitialReady(string interstitialAdUnitId)
                => MeticaAds.IsInterstitialReady(interstitialAdUnitId);

            public static void LoadRewarded(string rewardedAdUnitId)
                => MeticaAds.LoadRewarded(rewardedAdUnitId);

            public static void ShowRewarded(string rewardedAdUnitId)
                => MeticaAds.ShowRewarded(rewardedAdUnitId);

            public static bool IsRewardedReady(string rewardedAdUnitId)
                => MeticaAds.IsRewardedReady(rewardedAdUnitId);

            internal static void Dispose()
                // TODO
                => Log.Info(() => "Metica.Sdk.Ads.Dispose called but not yet implemented.");
        }

        #endregion Ads
    }
}
