using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local


namespace Metica.Unity
{
    internal abstract class EventTypes
    {
        internal static readonly string OfferImpression = "meticaOfferImpression";
        internal static readonly string OfferInteraction = "meticaOfferInteraction";
        internal static readonly string OfferInAppPurchase = "meticaOfferInAppPurchase";
        internal static readonly string UserStateUpdate = "meticaUserStateUpdate";
    }

    internal abstract class Constants
    {
        internal static readonly string MeticaAttributes = "meticaAttributes";
        internal static readonly string CurrencyCode = "currencyCode";
        internal static readonly string TotalAmount = "totalAmount";
        internal static readonly string PlacementId = "placementId";
        internal static readonly string OfferId = "offerId";
        internal static readonly string VariantId = "variantId";
        internal static readonly string BundleId = "bundleId";
        internal static readonly string UserId = "userId";
        internal static readonly string AppId = "appId";
        internal static readonly string EventType = "eventType";
        internal static readonly string EventId = "eventId";
        internal static readonly string EventTime = "eventTime";
        internal static readonly string MeticaUnitySdk = "meticaUnitySdk";
        internal static readonly string InteractionType = "interactionType";
        internal static readonly string UserStateAttributes = "userStateAttributes";
        internal static readonly string Offer = "offer";
        internal static readonly string DefaultLocale = "en-US";
        internal static readonly string DefaultAppVersion = "1.0.0";
    }

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
        public String eventType = EventTypes.OfferImpression;
        public String userId;
        public String appId;
        public MeticaAttributes meticaAttributes;
    }

    [Serializable]
    public class InteractionMetric
    {
        public String eventType = EventTypes.OfferInteraction;
        public String userId;
        public String appId;
        public MeticaAttributes meticaAttributes;
    }

    [Serializable]
    public class PurchaseMetric
    {
        public String eventType = EventTypes.OfferInAppPurchase;
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
        [CanBeNull] public Dictionary<string, object> customPayload;
        [CanBeNull] public string creativeId;
        [CanBeNull] public Dictionary<string, object> creativeOverride;
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