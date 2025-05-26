// ShowCallbackProxy.cs
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class ShowCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    
    // Events for all callbacks - now using MeticaAd instead of string adUnitId
    public event Action<MeticaAd> AdShowSuccess;
    public event Action<MeticaAd, string> AdShowFailed;
    public event Action<MeticaAd> AdHidden;
    public event Action<MeticaAd> AdClicked;
    public event Action<MeticaAd> AdRewarded;
    
    public ShowCallbackProxy() 
        : base("com.metica.ads.MeticaShowAdCallback")
    {
        Debug.Log($"{TAG} ShowCallbackProxy created");
    }
    
    // Called when ad shows successfully - now receives MeticaAd object
    public void onAdShowSuccess(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdShowSuccess callback received for adUnitId={meticaAd.adUnitId}");
        AdShowSuccess?.Invoke(meticaAd);
    }
    
    // Called when ad fails to show - now receives MeticaAd object
    public void onAdShowFailed(AndroidJavaObject meticaAdObject, string error)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdShowFailed callback received for adUnitId={meticaAd.adUnitId}, error={error}");
        AdShowFailed?.Invoke(meticaAd, error);
    }
    
    // Called when ad is hidden (closed) - now receives MeticaAd object
    public void onAdHidden(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdHidden callback received for adUnitId={meticaAd.adUnitId}");
        AdHidden?.Invoke(meticaAd);
    }
    
    // Called when ad is clicked - now receives MeticaAd object
    public void onAdClicked(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdClicked callback received for adUnitId={meticaAd.adUnitId}");
        AdClicked?.Invoke(meticaAd);
    }
    
    // Called when ad provides a reward - this method might not be in the Kotlin interface
    // but keeping it for rewarded ads compatibility
    public void onAdRewarded(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdRewarded callback received for adUnitId={meticaAd.adUnitId}");
        AdRewarded?.Invoke(meticaAd);
    }
}
}
