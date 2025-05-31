#nullable enable

using System;
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

        public static async Task<bool> InitializeAsync()
        {
            return await platformDelegate.InitializeAsync(MeticaSdk.ApiKey, MeticaSdk.AppId, MeticaSdk.CurrentUserId, MeticaSdk.Version, MeticaSdk.BaseEndpoint);
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

internal interface PlatformDelegate {
    // Events for interstitial ad lifecycle callbacks
    public event Action<MeticaAd> InterstitialAdLoadSuccess;
    public event Action<string> InterstitialAdLoadFailed;
    public event Action<MeticaAd> InterstitialAdShowSuccess;
    public event Action<MeticaAd, string> InterstitialAdShowFailed;
    public event Action<MeticaAd> InterstitialAdHidden;
    public event Action<MeticaAd> InterstitialAdClicked;
    public event Action<MeticaAd> InterstitialAdRevenuePaid;
        
    // Events for rewarded ad lifecycle callbacks
    public event Action<MeticaAd> RewardedAdLoadSuccess;
    public event Action<string> RewardedAdLoadFailed;
    public event Action<MeticaAd> RewardedAdShowSuccess;
    public event Action<MeticaAd, string> RewardedAdShowFailed;
    public event Action<MeticaAd> RewardedAdHidden;
    public event Action<MeticaAd> RewardedAdClicked;
    public event Action<MeticaAd> RewardedAdRewarded;
    public event Action<MeticaAd> RewardedAdRevenuePaid;

    Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint);
    void SetLogEnabled(bool logEnabled);
        
    // Interstitial methods
    void LoadInterstitial();
    void ShowInterstitial();
    bool IsInterstitialReady();
        
    // Rewarded methods
    void LoadRewarded();
    void ShowRewarded();
    bool IsRewardedReady();
}
}
