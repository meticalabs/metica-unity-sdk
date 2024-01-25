using System;
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

        public bool Initialized { get; set; }

        private OffersManager _offersManager;
        
        void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            MeticaAPI.UserId = userId;
            MeticaAPI.AppId = appId;
            MeticaAPI.ApiKey = apiKey;
            this._offersManager = new OffersManager();
            initCallback.Invoke(new SdkResultImpl<bool>(true));
        }

        void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
            this._offersManager.GetOffers(placements, offersCallback);
        }
    }
}