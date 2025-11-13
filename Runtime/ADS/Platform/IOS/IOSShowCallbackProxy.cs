#nullable enable

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Metica.ADS.IOS
{
    internal class IOSShowCallbackProxy
    {
        private const string TAG = MeticaAds.TAG;
        
        public event Action<MeticaAd> AdShowSuccess;
        public event Action<MeticaAd, string> AdShowFailed;
        public event Action<MeticaAd> AdHidden;
        public event Action<MeticaAd> AdClicked;
        public event Action<MeticaAd> AdRewarded;
        public event Action<MeticaAd> AdRevenuePaid;

        public IOSShowCallbackProxy() 
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} ShowCallbackProxy created");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdShowSuccessDelegate(string meticaAdJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdShowFailedDelegate(string meticaAdJson, string error);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdHiddenDelegate(string meticaAdJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdClickedDelegate(string meticaAdJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdRewardedDelegate(string meticaAdJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdRevenuePaidDelegate(string meticaAdJson);

        [DllImport("__Internal")]
        private static extern void ios_showInterstitial(
            OnAdShowSuccessDelegate onShowSuccess,
            OnAdShowFailedDelegate onShowFailed,
            OnAdHiddenDelegate onAdHidden,
            OnAdClickedDelegate onAdClicked,
            OnAdRevenuePaidDelegate onAdRevenuePaid
        );

        [DllImport("__Internal")]
        private static extern void ios_showRewarded(
            OnAdShowSuccessDelegate onShowSuccess,
            OnAdShowFailedDelegate onShowFailed,
            OnAdHiddenDelegate onAdHidden,
            OnAdClickedDelegate onAdClicked,
            OnAdRewardedDelegate onAdRewarded,
            OnAdRevenuePaidDelegate onAdRevenuePaid
        );

        public void ShowInterstitial()
        {
            ios_showInterstitial(
                OnAdShowSuccess,
                OnAdShowFailed,
                OnAdHidden,
                OnAdClicked,
                OnAdRevenuePaid
            );
        }

        public void ShowRewarded()
        {
            ios_showRewarded(
                OnAdShowSuccess,
                OnAdShowFailed,
                OnAdHidden,
                OnAdClicked,
                OnAdRewarded,
                OnAdRevenuePaid
            );
        }

        private void OnAdShowSuccess(string meticaAdJson)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowSuccess for adUnitId={ad.adUnitId}");
            AdShowSuccess?.Invoke(ad);
        }

        private void OnAdShowFailed(string meticaAdJson, string error)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowFailed for adUnitId={ad.adUnitId}, error={error}");
            AdShowFailed?.Invoke(ad, error);
        }

        private void OnAdHidden(string meticaAdJson)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdHidden for adUnitId={ad.adUnitId}");
            AdHidden?.Invoke(ad);
        }

        private void OnAdClicked(string meticaAdJson)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdClicked for adUnitId={ad.adUnitId}");
            AdClicked?.Invoke(ad);
        }

        private void OnAdRewarded(string meticaAdJson)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRewarded for adUnitId={ad.adUnitId}");
            AdRewarded?.Invoke(ad);
        }

        private void OnAdRevenuePaid(string meticaAdJson)
        {
            var ad = MeticaAd.FromJson(meticaAdJson);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRevenuePaid for adUnitId={ad.adUnitId}");
            AdRevenuePaid?.Invoke(ad);
        }
    }
}
