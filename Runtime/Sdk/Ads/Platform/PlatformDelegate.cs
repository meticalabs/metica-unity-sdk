#nullable enable

using System;
using System.Threading.Tasks;
using Metica;

namespace Metica.Ads
{
internal interface PlatformDelegate
{
    // AppLovin-specific functionality
    MeticaApplovinFunctions Max { get; }

    // Events for banner ad lifecycle callbacks
    public event Action<MeticaAd> BannerAdLoadSuccess;
    public event Action<MeticaAdError> BannerAdLoadFailed;
    public event Action<MeticaAd> BannerAdClicked;
    public event Action<MeticaAd> BannerAdRevenuePaid;
    
    // Events for interstitial ad lifecycle callbacks
    public event Action<MeticaAd> InterstitialAdLoadSuccess;
    public event Action<MeticaAdError> InterstitialAdLoadFailed;
    public event Action<MeticaAd> InterstitialAdShowSuccess;
    public event Action<MeticaAd, MeticaAdError> InterstitialAdShowFailed;
    public event Action<MeticaAd> InterstitialAdHidden;
    public event Action<MeticaAd> InterstitialAdClicked;
    public event Action<MeticaAd> InterstitialAdRevenuePaid;
            
    // Events for rewarded ad lifecycle callbacks
    public event Action<MeticaAd> RewardedAdLoadSuccess;
    public event Action<MeticaAdError> RewardedAdLoadFailed;
    public event Action<MeticaAd> RewardedAdShowSuccess;
    public event Action<MeticaAd, MeticaAdError> RewardedAdShowFailed;
    public event Action<MeticaAd> RewardedAdHidden;
    public event Action<MeticaAd> RewardedAdClicked;
    public event Action<MeticaAd> RewardedAdRewarded;
    public event Action<MeticaAd> RewardedAdRevenuePaid;

    Task<MeticaInitResponse> InitializeAsync(string apiKey,
        string appId,
        string userId, string mediationKey);
    void SetLogEnabled(bool logEnabled);
    void SetHasUserConsent(bool hasUserConsent);
    void SetDoNotSell(bool doNotSell);
            
    // Banner methods
    void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position);
    void ShowBanner(string adUnitId);
    void HideBanner(string adUnitId);
    void DestroyBanner(string adUnitId);

    // Interstitial methods
    void LoadInterstitial(string interstitialAdUnitId);
    void ShowInterstitial(string interstitialAdUnitId, string? placementId, string? customData);
    bool IsInterstitialReady(string interstitialAdUnitId);

    // Rewarded methods
    void LoadRewarded(string rewardedAdUnitId);
    void ShowRewarded(string rewardedAdUnitId, string? placementId, string? customData);
    bool IsRewardedReady(string rewardedAdUnitId);
}
}
