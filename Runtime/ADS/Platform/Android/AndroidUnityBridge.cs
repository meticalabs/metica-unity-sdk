using UnityEngine;

namespace Metica.ADS
{
    internal static class AndroidUnityBridge
    {
        public static readonly AndroidJavaClass UnityBridgeClass =
            new("com.metica.unity_bridge.UnityBridge");

    }
}
