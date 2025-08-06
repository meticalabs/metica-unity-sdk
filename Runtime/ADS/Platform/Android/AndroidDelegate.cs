//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS.Android
{
internal class AndroidDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

    private static readonly AndroidJavaClass UnityBridgeAndroidClass
        = AndroidUnityBridge.UnityBridgeClass;

    private static readonly AndroidJavaClass MeticaAdsExternalTrackerAndroidClass
        = AndroidUnityBridge.MeticaAdsExternalTrackerClass;

    // Events for interstitial ad lifecycle callbacks - updated signatures
    public event Action<MeticaAd> InterstitialAdLoadSuccess;
    public event Action<string> InterstitialAdLoadFailed;
    public event Action<MeticaAd> InterstitialAdShowSuccess;
    public event Action<MeticaAd, string> InterstitialAdShowFailed;
    public event Action<MeticaAd> InterstitialAdHidden;
    public event Action<MeticaAd> InterstitialAdClicked;
    public event Action<MeticaAd> InterstitialAdRevenuePaid;

    // Events for rewarded ad lifecycle callbacks - updated signatures
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
        Debug.Log($"{TAG} SetLogEnabled called with: {logEnabled}");

        UnityBridgeAndroidClass.CallStatic("setLogEnabled", logEnabled);
    }

    public Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
        MeticaConfiguration configuration)
    {
        var tcs = new TaskCompletionSource<bool>();

        var callback = new InitializeCallbackProxy(tcs);
        UnityBridgeAndroidClass.CallStatic("initialize", apiKey, appId, userId, callback);
        return tcs.Task;
    }

    // Interstitial methods
    public void LoadInterstitial()
    {
        Debug.Log($"{TAG} LoadInterstitial called");

        var callback = new LoadCallbackProxy();

        // Wire up all events - now using MeticaAd and string directly
        callback.AdLoadSuccess += (meticaAd) => InterstitialAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => InterstitialAdLoadFailed?.Invoke(error);

        Debug.Log($"{TAG} About to call Android loadInterstitial method");
        UnityBridgeAndroidClass.CallStatic("loadInterstitial", callback);
        Debug.Log($"{TAG} Android loadInterstitial method called");
    }

    public void ShowInterstitial()
    {
        Debug.Log($"{TAG} ShowInterstitial called");

        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => InterstitialAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => InterstitialAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => InterstitialAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => InterstitialAdClicked?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => InterstitialAdRevenuePaid?.Invoke(meticaAd);

        Debug.Log($"{TAG} About to call Android showInterstitial method");
        UnityBridgeAndroidClass.CallStatic("showInterstitial", callback);
        Debug.Log($"{TAG} Android showInterstitial method called");
    }

    public bool IsInterstitialReady()
    {
        return UnityBridgeAndroidClass.CallStatic<bool>("isInterstitialReady");
    }

    // Rewarded methods
    public void LoadRewarded()
    {
        Debug.Log($"{TAG} LoadRewarded called");

        var callback = new LoadCallbackProxy();

        // Wire up all events - now using MeticaAd and string directly
        callback.AdLoadSuccess += (meticaAd) => RewardedAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => RewardedAdLoadFailed?.Invoke(error);

        Debug.Log($"{TAG} About to call Android loadRewarded method");
        UnityBridgeAndroidClass.CallStatic("loadRewarded", callback);
        Debug.Log($"{TAG} Android loadRewarded method called");
    }

    public void ShowRewarded()
    {
        Debug.Log($"{TAG} ShowRewarded called");

        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => RewardedAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => RewardedAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => RewardedAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => RewardedAdClicked?.Invoke(meticaAd);
        callback.AdRewarded += (meticaAd) => RewardedAdRewarded?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => RewardedAdRevenuePaid?.Invoke(meticaAd);

        Debug.Log($"{TAG} About to call Android showRewarded method");
        UnityBridgeAndroidClass.CallStatic("showRewarded", callback);
        Debug.Log($"{TAG} Android showRewarded method called");
    }

    public bool IsRewardedReady()
    {
        return UnityBridgeAndroidClass.CallStatic<bool>("isRewardedReady");
    }

    // Notification methods
    public void NotifyAdLoadAttempt(string adUnitId)
    {
        Debug.Log($"{TAG} NotifyAdLoadAttempt called for adUnitId: {adUnitId}");
        MeticaAdsExternalTrackerAndroidClass.CallStatic("notifyAdLoadAttempt", adUnitId);
    }

    public void NotifyAdLoadSuccess(MeticaAd meticaAd)
    {
        Debug.Log($"{TAG} NotifyAdLoadSuccess called for adUnitId: {meticaAd.adUnitId}");

        using (var meticaAdAndroid = meticaAd.ToAndroidJavaObject())
        {
            MeticaAdsExternalTrackerAndroidClass.CallStatic("notifyAdLoadSuccess", meticaAdAndroid);
        }
    }

    public void NotifyAdLoadFailed(string adUnitId, string error)
    {
        Debug.Log($"{TAG} NotifyAdLoadFailed called for adUnitId: {adUnitId}, error: {error}");
        MeticaAdsExternalTrackerAndroidClass.CallStatic("notifyAdLoadFailed", adUnitId, error);
    }

    public void NotifyAdRevenue(MeticaAd meticaAd)
    {
        Debug.Log($"{TAG} NotifyAdRevenue called for adUnitId: {meticaAd.adUnitId}");

        using (var meticaAdAndroid = meticaAd.ToAndroidJavaObject())
        {
            MeticaAdsExternalTrackerAndroidClass.CallStatic("notifyAdRevenue", meticaAdAndroid);
        }
    }
}
}
