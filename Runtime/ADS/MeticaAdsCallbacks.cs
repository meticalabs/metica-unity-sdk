using System;

namespace Metica.ADS
{
public static class MeticaAdsCallbacks
{
    public static class Interstitial
    {
        // Public events for consumers to subscribe to
        public static event Action<string> OnAdLoadSuccess;
        public static event Action<string, string> OnAdLoadFailed;
        public static event Action<string> OnAdShowSuccess;
        public static event Action<string, string> OnAdShowFailed;
        public static event Action<string> OnAdHidden;
        public static event Action<string> OnAdClicked;

        // Internal methods to trigger the events - these can only be called from within the assembly
        // TODO: why are these available to consumer even though marked as internal?
        internal static void OnAdLoadSuccessInternal(string adUnitId)
        {
            OnAdLoadSuccess?.Invoke(adUnitId);
        }

        internal static void OnAdLoadFailedInternal(string adUnitId, string error)
        {
            OnAdLoadFailed?.Invoke(adUnitId, error);
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
        public static event Action<string> OnAdLoadSuccess;
        public static event Action<string, string> OnAdLoadFailed;
        public static event Action<string> OnAdShowSuccess;
        public static event Action<string, string> OnAdShowFailed;
        public static event Action<string> OnAdHidden;
        public static event Action<string> OnAdClicked;
        public static event Action<string> OnAdRewarded; // Specific to rewarded ads - user received reward

        // Internal methods to trigger the events - these can only be called from within the assembly
        internal static void OnAdLoadSuccessInternal(string adUnitId)
        {
            OnAdLoadSuccess?.Invoke(adUnitId);
        }

        internal static void OnAdLoadFailedInternal(string adUnitId, string error)
        {
            OnAdLoadFailed?.Invoke(adUnitId, error);
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