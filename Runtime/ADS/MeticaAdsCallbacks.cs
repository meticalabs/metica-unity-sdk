using System;

namespace Metica.ADS
{
public static class MeticaAdsCallbacks
{
    public static class Interstitial
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<string> OnAdLoadFailed;
        public static event Action<string> OnAdShowSuccess;
        public static event Action<string, string> OnAdShowFailed;
        public static event Action<string> OnAdHidden;
        public static event Action<string> OnAdClicked;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(string error)
        {
            OnAdLoadFailed?.Invoke(error);
        }      
        internal static void OnAdShowSuccessInternal(string adUnitId)
        {
            OnAdShowSuccess?.Invoke(adUnitId);
        }

        internal static void OnAdShowFailedInternal(string adUnitId, string error)
        {
            OnAdShowFailed?.Invoke(adUnitId, error);
        }

        internal static void OnAdHiddenInternal(string adUnitId)
        {
            OnAdHidden?.Invoke(adUnitId);
        }

        internal static void OnAdClickedInternal(string adUnitId)
        {
            OnAdClicked?.Invoke(adUnitId);
        }
    }

    public static class Rewarded
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<string> OnAdLoadFailed;
        public static event Action<string> OnAdShowSuccess;
        public static event Action<string, string> OnAdShowFailed;
        public static event Action<string> OnAdHidden;
        public static event Action<string> OnAdClicked;
        public static event Action<string> OnAdRewarded;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(string error)
        {
            OnAdLoadFailed?.Invoke(error);
        }      
        
        internal static void OnAdShowSuccessInternal(string adUnitId)
        {
            OnAdShowSuccess?.Invoke(adUnitId);
        }

        internal static void OnAdShowFailedInternal(string adUnitId, string error)
        {
            OnAdShowFailed?.Invoke(adUnitId, error);
        }

        internal static void OnAdHiddenInternal(string adUnitId)
        {
            OnAdHidden?.Invoke(adUnitId);
        }

        internal static void OnAdClickedInternal(string adUnitId)
        {
            OnAdClicked?.Invoke(adUnitId);
        }
        
        internal static void OnAdRewardedInternal(string adUnitId)
        {
            OnAdRewarded?.Invoke(adUnitId);
        }
    }

    public static class Banner
    {
        // TODO
    }
}
}
