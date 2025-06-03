//  https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a

#nullable enable

using System;
using System.Threading.Tasks;

namespace Metica.ADS.IOS
{
internal class IOSDelegate : PlatformDelegate
{
    private const string TAG = MeticaAds.TAG;

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
        // TODO tomi
    }

    public Task<bool> InitializeAsync(string apiKey, string appId, string userId, string version, string baseEndpoint)
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
    }

    // Interstitial methods
    public void LoadInterstitial()
    {
    }

    public void ShowInterstitial()
    {
    }

    public bool IsInterstitialReady()
    {
        return false;
    }

    // Rewarded methods
    public void LoadRewarded()
    {
     
    }

    public void ShowRewarded()
    {
    
    }

    public bool IsRewardedReady()
    {
        return false;
    }
}
}
