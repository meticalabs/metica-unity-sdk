using System;
using UnityEngine;

namespace Metica.ADS
{
public class LoadCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    public event Action<MeticaAd> AdLoadSuccess;
    public event Action<string> AdLoadFailed;

    public LoadCallbackProxy()
        : base("com.metica.ads.MeticaAdsLoadCallback")
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} LoadCallbackProxy created");
    }

    // Called from Android when ad loads successfully - now receives MeticaAd object
    public void onAdLoadSuccess(AndroidJavaObject meticaAdObject)
    {
        // Convert AndroidJavaObject to C# MeticaAd object
        var meticaAd = meticaAdObject.ToMeticaAd();
        MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
        AdLoadSuccess?.Invoke(meticaAd);
    }

    // Called from Android when ad load fails - now only receives error string
    public void onAdLoadFailed(string error)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={error}");
        AdLoadFailed?.Invoke(error);
    }

}
}
