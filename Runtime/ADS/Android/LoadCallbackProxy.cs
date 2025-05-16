// LoadCallbackProxy.cs

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class LoadCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    public event Action<string> AdLoadSuccess;
    public event Action<string, string> AdLoadFailed;

    public LoadCallbackProxy()
        : base("com.metica.ads.MeticaLoadAdCallback")
    {
        Debug.Log($"{TAG} LoadCallbackProxy created");
    }

    // Called from Android when ad loads successfully
    public void onAdLoadSuccess(string adUnitId)
    {
        Debug.Log($"{TAG} onAdLoadSuccess callback received for adUnitId={adUnitId}");
        AdLoadSuccess?.Invoke(adUnitId);
    }

    // Called from Android when ad load fails
    public void onAdLoadFailed(string adUnitId, string error)
    {
        Debug.Log($"{TAG} onAdLoadFailed callback received for adUnitId={adUnitId}, error={error}");
        AdLoadFailed?.Invoke(adUnitId, error);
    }
}
}
