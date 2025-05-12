// LoadCallbackProxy.cs

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metica.ADS
{
public class InitializeCallbackProxy : AndroidJavaProxy
{
    private const string TAG = MeticaAds.TAG;

    public InitializeCallbackProxy()
        : base("com.metica.ads.MeticaAdsInitCallback")
    {
        Debug.Log($"{TAG} MeticaAdsInitCallback created");
    }

    // Called from Android when ad loads successfully
    public void onInitialized(bool enabled)
    {
        Debug.Log($"{TAG} onInitialized: {enabled}");
    }
}
}

