// LoadCallbackProxy.cs

using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class InitializeCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    private readonly TaskCompletionSource<bool> _tcs;


    public InitializeCallbackProxy(TaskCompletionSource<bool> tcs)
        : base("com.metica.ads.MeticaAdsInitCallback")
    {
        Debug.Log($"{TAG} MeticaAdsInitCallback created");
        _tcs = tcs;
    }

    // Called from Android when ad loads successfully
    public void onInitialized(bool enabled)
    {
        Debug.Log($"{TAG} onInitialized: {enabled}");
        _tcs.SetResult(enabled);
    }
}
}