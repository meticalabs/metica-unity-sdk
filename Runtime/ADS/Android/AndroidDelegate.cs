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

    // Events for interstitial ad lifecycle callbacks
    public event Action<string> InterstitialAdLoadSuccess;
    public event Action<string, string> InterstitialAdLoadFailed;
    public event Action<string> InterstitialAdShowSuccess;
    public event Action<string, string> InterstitialAdShowFailed;
    public event Action<string> InterstitialAdHidden;
    public event Action<string> InterstitialAdClicked;

    public void SetLogEnabled(bool logEnabled)
    {
        // TODO tomi
    }

    public void Initialize(string apiKey, string appId, string userId, string baseEndpoint)
    {
        var callback = new InitializeCallbackProxy();
        MeticaUnityPluginClass.CallStatic("initialize", apiKey, appId, userId, baseEndpoint, callback);
    }

    public void LoadInterstitial()
    {
        Debug.Log($"{TAG} LoadInterstitial called");

        var callback = new LoadCallbackProxy();
        
        // Wire up all events
        callback.AdLoadSuccess += InterstitialAdLoadSuccess;
        callback.AdLoadFailed += InterstitialAdLoadFailed;

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
}
}