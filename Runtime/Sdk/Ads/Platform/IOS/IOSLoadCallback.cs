using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Metica.ADS.IOS
{
    public class IOSLoadCallback
    {
        private const string TAG = MeticaAds.TAG;
        private static IOSLoadCallback _currentInstance;
        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<string> AdLoadFailed;

        public IOSLoadCallback()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} LoadCallbackProxy created");
            _currentInstance = this;
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
                OnAdLoadSuccess,
                OnAdLoadFailed
            );
        }

        public void LoadRewarded()
        {
            ios_loadRewarded(
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
        private static void OnAdLoadFailed(string error)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={error}");
            _currentInstance?.AdLoadFailed?.Invoke(error);
        }
    }
}
