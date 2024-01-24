using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local


namespace Metica.Unity
{
    [Serializable]
    public struct MeticaContext
    {
        public string appId;
        public string apiKey;
        public string userId;
    }

    [Serializable]
    public enum StoreTypeEnum
    {
        AppStore,
        GooglePlayStore
    }

    [Serializable]
    public class DeviceInfo
    {
        public string store;
        public string timezone;
        public string locale;
        public string appVersion;
    }


    [Serializable]
    public class OffersByPlacement
    {
        public Dictionary<string, List<Offer>> placements;
    }

    [Serializable]
    public class Offer
    {
        public string offerId;
        public double? price;
        public Dictionary<string, object> metrics;
        public List<Item> items;
        [CanBeNull] public string expirationTime;
        [CanBeNull] public string customPayload;
        [CanBeNull] public string creativeId;
        [CanBeNull] public string creativeOverride;
        [CanBeNull] public string iap;
        [CanBeNull] public string currencyId;
    }

    [Serializable]
    public class Item
    {
        public string id;
        public double quantity;
    }

    [Serializable]
    public class OffersList
    {
        public Offer[] offers;
    }
}