using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
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

        private static OffersManager _offersManager;

        public static void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            UserId = userId;
            AppId = appId;
            ApiKey = apiKey;

            ScriptingObjects.Init();

            _offersManager = new OffersManager();
            _offersManager.Init();

            DisplayLog = new DisplayLog();
            DisplayLog.Init();
            
            Initialized = true;

            initCallback.Invoke(SdkResultImpl<bool>.WithResult(true));
        }

        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
            _offersManager.GetOffers(placements, offersCallback);
        }

        public static void LogOfferDisplay(string offerId, string placementId)
        {
            Debug.Log("UserId: " + UserId);
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId);

            
            DisplayLog.AppendDisplayLogs(new []{ new DisplayLogEntry()
            {
                displayedOn = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                offerId = offerId,
                placementId = placementId,
            } });
        }

        public static void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferPurchase(offerId, placementId, amount, currency);
        }

        public static void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferInteraction(offerId, placementId, interactionType);
        }

        public static void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogUserAttributes(userAttributes);
        }

        public static void LogUserEvent(Dictionary<string, object> userEvent)
        {
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogCustomEvent(userEvent);
        }
    }
}