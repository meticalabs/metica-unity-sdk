#nullable enable

using System.Threading.Tasks;
using Metica.ADS.UnityPlayer;
using Metica.SDK;

// ReSharper disable once CheckNamespace
namespace Metica.ADS 
{
    public static class MeticaAds
    {   
        public const string TAG = "MeticaUnityPlugin";
        private static readonly PlatformDelegate PlatformDelegate;

        static MeticaAds()
        {

#if UNITY_EDITOR
            // Check for Unity Editor first since the editor also responds to the currently selected platform.
            PlatformDelegate = new UnityPlayerDelegate();
#elif UNITY_ANDROID
            platformDelegate = new Android.AndroidDelegate(AndroidUnityBridge.UnityBridgeClass, AndroidUnityBridge.MeticaAdsExternalTrackerClass);
#elif UNITY_IPHONE || UNITY_IOS
            platformDelegate = new IOS.IOSDelegate();
#else
            platformDelegate = new UnityPlayer.UnityPlayerDelegate();
#endif
            
            // Interstitial ad callbacks
            PlatformDelegate.InterstitialAdLoadSuccess += MeticaAdsCallbacks.Interstitial.OnAdLoadSuccessInternal;
            PlatformDelegate.InterstitialAdLoadFailed += MeticaAdsCallbacks.Interstitial.OnAdLoadFailedInternal;
            PlatformDelegate.InterstitialAdShowSuccess += MeticaAdsCallbacks.Interstitial.OnAdShowSuccessInternal;
            PlatformDelegate.InterstitialAdShowFailed += MeticaAdsCallbacks.Interstitial.OnAdShowFailedInternal;
            PlatformDelegate.InterstitialAdHidden += MeticaAdsCallbacks.Interstitial.OnAdHiddenInternal;
            PlatformDelegate.InterstitialAdClicked += MeticaAdsCallbacks.Interstitial.OnAdClickedInternal;
            PlatformDelegate.InterstitialAdRevenuePaid += MeticaAdsCallbacks.Interstitial.OnAdRevenuePaidInternal;
            
            // Rewarded ad callbacks
            PlatformDelegate.RewardedAdLoadSuccess += MeticaAdsCallbacks.Rewarded.OnAdLoadSuccessInternal;
            PlatformDelegate.RewardedAdLoadFailed += MeticaAdsCallbacks.Rewarded.OnAdLoadFailedInternal;
            PlatformDelegate.RewardedAdShowSuccess += MeticaAdsCallbacks.Rewarded.OnAdShowSuccessInternal;
            PlatformDelegate.RewardedAdShowFailed += MeticaAdsCallbacks.Rewarded.OnAdShowFailedInternal;
            PlatformDelegate.RewardedAdHidden += MeticaAdsCallbacks.Rewarded.OnAdHiddenInternal;
            PlatformDelegate.RewardedAdClicked += MeticaAdsCallbacks.Rewarded.OnAdClickedInternal;
            PlatformDelegate.RewardedAdRewarded += MeticaAdsCallbacks.Rewarded.OnAdRewardedInternal;
            PlatformDelegate.RewardedAdRevenuePaid += MeticaAdsCallbacks.Rewarded.OnAdRevenuePaidInternal;
        }

        public static async Task<bool> InitializeAsync(MeticaConfiguration configuration)
        {
            return await PlatformDelegate.InitializeAsync(MeticaSdk.ApiKey, MeticaSdk.AppId, MeticaSdk.CurrentUserId, MeticaSdk.Version, MeticaSdk.BaseEndpoint, configuration);
        }
        public static void SetLogEnabled(bool logEnabled) 
        {
            PlatformDelegate.SetLogEnabled(logEnabled);
        }
        
        // Interstitial ad methods
        public static void LoadInterstitial()
        {
            PlatformDelegate.LoadInterstitial();
        }
        public static void ShowInterstitial()
        {
            PlatformDelegate.ShowInterstitial();
        }
        public static bool IsInterstitialReady()
        {
            return PlatformDelegate.IsInterstitialReady();
        }
        
        // Rewarded ad methods
        public static void LoadRewarded()
        {
            PlatformDelegate.LoadRewarded();
        }
        public static void ShowRewarded()
        {
            PlatformDelegate.ShowRewarded();
        }
        public static bool IsRewardedReady()
        {
            return PlatformDelegate.IsRewardedReady();
        }

        public static void NotifyAdLoadAttempt(string interstitialAdUnitId)
        {
            PlatformDelegate.NotifyAdLoadAttempt(interstitialAdUnitId);
        }

        public static void NotifyAdLoadSuccess(MeticaAd meticaAd)
        {
            PlatformDelegate.NotifyAdLoadSuccess(meticaAd);
        }

        public static void NotifyAdLoadFailed(string adUnitId, string error)
        {
            PlatformDelegate.NotifyAdLoadFailed(adUnitId, error);
        }

        public static void NotifyAdShowSuccess(MeticaAd meticaAd)
        {
            // Internally we use the NotifyAdRevenue call, but externally as to not
            // break API we use NotifyAdShowSuccess. Which is similar as both will tel you ad was shown.
            PlatformDelegate.NotifyAdRevenue(meticaAd);
        }
    }
}
