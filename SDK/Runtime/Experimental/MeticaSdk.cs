using Metica.Experimental.Network;
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Experimental.Unity
{
    public class MeticaSdk : MonoBehaviour
    {
        #region Fields

        public static string CurrentUserId {  get; set; }

        [SerializeField] private Metica.Unity.SdkConfigProvider _sdkConfigProvider = null;
        private Metica.Unity.SdkConfig Config { get => _sdkConfigProvider.SdkConfig; } // alias for above
        private IHttpService _http;
        private OffersManager _offersManager;
        
        #endregion Fields

        private void Initialize()
        {
            if( _sdkConfigProvider == null)
            {
                Debug.LogError("Sdk Config Provider must be set");
            }
            CurrentUserId = _sdkConfigProvider.SdkConfig.initialUserId;
            _http = new HttpServiceDotnet().WithPersistentHeaders(new Dictionary<string, string> { { "X-API-Key", Config.apiKey } });
            _offersManager = new OffersManager(_http, $"{Config.offersEndpoint}/offers/v1/apps/{Config.appId}");
        }

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private async void Start()
        {
            var offersResult = await _offersManager.GetOffersAsync(CurrentUserId, null);
            Debug.Log(offersResult);
        }

        private void OnDestroy()
        {
            _http.Dispose();
        }

        #endregion Unity Lifecycle
    }
}
