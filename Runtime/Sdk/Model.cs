using System;
using System.Collections.Generic;

namespace Metica.Model
{
    internal static class EventTypes
    {
        internal static readonly string Install = "install";
        internal static readonly string Login = "login";
        internal static readonly string OfferImpression = "impression";
        internal static readonly string OfferInteraction = "interaction";
        internal static readonly string OfferPurchase = "purchase";
        internal static readonly string FullStateUpdate = "fullStateUpdate";
        internal static readonly string PartialStateUpdate = "partialStateUpdate";
        internal static readonly string AdRevenue = "adRevenue";
        // When adding new EventTypes, make sure they are added to IsEventType below.

        /// <summary>
        /// Checks if a string is one of the 
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        internal static bool IsEventType(string eventType)
        {
            return eventType == Install ||
                   eventType == Login ||
                   eventType == OfferImpression ||
                   eventType == OfferInteraction ||
                   eventType == OfferPurchase ||
                   eventType == FullStateUpdate ||
                   eventType == PartialStateUpdate ||
                   eventType == AdRevenue;
        }
    }

    internal static class FieldNames
    {
        internal static readonly string MeticaAttributes = "meticaAttributes";
        internal static readonly string CustomPayload = "customPayload";
        internal static readonly string CurrencyCode = "currencyCode";
        internal static readonly string TotalAmount = "totalAmount";
        internal static readonly string PlacementId = "placementId";
        internal static readonly string AdPlacement = "placement";
        internal static readonly string AdPlacementType = "type";
        internal static readonly string AdPlacementSource = "source";
        internal static readonly string OfferId = "offerId";
        internal static readonly string ProductId = "productId";
        internal static readonly string VariantId = "variantId";
        internal static readonly string BundleId = "bundleId";
        internal static readonly string UserId = "userId";
        internal static readonly string AppId = "appId";
        internal static readonly string ConfigKeys = "configKeys";
        internal static readonly string EventType = "eventType";
        internal static readonly string EventId = "eventId";
        internal static readonly string EventTime = "eventTime";
        internal static readonly string MeticaUnitySdk = "meticaUnitySdk";
        internal static readonly string InteractionType = "interactionType";
        internal static readonly string UserStateAttributes = "userStateAttributes";
        internal static readonly string UserData = "userData";
        internal static readonly string Offer = "offer";
        internal static readonly string DeviceInfo = "deviceInfo";
    }

    internal static class Defaults
    {
        internal static readonly string DefaultLocale = "en-US";
        internal static readonly string DefaultAppVersion = "0.0.0";
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

    /// <summary>
    /// Device information.
    /// </summary>
    [Serializable]
    public class DeviceInfo
    {
        public string store;
        public string timezone;
        public string locale;
        public string appVersion;

        public override string ToString()
        {
            return $"Store: {store}\nTimezone: {timezone}\nLocale: {locale}\nAppVersion: {appVersion}";
        }
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
        public String eventType = EventTypes.OfferPurchase;
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
    public class Item
    {
        public string id;
        public double quantity;
    }

    [Serializable]
    public class Offer
    {
        public string offerId;
        public double? price;
        public OfferMetrics metrics;
        public List<Item> items;
        public string? expirationTime;
        public Dictionary<string, object>? customPayload;
        public string? creativeId;
        public Dictionary<string, object>? creativeOverride;
        public string? iap;
        public string? currencyId;
        public List<DisplayLimit>? displayLimits;

        public object GetMeticaAttributesObject()
        {
            return new
            {
                offerId =   metrics.display.meticaAttributes.offer.offerId,
                variantId = metrics.display.meticaAttributes.offer.variantId,
                bundleId = metrics.display.meticaAttributes.offer.bundleId,
            };
        }
    }

}
