using Metica.Unity;
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
            _meticaSdk = new MeticaSdk(_sdkConfigProvider.SdkConfig);
        }

        private async void Start()
        {
            //var offersResult = await _meticaSdk.GetOffersAsync(null);
            //Debug.Log($"Offers: {offersResult}");

            var offersResult = await _meticaSdk.GetOffersAsync(new string[] { "generic" });
            Debug.Log($"Offers: {offersResult}");

            var configResult = await _meticaSdk.GetConfigsAsync(null);
            Debug.Log($"Configs: {configResult}");

            configResult = await _meticaSdk.GetConfigsAsync(new List<string> { "dynamic_difficulty" });
            Debug.Log($"Configs: {configResult}");

            _meticaSdk.LogPurchaseEventWithProductId("mega_offer_123", "EUR", 1.99);
            _meticaSdk.LogOfferPurchaseEvent("generic", "23851", "EUR", 9.99);

            _meticaSdk.LogPartialStateUserUpdateEvent(
                new()
                {
                    { "level", 100.0 },
                    { "faction", "Dragoons" },
                });
        }
    }
}
