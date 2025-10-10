using System;
using System.Threading.Tasks;

namespace Metica.ADS.UnityPlayer
{
    internal class UnityPlayerDelegate : PlatformDelegate
    {
        // Mock events - these won't actually fire in editor
        public event Action<MeticaAd> BannerAdLoadSuccess;
        public event Action<string> BannerAdLoadFailed;
        public event Action<MeticaAd> BannerAdClicked;
        public event Action<MeticaAd> BannerAdRevenuePaid;
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

        public Task<MeticaAdsInitializationResult> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
            MeticaConfiguration meticaConfiguration)
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock initialization - always returns HoldoutDueToError");
            var tcs = new TaskCompletionSource<MeticaAdsInitializationResult>();
            tcs.SetResult(
                new MeticaAdsInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError)
            );
            return tcs.Task;
        }

        public void SetLogEnabled(bool logEnabled)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock SetLogEnabled: {logEnabled}");
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

        public void LoadInterstitial()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock LoadInterstitial called");
        }

        public void ShowInterstitial()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock ShowInterstitial called");
        }

        public bool IsInterstitialReady()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock IsInterstitialReady - always returns false");
            return false;
        }

        public void LoadRewarded()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock LoadRewarded called");
        }

        public void ShowRewarded()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock ShowRewarded called");
        }

        public bool IsRewardedReady()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock IsRewardedReady - always returns false");
            return false;
        }
        // Notification methods
        public void NotifyAdLoadAttempt(string adUnitId)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock NotifyAdLoadAttempt called for adUnitId: {adUnitId}");
        }

        public void NotifyAdLoadSuccess(MeticaAd meticaAd)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock NotifyAdLoadSuccess called for adUnitId: {meticaAd.adUnitId}");
        }

        public void NotifyAdLoadFailed(string adUnitId, string error)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock NotifyAdLoadFailed called for adUnitId: {adUnitId}, error: {error}");
        }

        public void NotifyAdRevenue(MeticaAd meticaAd)
        {
            MeticaAds.Log.LogDebug(() => $"[MeticaAds Unity] Mock NotifyAdShowSuccess called for adUnitId: {meticaAd.adUnitId}");
        }
    }
}
