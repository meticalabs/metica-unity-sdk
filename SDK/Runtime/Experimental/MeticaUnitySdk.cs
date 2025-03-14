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
        }
    }
}
