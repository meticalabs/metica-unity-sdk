#nullable enable

using System;
using System.Threading.Tasks;

namespace Metica.ADS
{
internal interface PlatformDelegate 
{
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

    Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
        MeticaConfiguration configuration);
    void SetLogEnabled(bool logEnabled);
            
    // Interstitial methods
    void LoadInterstitial();
    void ShowInterstitial();
    bool IsInterstitialReady();
            
    // Rewarded methods
    void LoadRewarded();
    void ShowRewarded();
    bool IsRewardedReady();
    
    // Notification methods  
    void NotifyAdLoadAttempt(string adUnitId);
    void NotifyAdLoadSuccess(MeticaAd meticaAd);
    void NotifyAdLoadFailed(string adUnitId, string error);
    void NotifyAdRevenue(MeticaAd meticaAd);
}
}
