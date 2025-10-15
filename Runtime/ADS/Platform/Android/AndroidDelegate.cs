//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS.Android
{
internal class AndroidDelegate : PlatformDelegate
{
    public AndroidDelegate(AndroidJavaClass unityBridgeClass)
    {
        _unityBridgeAndroidClass = unityBridgeClass;
    }

    private const string TAG = MeticaAds.TAG;

    private readonly AndroidJavaClass _unityBridgeAndroidClass;

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

    public void SetLogEnabled(bool logEnabled)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetLogEnabled called with: {logEnabled}");

        _unityBridgeAndroidClass.CallStatic("setLogEnabled", logEnabled);
    }

    public Task<MeticaInitResponse> InitializeAsync(string apiKey, string appId, string userId, string version,
        string baseEndpoint,
        MeticaConfiguration configuration)
    {
        var tcs = new TaskCompletionSource<MeticaInitResponse>();

        var callback = new InitCallbackProxy(tcs);

        // TODO: pass it in the function
        const string applovinSdkKey =
            "CZ_XxS0v1pDXVdV2yDXaxO4dOV8849QwTq7iDFlGLsJZngU95AEyaq2z8lF0GRlSvdknWDpTDp1GmprFC1FiJ1";

        _unityBridgeAndroidClass.CallStatic("initialize", apiKey, appId, applovinSdkKey, userId, callback);
        return tcs.Task;
    }

    // Banner methods
    public void CreateBanner(string adUnitId, MeticaBannerPosition position)
    {
        var callback = new BannerCallbackProxy();
        callback.AdLoadSuccess += (meticaAd) => BannerAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => BannerAdLoadFailed?.Invoke(error);
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


        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android createBanner method");
        _unityBridgeAndroidClass.CallStatic("createBanner", adUnitId, yPosition, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android createBanner method called");
    }

    public void ShowBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android showBanner method");
        _unityBridgeAndroidClass.CallStatic("showBanner", adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android showBanner method called");
    }

    public void HideBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android hideBanner method");
        _unityBridgeAndroidClass.CallStatic("hideBanner", adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android hideBanner method called");
    }

    public void DestroyBanner(string adUnitId)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android destroyBanner method");
        _unityBridgeAndroidClass.CallStatic("destroyBanner", adUnitId);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android destroyBanner method called");
    }

    // Interstitial methods
    public void LoadInterstitial(string interstitialAdUnitId)
    {
        var callback = new LoadCallbackProxy();

        // Wire up all events
        callback.AdLoadSuccess += (meticaAd) => InterstitialAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => InterstitialAdLoadFailed?.Invoke(error);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android loadInterstitial method");
        _unityBridgeAndroidClass.CallStatic("loadInterstitial", interstitialAdUnitId, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android loadInterstitial method called");
    }

    public void ShowInterstitial(string interstitialAdUnitId)
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => InterstitialAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => InterstitialAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => InterstitialAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => InterstitialAdClicked?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => InterstitialAdRevenuePaid?.Invoke(meticaAd);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android showInterstitial method");
        _unityBridgeAndroidClass.CallStatic("showInterstitial", interstitialAdUnitId, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android showInterstitial method called");
    }

    public bool IsInterstitialReady(string interstitialAdUnitId)
    {
        return _unityBridgeAndroidClass.CallStatic<bool>("isInterstitialReady", interstitialAdUnitId);
    }

    // Rewarded methods
    public void LoadRewarded(string rewardedAdUnitId)
    {
        var callback = new LoadCallbackProxy();

        // Wire up all events - now using MeticaAd and string directly
        callback.AdLoadSuccess += (meticaAd) => RewardedAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => RewardedAdLoadFailed?.Invoke(error);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android loadRewarded method");
        _unityBridgeAndroidClass.CallStatic("loadRewarded", rewardedAdUnitId, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android loadRewarded method called");
    }

    public void ShowRewarded(string rewardedAdUnitId)
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => RewardedAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => RewardedAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => RewardedAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => RewardedAdClicked?.Invoke(meticaAd);
        callback.AdRewarded += (meticaAd) => RewardedAdRewarded?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => RewardedAdRevenuePaid?.Invoke(meticaAd);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android showRewarded method");
        _unityBridgeAndroidClass.CallStatic("showRewarded", rewardedAdUnitId, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android showRewarded method called");
    }

    public bool IsRewardedReady(string rewardedAdUnitId)
    {
        return _unityBridgeAndroidClass.CallStatic<bool>("isRewardedReady", rewardedAdUnitId);
    }
}
}
