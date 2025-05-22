//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Metica.ADS
{
internal class AndroidDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

    private static readonly AndroidJavaClass MeticaUnityPluginClass =
        new("com.metica.ads.MeticaAds");

    // Events for interstitial ad lifecycle callbacks - updated signatures
    public event Action<MeticaAd> InterstitialAdLoadSuccess;
    public event Action<string> InterstitialAdLoadFailed;
    public event Action<string> InterstitialAdShowSuccess;
    public event Action<string, string> InterstitialAdShowFailed;
    public event Action<string> InterstitialAdHidden;
    public event Action<string> InterstitialAdClicked;
    
    // Events for rewarded ad lifecycle callbacks - updated signatures
    public event Action<MeticaAd> RewardedAdLoadSuccess;
    public event Action<string> RewardedAdLoadFailed;
    public event Action<string> RewardedAdShowSuccess;
    public event Action<string, string> RewardedAdShowFailed;
    public event Action<string> RewardedAdHidden;
    public event Action<string> RewardedAdClicked;
    public event Action<string> RewardedAdRewarded;

    public void SetLogEnabled(bool logEnabled)
    {
        // TODO tomi
    }

    public Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint)
    {
        var tcs = new TaskCompletionSource<bool>();

        var callback = new InitializeCallbackProxy(tcs);
        MeticaUnityPluginClass.CallStatic("initialize", apiKey, appId, userId, version, baseEndpoint, callback);
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
        MeticaUnityPluginClass.CallStatic("loadInterstitial", callback);
        Debug.Log($"{TAG} Android loadInterstitial method called");
    }

    public void ShowInterstitial()
    {
        Debug.Log($"{TAG} ShowInterstitial called");

        var callback = new ShowCallbackProxy();

        // Wire up all events
        callback.AdShowSuccess += (adUnitId) => InterstitialAdShowSuccess?.Invoke(adUnitId);
        callback.AdShowFailed += (adUnitId, error) => InterstitialAdShowFailed?.Invoke(adUnitId, error);
        callback.AdHidden += (adUnitId) => InterstitialAdHidden?.Invoke(adUnitId);
        callback.AdClicked += (adUnitId) => InterstitialAdClicked?.Invoke(adUnitId);

        Debug.Log($"{TAG} About to call Android showInterstitial method");
        MeticaUnityPluginClass.CallStatic("showInterstitial", callback);
        Debug.Log($"{TAG} Android showInterstitial method called");
    }

    public bool IsInterstitialReady()
    {
        return MeticaUnityPluginClass.CallStatic<bool>("isInterstitialReady");
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
        MeticaUnityPluginClass.CallStatic("loadRewarded", callback);
        Debug.Log($"{TAG} Android loadRewarded method called");
    }

    public void ShowRewarded()
    {
        Debug.Log($"{TAG} ShowRewarded called");

        var callback = new ShowCallbackProxy();

        // Wire up all events
        callback.AdShowSuccess += (adUnitId) => RewardedAdShowSuccess?.Invoke(adUnitId);
        callback.AdShowFailed += (adUnitId, error) => RewardedAdShowFailed?.Invoke(adUnitId, error);
        callback.AdHidden += (adUnitId) => RewardedAdHidden?.Invoke(adUnitId);
        callback.AdClicked += (adUnitId) => RewardedAdClicked?.Invoke(adUnitId);
        callback.AdRewarded += (adUnitId) => RewardedAdRewarded?.Invoke(adUnitId);

        Debug.Log($"{TAG} About to call Android showRewarded method");
        MeticaUnityPluginClass.CallStatic("showRewarded", callback);
        Debug.Log($"{TAG} Android showRewarded method called");
    }

    public bool IsRewardedReady()
    {
        return MeticaUnityPluginClass.CallStatic<bool>("isRewardedReady");
    }
}
}
