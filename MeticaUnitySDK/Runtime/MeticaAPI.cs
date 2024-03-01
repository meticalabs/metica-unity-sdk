using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    public class MeticaAPI : ScriptableObject
    {
        private static string SDKVersion = "1.0.0";
        private static string meticaOffersEndpoint = "http://localhost:9090";
        public static string UserId { get; private set; }
        public static string AppId { get; private set; }
        public static string ApiKey { get; private set; }

        public static string MeticaOffersEndpoint
        {
            get { return meticaOffersEndpoint;}
            set { meticaOffersEndpoint = value; }
        }

        public static bool Initialized { get; set; }

        private static ScriptingObjects _scriptingObjects;
        private static OffersManager _offersManager;
        
        public static void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            MeticaAPI.UserId = userId;
            MeticaAPI.AppId = appId;
            MeticaAPI.ApiKey = apiKey;
            
            _scriptingObjects = new ScriptingObjects();
            _scriptingObjects.Init();
            
            _offersManager = new OffersManager();
            _offersManager.Init(new MeticaContext()
            {
                apiKey = MeticaAPI.ApiKey,
                appId = MeticaAPI.AppId,
                userId = MeticaAPI.UserId
            });

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
    }
}