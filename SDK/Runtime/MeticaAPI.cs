using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

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
    public static class MeticaAPI
    {
        private static SdkInfo _sdkInfoCache = null;

        public static string SDKVersion
        {
            get {
                if (_sdkInfoCache == null)
                {
                    _sdkInfoCache = GetSdkInfo();
                }
                return _sdkInfoCache.Version;
            }
        }
        public static string UserId { get; set; }
        public static string AppId { get; internal set; }
        public static string ApiKey { get; internal set; }

        public static bool Initialized { get; private set; }

        public static SdkConfig Config { get; internal set; }
        
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

        public static void Initialise(SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback) => Initialise(sdkConfig.initialUserId, sdkConfig.appId, sdkConfig.apiKey, sdkConfig, initCallback);
        // TODO: deprecate the following method.
        public static void Initialise(string initialUserId, string appId, string apiKey, SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback)
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

        #region Install & Login Events

        public static void LogInstall(Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogInstall(customPayload);
        }

        /// <summary>
        /// Logs a login event with an optional current user id change.
        /// </summary>
        /// <param name="newCurrentUserId"></param>
        public static void LogLogin(string newCurrentUserId = null, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogLogin(newCurrentUserId, customPayload);
        }
        
        #endregion Install & Login Events

        #region Offer Impression

        /// <summary>
        /// Logs the display of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        public static void LogOfferDisplay(string offerId, string placementId, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId, customPayload);
        }

        /// <summary>
        /// Logs an offer impression event using a `productId` value instead of Metica information.
        /// </summary>
        /// <param name="productId">The id of the displayed product.</param>
        public static void LogOfferDisplayWithProductId(string productId, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplayWithProductId(productId, customPayload);
        }

       #endregion Offer Impression

        #region Offer Purchase

        /// <summary>
        /// Logs the purchase of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer being purchased.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        /// <param name="amount">The amount of the purchase.</param>
        /// <param name="currency">The currency of the purchase.</param>
        public static void LogOfferPurchase(string offerId, string placementId, double amount, string currency, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferPurchase(offerId, placementId, amount, currency, customPayload);
        }

        /// <summary>
        /// Logs an offer purchase event using a `productId` value instead of Metica information.
        /// </summary>
        /// <param name="productId">The id of the purchased product.</param>
        /// <param name="amount">The spent amount.</param>
        /// <param name="currency">The currency used for this purchase.</param>
        public static void LogOfferPurchaseWithProductId(string productId, double amount, string currency, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferPurchaseWithProductId(productId, amount, currency, customPayload);
        }

        #endregion Offer Purchase

        #region Offer Interaction

        /// <summary>
        /// Logs an offer interaction event.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        /// <param name="interactionType">The type of interaction.</param>
        public static void LogOfferInteraction(string offerId, string placementId, string interactionType, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferInteraction(offerId, placementId, interactionType, customPayload);
        }

        /// <summary>
        /// Logs an offer interaction event using a `productId` value instead of Metica information.
        /// </summary>
        /// <param name="productId">The id of the purchased product.</param>
        /// <param name="interactionType">The type of interaction performed by the user.</param>
        public static void LogOfferInteractionWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferInteractionWithProductId(productId, interactionType, customPayload);
        }

        #endregion Offer Interaction

        #region State Update

        /// <summary>
        /// Alias for <see cref="LogFullStateUpdate(Dictionary{string, object})"/>
        /// </summary>
        /// <param name="userAttributes">A mutable dictionary of user attribute identifiers and their new values.</param>
        [Obsolete("Please use LogFullStateUpdate")]
        public static void LogUserAttributes(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
            => LogFullStateUpdate(userAttributes, customPayload);
        /// <summary>
        /// Sends a complete snapshot of the user's state to the server, replacing any previously stored data. 
        /// This method fully resets the user's state on the server and expects all relevant state information to be included in the request.
        /// Any user attributes that are currently stored in the server with the given userId but are not sent with this update, will be erased.
        /// </summary>
        /// <param name="userAttributes">An exhaustive dictionary of user attribute identifiers and their new values.
        /// Please note that ALL user properties should be passed in the payload.</param>
        public static void LogFullStateUpdate(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogFullStateUpdate(userAttributes, customPayload);
        }

        /// <summary>
        /// Sends a partial update of the user's state to the server
        /// modifying or adding only the provided fields while preserving those that are currently stored on the server.
        /// This method cannot erase existing fields (like `LogFullStateUpdate` does); it can only overwrite values or introduce new ones.
        /// </summary>
        /// <param name="userAttributes">A dictionary of user attribute identifiers and their new values.</param>
        public static void LogPartialStateUpdate(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogPartialUserAttributes(userAttributes, customPayload);
        }

        #endregion State Update

        #region Ad Revenue

        /// <summary>
        /// An event triggered every time revenue is generated by an ad within the game.
        /// </summary>
        /// <param name="totalAmount">The revenue amount from the ad. i.e <code>0.99</code>, <code>1000</code></param>
        /// <param name="currencyCode">The currency code of the specified amount. Must be an <see href="https://en.wikipedia.org/wiki/ISO_4217">ISO 4217</see> string. i.e. <code>GBP</code>, <code>EUR</code></param>
        /// <param name="adPlacement">String that represents the place where the ad has been displayed inside the game, i.e. <code>shop_daily_reward</code></param>
        /// <param name="adPlacementType">Type of ad that has been displayed, i.e. <code>video</code>, <code>banner</code>, etc...</param>
        /// <param name="adPlacementSource">Source of the ad, i.e. <code>apploving</code>, <code>unity</code>, etc... </param>
        /// <remarks>
        /// Documentation: <see href="https://docs.metica.com/integration#adrevenue"/>
        /// </remarks>
        public static void LogAdRevenue(double totalAmount, string currencyCode, string adPlacement = null, string adPlacementType = null, string adPlacementSource = null, Dictionary<string, object> customPayload = null)
        {
            if (!checkPreconditions())
            {
                return;
            }
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogAdRevenue(totalAmount, currencyCode, adPlacement, adPlacementType, adPlacementSource, customPayload);
        }

        #endregion Ad Revenue

        #region Custom Event

        // ALIAS (will be promoted to main method call for custom events.
        public static void LogUserEvent(string eventType, Dictionary<string, object> userEvent, bool reuseDictionary) =>
            LogCustomEvent(eventType, userEvent);
        /// <summary>
        /// Logs a custom user event to the Metica API.
        /// </summary>
        /// <remarks>
        /// Do not use this method for logging any of the documented <see href="https://docs.metica.com/integration#core-events">Core Events</see>.
        /// If you do, a warning will be thrown. Specific methods are available for all of the listed Core event tpyes.
        /// </remarks>
        /// <param name="eventType">The name/type of the event</param>
        /// <param name="userEvent">A dictionary containing the details of the user event. The dictionary should have string keys and object values.</param>
        /// <param name="reuseDictionary">Indicates if the passed dictionary can be modified to add additional Metica-specific attribute. Re-using the dictionary instance in this way can potentially save an allocation.</param>
        public static void LogCustomEvent(string eventType, Dictionary<string, object> userEvent)
        {
            if (!checkPreconditions())
            {
                return;
            }

            if(Constants.ReservedEventNames.Contains(eventType))
            {
                MeticaLogger.LogWarning(() => $"A reserved name was used as {nameof(eventType)}. Core Events should be submitted via their specific methods, for example, for the 'purchase' {nameof(eventType)} relative to an offer, {nameof(LogOfferPurchase)} should be used.");
            }

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogCustomEvent(eventType, userEvent);
        }

        #endregion Custom Event

        #region SDK Info
        internal class SdkInfo
        {
            public string Version { get; set; }
        }

        /// <summary>
        /// Gets an SdkInfo file, obtained from the correspondent json file in StreamingAssets.
        /// </summary>
        /// <returns></returns>
        private static SdkInfo GetSdkInfo()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "sdkInfo.json");

            if (!File.Exists(filePath))
            {
#if UNITY_EDITOR
                //WriteJsonSdkInfo();
#else
                return new SdkInfo { Version = "unknown" };
#endif
            }

            string json = File.ReadAllText(filePath);
            SdkInfo sdkInfo = JsonConvert.DeserializeObject<SdkInfo>(json);
            return sdkInfo;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Just to force Unity to check the info file.
        /// </summary>
        [UnityEditor.InitializeOnLoadMethod]
        private static void TouchSdkInfo()
        {
            WriteJsonSdkInfo();
        }

        /// <summary>
        /// Get a package version by its name.
        /// </summary>
        /// <param name="packageName">Name of the package as it appears in the manifest file.</param>
        /// <returns>The version of the package if the package is found, null otherwise.</returns>
        private static string GetPackageVersion(string packageName)
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true); // true = include dependencies
            while (!listRequest.IsCompleted) { }

            if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    Debug.Log($"{package.name} : {package.version}");
                    if (package.name == packageName)
                    {
                        return package.version;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Editor utility to write an SdkInfo json file in Unity's StreamingAssets folder.
        /// If StreamingAssets doesn't exist, it will be created.
        /// </summary>
        private static void WriteJsonSdkInfo()
        {
            string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");

            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            string filePath = Path.Combine(streamingAssetsPath, "sdkInfo.json");

 
            string packageVersion = GetPackageVersion("com.metica.unity");
            if (packageVersion != null)
            {
                string jsonData = $"{{\"Version\": \"{packageVersion}\"}}";  // Ensure version is quoted for valid JSON
                File.WriteAllText(filePath, jsonData);
                Debug.Log($"SDK Info JSON written to: {filePath}");
                UnityEditor.AssetDatabase.Refresh(); 
            }
            else
            {
                Debug.LogError("Package version not found.");
            }
        }

#endif

        #endregion SDK Info

        private static bool checkPreconditions()
        {
            return UserId != null && AppId != null && ApiKey != null;
        }
    }
}