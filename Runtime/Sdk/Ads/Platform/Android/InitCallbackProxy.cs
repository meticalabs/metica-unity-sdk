using System.Threading.Tasks;
using Metica;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Metica.Ads
{
public class InitCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;
    private readonly TaskCompletionSource<MeticaInitResponse> _tcs;

    public InitCallbackProxy(TaskCompletionSource<MeticaInitResponse> tcs)
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
        _tcs.SetResult(new MeticaInitResponse(smartFloors));
    }
}
}
