#nullable enable

using System.Threading.Tasks;
using Metica.SDK;

namespace Metica.ADS 
{
    public static class MeticaAds
    {   
        public const string TAG = "MeticaUnityPlugin";
        private static readonly PlatformDelegate platformDelegate;

        static MeticaAds()
        {
#if UNITY_EDITOR
            platformDelegate = new UnityPlayer.UnityPlayerDelegate();
#elif UNITY_ANDROID
            platformDelegate = new Android.AndroidDelegate();
#elif UNITY_IOS
            platformDelegate = new IOS.IOSDelegate();
#else
            throw new PlatformNotSupportedException("MeticaAds is only supported on Android, iOS, and Unity Editor platforms.");
#endif
            // Interstitial ad callbacks
            platformDelegate.InterstitialAdLoadSuccess += MeticaAdsCallbacks.Interstitial.OnAdLoadSuccessInternal;
            platformDelegate.InterstitialAdLoadFailed += MeticaAdsCallbacks.Interstitial.OnAdLoadFailedInternal;
            platformDelegate.InterstitialAdShowSuccess += MeticaAdsCallbacks.Interstitial.OnAdShowSuccessInternal;
            platformDelegate.InterstitialAdShowFailed += MeticaAdsCallbacks.Interstitial.OnAdShowFailedInternal;
            platformDelegate.InterstitialAdHidden += MeticaAdsCallbacks.Interstitial.OnAdHiddenInternal;
            platformDelegate.InterstitialAdClicked += MeticaAdsCallbacks.Interstitial.OnAdClickedInternal;
            platformDelegate.InterstitialAdRevenuePaid += MeticaAdsCallbacks.Interstitial.OnAdRevenuePaidInternal;
            
            // Rewarded ad callbacks
            platformDelegate.RewardedAdLoadSuccess += MeticaAdsCallbacks.Rewarded.OnAdLoadSuccessInternal;
            platformDelegate.RewardedAdLoadFailed += MeticaAdsCallbacks.Rewarded.OnAdLoadFailedInternal;
            platformDelegate.RewardedAdShowSuccess += MeticaAdsCallbacks.Rewarded.OnAdShowSuccessInternal;
            platformDelegate.RewardedAdShowFailed += MeticaAdsCallbacks.Rewarded.OnAdShowFailedInternal;
            platformDelegate.RewardedAdHidden += MeticaAdsCallbacks.Rewarded.OnAdHiddenInternal;
            platformDelegate.RewardedAdClicked += MeticaAdsCallbacks.Rewarded.OnAdClickedInternal;
            platformDelegate.RewardedAdRewarded += MeticaAdsCallbacks.Rewarded.OnAdRewardedInternal;
            platformDelegate.RewardedAdRevenuePaid += MeticaAdsCallbacks.Rewarded.OnAdRevenuePaidInternal;
        }

        public static async Task<bool> InitializeAsync(MeticaConfiguration configuration)
        {
            return await platformDelegate.InitializeAsync(MeticaSdk.ApiKey, MeticaSdk.AppId, MeticaSdk.CurrentUserId, MeticaSdk.Version, MeticaSdk.BaseEndpoint, configuration);
        }
        public static void SetLogEnabled(bool logEnabled) 
        {
            platformDelegate.SetLogEnabled(logEnabled);
        }
        
        // Interstitial ad methods
        public static void LoadInterstitial()
        {
            platformDelegate.LoadInterstitial();
        }
        public static void ShowInterstitial()
        {
            platformDelegate.ShowInterstitial();
        }
        public static bool IsInterstitialReady()
        {
            return platformDelegate.IsInterstitialReady();
        }
        
        // Rewarded ad methods
        public static void LoadRewarded()
        {
            platformDelegate.LoadRewarded();
        }
        public static void ShowRewarded()
        {
            platformDelegate.ShowRewarded();
        }
        public static bool IsRewardedReady()
        {
            return platformDelegate.IsRewardedReady();
        }
    }
}
