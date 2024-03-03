using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local


namespace Metica.Unity
{
    using Event = List<Dictionary<string, object>>;
    
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
        public Dictionary<string, List<Offer>> placements = new Dictionary<string, List<Offer>>();
    }
    
    [Serializable]
    public class DisplayLimit
    {
        public ulong timeWindowInHours;
        public ulong maxDisplayCount;
    }


    [Serializable]
    public class OfferVariant
    {
        public string offerId;
        public string bundleId;
        public string variantId;
    }
    
    [Serializable]
    public class MeticaAttributes
    {
        public OfferVariant offer;
        public String placementId;

    }
  
    [Serializable]
    public class DisplayMetric
    {
        public String eventType = "meticaOfferImpression";
        public String userId;
        public String appId;
        public MeticaAttributes meticaAttributes;
    }
    
    [Serializable]
    public class InteractionMetric
    {
        public String eventType = "meticaOfferInteraction";
        public String userId;
        public String appId;
        public MeticaAttributes meticaAttributes;
    }
    
    [Serializable]
    public class PurchaseMetric
    {
        public String eventType = "meticaOfferInAppPurchase";
        public String userId;
        public String appId;
        public MeticaAttributes meticaAttributes;
    }
    
    [Serializable]
    public class OfferMetrics
    {
        public DisplayMetric display;
        public PurchaseMetric purchase;
        public InteractionMetric interaction;
    }
    
    [Serializable]
    public class Offer
    {
        public string offerId;
        public double? price;
        public OfferMetrics metrics;
        public List<Item> items;
        [CanBeNull] public string expirationTime;
        [CanBeNull] public string customPayload;
        [CanBeNull] public string creativeId;
        [CanBeNull] public string creativeOverride;
        [CanBeNull] public string iap;
        [CanBeNull] public string currencyId;
        [CanBeNull] public List<DisplayLimit> displayLimits;
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