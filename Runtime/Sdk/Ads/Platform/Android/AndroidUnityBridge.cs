using UnityEngine;

namespace Metica.Ads
{
    internal static class AndroidUnityBridge
    {
        public static readonly AndroidJavaClass UnityBridgeClass =
            new("com.metica.unity_bridge.UnityBridge");

    }
}
