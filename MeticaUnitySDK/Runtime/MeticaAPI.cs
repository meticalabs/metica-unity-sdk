using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    /// <summary>
    /// The main class for interacting with the Metica API.
    /// </summary>
    public class MeticaAPI : ScriptableObject
    {
        public static string SDKVersion = "1.0.0";
        private static string meticaOffersEndpoint = "http://localhost:9090";
        private static string meticaIngestionEndpoint = "http://localhost:8080";
        public static string UserId { get; internal set; }
        public static string AppId { get; internal set; }
        public static string ApiKey { get; internal set; }

        public static string MeticaOffersEndpoint
        {
            get { return meticaOffersEndpoint; }
            set { meticaOffersEndpoint = value; }
        }

        public static string MeticaIngestionEndpoint
        {
            get { return meticaIngestionEndpoint; }
            set { meticaIngestionEndpoint = value; }
        }

        public static bool Initialized { get; set; }
        
        internal static DisplayLog DisplayLog { get; set; }

        internal static OffersManager OffersManager{ get; set; }

        /// <summary>
        /// Initializes the Metica API.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="appId">The app's ID.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="initCallback">The callback function to be invoked after initialization.</param>
        public static void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            UserId = userId;
            AppId = appId;
            ApiKey = apiKey;

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
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            OffersManager.GetOffers(placements, offersCallback, userProperties, deviceInfo);
        }

        /// <summary>
        /// Logs the display of an offer.
        /// </summary>
        /// <param name="offerId">The ID of the offer.</param>
        /// <param name="placementId">The ID of the placement.</param>
        public static void LogOfferDisplay(string offerId, string placementId)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId);

            
            DisplayLog.AppendDisplayLogs(new []{ new DisplayLogEntry()
            {
                displayedOn = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                offerId = offerId,
                placementId = placementId,
            } });
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
        /// <param name="placementId">The ID of the placement.</param>
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
        /// <param name="userEvent">A dictionary containing the details of the user event. The dictionary should have string keys and object values.</param>
        public static void LogUserEvent(Dictionary<string, object> userEvent)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogCustomEvent(userEvent);
        }
    }
}