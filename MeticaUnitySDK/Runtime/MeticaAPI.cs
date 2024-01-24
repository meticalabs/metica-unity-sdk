using System;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local

namespace Metica.Unity
{
    public class MeticaAPI : ScriptableObject
    {
        private const string SDKVersion = "1.0.0";
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

        void Initialise(string userId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
        {
            MeticaAPI.UserId = userId;
            MeticaAPI.AppId = appId;
            MeticaAPI.ApiKey = apiKey;
            initCallback.Invoke(new SdkResultImpl<bool>(true));
        }

        void GetOffers(MeticaSdkDelegate<OffersByPlacement> offersCallback)
        {
        }
    }
}