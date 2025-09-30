namespace Metica.SDK
{
    [System.Serializable]
    public struct SdkConfig
    {
        public string apiKey;
        public string appId;
        public string userId;
        public string baseEndpoint;

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
                // - - - - - - - - - -
                baseEndpoint = ProductionEndpoint,
                logLevel = LogLevel.Error
            };
        }
    }
}
