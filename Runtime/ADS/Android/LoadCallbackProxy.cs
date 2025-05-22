// LoadCallbackProxy.cs

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class LoadCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    public event Action<MeticaAd> AdLoadSuccess;
    public event Action<string> AdLoadFailed;

    public LoadCallbackProxy()
        : base("com.metica.ads.MeticaLoadAdCallback")
    {
        Debug.Log($"{TAG} LoadCallbackProxy created");
    }

    // Called from Android when ad loads successfully - now receives MeticaAd object
    public void onAdLoadSuccess(AndroidJavaObject meticaAdObject)
    {
        // Convert AndroidJavaObject to C# MeticaAd object
        var meticaAd = ConvertToMeticaAd(meticaAdObject);
        Debug.Log($"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
        AdLoadSuccess?.Invoke(meticaAd);
    }

    // Called from Android when ad load fails - now only receives error string
    public void onAdLoadFailed(string error)
    {
        Debug.Log($"{TAG} onAdLoadFailed callback received, error={error}");
        AdLoadFailed?.Invoke(error);
    }

    private MeticaAd ConvertToMeticaAd(AndroidJavaObject javaObject)
    {
        string adUnitId = javaObject.Get<string>("adUnitId");

        // Revenue is no longer nullable, can get directly
        double revenue = javaObject.Get<double>("revenue");

        string networkName = javaObject.Get<string>("networkName");
        string placementTag = javaObject.Get<string>("placementTag");
        string adFormat = javaObject.Get<string>("adFormat");

        return new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat);
    }
}
}
