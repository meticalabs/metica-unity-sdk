using System;
using System.Collections.Generic;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    public delegate void EventsSubmissionResultDelegate(ISdkResult<Int32> result);

    internal interface ITimeSource
    {
        long EpochSeconds();
    }

    internal class SystemDateTimeSource : ITimeSource
    {
        public long EpochSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    /// <summary>
    /// The main class for interacting with the Metica API.
    /// </summary>
    public class MeticaAPI
    {
        public static string SDKVersion = "1.2.0";
        public static string UserId { get; internal set; }
        public static string AppId { get; internal set; }
        public static string ApiKey { get; internal set; }

        public static bool Initialized { get; private set; }

        public static SdkConfig Config { get; internal set; }
        
        internal static DisplayLog DisplayLog { get; set; }

        internal static IOffersManager OffersManager { get; set; }

        internal static IRemoteConfigManager RemoteConfigManager { get; set; }


        internal static ITimeSource TimeSource { get; set; }

        internal static IBackendOperations BackendOperations { get; set; }
        
        internal static OffersCache OffersCache { get; set; }
        
        internal static RemoteConfigCache RemoteConfigCache { get; set; }

        /// <summary>
        /// Initializes the Metica API.
        /// </summary>
        /// <param name="initialUserId">The user's ID.</param>
        /// <param name="appId">The application's ID. It must match the ID with which it was registered in the Metica platform</param>
        /// <param name="apiKey">The API key provided by Metica.</param>
        /// <param name="initCallback">The callback function to be invoked after initialization. If the initialisation was successful, then the result value will be true</param>
        [Obsolete("Please use any other overload of this method.")]
        public static void Initialise(string initialUserId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            Initialise(initialUserId, appId, apiKey, SdkConfig.Default(), initCallback);
        }

        public static void Initialise(string initialUserId, string appId, string apiKey, SdkConfig sdkConfig,
            MeticaSdkDelegate<bool> initCallback)
        {
            UserId = initialUserId;
            AppId = appId;
            ApiKey = apiKey;
            Config = sdkConfig;
            MeticaLogger.CurrentLogLevel = sdkConfig.logLevel;
            TimeSource = new SystemDateTimeSource();

            ScriptingObjects.Init();

            OffersManager = new OffersManager();
            RemoteConfigManager = new RemoteConfigManager();

            DisplayLog = ScriptingObjects.GetComponent<DisplayLog>();
            OffersCache = ScriptingObjects.GetComponent<OffersCache>();
            RemoteConfigCache = ScriptingObjects.GetComponent<RemoteConfigCache>();

            BackendOperations = new BackendOperationsImpl();

            Initialized = true;

            initCallback.Invoke(SdkResultImpl<bool>.WithResult(true));
        }

        /// <summary>
        /// Retrieves the application's remote config after personalising it for the current user.
        /// The configuration is specific to each application so it's represented very generically as
        /// a simple Dictionary with string keys.
        /// </summary>
        /// <param name="userProperties">Optional. A dictionary of user properties to be included in the request.</param>
        /// <param name="responseCallback">A callback function to handle the returned configuration.</param>
        /// <param name="deviceInfo">Optional. A DeviceInfo object containing device information to be included in the request.</param>
        /// <param name="configKeys">Optional. A list of configuration keys whose values' are going to be queried.
        /// If missing or empty, then the entire configuration is retrieved.</param>
        public static void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback,
            List<string> configKeys = null,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null
        )
        {
            if (!checkPreconditions())
            {
                responseCallback.Invoke(
                    SdkResultImpl<Dictionary<string, object>>.WithError(
                        "Required information missing. Please initialise the SDK first"));
            }
            else
            {
                RemoteConfigManager.GetConfig(configKeys, responseCallback, userProperties, deviceInfo);
            }
        }

        /// <summary>
        /// Retrieves offers from the Metica API for the specified placements.
        /// </summary>
        /// <param name="placements">An array of placement IDs for which offers should be retrieved.</param>
        /// <param name="offersCallback">A callback function to handle the retrieved offers.</param>
        /// <param name="userProperties">Optional. A dictionary of user properties to be included in the request.</param>
        /// <param name="deviceInfo">Optional. A DeviceInfo object containing device information to be included in the request.</param>
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            if (!checkPreconditions())
            {
                offersCallback.Invoke(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement()));
            }
            else
            {
                OffersManager.GetOffers(placements, offersCallback, userProperties, deviceInfo);
            }
        }

        /// <summary>
        /// Logs the display of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        public static void LogOfferDisplay(string offerId, string placementId)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId);

            DisplayLog.AppendDisplayLogs(new[]
            {
                DisplayLogEntry.Create(offerId, placementId)
            });
        }

        /// <summary>
        /// Logs the purchase of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer being purchased.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        /// <param name="amount">The amount of the purchase.</param>
        /// <param name="currency">The currency of the purchase.</param>
        public static void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferPurchase(offerId, placementId, amount, currency);
        }

        /// <summary>
        /// Logs an offer interaction event.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        /// <param name="interactionType">The type of interaction.</param>
        public static void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferInteraction(offerId, placementId, interactionType);
        }

        /// <summary>
        /// Log an update of the user attributes.
        /// </summary>
        /// <param name="userAttributes">A mutable dictionary of user attributes. The keys represent attribute names and the values represent attribute values.</param>
        public static void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogUserAttributes(userAttributes);
        }

        /// <summary>
        /// Logs a custom user event to the Metica API.
        /// </summary>
        /// <param name="eventType">The name/type of the event</param>
        /// <param name="userEvent">A dictionary containing the details of the user event. The dictionary should have string keys and object values.</param>
        /// <param name="reuseDictionary">Indicates if the passed dictionary can be modified to add additional Metica-specific attribute. Re-using the dictionary instance in this way can potentially save an allocation.</param>
        public static void LogUserEvent(string eventType, Dictionary<string, object> userEvent,
            bool reuseDictionary = false)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogCustomEvent(eventType, userEvent, reuseDictionary);
        }

        private static bool checkPreconditions()
        {
            return UserId != null && AppId != null && ApiKey != null;
        }
    }
}