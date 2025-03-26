using static System.Net.WebRequestMethods;

namespace Metica.Experimental.SDK
{
    [System.Serializable]
    public struct SdkConfig
    {
        public string apiKey;
        public string appId;
        public string initialUserId;

        private const string DefaultEndpoint = "https://api-gateway.prod-eu.metica.com";

        /// <summary>
        /// The full endpoint to the Metica offers endpoint.
        /// </summary>
        public string offersEndpoint;

        /// <summary>
        /// The full endpoint to the Metica ingestion service.
        /// </summary>
        public string ingestionEndpoint;

        /// <summary>
        /// The full endpoint to the Metica remote-config service.
        /// </summary>
        public string remoteConfigEndpoint;

        /// <summary>
        /// The maximum number of entries stored in the displays log. 
        /// </summary>
        /// <remarks>
        /// This limit is shared by all offers. Once the limit is reached, then the oldest entries
        /// will be removed and replaced by the newly incoming ones.
        /// </remarks>
        //public uint maxDisplayLogEntries;

        /// <summary>
        /// The filesystem path where the display log will be persisted.
        /// </summary>
        //public string displayLogPath;

        /// <summary>
        /// The cadence, in seconds, by which the displays log will be persisted to the filesystem.
        /// </summary>
        //public uint displayLogFlushCadence;

        /// <summary>
        /// The cadence, in seconds, by which the logged events will be sent to the ingestion service.
        /// </summary>
        public uint eventsLogDispatchCadence;

        /// <summary>
        /// The maximum number of pending logged events before they are sent to the ingestion service.
        /// </summary>
        /// <remarks>
        /// When the number of pending logged events reaches this maximum value, then the oldest accumulated events
        /// will be dropped to accomodate the most recent ones.
        /// </remarks>
        public uint eventsLogDispatchMaxQueueSize;

        /// <summary>
        /// The time-to-live, in minutes, for the offers cache.
        /// </summary>
        public uint httpCacheTTLSeconds;

        /// <summary>
        /// The filesystem path where the offers cache will be stored.
        /// </summary>
        //public string offersCachePath;

        /// <summary>
        /// The filesystem path where the remote config cache will be stored.
        /// </summary>
        //public string remoteConfigCachePath;

        /// <summary>
        /// The network timeout, in seconds, for the calls to any Metica endpoint.
        /// </summary>
        public int httpRequestTimeout;

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
                initialUserId = string.Empty,
                // - - - - - - - - - -
                ingestionEndpoint = DefaultEndpoint,
                offersEndpoint = DefaultEndpoint,
                remoteConfigEndpoint = DefaultEndpoint,
                //maxDisplayLogEntries = 256,
                //displayLogFlushCadence = 60,
                //displayLogPath = Path.Combine(Application.persistentDataPath, "display_log"),
                eventsLogDispatchMaxQueueSize = 32,
                eventsLogDispatchCadence = 60,
                httpCacheTTLSeconds = 60,
                //offersCachePath = Path.Combine(Application.persistentDataPath, "metica-offers.json"),
                //remoteConfigCachePath = Path.Combine(Application.persistentDataPath, "metica-rc.json"),
                httpRequestTimeout = 2,
                logLevel = LogLevel.Error
            };
        }
    }
}
