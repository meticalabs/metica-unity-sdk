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
        public static string UserId { get; private set; }
        public static string AppId { get; private set; }
        public static string ApiKey { get; private set; }

        public static MeticaContext Context { get; set; }

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
            Context = new MeticaContext()
            {
                apiKey = ApiKey,
                appId = AppId,
                userId = UserId
            };

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
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.LogOfferDisplay(offerId, placementId);
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
            Debug.Log(userEvent);
            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            Debug.Log(logger);
            logger.LogEvent(userEvent);
        }
    }
}