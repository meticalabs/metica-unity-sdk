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
        MeticaAds.Log.LogDebug(() => $"{TAG} InitCallbackProxy onInit");
        var smartFloorsJavaObject = initResponseJavaObject.Call<AndroidJavaObject>("getSmartFloors");
        var smartFloors = smartFloorsJavaObject.ToMeticaSmartFloors();

        MeticaAds.Log.LogDebug(() => $"{TAG} InitCallbackProxy smartFloorsObj = {smartFloors}");

        if (smartFloors.userGroup == MeticaUserGroup.TRIAL)
        {
            _tcs.SetResult(
                new MeticaInitializationResult(MeticaAdsAssignmentStatus.Normal)
            );
        }
        else
        {
            _tcs.SetResult(
                smartFloors.isSuccess
                    ? new MeticaInitializationResult(MeticaAdsAssignmentStatus.Holdout)
                    : new MeticaInitializationResult(MeticaAdsAssignmentStatus.HoldoutDueToError)
            );
        }
    }
}
}
