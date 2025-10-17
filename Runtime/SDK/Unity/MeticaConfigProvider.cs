using Metica.ADS;
using UnityEngine;

namespace Metica.SDK.Unity
{
    [CreateAssetMenu(fileName = "MeticaConfiguration", menuName = "Metica/SDK/New Metica Configuration")]
    public class MeticaConfigProvider: ScriptableObject, IMeticaConfigProvider
    {
        [SerializeField] private MeticaConfiguration _config;

        public MeticaConfiguration Config => _config;

    }
}
