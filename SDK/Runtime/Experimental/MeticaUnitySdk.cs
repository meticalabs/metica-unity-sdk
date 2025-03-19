using Metica.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.Experimental.Unity
{
    public class MeticaUnitySdk : MonoBehaviour
    {
        [SerializeField] private SdkConfigProvider _sdkConfigProvider;
        private MeticaSdk _meticaSdk;

        private void Awake()
        {
            _meticaSdk = new MeticaSdk(_sdkConfigProvider.SdkConfig);
        }

        private async void Start()
        {
            var offersResult = await _meticaSdk.GetOffersAsync(new string[] { "generic" });
            Debug.Log($"Offers: {offersResult}");

            var configResult = await _meticaSdk.GetConfigsAsync(null);
            Debug.Log($"Configs: {configResult}");

            configResult = await _meticaSdk.GetConfigsAsync(new List<string> { "dynamic_difficulty" });
            Debug.Log($"Configs: {configResult}");

            _meticaSdk.LogInstallEvent();
            _meticaSdk.LogLoginEvent();

            _meticaSdk.LogOfferPurchaseEventWithProductId("mega_offer_123", "EUR", 1.99);
            _meticaSdk.LogOfferPurchaseEvent("generic", "23851", "EUR", 9.99);

            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "MadAds", 1.50);
            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", 1.10);
            _meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", 1.10);

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
}
