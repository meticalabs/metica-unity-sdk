//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Metica.ADS.IOS
{
internal class IOSDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

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

        [DllImport("__Internal")]
        private static extern void ios_setLogEnabled(bool value);
        [DllImport("__Internal")]
        private static extern bool ios_isRewardedReady();
        [DllImport("__Internal")]
        private static extern bool ios_isInterstitialReady();
        
    public void SetLogEnabled(bool logEnabled)
    {
            MeticaAds.Log.LogDebug(() => $"{TAG} SetLogEnabled called with: {logEnabled}");
            ios_setLogEnabled(logEnabled);
    }

    public void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
    {
    }

    public void ShowBanner(string adUnitId)
    {
    }

    public void HideBanner(string adUnitId)
    {
    }

    public void DestroyBanner(string adUnitId)
    {
    }

        public Task<MeticaAdsInitializationResult> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
            MeticaConfiguration configuration)
        {
            var tcs = new TaskCompletionSource<MeticaAdsInitializationResult>();
            var callback = new IOSInitializeCallback(tcs);
            callback.InitializeSDK(apiKey, appId, userId, version, baseEndpoint);
            return tcs.Task;
        }

    // Interstitial methods
    public void LoadInterstitial()
    {
            var callback = new IOSLoadCallback();
            callback.AdLoadSuccess += (meticaAd) => InterstitialAdLoadSuccess?.Invoke(meticaAd);
            callback.AdLoadFailed += (error) => InterstitialAdLoadFailed?.Invoke(error);
            
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS loadInterstitial method");
            callback.LoadInterstitial();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS loadInterstitial method called");
    }

    public void ShowInterstitial()
    {
            var callback = new IOSShowCallbackProxy();

            callback.AdShowSuccess += (ad) => InterstitialAdShowSuccess?.Invoke(ad);
            callback.AdShowFailed += (ad, error) => InterstitialAdShowFailed?.Invoke(ad, error);
            callback.AdHidden += (ad) => InterstitialAdHidden?.Invoke(ad);
            callback.AdClicked += (ad) => InterstitialAdClicked?.Invoke(ad);
            callback.AdRevenuePaid += (ad) => InterstitialAdRevenuePaid?.Invoke(ad);

            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS showInterstitial method");
            callback.ShowInterstitial();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS showInterstitial method called");
    }

    public bool IsInterstitialReady()
    {
        return false;
    }

    // Rewarded methods
    public void LoadRewarded()
    {
            var callback = new IOSLoadCallback();
            callback.AdLoadSuccess += (meticaAd) => RewardedAdLoadSuccess?.Invoke(meticaAd);
            callback.AdLoadFailed += (error) => RewardedAdLoadFailed?.Invoke(error);
     
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS loadRewarded method");
            callback.LoadRewarded();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS loadRewarded method called");
    }

    public void ShowRewarded()
    {
            var callback = new IOSShowCallbackProxy();
            callback.AdShowSuccess += (ad) => RewardedAdShowSuccess?.Invoke(ad);
            callback.AdShowFailed += (ad, error) => RewardedAdShowFailed?.Invoke(ad, error);
            callback.AdHidden += (ad) => RewardedAdHidden?.Invoke(ad);
            callback.AdClicked += (ad) => RewardedAdClicked?.Invoke(ad);
            callback.AdRewarded += (ad) => RewardedAdRewarded?.Invoke(ad);
            callback.AdRevenuePaid += (ad) => RewardedAdRevenuePaid?.Invoke(ad);
    
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS showRewarded method");
            callback.ShowRewarded();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS showRewarded method called");
    }

    public bool IsRewardedReady()
    {
        return false;
    }

    // Notification methods
    public void NotifyAdLoadAttempt(string adUnitId)
    {
        // TODO: Implement iOS native call
    }

    public void NotifyAdLoadSuccess(MeticaAd meticaAd)
    {
        // TODO: Implement iOS native call
    }

    public void NotifyAdLoadFailed(string adUnitId, string error)
    {
        // TODO: Implement iOS native call
    }

    public void NotifyAdRevenue(MeticaAd meticaAd)
    {
        // TODO: Implement iOS native call
    }
}
}
