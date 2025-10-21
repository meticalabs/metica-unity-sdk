// LoadCallbackProxy.cs

using System;
using UnityEngine;

namespace Metica.Ads
{
public class InitResponseProxy
{
    private const string TAG = MeticaAds.TAG;

    public InitResponseProxy(AndroidJavaObject javaObject)
    {
        MeticaAds.Log.LogDebug(() => $"{TAG} InitResponseProxy created");
        var smartFloorsObj = javaObject.Get<AndroidJavaObject>("smartFloors");
        MeticaAds.Log.LogDebug(() => $"{TAG} InitResponseProxy smartFloorsObj = {smartFloorsObj}");
    }
}
}
