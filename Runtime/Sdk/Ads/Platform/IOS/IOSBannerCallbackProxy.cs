using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    public class IOSBannerCallbackProxy
    {
        private const string TAG = MeticaAds.TAG;
        private static IOSBannerCallbackProxy _currentInstance;

        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<MeticaAdError> AdLoadFailed;
        public event Action<MeticaAd> AdClicked;
        public event Action<MeticaAd> AdRevenuePaid;

        public IOSBannerCallbackProxy()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} BannerCallbackProxy created");
            _currentInstance = this;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadSuccessDelegate(
            string adUnitId, 
            double revenue,
            string networkName, 
            string placementTag, 
            string adFormat, 
            string creativeId, 
            long latency
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAdLoadFailedDelegate(
            string error, 
            string adUnitId
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
        private static extern void ios_createBanner(
            string adUnitId, 
            int position, 
            OnAdLoadSuccessDelegate onSuccess, 
            OnAdLoadFailedDelegate onFailure, 
            OnAdClickedDelegate onClicked, 
            OnAdRevenuePaidDelegate onRevenuePaid
        );

        public void CreateBanner(string adUnitId, int position)
        {
            ios_createBanner(
                adUnitId,
                position,
                OnAdLoadSuccess,
                OnAdLoadFailed,
                OnAdClicked,
                OnAdRevenuePaid
            );
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdLoadSuccessDelegate))]
        private static void OnAdLoadSuccess(string adUnitId, double revenue, string networkName, string placementTag, string adFormat, string creativeId, long latency)
        {
            var meticaAd = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
            _currentInstance?.AdLoadSuccess?.Invoke(meticaAd);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdLoadFailedDelegate))]
        private static void OnAdLoadFailed(string error, string adUnitId)
        {
            var meticaAdError = new MeticaAdError(error, adUnitId);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={meticaAdError.message}");
            _currentInstance?.AdLoadFailed?.Invoke(meticaAdError);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdClickedDelegate))]
        private static void OnAdClicked(string adUnitId, double revenue, string networkName, string placementTag, string adFormat, string creativeId, long latency)
        {
            var meticaAd = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdClicked callback received for adUnitId={meticaAd.adUnitId}");
            _currentInstance?.AdClicked?.Invoke(meticaAd);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAdRevenuePaidDelegate))]
        private static void OnAdRevenuePaid(string adUnitId, double revenue, string networkName, string placementTag, string adFormat, string creativeId, long latency)
        {
            var meticaAd = new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRevenuePaid callback received for adUnitId={meticaAd.adUnitId}");
            _currentInstance?.AdRevenuePaid?.Invoke(meticaAd);
        }
    }
}
