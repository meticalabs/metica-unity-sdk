using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS.UnityPlayer
{
internal class UnityPlayerDelegate : PlatformDelegate
{
    // Mock events - these won't actually fire in editor
    public event Action<MeticaAd> InterstitialAdLoadSuccess;
    public event Action<string> InterstitialAdLoadFailed;
    public event Action<MeticaAd> InterstitialAdShowSuccess;
    public event Action<MeticaAd, string> InterstitialAdShowFailed;
    public event Action<MeticaAd> InterstitialAdHidden;
    public event Action<MeticaAd> InterstitialAdClicked;
    public event Action<MeticaAd> InterstitialAdRevenuePaid;

    public event Action<MeticaAd> RewardedAdLoadSuccess;
    public event Action<string> RewardedAdLoadFailed;
    public event Action<MeticaAd> RewardedAdShowSuccess;
    public event Action<MeticaAd, string> RewardedAdShowFailed;
    public event Action<MeticaAd> RewardedAdHidden;
    public event Action<MeticaAd> RewardedAdClicked;
    public event Action<MeticaAd> RewardedAdRewarded;
    public event Action<MeticaAd> RewardedAdRevenuePaid;

    public Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
        MeticaConfiguration meticaConfiguration)
    {
        Debug.Log("[MeticaAds Unity] Mock initialization - always returns false");
        return Task.FromResult(false);
    }

    public void SetLogEnabled(bool logEnabled)
    {
        Debug.Log($"[MeticaAds Unity] Mock SetLogEnabled: {logEnabled}");
    }

    public void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
    {
        Debug.Log("[MeticaAds Unity] Mock CreateBanner called");
    }

    public void ShowBanner(string adUnitId)
    {
        Debug.Log("[MeticaAds Unity] Mock ShowBanner called");
    }

    public void HideBanner(string adUnitId)
    {
        Debug.Log("[MeticaAds Unity] Mock HideBanner called");

    }

    public void DestroyBanner(string adUnitId)
    {
        Debug.Log("[MeticaAds Unity] Mock DestroyBanner called");
    }

    public void LoadInterstitial()
    {
        Debug.Log("[MeticaAds Unity] Mock LoadInterstitial called");
    }

    public void ShowInterstitial()
    {
        Debug.Log("[MeticaAds Unity] Mock ShowInterstitial called");
    }

    public bool IsInterstitialReady()
    {
        Debug.Log("[MeticaAds Unity] Mock IsInterstitialReady - always returns false");
        return false;
    }

    public void LoadRewarded()
    {
        Debug.Log("[MeticaAds Unity] Mock LoadRewarded called");
    }

    public void ShowRewarded()
    {
        Debug.Log("[MeticaAds Unity] Mock ShowRewarded called");
    }

    public bool IsRewardedReady()
    {
        Debug.Log("[MeticaAds Unity] Mock IsRewardedReady - always returns false");
        return false;
    }
    // Notification methods
    public void NotifyAdLoadAttempt(string adUnitId)
    {
        Debug.Log($"[MeticaAds Unity] Mock NotifyAdLoadAttempt called for adUnitId: {adUnitId}");
    }

    public void NotifyAdLoadSuccess(MeticaAd meticaAd)
    {
        Debug.Log($"[MeticaAds Unity] Mock NotifyAdLoadSuccess called for adUnitId: {meticaAd.adUnitId}");
    }

    public void NotifyAdLoadFailed(string adUnitId, string error)
    {
        Debug.Log($"[MeticaAds Unity] Mock NotifyAdLoadFailed called for adUnitId: {adUnitId}, error: {error}");
    }

    public void NotifyAdRevenue(MeticaAd meticaAd)
    {
        Debug.Log($"[MeticaAds Unity] Mock NotifyAdShowSuccess called for adUnitId: {meticaAd.adUnitId}");
    }
}
}
