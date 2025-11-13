using System;
using UnityEngine;

namespace Metica.Ads
{
public static class MeticaAdsCallbacks
{
    public static class Banner
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<MeticaAdError> OnAdLoadFailed;
        public static event Action<MeticaAd> OnAdClicked;
        public static event Action<MeticaAd> OnAdRevenuePaid;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(MeticaAdError meticaAdError)
        {
            OnAdLoadFailed?.Invoke(meticaAdError);
        }

        internal static void OnAdClickedInternal(MeticaAd meticaAd)
        {
            OnAdClicked?.Invoke(meticaAd);
        }

        internal static void OnAdRevenuePaidInternal(MeticaAd meticaAd)
        {
            OnAdRevenuePaid?.Invoke(meticaAd);
        }

        // Add a method to clear all events
        internal static void ResetEvents()
        {
            OnAdLoadSuccess = null;
            OnAdLoadFailed = null;
            OnAdClicked = null;
            OnAdRevenuePaid = null;
        }
    }
    
    public static class Interstitial
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<MeticaAdError> OnAdLoadFailed;
        public static event Action<MeticaAd> OnAdShowSuccess;
        public static event Action<MeticaAd, MeticaAdError> OnAdShowFailed;
        public static event Action<MeticaAd> OnAdHidden;
        public static event Action<MeticaAd> OnAdClicked;
        public static event Action<MeticaAd> OnAdRevenuePaid;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(MeticaAdError meticaAdError)
        {
            OnAdLoadFailed?.Invoke(meticaAdError);
        }      
        internal static void OnAdShowSuccessInternal(MeticaAd meticaAd)
        {
            OnAdShowSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdShowFailedInternal(MeticaAd meticaAd, MeticaAdError meticaAdError)
        {
            OnAdShowFailed?.Invoke(meticaAd, meticaAdError);
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

        // Add a method to clear all events
        internal static void ResetEvents()
        {
            OnAdLoadSuccess = null;
            OnAdLoadFailed = null;
            OnAdShowSuccess = null;
            OnAdShowFailed = null;
            OnAdHidden = null;
            OnAdClicked = null;
            OnAdRevenuePaid = null;
        }
    }

    public static class Rewarded
    {
        // Public events for consumers to subscribe to
        public static event Action<MeticaAd> OnAdLoadSuccess;
        public static event Action<MeticaAdError> OnAdLoadFailed;
        public static event Action<MeticaAd> OnAdShowSuccess;
        public static event Action<MeticaAd, MeticaAdError> OnAdShowFailed;
        public static event Action<MeticaAd> OnAdHidden;
        public static event Action<MeticaAd> OnAdClicked;
        public static event Action<MeticaAd> OnAdRewarded;
        public static event Action<MeticaAd> OnAdRevenuePaid;

        // Internal methods to trigger the events
        internal static void OnAdLoadSuccessInternal(MeticaAd meticaAd)
        {
            OnAdLoadSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdLoadFailedInternal(MeticaAdError meticaAdError)
        {
            OnAdLoadFailed?.Invoke(meticaAdError);
        }      
        
        internal static void OnAdShowSuccessInternal(MeticaAd meticaAd)
        {
            OnAdShowSuccess?.Invoke(meticaAd);
        }

        internal static void OnAdShowFailedInternal(MeticaAd meticaAd, MeticaAdError meticaAdError)
        {
            OnAdShowFailed?.Invoke(meticaAd, meticaAdError);
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

        // Add a method to clear all events
        internal static void ResetEvents()
        {
            OnAdLoadSuccess = null;
            OnAdLoadFailed = null;
            OnAdShowSuccess = null;
            OnAdShowFailed = null;
            OnAdHidden = null;
            OnAdClicked = null;
            OnAdRewarded = null;
            OnAdRevenuePaid = null;
        }
    }

    // Reset all static events to null at the start of each Play Mode session
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents()
    {
        Banner.ResetEvents();
        Interstitial.ResetEvents();
        Rewarded.ResetEvents();
    }
}
}
