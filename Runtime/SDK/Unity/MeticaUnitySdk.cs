using UnityEngine;
using System;

namespace Metica.Unity
{
    public class MeticaUnitySdk : MonoBehaviour
    {
        [SerializeField] private MeticaConfigProvider _sdkConfigProvider;

#if UNITY_EDITOR
        /// <summary>EDITOR ONLY</summary>
        public MeticaConfigProvider SdkConfigProviderEditor { get { return _sdkConfigProvider; } set { _sdkConfigProvider = value; } }
#endif

        private void Awake()
        {
            // Initialize Metica SDK.
            // PREFAB IS NOT USED
            // var result = MeticaSdk.InitializeAsync(_sdkConfigProvider.Config);

            DontDestroyOnLoad(this);
        }

        [Obsolete]
        /// <summary>
        /// Creates and returns an instance of <see cref="MeticaSdk"/> with Unity implementations of needed services.
        /// </summary>
        /// <param name="sdkConfig">A Metica SDK configuration object.</param>
        /// <returns>A new <see cref="MeticaSdk"/> instance.</returns>

        private async void OnApplicationFocus(bool focus)
        {
            if (focus == false) // focus lost
            {
                MeticaSdk.Events.RequestDispatchEvents();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true) // paused
            {
                MeticaSdk.Events.RequestDispatchEvents();
            }
        }

        private async void OnDestroy()
        {
            await MeticaSdk.DisposeAsync();
        }
    }
}
