// InitializeCallbackProxy.cs

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class InitCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    private readonly TaskCompletionSource<MeticaInitializationResult> _tcs;

    public InitCallbackProxy(TaskCompletionSource<MeticaInitializationResult> tcs)
        : base("com.metica.MeticaInitCallback")
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} MeticaAdsInitCallback created");
        _tcs = tcs;
    }

    // Called from Android when initialization succeeds
    public void onInit(AndroidJavaObject initResponseJavaObject)
    {
        //TODO: Use Metica Logger
        MeticaAds.Log.LogDebug(() => $"{TAG} InitCallbackProxy onInit");
        var smartFloorsJavaObject = initResponseJavaObject.Call<AndroidJavaObject>("getSmartFloors");

        MeticaAds.Log.LogDebug(() =>
            $"{TAG} InitCallbackProxy smartFloorsObj = {smartFloorsJavaObject.ToMeticaSmartFloors()}");


        // TODO: currently we force trial user group
        _tcs.SetResult(
            new MeticaInitializationResult(MeticaAdsAssignmentStatus.Normal)
        );

        /*
        MeticaAds.Log.LogDebug(() => $"{TAG} onInitialized: {adsEnabled}");
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
