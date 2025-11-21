//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Metica;
using Metica.Ads;

namespace Metica.Ads.IOS
{
internal class IOSDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

    // AppLovin-specific functionality
    public MeticaApplovinFunctions Max { get; } = new IOSApplovinFunctions();

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

    [DllImport("__Internal")]
    private static extern void ios_setLogEnabled(bool value);
    [DllImport("__Internal")]
    private static extern bool ios_isRewardedReady(string adUnitId);
    [DllImport("__Internal")]
    private static extern bool ios_isInterstitialReady(string adUnitId);
    [DllImport("__Internal")]
    private static extern void ios_setHasUserConsent(bool value);
    [DllImport("__Internal")]
    private static extern void ios_setDoNotSell(bool value);

    [DllImport("__Internal")]
    private static extern void ios_showBanner(string adUnitId);
    [DllImport("__Internal")]
    private static extern void ios_hideBanner(string adUnitId);
    [DllImport("__Internal")]
    private static extern void ios_destroyBanner(string adUnitId);

    public void SetLogEnabled(bool logEnabled)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetLogEnabled called with: {logEnabled}");
        ios_setLogEnabled(logEnabled);
    }

    public void SetHasUserConsent(bool hasUserConsent)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetHasUserConsent called with: {hasUserConsent}");
        ios_setHasUserConsent(hasUserConsent);
    }

    public void SetDoNotSell(bool doNotSell)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetDoNotSell called with: {doNotSell}");
        ios_setDoNotSell(doNotSell);
    }

    public void CreateBanner(string bannerAdUnitId, MeticaBannerPosition position)
    {
        var callback = new IOSBannerCallbackProxy();
        callback.AdLoadSuccess += BannerAdLoadSuccess;
        callback.AdLoadFailed += BannerAdLoadFailed;
        callback.AdClicked += (meticaAd) => BannerAdClicked?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => BannerAdRevenuePaid?.Invoke(meticaAd);

        // On Native side the matrix is:
        //  0 -> top position
        // -1 -> bottom position
        var yPosition = position switch
        {
            MeticaBannerPosition.Bottom => -1,
            MeticaBannerPosition.Top => 0,
            _ => 0,
        };

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS createBanner method");
        callback.CreateBanner(bannerAdUnitId, yPosition);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS createBanner method called");
    }

    public void ShowBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS showBanner method");
        ios_showBanner(adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS showBanner method called");
    }

    public void HideBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS hideBanner method");
        ios_hideBanner(adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS hideBanner method called");
    }

    public void DestroyBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS destroyBanner method");
        ios_destroyBanner(adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS destroyBanner method called");
    }

    public Task<MeticaInitResponse> InitializeAsync(string apiKey, string appId, string userId, string mediationInfoKey)
    {
        var tcs = new TaskCompletionSource<MeticaInitResponse>();
        var callback = new IOSInitializeCallback(tcs);
        callback.InitializeSDK(apiKey, appId, userId, mediationInfoKey);
        return tcs.Task;
    }

    // Interstitial methods
    public void LoadInterstitial(string interstitialAdUnitId)
    {
        var callback = new IOSLoadCallback();
        callback.AdLoadSuccess += (meticaAd) => InterstitialAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => InterstitialAdLoadFailed?.Invoke(error);
            
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS loadInterstitial method");
        callback.LoadInterstitial(interstitialAdUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS loadInterstitial method called");
    }

    public void ShowInterstitial(string interstitialAdUnitId, string? placementId, string? customData)
    {
        var callback = new IOSShowCallbackProxy();

        callback.AdShowSuccess += (ad) => InterstitialAdShowSuccess?.Invoke(ad);
        callback.AdShowFailed += (ad, error) => InterstitialAdShowFailed?.Invoke(ad, error);
        callback.AdHidden += (ad) => InterstitialAdHidden?.Invoke(ad);
        callback.AdClicked += (ad) => InterstitialAdClicked?.Invoke(ad);
        callback.AdRevenuePaid += (ad) => InterstitialAdRevenuePaid?.Invoke(ad);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS showInterstitial method");
        callback.ShowInterstitial(interstitialAdUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS showInterstitial method called");
    }

    public bool IsInterstitialReady(string interstitialAdUnitId)
    {
        return ios_isInterstitialReady(interstitialAdUnitId);
    }

    // Rewarded methods
    public void LoadRewarded(string rewardedAdUnitId)
    {
        var callback = new IOSLoadCallback();
        callback.AdLoadSuccess += (meticaAd) => RewardedAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => RewardedAdLoadFailed?.Invoke(error);
     
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS loadRewarded method");
        callback.LoadRewarded(rewardedAdUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS loadRewarded method called");
    }

    public void ShowRewarded(string rewardedAdUnitId, string? placementId, string? customData)
    {
        var callback = new IOSShowCallbackProxy();
        callback.AdShowSuccess += (ad) => RewardedAdShowSuccess?.Invoke(ad);
        callback.AdShowFailed += (ad, error) => RewardedAdShowFailed?.Invoke(ad, error);
        callback.AdHidden += (ad) => RewardedAdHidden?.Invoke(ad);
        callback.AdClicked += (ad) => RewardedAdClicked?.Invoke(ad);
        callback.AdRewarded += (ad) => RewardedAdRewarded?.Invoke(ad);
        callback.AdRevenuePaid += (ad) => RewardedAdRevenuePaid?.Invoke(ad);
    
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS showRewarded method");
        callback.ShowRewarded(rewardedAdUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} iOS showRewarded method called");
    }

    public bool IsRewardedReady(string rewardedAdUnitId)
    {
        return ios_isRewardedReady(rewardedAdUnitId);
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
