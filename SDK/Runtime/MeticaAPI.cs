using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    public struct MeticaSDKConfig
    {
        /// <summary>
        /// The full endpoint to the Metica offers endpoint.
        /// </summary>
        public string offersEndpoint;

        /// <summary>
        /// The full endpoint to the Metica ingestion service.
        /// </summary>
        public string ingestionEndpoint;

        /// <summary>
        /// The maximum number of entries stored in the displays log. 
        /// </summary>
        /// <remarks>
        /// This limit is shared by all offers. Once the limit is reached, then the oldest entries
        /// will be removed and replaced by the newly incoming ones.
        /// </remarks>
        public uint maxDisplayLogEntries;

        /// <summary>
        /// The cadence, in seconds, by which the displays log will be persisted to the filesystem.
        /// </summary>
        public uint displayLogFlushCadence;

        /// <summary>
        /// The cadence, in seconds, by which the logged events will be sent to the ingestion service.
        /// </summary>
        public uint eventsLogFlushCadence;

        /// <summary>
        /// The maximum number of pending logged events before they are sent to the ingestion service.
        /// </summary>
        /// <remarks>
        /// When the number of pending logged events reaches this maximum value, then the oldest accumulated events
        /// will be dropped to accomodate the most recent ones.
        /// </remarks>
        public uint maxPendingLoggedEvents;

        public static MeticaSDKConfig Default()
        {
            return new MeticaSDKConfig()
            {
                ingestionEndpoint = "https://api.prod-eu.metica.com",
                offersEndpoint = "https://api.prod-eu.metica.com",
                maxDisplayLogEntries = 256,
                maxPendingLoggedEvents = 256,
                displayLogFlushCadence = 60,
                eventsLogFlushCadence = 60
            };
        }
    }

    /// <summary>
    /// The main class for interacting with the Metica API.
    /// </summary>
    public class MeticaAPI : ScriptableObject
    {
        public static string SDKVersion = "1.0.0";
        public static string UserId { get; set; }
        public static string AppId { get; set; }
        public static string ApiKey { get; set; }

        public static bool Initialized { get; set; }

        public static DisplayLog DisplayLog { get; set; }

        public static IOffersManager OffersManager { get; set; }

        public static MeticaSDKConfig Config { get; private set; }

        /// <summary>
        /// Initializes the Metica API.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="appId">The application's ID. It must match the ID with which it was registered in the Metica platform</param>
        /// <param name="apiKey">The API key provided by Metica.</param>
        /// <param name="initCallback">The callback function to be invoked after initialization. If the initialisation was successful, then the result value will be true</param>
        public static void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            Initialise(userId, appId, apiKey, MeticaSDKConfig.Default(), initCallback);
        }

        public static void Initialise(string userId, string appId, string apiKey, MeticaSDKConfig sdkConfig,
            MeticaSdkDelegate<bool> initCallback)
        {
            UserId = userId;
            AppId = appId;
            ApiKey = apiKey;
            Config = sdkConfig;

            ScriptingObjects.Init();

            OffersManager = new OffersManager();
            OffersManager.Init();

            DisplayLog = new DisplayLog();
            DisplayLog.Init();

            Initialized = true;

            initCallback.Invoke(SdkResultImpl<bool>.WithResult(true));
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
            OffersManager.GetOffers(placements, offersCallback, userProperties, deviceInfo);
        }

        /// <summary>
        /// Logs the display of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement where the offer is displayed.</param>
        public static void LogOfferDisplay(string offerId, string placementId)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId);


            DisplayLog.AppendDisplayLogs(new[]
            {
                new DisplayLogEntry()
                {
                    displayedOn = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    offerId = offerId,
                    placementId = placementId,
                }
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
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferInteraction(offerId, placementId, interactionType);
        }

        /// <summary>
        /// Log an update of the user attributes.
        /// </summary>
        /// <param name="userAttributes">A dictionary of user attributes. The keys represent attribute names and the values represent attribute values.</param>
        public static void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogUserAttributes(userAttributes);
        }

        /// <summary>
        /// Logs a custom user event to the Metica API.
        /// </summary>
        /// <param name="eventType">The name/type of the event</param>
        /// <param name="userEvent">A dictionary containing the details of the user event. The dictionary should have string keys and object values.</param>
        public static void LogUserEvent(string eventType, Dictionary<string, object> userEvent)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogCustomEvent(eventType, userEvent);
        }
    }
}