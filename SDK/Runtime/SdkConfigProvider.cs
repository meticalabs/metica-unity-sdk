using UnityEngine;

namespace Metica.Unity
{
    [CreateAssetMenu(fileName = "MeticaSdkConfiguration", menuName = "Metica/SDK/New SDK Configuration")]
    public class SdkConfigProvider: ScriptableObject, ISdkConfigProvider
    {
        [SerializeField] private SdkConfig _sdkConfig;

        public SdkConfig SdkConfig => _sdkConfig;

#if UNITY_EDITOR // Editor aid to initialize the ScriptableObject to defaults only once.
        [SerializeField, HideInInspector] private bool _defaultInitialized = false; // hidden utility field to initialize to defaults only when created
        private void OnEnable()
        {
            if (!_defaultInitialized)
            {
                _sdkConfig = SdkConfig.Default();
                _defaultInitialized = true;
            }
        }
#endif

    }
}
