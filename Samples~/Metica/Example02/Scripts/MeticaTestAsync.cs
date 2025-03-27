using Metica.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeticaTestAsync : MonoBehaviour
{
    private async void Start()
    {
        await (Example(MeticaSdk.SDK));
    }

    private static async Task Example(IMeticaSdk meticaSdk)
    {
        var offersResult = await meticaSdk.GetOffersAsync(new string[] { "generic" });
        Log.Debug(() => $"Offers: {offersResult}");

        var configResult = await meticaSdk.GetConfigsAsync(null);
        Log.Debug(() => $"Configs: {configResult}");

        configResult = await meticaSdk.GetConfigsAsync(new List<string> { "dynamic_difficulty" });
        Log.Debug(() => $"Configs: {configResult}");

        meticaSdk.LogInstallEvent();
        meticaSdk.LogLoginEvent();

        meticaSdk.LogOfferPurchaseEventWithProductId("mega_offer_123", "EUR", 1.99);
        meticaSdk.LogOfferPurchaseEvent("generic", "23851", "EUR", 9.99);

        meticaSdk.LogOfferPurchaseEvent("none", "0000", "EUR", 9.99); // expected failure
        meticaSdk.LogOfferPurchaseEvent("none", "0000", "EUR", 9.99); //expected failure

        meticaSdk.LogAdRevenueEvent("top_slot", "popup", "MadAds", "GBP", 1.50);
        meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", "GBP", 1.10);
        meticaSdk.LogAdRevenueEvent("top_slot", "popup", "AdLads", "GBP", 1.10);

        meticaSdk.LogOfferImpressionEventWithProductId("mega_offer_456");
        meticaSdk.LogOfferImpressionEventWithProductId("mega_offer_458");

        meticaSdk.LogOfferInteractionEvent("generic", "23851", "click", new() { { "custom_stuff", "custom_content" } });
        meticaSdk.LogOfferInteractionEvent("generic", "23851", "scroll", new() { { "custom_stuff", "custom_content" } });

        meticaSdk.LogFullStateUserUpdateEvent(
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

        meticaSdk.LogPartialStateUserUpdateEvent(
            new()
            {
                { "level", 100.0 },
                { "faction", "Dragoons" },
                { "totalDeaths", 982 }
            });

        meticaSdk.LogPartialStateUserUpdateEvent(
            new()
            {
                { "level", 101.0 },
                { "totalDeaths", 983 }
            });
    }

}
