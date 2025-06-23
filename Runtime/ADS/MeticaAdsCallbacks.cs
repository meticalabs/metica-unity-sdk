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
        public static event Action<MeticaAd> OnAdShowSuccess;
        public static event Action<MeticaAd, string> OnAdShowFailed;
        public static event Action<MeticaAd> OnAdHidden;
        public static event Action<MeticaAd> OnAdClicked;
        public static event Action<MeticaAd> OnAdRevenuePaid;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(string error)
        {
            OnAdLoadFailed?.Invoke(error);
        }      
        internal static void OnAdShowSuccessInternal(MeticaAd meticaAd)
        {
            OnAdShowSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdShowFailedInternal(MeticaAd meticaAd, string error)
        {
            OnAdShowFailed?.Invoke(meticaAd, error);
        }

        internal static void OnAdHiddenInternal(MeticaAd meticaAd)
        {
            OnAdHidden?.Invoke(meticaAd);
        }

        internal static void OnAdClickedInternal(MeticaAd meticaAd)
        {
            OnAdClicked?.Invoke(meticaAd);
        }

        internal static void OnAdRevenuePaidInternal(MeticaAd meticaAd)
        {
            OnAdRevenuePaid?.Invoke(meticaAd);
        }
    }

    public static class Rewarded
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<string> OnAdLoadFailed;
        public static event Action<MeticaAd> OnAdShowSuccess;
        public static event Action<MeticaAd, string> OnAdShowFailed;
        public static event Action<MeticaAd> OnAdHidden;
        public static event Action<MeticaAd> OnAdClicked;
        public static event Action<MeticaAd> OnAdRewarded;
        public static event Action<MeticaAd> OnAdRevenuePaid;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(string error)
        {
            OnAdLoadFailed?.Invoke(error);
        }      
        
        internal static void OnAdShowSuccessInternal(MeticaAd meticaAd)
        {
            OnAdShowSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdShowFailedInternal(MeticaAd meticaAd, string error)
        {
            OnAdShowFailed?.Invoke(meticaAd, error);
        }

        internal static void OnAdHiddenInternal(MeticaAd meticaAd)
        {
            OnAdHidden?.Invoke(meticaAd);
        }

        internal static void OnAdClickedInternal(MeticaAd meticaAd)
        {
            OnAdClicked?.Invoke(meticaAd);
        }
        
        internal static void OnAdRewardedInternal(MeticaAd meticaAd)
        {
            OnAdRewarded?.Invoke(meticaAd);
        }

        internal static void OnAdRevenuePaidInternal(MeticaAd meticaAd)
        {
            OnAdRevenuePaid?.Invoke(meticaAd);
        }
    }

    public static class Banner
    {
        // TODO
    }

    // Reset all static events to null at the start of each Play Mode session
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents()
    {
        Interstitial.OnAdLoadSuccess = null;
        Interstitial.OnAdLoadFailed = null;
        Interstitial.OnAdShowSuccess = null;
        Interstitial.OnAdShowFailed = null;
        Interstitial.OnAdHidden = null;
        Interstitial.OnAdClicked = null;
        Interstitial.OnAdRevenuePaid = null;

        Rewarded.OnAdLoadSuccess = null;
        Rewarded.OnAdLoadFailed = null;
        Rewarded.OnAdShowSuccess = null;
        Rewarded.OnAdShowFailed = null;
        Rewarded.OnAdHidden = null;
        Rewarded.OnAdClicked = null;
        Rewarded.OnAdRewarded = null;
        Rewarded.OnAdRevenuePaid = null;
    }
}
}
