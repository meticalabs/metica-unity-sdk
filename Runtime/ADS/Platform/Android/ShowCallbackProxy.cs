// ShowCallbackProxy.cs

using System;
using UnityEngine;

namespace Metica.ADS
{
public class ShowCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    
    public event Action<MeticaAd> AdShowSuccess;
    public event Action<MeticaAd, string> AdShowFailed;
    public event Action<MeticaAd> AdHidden;
    public event Action<MeticaAd> AdClicked;
    public event Action<MeticaAd> AdRewarded;
    public event Action<MeticaAd> AdRevenuePaid;
    
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
    
    // Called when ad provides a reward 
    public void onAdRewarded(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdRewarded callback received for adUnitId={meticaAd.adUnitId}");
        AdRewarded?.Invoke(meticaAd);
    }
    
    // Called when revenue is paid for the ad 
    public void onAdRevenuePaid(AndroidJavaObject meticaAdObject)
    {
        var meticaAd = meticaAdObject.ToMeticaAd();
        Debug.Log($"{TAG} onAdRevenuePaid callback received for adUnitId={meticaAd.adUnitId}");
        AdRevenuePaid?.Invoke(meticaAd);
    }
}
}
