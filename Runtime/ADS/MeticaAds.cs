#nullable enable

using System.Threading.Tasks;
using Metica.ADS.Android;
using Metica.SDK;

namespace Metica.ADS 
{
    public static class MeticaAds
    {   
        public const string TAG = "MeticaUnityPlugin";
        private static readonly PlatformDelegate platformDelegate;

        static MeticaAds()
        {
#if UNITY_ANDROID
            platformDelegate = new AndroidDelegate(AndroidUnityBridge.UnityBridgeClass, AndroidUnityBridge.MeticaAdsExternalTrackerClass);
#elif UNITY_IOS
            platformDelegate = new IOS.IOSDelegate();
#elif UNITY_EDITOR
            platformDelegate = new UnityPlayerDelegate();
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
        
        public static void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
        {
            platformDelegate.CreateBanner(bannerAdUnitId, position);
        }
        
        // Banner ad methods
        public static void ShowBanner(string adUnitId)
        {
            
            platformDelegate.ShowBanner(adUnitId);
        }
        public static void HideBanner(string adUnitId)
        {
            platformDelegate.HideBanner(adUnitId);
        }
        
        public static void DestroyBanner(string adUnitId)
        {
            platformDelegate.DestroyBanner(adUnitId);
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

        public static void NotifyAdLoadAttempt(string interstitialAdUnitId)
        {
            platformDelegate.NotifyAdLoadAttempt(interstitialAdUnitId);
        }

        public static void NotifyAdLoadSuccess(MeticaAd meticaAd)
        {
            platformDelegate.NotifyAdLoadSuccess(meticaAd);
        }

        public static void NotifyAdLoadFailed(string adUnitId, string error)
        {
            platformDelegate.NotifyAdLoadFailed(adUnitId, error);
        }

        public static void NotifyAdShowSuccess(MeticaAd meticaAd)
        {
            // Internally we use the NotifyAdRevenue call, but externally as to not
            // break API we use NotifyAdShowSuccess. Which is similar as both will tel you ad was shown.
            platformDelegate.NotifyAdRevenue(meticaAd);
        }
    }
}
