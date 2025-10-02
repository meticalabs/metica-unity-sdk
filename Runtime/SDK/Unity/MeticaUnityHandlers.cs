using UnityEngine;

namespace Metica.SDK.Unity
{
    public static class MeticaUnityHandlers
    {
        /// <summary>
        ///  Ensures static fields are reset when entering Play Mode without Domain Reload
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticFields() => MeticaSdk.ResetStaticFields();
    }
}
