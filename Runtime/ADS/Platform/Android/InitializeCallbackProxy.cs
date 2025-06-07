// InitializeCallbackProxy.cs

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
[SuppressMessage("ReSharper", "InconsistentNaming")]
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

    // Called from Android when initialization succeeds
    public void onInitialized(bool enabled)
    {
        Debug.Log($"{TAG} onInitialized: {enabled}");
        _tcs.SetResult(enabled);
    }

    // Called from Android when initialization fails
    public void onFailed(string reason)
    {
        Debug.Log($"{TAG} onFailed: {reason}");
        _tcs.SetResult(false);
    }
}
}
