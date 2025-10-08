using Metica.ADS;
using UnityEngine;

namespace Metica.SDK.Unity
{
    [CreateAssetMenu(fileName = "MeticaSdkConfiguration", menuName = "Metica/SDK/New SDK Configuration")]
    public class SdkConfigProvider: ScriptableObject, ISdkConfigProvider
    {
        [SerializeField] private MeticaConfiguration _sdkConfig;

        public MeticaConfiguration SdkConfig => _sdkConfig;

    }
}
