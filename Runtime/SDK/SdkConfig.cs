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
        public string apiKey;
        public string appId;
        public string userId;
        public string version;
        public string baseEndpoint;
        public List<KeyMap> customKeys;

        public static string ProductionEndpoint = "https://api-gateway.prod-eu.metica.com";

        /// <summary>
        /// The log level for the SDK.
        /// </summary>
        public LogLevel logLevel;

        public static SdkConfig Default()
        {
            return new SdkConfig()
            {
                // Parameters for MeticaContext
                apiKey = string.Empty,
                appId = string.Empty,
                userId = string.Empty,
                version = string.Empty,
                // - - - - - - - - - -
                baseEndpoint = ProductionEndpoint,
                logLevel = LogLevel.Error
            };
        }

        // TODO this was fitted in for compatibility with MeticaAds' MeticaConfiguration.
        // We should upgrade this to substitute SdkConfig completely.
        public MeticaConfiguration ToMeticaConfiguration()
            => new()
            {
                ApiKey = apiKey,
                AppId = appId,
                UserId = userId,
                Version = version,
                BaseEndpoint = baseEndpoint
            };
    }
}
