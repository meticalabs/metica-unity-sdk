using Metica.Experimental.Core;
using Metica.Experimental.SDK;
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Experimental.Unity
{
    public class MeticaUnitySdk : MonoBehaviour
    {
        [SerializeField] private SdkConfigProvider _sdkConfigProvider;
        private MeticaSdk _meticaSdk;

        private void Awake()
        {
            // Register implementations before anything else. These are Unity implementations.
            Registry.Register<IDeviceInfoProvider>(new DeviceInfoProvider());
            Registry.Register<ILog>(new MeticaLogger(_sdkConfigProvider.SdkConfig.logLevel));

            // Initialize Metica SDK.
            _meticaSdk = new MeticaSdk(_sdkConfigProvider.SdkConfig);
        }

        private async void Start()
        {
            var offersResult = await _meticaSdk.GetOffersAsync(new string[] { "generic" });
            Log.Debug(() => $"Offers: {offersResult}");

            var configResult = await _meticaSdk.GetConfigsAsync(null);
            Log.Debug(() => $"Configs: {configResult}");

            configResult = await _meticaSdk.GetConfigsAsync(new List<string> { "dynamic_difficulty" });
            Log.Debug(() => $"Configs: {configResult}");

            _meticaSdk.LogInstallEvent();
            _meticaSdk.LogLoginEvent();

            _meticaSdk.LogOfferPurchaseEventWithProductId("mega_offer_123", "EUR", 1.99);
            _meticaSdk.LogOfferPurchaseEvent("generic", "23851", "EUR", 9.99);

            _meticaSdk.LogOfferPurchaseEvent("none", "0000", "EUR", 9.99); // expected failure
            _meticaSdk.LogOfferPurchaseEvent("none", "0000", "EUR", 9.99); //expected failure

            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "MadAds", "GBP", 1.50);
            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", "GBP", 1.10);
            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", "GBP", 1.10);

            _meticaSdk.LogOfferImpressionEventWithProductId("mega_offer_456", "click");
            _meticaSdk.LogOfferImpressionEventWithProductId("mega_offer_456", "dismiss");

            _meticaSdk.LogOfferInteractionEvent("generic", "23851", "click", new() { { "custom_stuff", "custom_content" } });
            _meticaSdk.LogOfferInteractionEvent("generic", "23851", "scroll", new() { { "custom_stuff", "custom_content" } });

            _meticaSdk.LogFullStateUserUpdateEvent(
                new()
                {
                    { "level", 91.0 },
                    { "faction", "Dragoons" },
                    { "gear_slot_LH", "Sword of Diverging Fates" },
                    { "gear_slot_RH", "Sword of the Thalos Masters" },
                    { "gear_slot_NK", "Second Soul Talisman" },
                    { "gear_slot_FT", "Dragonscale Boots of Zeal" },
                    { "gear_slot_BD", "Spiked Cape of Kuhaman" },
                    { "gear_slot_HD", "Shadow Mask of the Crow" },
                    { "gear_slot_LR", "Diamond Ring of Blood Pacts" },
                    { "gear_slot_RR", "The Sink Of Purity" },
                    { "gear_slot_BT", "Dragonscale Belt of Madness"},
                    { "last_map", "Thalosean Cemetery" },
                    { "totalDeaths", 920 },
                    { "totalWalkingDistance", 20038.82}
                });

            _meticaSdk.LogPartialStateUserUpdateEvent(
                new()
                {
                    { "level", 100.0 },
                    { "faction", "Dragoons" },
                    { "totalDeaths", 982 }
                });

            _meticaSdk.LogPartialStateUserUpdateEvent(
                new()
                {
                    { "level", 101.0 },
                    { "totalDeaths", 983 }
                });
        }

        private async void OnDestroy()
        {
            await _meticaSdk.DisposeAsync();
        }
    }

    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
    // Metica API for retro compatibility with previous SDK.
    // NOTE that this part will soon be removed or subject to cahnges.
    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-

    public static class MeticaAPI
    {
        /* 
        // Fields
        public static string SDKVersion
        public static string UserId
        public static string AppId
        public static string ApiKey
        public static bool Initialized
        public static SdkConfig Config

        //Methods
        public static void Initialise(string initialUserId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        public static void Initialise(SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback)
        public static void Initialise(string initialUserId, string appId, string apiKey, SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback)
        public static void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        public static void LogInstall(Dictionary<string, object> customPayload = null)
        public static void LogLogin(string newCurrentUserId = null, Dictionary<string, object> customPayload = null)
        public static void LogOfferDisplay(string offerId, string placementId, Dictionary<string, object> customPayload = null)
        public static void LogOfferDisplayWithProductId(string productId, Dictionary<string, object> customPayload = null)
        public static void LogOfferPurchase(string offerId, string placementId, double amount, string currency, Dictionary<string, object> customPayload = null)
        public static void LogOfferPurchaseWithProductId(string productId, double amount, string currency, Dictionary<string, object> customPayload = null)
        public static void LogOfferInteraction(string offerId, string placementId, string interactionType, Dictionary<string, object> customPayload = null)
        public static void LogOfferInteractionWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
        public static void LogFullStateUpdate(Dictionary<string, object> fullUserAttributes)
        public static void LogPartialStateUpdate(Dictionary<string, object> partialUserAttributes)
        public static void LogAdRevenue(double totalAmount, string currencyCode, string adPlacement = null, string adPlacementType = null, string adPlacementSource = null, Dictionary<string, object> customPayload = null)
        public static void LogCustomEvent(string eventType, Dictionary<string, object> userEvent)
        public static void LogUserEvent(string eventType, Dictionary<string, object> userEvent, bool reuseDictionary)
        public static void LogUserAttributes(Dictionary<string, object> userAttributes)
        */
    }
}
