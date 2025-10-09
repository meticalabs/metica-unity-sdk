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
    private readonly TaskCompletionSource<MeticaInitializationResult> _tcs;

    public InitializeCallbackProxy(TaskCompletionSource<MeticaInitializationResult> tcs)
        : base("com.metica.MeticaInitCallback")
    {
        Debug.Log($"{TAG} MeticaAdsInitCallback created");
        _tcs = tcs;
    }

    // Called from Android when initialization succeeds
    public void onInitialized(AndroidJavaObject initResponse)
    {
        // TODO: currently we force trial user group
        _tcs.SetResult(
            new MeticaInitializationResult(MeticaAdsAssignmentStatus.Normal)
        );
        /*
        Debug.Log($"{TAG} onInitialized: {adsEnabled}");
        if (adsEnabled)
        {
            _tcs.SetResult(
                new MeticaInitializationResult(MeticaAdsAssignmentStatus.Normal)
            );
        }
        else
        {
            _tcs.SetResult(
                new MeticaInitializationResult(MeticaAdsAssignmentStatus.Holdout)
            );
        }
    */
    }

}
}
