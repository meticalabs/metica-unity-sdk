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

    // TODO should be passed from consumer.
    private const string InterstitialAdUnitId = "8c63ff91a4a47584";
    private const string RewardedAdUnitId = "0fdedfeb3c56e0c3";

    
    public void SetLogEnabled(bool logEnabled)
    {
        Debug.Log($"{TAG} SetLogEnabled called with: {logEnabled}");

        _unityBridgeAndroidClass.CallStatic("setLogEnabled", logEnabled);
    }

    public Task<MeticaInitializationResult> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint,
        MeticaConfiguration configuration)
    {
        var tcs = new TaskCompletionSource<MeticaInitializationResult>();

        var callback = new InitCallbackProxy(tcs);
       
        // TODO: pass it in the function
        const string applovinSdkKey = "CZ_XxS0v1pDXVdV2yDXaxO4dOV8849QwTq7iDFlGLsJZngU95AEyaq2z8lF0GRlSvdknWDpTDp1GmprFC1FiJ1";

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
        
        
        Debug.Log($"{TAG} About to call Android createBanner method");
        _unityBridgeAndroidClass.CallStatic("createBanner", adUnitId, yPosition, callback);
        Debug.Log($"{TAG} Android createBanner method called");
    }
    public void ShowBanner(string adUnitId)
    {
        Debug.Log($"{TAG} About to call Android showBanner method");
        _unityBridgeAndroidClass.CallStatic("showBanner", adUnitId);
        Debug.Log($"{TAG} Android showBanner method called");
    }

    public void HideBanner(string adUnitId)
    {
        Debug.Log($"{TAG} About to call Android hideBanner method");
        _unityBridgeAndroidClass.CallStatic("hideBanner", adUnitId);
        Debug.Log($"{TAG} Android hideBanner method called");
    }

    public void DestroyBanner(string adUnitId)
    {
        Debug.Log($"{TAG} About to call Android destroyBanner method");
        _unityBridgeAndroidClass.CallStatic("destroyBanner", adUnitId);
        Debug.Log($"{TAG} Android destroyBanner method called");
    }
    
    // Interstitial methods
    public void LoadInterstitial()
    {
        var callback = new LoadCallbackProxy();

        // Wire up all events
        callback.AdLoadSuccess += (meticaAd) => InterstitialAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => InterstitialAdLoadFailed?.Invoke(error);

        Debug.Log($"{TAG} About to call Android loadInterstitial method");
        _unityBridgeAndroidClass.CallStatic("loadInterstitial", InterstitialAdUnitId, callback);
        Debug.Log($"{TAG} Android loadInterstitial method called");
    }

    public void ShowInterstitial()
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => InterstitialAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => InterstitialAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => InterstitialAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => InterstitialAdClicked?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => InterstitialAdRevenuePaid?.Invoke(meticaAd);

        Debug.Log($"{TAG} About to call Android showInterstitial method");
        _unityBridgeAndroidClass.CallStatic("showInterstitial", InterstitialAdUnitId, callback);
        Debug.Log($"{TAG} Android showInterstitial method called");
    }

    public bool IsInterstitialReady()
    {
        return _unityBridgeAndroidClass.CallStatic<bool>("isInterstitialReady", InterstitialAdUnitId);
    }

    // Rewarded methods
    public void LoadRewarded()
    {
        var callback = new LoadCallbackProxy();

        // Wire up all events - now using MeticaAd and string directly
        callback.AdLoadSuccess += (meticaAd) => RewardedAdLoadSuccess?.Invoke(meticaAd);
        callback.AdLoadFailed += (error) => RewardedAdLoadFailed?.Invoke(error);

        Debug.Log($"{TAG} About to call Android loadRewarded method");
        _unityBridgeAndroidClass.CallStatic("loadRewarded", RewardedAdUnitId, callback);
        Debug.Log($"{TAG} Android loadRewarded method called");
    }

    public void ShowRewarded()
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => RewardedAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => RewardedAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => RewardedAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => RewardedAdClicked?.Invoke(meticaAd);
        callback.AdRewarded += (meticaAd) => RewardedAdRewarded?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => RewardedAdRevenuePaid?.Invoke(meticaAd);

        Debug.Log($"{TAG} About to call Android showRewarded method");
        _unityBridgeAndroidClass.CallStatic("showRewarded", RewardedAdUnitId, callback);
        Debug.Log($"{TAG} Android showRewarded method called");
    }

    public bool IsRewardedReady()
    {
        return _unityBridgeAndroidClass.CallStatic<bool>("isRewardedReady", RewardedAdUnitId);
    }
}
}
