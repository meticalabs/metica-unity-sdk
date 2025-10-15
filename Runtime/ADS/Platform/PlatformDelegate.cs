#nullable enable

using System;
using System.Threading.Tasks;

namespace Metica.ADS
{
internal interface PlatformDelegate
{
    // Events for banner ad lifecycle callbacks
    public event Action<MeticaAd> BannerAdLoadSuccess;
    public event Action<string> BannerAdLoadFailed;
    public event Action<MeticaAd> BannerAdClicked;
    public event Action<MeticaAd> BannerAdRevenuePaid;
    
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

    Task<MeticaInitializationResult> InitializeAsync(
        string apiKey,
        string appId,
        string userId,
        string version,
        string baseEndpoint,
        MeticaConfiguration configuration
    );
    void SetLogEnabled(bool logEnabled);
            
    // Banner methods
    void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position);
    void ShowBanner(string adUnitId);
    void HideBanner(string adUnitId);
    void DestroyBanner(string adUnitId);

    // Interstitial methods
    void LoadInterstitial(string interstitialAdUnitId);
    void ShowInterstitial(string interstitialAdUnitId);
    bool IsInterstitialReady(string interstitialAdUnitId);
            
    // Rewarded methods
    void LoadRewarded();
    void ShowRewarded();
    bool IsRewardedReady();
}
}
