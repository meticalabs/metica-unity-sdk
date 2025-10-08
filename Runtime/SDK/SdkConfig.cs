using System;
using System.Collections.Generic;
using Metica.ADS;

namespace Metica.SDK
{
    [System.Serializable]
    public struct SdkConfig
    {
        [Serializable]
        // TODO: use KeyMap to pass it to the SDK
        public struct KeyMap
        {
            public string Key;
            public string Value;
        }
        public string ApiKey;
        public string AppId;
        public string UserId;
        public string BaseEndpoint;
        public List<KeyMap> CustomKeys;

        public static string ProductionEndpoint = "https://api-gateway.prod-eu.metica.com";

        /// <summary>
        /// The log level for the SDK.
        /// </summary>

        // Only used to initialize scriptable object wrappers
        internal static SdkConfig Default()
        {
            return new SdkConfig()
            {
                // Parameters for MeticaContext
                ApiKey = string.Empty,
                AppId = string.Empty,
                UserId = string.Empty,
                // - - - - - - - - - -
                BaseEndpoint = ProductionEndpoint,
            };
        }

        // TODO this was fitted in for compatibility with MeticaAds' MeticaConfiguration.
        // We should upgrade this to substitute SdkConfig completely.
        public MeticaConfiguration ToMeticaConfiguration()
            => new()
            {
                ApiKey = ApiKey,
                AppId = AppId,
                UserId = UserId,
                BaseEndpoint = BaseEndpoint
            };
    }
}
