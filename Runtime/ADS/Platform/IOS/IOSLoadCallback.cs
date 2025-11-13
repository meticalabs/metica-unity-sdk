using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Metica.ADS.IOS
{
    public class IOSLoadCallback
    {
        private const string TAG = MeticaAds.TAG;
        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<string> AdLoadFailed;

        public IOSLoadCallback()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} LoadCallbackProxy created");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadSuccessDelegate(string meticaAdJson);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadFailedDelegate(string error);

        [DllImport("__Internal")]
        private static extern void ios_loadInterstitial(OnAdLoadSuccessDelegate onSuccess, OnAdLoadFailedDelegate onFailure);
        [DllImport("__Internal")]
        private static extern void ios_loadRewarded(OnAdLoadSuccessDelegate onSuccess, OnAdLoadFailedDelegate onFail);

        public void LoadInterstitial()
        {
            ios_loadInterstitial(
                onAdLoadSuccess,
                onAdLoadFailed
            );
        }

        public void LoadRewarded()
        {
            ios_loadRewarded(
                onAdLoadSuccess,
                onAdLoadFailed
            );
        }

        public void onAdLoadSuccess(string meticaAdJson)
        {
            var meticaAd = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
            AdLoadSuccess?.Invoke(meticaAd);
        }

        public void onAdLoadFailed(string error)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={error}");
            AdLoadFailed?.Invoke(error);
        }
    }
}
