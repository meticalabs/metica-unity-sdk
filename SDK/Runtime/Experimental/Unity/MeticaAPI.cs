using Metica.Experimental.Core;
using Metica.Experimental.SDK;
using Metica.Experimental.SDK.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.Experimental.Unity
{
    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
    // Metica API for retro compatibility with previous SDK.
    // NOTE that this part will soon be removed or subject to changes.
    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-

    public static class MeticaAPI
    {
        // Reference to the SDK
        private static IMeticaSdk _sdk = null;
        private static IMeticaSdk SDK {
            get
            {
                if (!Initialized)
                {
                    _sdk = Registry.Resolve<IMeticaSdk>();
                    Initialized = true;
                }
                return _sdk;
            }
        }

        // Fields
        public static string SDKVersion { get; } = "0.0.0";
        public static string UserId { get => MeticaSdk.CurrentUserId; set => MeticaSdk.CurrentUserId = value; }
        public static string AppId { get => MeticaSdk.AppId; }
        public static string ApiKey { get => MeticaSdk.ApiKey; }
        public static bool Initialized { get; private set; }
        public static SdkConfig Config { get; }

        //Methods
        [Obsolete("Please use any other overload of this method.")]
        public static void Initialise(string initialUserId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
            => Initialise(initialUserId, appId, apiKey, SdkConfig.Default(), initCallback);

        public static void Initialise(SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback) => Initialise(sdkConfig.initialUserId, sdkConfig.appId, sdkConfig.apiKey, sdkConfig, initCallback);
        public static void Initialise(string initialUserId, string appId, string apiKey, SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback)
            => Log.Info(() => "There is no need to manually call 'Initialize'.");

        /// <summary>
        /// Gets all or the StartConfigs with the given keys.
        /// </summary>
        /// <param name="responseCallback">Callback method to process the result as a dictionary.</param>
        /// <param name="configKeys">List of keys of required SmartConfigs. Leave to null or empty to get all SmartConfigs.</param>
        /// <param name="userData">Real-time user state data to override pre-ingested user state attributes, conforms to userStateAttributes.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be automatically created to retrieve information about the device.</param>
        public static void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string> configKeys = null, Dictionary<string, object> userData = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await SDK.GetConfigsAsync(configKeys = null, userData = null, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, object>>().WithResult(result.Configs));
            });
        }
        /// <summary>
        /// Gets all or the StartConfigs with the given keys.
        /// </summary>
        /// <param name="responseCallback">Callback method to process the result as a <see cref="ConfigResult"/> object.</param>
        /// <param name="configKeys">List of keys of required SmartConfigs. Leave to null or empty to get all SmartConfigs.</param>
        /// <param name="userData">Real-time user state data to override pre-ingested user state attributes, conforms to userStateAttributes.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be automatically created to retrieve information about the device.</param>
        public static void GetConfig(MeticaSdkDelegate<ConfigResult> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await SDK.GetConfigsAsync(configKeys = null, userProperties = null, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<ConfigResult>().WithResult(result));
            });
        }

        /// <summary>
        /// Gets all or 
        /// </summary>
        /// <param name="placements"></param>
        /// <param name="responseCallback"></param>
        /// <param name="userProperties"></param>
        /// <param name="deviceInfo"></param>
        /// <remarks>
        /// Example call:
        /// <code>
        /// // Get all placements:
        /// MeticaAPI.GetOffers(null, MyOfferResultHandler, myUserProperties);
        /// </code>
        /// A handler example:
        /// <code>
        /// void MyOfferResultHandler(Dictionary<string, List<Offer>> placements)
        /// {
        ///     foreach(var pId in placements.Keys)
        ///     {
        ///         List<Offer> offers = placements[pId];
        ///         foreach(var offer in offers)
        ///             Log.Info(()=> $"{offer.offerId} :: {offer.currencyId}{offer.price}");
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static void GetOffers(String[] placements, MeticaSdkDelegate<Dictionary<string, List<Offer>>> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await SDK.GetOffersAsync(placements, userProperties, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, List<Offer>>>().WithResult(result.Placements));
            });
        }
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await SDK.GetOffersAsync(placements, userProperties, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<OfferResult>().WithResult(result));
            });
        }
        [Obsolete(@"Please use GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
                    or GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)")]
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await SDK.GetOffersAsync(placements, userProperties, deviceInfo);
                var offersByPlacement = new OffersByPlacement();
                offersByPlacement.placements = result.Placements;
                responseCallback?.Invoke(new SdkResultImpl<OffersByPlacement>().WithResult(offersByPlacement));
            });
        }


        public static void LogInstall(Dictionary<string, object> customPayload = null) => 
            SDK.LogInstallEvent(customPayload);

        public static void LogLogin(string newCurrentUserId = null, Dictionary<string, object> customPayload = null) =>
            SDK.LogLoginEvent(customPayload);

        public static void LogOfferDisplay(string offerId, string placementId, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferImpressionEvent(placementId, offerId, customPayload);

        public static void LogOfferDisplayWithProductId(string productId, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferImpressionEventWithProductId(productId, customPayload);

        public static void LogOfferPurchase(string offerId, string placementId, double totalAmount, string currencyCode, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferPurchaseEvent(placementId, offerId, currencyCode, totalAmount, customPayload);

        public static void LogOfferPurchaseWithProductId(string productId, double totalAmount, string currencyCode, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferPurchaseEventWithProductId(productId, currencyCode, totalAmount, customPayload);

        public static void LogOfferInteraction(string offerId, string placementId, string interactionType, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferInteractionEvent(placementId, offerId, interactionType, customPayload);

        public static void LogOfferInteractionWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null) =>
            SDK.LogOfferInteractionEventWithProductId(productId, interactionType, customPayload);

        public static void LogAdRevenue(double totalAmount, string currencyCode, string adPlacement = null, string adPlacementType = null, string adPlacementSource = null, Dictionary<string, object> customPayload = null) =>
            SDK.LogAdRevenueEvent(adPlacement, adPlacementType, adPlacementSource, currencyCode, totalAmount, customPayload);

        public static void LogFullStateUpdate(Dictionary<string, object> fullUserStateAttributes) =>
            SDK.LogFullStateUserUpdateEvent(fullUserStateAttributes, null);

        public static void LogPartialStateUpdate(Dictionary<string, object> partialUserAttributes) =>
            SDK.LogPartialStateUserUpdateEvent(partialUserAttributes, null);

        public static void LogCustomEvent(string eventType, Dictionary<string, object> userEvent) =>
            SDK.LogCustomEvent(eventType, null);

        [Obsolete("Please use LogCustomEvent")]
        public static void LogUserEvent(string eventType, Dictionary<string, object> userEvent, bool reuseDictionary) =>
            LogCustomEvent(eventType, userEvent);
        [Obsolete("Please use LogFullStateUpdate")]
        public static void LogUserAttributes(Dictionary<string, object> userAttributes) =>
            LogFullStateUpdate(userAttributes);


        // Related types from old SDK

        public delegate void MeticaSdkDelegate<T>(ISdkResult<T> result);

        /// <summary>
        /// Represents the result of an SDK operation.
        /// </summary>
        public interface ISdkResult<T>
        {
            /// <summary>
            /// The result of the operation, if any.
            /// </summary>
            T Result { get; }

            /// <summary>
            /// Contains an error code, if the operation failed. Will be null if the operation was successful.
            /// </summary>
            /// <value>The error string from the result. If no error occured value is null or empty.</value>
            string Error { get; }
        }

        public class SdkResultImpl<T> : ISdkResult<T>
        {
            public T Result { get; internal set; }

            public string Error { get; internal set; }

            public ISdkResult<T> WithResult(T result)
            {
                return new SdkResultImpl<T>()
                {
                    Result = result
                };
            }

            public ISdkResult<T> WithError(string error)
            {
                return new SdkResultImpl<T>()
                {
                    Error = error,
                };
            }

        }

        [Serializable, Obsolete("Please use 'OfferResult'")]
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

    }

}
