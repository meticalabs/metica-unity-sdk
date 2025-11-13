using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using Metica;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    public class IOSInitializeCallback
    {
        private const string TAG = MeticaAds.TAG;
        private static TaskCompletionSource<MeticaInitResponse> _currentTcs;

        public IOSInitializeCallback(TaskCompletionSource<MeticaInitResponse> tcs)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
            _currentTcs = tcs;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnInitializeSuccessDelegate(string meticaAdsInitializationResponseJson);

        [DllImport("__Internal")]
        private static extern void ios_sdkInitialize(string apiKey, string appId, string userId, string mediationInfoKey, OnInitializeSuccessDelegate result);

        public void InitializeSDK(string apiKey, string appId, string userId, string mediationInfoKey)
        {
            ios_sdkInitialize(
                apiKey,
                appId,
                userId,
                mediationInfoKey,
                OnInitialized
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnInitializeSuccessDelegate))]
        private static void OnInitialized(string meticaAdsInitializationResponseJson)
        {
            var meticaInitResponse = MeticaInitResponse.FromJson(meticaAdsInitializationResponseJson);
            var tcs = _currentTcs;
            if (tcs != null)
            {
                MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback onInit");
                var smartFloors = meticaInitResponse.SmartFloors;
                MeticaAds.Log.LogDebug(() => $"{TAG} IOSInitializeCallback smartFloorsObj = {smartFloors}");
                tcs.SetResult(meticaInitResponse);
                _currentTcs = null;
            }
        }
    }
}