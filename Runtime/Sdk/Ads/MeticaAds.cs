#nullable enable

using System.Threading.Tasks;
using Metica.Core;
using Metica;
using UnityEngine.Assertions;

// ReSharper disable once CheckNamespace
namespace Metica.Ads
{
    internal static class MeticaAds
    {
        public const string TAG = "[MeticaUnityPlugin]";
        private static readonly PlatformDelegate PlatformDelegate;
        internal static ILog Log => Registry.Resolve<ILog>();

        public static MeticaApplovinFunctions Max => PlatformDelegate.Max;

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

        public static async Task<MeticaInitResponse> InitializeAsync(MeticaInitConfig initConfig,
            MeticaMediationInfo mediationInfo)
        {
            return await PlatformDelegate.InitializeAsync(
                initConfig.ApiKey,
                initConfig.AppId,
                initConfig.UserId,
                mediationInfo.Key
            );
        }

        public static void SetLogEnabled(bool logEnabled) 
        {
            PlatformDelegate.SetLogEnabled(logEnabled);
        }
        public static void SetHasUserConsent(bool hasUserConsent)
        {
            PlatformDelegate.SetHasUserConsent(hasUserConsent);
        }

        public static void SetDoNotSell(bool doNotSell)
        {
            PlatformDelegate.SetDoNotSell(doNotSell);
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
        public static void LoadInterstitial(string interstitialAdUnitId)
        {
            PlatformDelegate.LoadInterstitial(interstitialAdUnitId);
        }
        public static void ShowInterstitial(string interstitialAdUnitId, string? placementId = null, string? customData = null)
        {
            PlatformDelegate.ShowInterstitial(interstitialAdUnitId, placementId, customData);
        }
        public static bool IsInterstitialReady(string interstitialAdUnitId)
        {
            return PlatformDelegate.IsInterstitialReady(interstitialAdUnitId);
        }

        // Rewarded ad methods
        public static void LoadRewarded(string rewardedAdUnitId)
        {
            PlatformDelegate.LoadRewarded(rewardedAdUnitId);
        }
        public static void ShowRewarded(string rewardedAdUnitId, string? placementId = null, string? customData = null)
        {
            PlatformDelegate.ShowRewarded(rewardedAdUnitId, placementId, customData);
        }
        public static bool IsRewardedReady(string rewardedAdUnitId)
        {
            return PlatformDelegate.IsRewardedReady(rewardedAdUnitId);
        }
        
        private static UnityPlayer.UnityPlayerDelegate GetUnityPlayerDelegate()
        {
            // IMPORTANT: DO NOT IMPORT THE QUALIFIER  `UnityPlayer.`. 
            // Otherwise, Jetbrains Rider could remove unused qualifiers when commiting.
            return new UnityPlayer.UnityPlayerDelegate();
        }
    }
}
