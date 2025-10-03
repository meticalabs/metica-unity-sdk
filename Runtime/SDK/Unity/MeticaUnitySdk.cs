using UnityEngine;
using Metica.Core;
using System;

namespace Metica.SDK.Unity
{
    public class MeticaUnitySdk : MonoBehaviour
    {
        [SerializeField] private SdkConfigProvider _sdkConfigProvider;
        private MeticaSdk _meticaSdk;

#if UNITY_EDITOR
        /// <summary>EDITOR ONLY</summary>
        public SdkConfigProvider SdkConfigProviderEditor { get { return _sdkConfigProvider; } set { _sdkConfigProvider = value; } }
#endif

        private void Awake()
        {
            // Initialize Metica SDK.
            _meticaSdk = Initialize(_sdkConfigProvider.SdkConfig);

            DontDestroyOnLoad(this);
        }

        [Obsolete]
        /// <summary>
        /// Creates and returns an instance of <see cref="MeticaSdk"/> with Unity implementations of needed services.
        /// </summary>
        /// <param name="sdkConfig">A Metica SDK configuration object.</param>
        /// <returns>A new <see cref="MeticaSdk"/> instance.</returns>
        public static MeticaSdk Initialize(SdkConfig sdkConfig)
        {
            Registry.Register<IDeviceInfoProvider>(new DeviceInfoProvider());
            Registry.Register<ILog>(new MeticaLogger(sdkConfig.logLevel));
            return new MeticaSdk(sdkConfig);
        }

        private async void OnApplicationFocus(bool focus)
        {
            if (focus == false) // focus lost
            {
                _meticaSdk.RequestDispatchEvents();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true) // paused
            {
                _meticaSdk.RequestDispatchEvents();
            }
        }

        private async void OnDestroy()
        {
            await _meticaSdk.DisposeAsync();
        }
    }
}
