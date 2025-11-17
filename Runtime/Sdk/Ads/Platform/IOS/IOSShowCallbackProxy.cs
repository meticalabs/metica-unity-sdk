using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    internal class IOSShowCallbackProxy
    {
        private const string TAG = MeticaAds.TAG;
        private static IOSShowCallbackProxy _currentInstance;
        
        public event Action<MeticaAd> AdShowSuccess;
        public event Action<MeticaAd, MeticaAdError> AdShowFailed;
        public event Action<MeticaAd> AdHidden;
        public event Action<MeticaAd> AdClicked;
        public event Action<MeticaAd> AdRewarded;
        public event Action<MeticaAd> AdRevenuePaid;

        public IOSShowCallbackProxy() 
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} ShowCallbackProxy created");
            _currentInstance = this;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdShowSuccessDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdShowFailedDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency, 
            string meticaError,
            string errorAdUnitId
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdHiddenDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdClickedDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdRewardedDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdRevenuePaidDelegate(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [DllImport("__Internal")]
        private static extern void ios_showInterstitial(
            string interstitialAdUnitId,
            OnAdShowSuccessDelegate onShowSuccess,
            OnAdShowFailedDelegate onShowFailed,
            OnAdHiddenDelegate onAdHidden,
            OnAdClickedDelegate onAdClicked,
            OnAdRevenuePaidDelegate onAdRevenuePaid
        );

        [DllImport("__Internal")]
        private static extern void ios_showRewarded(
            string rewardedAdUnitId,
            OnAdShowSuccessDelegate onShowSuccess,
            OnAdShowFailedDelegate onShowFailed,
            OnAdHiddenDelegate onAdHidden,
            OnAdClickedDelegate onAdClicked,
            OnAdRewardedDelegate onAdRewarded,
            OnAdRevenuePaidDelegate onAdRevenuePaid
        );

        public void ShowInterstitial(string interstitialAdUnitId)
        {
            ios_showInterstitial(
                interstitialAdUnitId,
                OnAdShowSuccess,
                OnAdShowFailed,
                OnAdHidden,
                OnAdClicked,
                OnAdRevenuePaid
            );
        }

        public void ShowRewarded(string rewardedAdUnitId)
        {
            ios_showRewarded(
                rewardedAdUnitId,
                OnAdShowSuccess,
                OnAdShowFailed,
                OnAdHidden,
                OnAdClicked,
                OnAdRewarded,
                OnAdRevenuePaid
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdShowSuccessDelegate))]
        private static void OnAdShowSuccess(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowSuccess for adUnitId={ad.adUnitId}");
            _currentInstance?.AdShowSuccess?.Invoke(ad);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdShowFailedDelegate))]
        private static void OnAdShowFailed(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency, 
            string meticaError,
            string errorAdUnitId
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            var meticaAdError = new MeticaAdError(meticaError, errorAdUnitId);

            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowFailed for adUnitId={ad.adUnitId}, error={meticaAdError.message}");
            _currentInstance?.AdShowFailed?.Invoke(ad, meticaAdError);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdHiddenDelegate))]
        private static void OnAdHidden(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdHidden for adUnitId={ad.adUnitId}");
            _currentInstance?.AdHidden?.Invoke(ad);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdClickedDelegate))]
        private static void OnAdClicked(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdClicked for adUnitId={ad.adUnitId}");
            _currentInstance?.AdClicked?.Invoke(ad);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdRewardedDelegate))]
        private static void OnAdRewarded(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRewarded for adUnitId={ad.adUnitId}");
            _currentInstance?.AdRewarded?.Invoke(ad);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdRevenuePaidDelegate))]
        private static void OnAdRevenuePaid(
            string adUnitId,
            double revenue, 
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        ) {
            var ad = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRevenuePaid for adUnitId={ad.adUnitId}");
            _currentInstance?.AdRevenuePaid?.Invoke(ad);
        }
    }
}
