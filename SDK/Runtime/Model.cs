#nullable enable
using System;
using System.Collections.Generic;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local


namespace Metica.Unity
{
    internal abstract class EventTypes
    {
        internal static readonly string Install = "install";
        internal static readonly string Login = "login";
        internal static readonly string OfferImpression = "impression";
        internal static readonly string OfferInteraction = "interaction";
        internal static readonly string OfferInAppPurchase = "purchase";
        [Obsolete("Use EventTypes.FullStateUpdate")]
        internal static readonly string UserStateUpdate = "fullStateUpdate";
        internal static readonly string FullStateUpdate = "fullStateUpdate";
        internal static readonly string PartialStateUpdate = "partialStateUpdate";
        internal static readonly string AdRevenue = "adRevenue";
    }

    internal abstract class Constants
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
        internal static readonly string EventType = "eventType";
        internal static readonly string EventId = "eventId";
        internal static readonly string EventTime = "eventTime";
        internal static readonly string MeticaUnitySdk = "meticaUnitySdk";
        internal static readonly string InteractionType = "interactionType";
        internal static readonly string UserStateAttributes = "userStateAttributes";
        internal static readonly string Offer = "offer";
        internal static readonly string DefaultLocale = "en-US";
        internal static readonly string DefaultAppVersion = "1.0.0";

        internal static readonly string[] ReservedEventNames = new string[] {
            "purchase", "impression", "interaction", "adRevenue", "fullStateUpdate", "partialStateUpdate", "login", "install"
        };
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
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("OffersByPlacement:");
            foreach (var placement in placements.Keys)
            {
                sb.AppendLine($"Placement: {placement}");
                foreach (var offer in placements[placement])
                {
                    sb.AppendLine($"\tOffer: {offer.offerId}");
                    sb.AppendLine($"\t\t{offer.iap}");
                }
            }
            return sb.ToString();
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


    [Serializable]
    internal class RemoteConfig
    {
        public Dictionary<string, object> Config;
        public long CacheDurationSecs;

        public RemoteConfig(Dictionary<string, object> config, long cacheDurationSecs)
        {
            Config = config;
            CacheDurationSecs = cacheDurationSecs;
        }
    }
}