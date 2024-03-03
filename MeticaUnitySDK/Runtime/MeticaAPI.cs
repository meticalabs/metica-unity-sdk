using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static MeticaContext Context { get; private set; }

        public static string MeticaOffersEndpoint
        {
            get { return meticaOffersEndpoint;}
            set { meticaOffersEndpoint = value; }
        }
        
        public static string MeticaIngestionEndpoint
        {
            get { return meticaIngestionEndpoint;}
            set { meticaIngestionEndpoint = value; }
        }

        public static bool Initialized { get; set; }

        private static ScriptingObjects _scriptingObjects;
        private static OffersManager _offersManager;
        
        public static void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            MeticaAPI.UserId = userId;
            MeticaAPI.AppId = appId;
            MeticaAPI.ApiKey = apiKey;
            MeticaAPI.Context = new MeticaContext()
            {
                apiKey = MeticaAPI.ApiKey,
                appId = MeticaAPI.AppId,
                userId = MeticaAPI.UserId
            };
            
            _scriptingObjects = new ScriptingObjects();
            _scriptingObjects.Init();
            
            _offersManager = new OffersManager();
            _offersManager.Init(MeticaAPI.Context);

            Initialized = true;
            
            initCallback.Invoke(SdkResultImpl<bool>.WithResult(true));
        }

        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
            _offersManager.GetOffers(placements, offersCallback);
        }

        public void LogOfferDisplay(string offerId, string placementId)
        {
            
        }
        
        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            
        }

        public void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            
        }

        public void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            
        }
        
        public void LogUserEvent(Dictionary<string, object> userEvent)
        {
            
            // BackendOperations.CallSubmitEventsAPI(MeticaAPI.Context, new ArrayList() userEvent, result => { });
        }
    }
}