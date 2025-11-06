//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;
using Metica;

namespace Metica.Ads.IOS
{
internal class IOSDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

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
    }

    public void SetHasUserConsent(bool value)
    {
        throw new NotImplementedException();
    }

    public void SetDoNotSell(bool value)
    {
        throw new NotImplementedException();
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

    public Task<MeticaInitResponse> InitializeAsync(string apiKey, string appId, string userId, string mediationInfoKey)
    {
        var tcs = new TaskCompletionSource<MeticaInitResponse>();
        tcs.SetResult(
            new MeticaInitResponse(new MeticaSmartFloors(MeticaUserGroup.HOLDOUT, false))
        );
        return tcs.Task;
    }

    // Interstitial methods
    public void LoadInterstitial(string interstitialAdUnitId)
    {
    }

    public void ShowInterstitial(string interstitialAdUnitId, string? placementId, string? customData)
    {
    }

    public bool IsInterstitialReady(string interstitialAdUnitId)
    {
        return false;
    }

    // Rewarded methods
    public void LoadRewarded(string rewardedAdUnitId)
    {

    }

    public void ShowRewarded(string rewardedAdUnitId, string? placementId, string? customData)
    {

    }

    public bool IsRewardedReady(string rewardedAdUnitId)
    {
        return false;
    }
}
}
