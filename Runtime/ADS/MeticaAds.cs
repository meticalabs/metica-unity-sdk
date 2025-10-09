#nullable enable

using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Metica.ADS
{
    internal static class MeticaAds
    {   
        public const string TAG = "MeticaUnityPlugin";
        private static readonly PlatformDelegate PlatformDelegate;

        static MeticaAds()
        {

#if UNITY_EDITOR
            // Check for Unity Editor first since the editor also responds to the currently selected platform.
            PlatformDelegate = GetUnityPlayerDelegate();
#elif UNITY_ANDROID
            PlatformDelegate = new Android.AndroidDelegate(AndroidUnityBridge.UnityBridgeClass);
#elif UNITY_IPHONE || UNITY_IOS
            PlatformDelegate = new IOS.IOSDelegate();
#else
            PlatformDelegate = GetUnityPlayerDelegate();
#endif
            // Banner ad callbacks
            PlatformDelegate.BannerAdLoadSuccess += MeticaAdsCallbacks.Banner.OnAdLoadSuccessInternal;
            PlatformDelegate.BannerAdLoadFailed += MeticaAdsCallbacks.Banner.OnAdLoadFailedInternal;
            PlatformDelegate.BannerAdClicked += MeticaAdsCallbacks.Banner.OnAdClickedInternal;
            PlatformDelegate.BannerAdRevenuePaid += MeticaAdsCallbacks.Banner.OnAdRevenuePaidInternal;
            
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

        public static async Task<MeticaInitializationResult> InitializeAsync(MeticaConfiguration configuration)
            => await InitializeWithResultAsync(configuration);


        public static async Task<MeticaInitializationResult> InitializeWithResultAsync(MeticaConfiguration configuration)
        {
            return await PlatformDelegate.InitializeAsync(
                configuration.ApiKey,
                configuration.AppId,
                configuration.UserId,
                configuration.Version,
                configuration.BaseEndpoint,
                configuration
            );
        }

        public static void SetLogEnabled(bool logEnabled) 
        {
            PlatformDelegate.SetLogEnabled(logEnabled);
        }
        
        public static void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
        {
            PlatformDelegate.CreateBanner(bannerAdUnitId, position);
        }
        
        // Banner ad methods
        public static void ShowBanner(string adUnitId)
        {
            
            PlatformDelegate.ShowBanner(adUnitId);
        }
        public static void HideBanner(string adUnitId)
        {
            PlatformDelegate.HideBanner(adUnitId);
        }
        
        public static void DestroyBanner(string adUnitId)
        {
            PlatformDelegate.DestroyBanner(adUnitId);
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
        
        private static UnityPlayer.UnityPlayerDelegate GetUnityPlayerDelegate()
        {
            // IMPORTANT: DO NOT IMPORT THE QUALIFIER  `UnityPlayer.`. 
            // Otherwise, Jetbrains Rider could remove unused qualifiers when commiting.
            return new UnityPlayer.UnityPlayerDelegate();
        }
    }
}
