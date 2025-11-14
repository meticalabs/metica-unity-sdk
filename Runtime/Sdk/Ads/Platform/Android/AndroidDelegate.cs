//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using Metica;
using UnityEngine;

namespace Metica.Ads.Android
{
internal class AndroidDelegate : PlatformDelegate
{
    public AndroidDelegate(AndroidJavaClass unityBridgeClass)
    {
        _unityBridgeAndroidClass = unityBridgeClass;
    }

    private const string TAG = MeticaAds.TAG;

    private readonly AndroidJavaClass _unityBridgeAndroidClass;

    // AppLovin-specific functionality
    //
    // IMPORTANT: Max is lazily initialized to avoid accessing the native MeticaSdk.Ads before initialization.
    // If we eagerly initialize Max in the constructor, it will call UnityBridge.Mediation.getMax(),
    // which internally accesses MeticaSdk.Ads. This throws IllegalStateException if MeticaSdk.initialize()
    // hasn't been called yet on the native side.
    //
    // Lazy initialization ensures Max is only created when first accessed, which happens after
    // MeticaSdk.InitializeAsync() completes successfully.
    private readonly Lazy<MeticaApplovinFunctions> _max = new Lazy<MeticaApplovinFunctions>(() =>
    {
        var mediationClass = new AndroidJavaClass("com.metica.unity_bridge.UnityBridge$Mediation");
        var maxObject = mediationClass.CallStatic<AndroidJavaObject>("getMax");
        return new AndroidApplovinFunctions(maxObject);
    });

    public MeticaApplovinFunctions Max => _max.Value;

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

    public void SetLogEnabled(bool logEnabled)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetLogEnabled called with: {logEnabled}");

        _unityBridgeAndroidClass.CallStatic("setLogEnabled", logEnabled);
    }

    public void SetHasUserConsent(bool hasUserConsent)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetHasUserConsent called with: {hasUserConsent}");

        _unityBridgeAndroidClass.CallStatic("setHasUserConsent", hasUserConsent);
    }

    public void SetDoNotSell(bool doNotSell)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} SetDoNotSell called with: {doNotSell}");

        _unityBridgeAndroidClass.CallStatic("setDoNotSell", doNotSell);
    }

    public Task<MeticaInitResponse> InitializeAsync(string apiKey, string appId, string userId, string mediationKey)
    {
        var tcs = new TaskCompletionSource<MeticaInitResponse>();

        var callback = new InitCallbackProxy(tcs);

        _unityBridgeAndroidClass.CallStatic("initialize", apiKey, appId, mediationKey, userId, callback);
        return tcs.Task;
    }

    // Banner methods
    public void CreateBanner(string adUnitId, MeticaBannerPosition position)
    {
        var callback = new BannerCallbackProxy();
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
        callback.AdLoadSuccess += InterstitialAdLoadSuccess;
        callback.AdLoadFailed += InterstitialAdLoadFailed;

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android loadInterstitial method");
        _unityBridgeAndroidClass.CallStatic("loadInterstitial", interstitialAdUnitId, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android loadInterstitial method called");
    }

    public void ShowInterstitial(string interstitialAdUnitId, string? placementId, string? customData)
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => InterstitialAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => InterstitialAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => InterstitialAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => InterstitialAdClicked?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => InterstitialAdRevenuePaid?.Invoke(meticaAd);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android showInterstitial method with placementId={placementId}, customData={customData}");
        _unityBridgeAndroidClass.CallStatic("showInterstitial", interstitialAdUnitId, placementId, customData, callback);
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

    public void ShowRewarded(string rewardedAdUnitId, string? placementId, string? customData)
    {
        var callback = new ShowCallbackProxy();

        // Wire up all events - now using MeticaAd objects directly
        callback.AdShowSuccess += (meticaAd) => RewardedAdShowSuccess?.Invoke(meticaAd);
        callback.AdShowFailed += (meticaAd, error) => RewardedAdShowFailed?.Invoke(meticaAd, error);
        callback.AdHidden += (meticaAd) => RewardedAdHidden?.Invoke(meticaAd);
        callback.AdClicked += (meticaAd) => RewardedAdClicked?.Invoke(meticaAd);
        callback.AdRewarded += (meticaAd) => RewardedAdRewarded?.Invoke(meticaAd);
        callback.AdRevenuePaid += (meticaAd) => RewardedAdRevenuePaid?.Invoke(meticaAd);

        MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android showRewarded method with placementId={placementId}, customData={customData}");
        _unityBridgeAndroidClass.CallStatic("showRewarded", rewardedAdUnitId, placementId, customData, callback);
        MeticaAds.Log.LogDebug(() => $"{TAG} Android showRewarded method called");
    }

    public bool IsRewardedReady(string rewardedAdUnitId)
    {
        return _unityBridgeAndroidClass.CallStatic<bool>("isRewardedReady", rewardedAdUnitId);
    }
}
}
