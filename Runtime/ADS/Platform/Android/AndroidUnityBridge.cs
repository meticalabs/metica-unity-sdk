using UnityEngine;

namespace Metica.ADS.Android
{
internal static class AndroidUnityBridge
{
    public static readonly AndroidJavaClass UnityBridgeClass =
        new("com.metica.unity_bridge.UnityBridge");

    public static readonly AndroidJavaClass MeticaAdsExternalTrackerClass =
        new("com.metica.ads.MeticaAdsExternalTracker");
}
}
