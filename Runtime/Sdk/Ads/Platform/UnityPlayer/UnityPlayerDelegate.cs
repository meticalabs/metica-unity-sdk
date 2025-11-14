#nullable enable

using System;
using System.Threading.Tasks;
using Metica;

namespace Metica.Ads.UnityPlayer
{
    internal class UnityPlayerDelegate : PlatformDelegate
    {
        // AppLovin-specific functionality
        public MeticaApplovinFunctions Max { get; } = new UnityPlayerApplovinFunctions();

        // Mock events - these won't actually fire in editor
        public event Action<MeticaAd> BannerAdLoadSuccess;
        public event Action<MeticaAdError> BannerAdLoadFailed;
        public event Action<MeticaAd> BannerAdClicked;
        public event Action<MeticaAd> BannerAdRevenuePaid;
        public event Action<MeticaAd> InterstitialAdLoadSuccess;
        public event Action<MeticaAdError> InterstitialAdLoadFailed;
        public event Action<MeticaAd> InterstitialAdShowSuccess;
        public event Action<MeticaAd, MeticaAdError> InterstitialAdShowFailed;
        public event Action<MeticaAd> InterstitialAdHidden;
        public event Action<MeticaAd> InterstitialAdClicked;
        public event Action<MeticaAd> InterstitialAdRevenuePaid;

        public event Action<MeticaAd> RewardedAdLoadSuccess;
        public event Action<MeticaAdError> RewardedAdLoadFailed;
        public event Action<MeticaAd> RewardedAdShowSuccess;
        public event Action<MeticaAd, MeticaAdError> RewardedAdShowFailed;
        public event Action<MeticaAd> RewardedAdHidden;
        public event Action<MeticaAd> RewardedAdClicked;
        public event Action<MeticaAd> RewardedAdRewarded;
        public event Action<MeticaAd> RewardedAdRevenuePaid;

        public Task<MeticaInitResponse> InitializeAsync(string apiKey, string appId, string userId,
            string mediationInfoKey)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock initialization - always returns HoldoutDueToError");
            var tcs = new TaskCompletionSource<MeticaInitResponse>();
            tcs.SetResult(
                new MeticaInitResponse(new MeticaSmartFloors(MeticaUserGroup.HOLDOUT, false))
            );
            return tcs.Task;
        }

        public void SetLogEnabled(bool logEnabled)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock SetLogEnabled: {logEnabled}");
        }

        public void SetHasUserConsent(bool value)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock SetHasUserConsent: {value}");
        }

        public void SetDoNotSell(bool value)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock SetDoNotSell: {value}");
        }

        public void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock CreateBanner called");
        }

        public void ShowBanner(string adUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock ShowBanner called");
        }

        public void HideBanner(string adUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock HideBanner called");

        }

        public void DestroyBanner(string adUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock DestroyBanner called");
        }

        public void LoadInterstitial(string interstitialAdUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock LoadInterstitial called");
        }

        public void ShowInterstitial(string interstitialAdUnitId, string? placementId, string? customData)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock ShowInterstitial called with placementId={placementId}, customData={customData}");
        }

        public bool IsInterstitialReady(string interstitialAdUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock IsInterstitialReady - always returns false");
            return false;
        }

        public void LoadRewarded(string rewardedAdUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock LoadRewarded called");
        }

        public void ShowRewarded(string rewardedAdUnitId, string? placementId, string? customData)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock ShowRewarded called with placementId={placementId}, customData={customData}");
        }

        public bool IsRewardedReady(string rewardedAdUnitId)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock IsRewardedReady - always returns false");
            return false;
        }
    }
}
