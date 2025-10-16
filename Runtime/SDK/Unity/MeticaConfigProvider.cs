using Metica.ADS;
using UnityEngine;

namespace Metica.SDK.Unity
{
    [CreateAssetMenu(fileName = "MeticaConfiguration", menuName = "Metica/SDK/New Metica Configuration")]
    public class MeticaConfigProvider: ScriptableObject, IMeticaConfigProvider
    {
        [SerializeField] private MeticaInitConfig _config;

        public MeticaInitConfig Config => _config;

    }
}
