using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    public class IOSLoadCallback
    {
        private const string TAG = MeticaAds.TAG;
        private static IOSLoadCallback _currentInstance;
        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<MeticaAdError> AdLoadFailed;

        public IOSLoadCallback()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} LoadCallbackProxy created");
            _currentInstance = this;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadSuccessDelegate(string meticaAdJson);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadFailedDelegate(string errorJson);

        [DllImport("__Internal")]
        private static extern void ios_loadInterstitial(string adUnitId, OnAdLoadSuccessDelegate onSuccess, OnAdLoadFailedDelegate onFailure);
        [DllImport("__Internal")]
        private static extern void ios_loadRewarded(string adUnitId, OnAdLoadSuccessDelegate onSuccess, OnAdLoadFailedDelegate onFail);

        public void LoadInterstitial(string adUnitId)
        {
            ios_loadInterstitial(
                adUnitId,
                OnAdLoadSuccess,
                OnAdLoadFailed
            );
        }

        public void LoadRewarded(string adUnitId)
        {
            ios_loadRewarded(
                adUnitId,
                OnAdLoadSuccess,
                OnAdLoadFailed
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdLoadSuccessDelegate))]
        private static void OnAdLoadSuccess(string meticaAdJson)
        {
            var meticaAd = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
            _currentInstance?.AdLoadSuccess?.Invoke(meticaAd);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdLoadFailedDelegate))]
        private static void OnAdLoadFailed(string errorJson)
        {
            var meticaAdError = MeticaAdError.FromJson(errorJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={meticaAdError.message}");
            _currentInstance?.AdLoadFailed?.Invoke(meticaAdError);
        }
    }
}
