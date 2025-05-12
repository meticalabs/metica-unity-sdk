// ShowCallbackProxy.cs
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class ShowCallbackProxy : AndroidJavaProxy
{    private const string TAG = MeticaAds.TAG;
    
    // Events for all callbacks
    public event Action<string> AdShowSuccess;
    public event Action<string, string> AdShowFailed;
    public event Action<string> AdHidden;
    public event Action<string> AdClicked;
    public event Action<string> AdRewarded;
    
    public ShowCallbackProxy() 
        : base("com.metica.ads.MeticaShowAdCallback")
    {
        Debug.Log($"{TAG} ShowCallbackProxy created");
    }
    
    // Called when ad shows successfully
    public void onAdShowSuccess(string adUnitId)
    {
        Debug.Log($"{TAG} onAdShowSuccess callback received for adUnitId={adUnitId}");
        AdShowSuccess?.Invoke(adUnitId);
    }
    
    // Called when ad fails to show
    public void onAdShowFailed(string adUnitId, string error)
    {
        Debug.Log($"{TAG} onAdShowFailed callback received for adUnitId={adUnitId}, error={error}");
        AdShowFailed?.Invoke(adUnitId, error);
    }
    
    // Called when ad is hidden (closed)
    public void onAdHidden(string adUnitId)
    {
        Debug.Log($"{TAG} onAdHidden callback received for adUnitId={adUnitId}");
        AdHidden?.Invoke(adUnitId);
    }
    
    // Called when ad is clicked
    public void onAdClicked(string adUnitId)
    {
        Debug.Log($"{TAG} onAdClicked callback received for adUnitId={adUnitId}");
        AdClicked?.Invoke(adUnitId);
    }
    
    // Called when ad provides a reward
    public void onAdRewarded(string adUnitId)
    {
        Debug.Log($"{TAG} onAdRewarded callback received for adUnitId={adUnitId}");
        AdRewarded?.Invoke(adUnitId);
    }
}
}