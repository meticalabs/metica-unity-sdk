using UnityEngine;

using Metica.Core;
using Metica.SDK;

namespace Metica.Unity
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
            RegisterUnityServices(_sdkConfigProvider.SdkConfig);
            // Initialize Metica SDK.
            _meticaSdk = new MeticaSdk(_sdkConfigProvider.SdkConfig);

            DontDestroyOnLoad(this);
        }

        public static void RegisterUnityServices(SdkConfig sdkConfig)
        {
            // Register implementations before anything else. These are Unity implementations.
            Registry.Register<IDeviceInfoProvider>(new DeviceInfoProvider());
            Registry.Register<ILog>(new MeticaLogger(sdkConfig.logLevel));
        }

        private async void OnApplicationFocus(bool focus)
        {
            if(focus == false) // focus lost
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
