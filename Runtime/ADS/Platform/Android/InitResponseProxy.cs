// LoadCallbackProxy.cs

using System;
using UnityEngine;

namespace Metica.ADS
{
public class InitResponseProxy
{
    private const string TAG = MeticaAds.TAG;

    public InitResponseProxy(AndroidJavaObject javaObject)
    {
        Debug.Log($"{TAG} InitResponseProxy created");
        var smartFloorsObj = javaObject.Get<AndroidJavaObject>("smartFloors");
        Debug.Log($"{TAG} InitResponseProxy smartFloorsObj = {smartFloorsObj}");
    }
}
}
