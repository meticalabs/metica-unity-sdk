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
        /// TODO : complete
        /// </summary>
        /// <param name="responseCallback"></param>
        /// <param name="configKeys"></param>
        /// <param name="userProperties"></param>
        /// <param name="deviceInfo"></param>
        /// <remarks>
        /// 
        /// </remarks>
        public static void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await _sdk.GetConfigsAsync(configKeys = null, userProperties = null, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, object>>().WithResult(result.Configs));
            });
        }
        public static void GetConfig(MeticaSdkDelegate<ConfigResult> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await _sdk.GetConfigsAsync(configKeys = null, userProperties = null, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<ConfigResult>().WithResult(result));
            });
        }

        /// <summary>
        /// TODO : copmplete
        /// </summary>
        /// <param name="placements"></param>
        /// <param name="responseCallback"></param>
        /// <param name="userProperties"></param>
        /// <param name="deviceInfo"></param>
        /// <remarks>
        /// ## HOW TO UPGRADE
        /// The only difference for this call from version < 1.3.1 is the result type which is <code>Dictionary<string, List<Offer>></code> and not 'OffersByPlacement'.
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
                var result = await _sdk.GetOffersAsync(placements, userProperties, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, List<Offer>>>().WithResult(result.Placements));
            });
        }
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await _sdk.GetOffersAsync(placements, userProperties, deviceInfo);
                responseCallback?.Invoke(new SdkResultImpl<OfferResult>().WithResult(result));
            });
        }
        [Obsolete(@"Please use GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
                    or GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)")]
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OffersByPlacement> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Task.Run(async () =>
            {
                var result = await _sdk.GetOffersAsync(placements, userProperties, deviceInfo);
                var offersByPlacement = new OffersByPlacement();
                offersByPlacement.placements = result.Placements;
                responseCallback?.Invoke(new SdkResultImpl<OffersByPlacement>().WithResult(offersByPlacement)); // TODO
            });
        }


        public static void LogInstall(Dictionary<string, object> customPayload = null) => 
            _sdk.LogInstallEvent(customPayload);

        public static void LogLogin(string newCurrentUserId = null, Dictionary<string, object> customPayload = null) =>
            _sdk.LogLoginEvent(customPayload);

        public static void LogOfferDisplay(string offerId, string placementId, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferImpressionEvent(placementId, offerId, customPayload);

        public static void LogOfferDisplayWithProductId(string productId, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferImpressionEventWithProductId(productId, customPayload);

        public static void LogOfferPurchase(string offerId, string placementId, double totalAmount, string currencyCode, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferPurchaseEvent(placementId, offerId, currencyCode, totalAmount, customPayload);

        public static void LogOfferPurchaseWithProductId(string productId, double totalAmount, string currencyCode, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferPurchaseEventWithProductId(productId, currencyCode, totalAmount, customPayload);

        public static void LogOfferInteraction(string offerId, string placementId, string interactionType, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferInteractionEvent(placementId, offerId, interactionType, customPayload);

        public static void LogOfferInteractionWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null) =>
            _sdk.LogOfferInteractionEventWithProductId(productId, interactionType, customPayload);

        public static void LogAdRevenue(double totalAmount, string currencyCode, string adPlacement = null, string adPlacementType = null, string adPlacementSource = null, Dictionary<string, object> customPayload = null) =>
            _sdk.LogAdRevenueEvent(adPlacement, adPlacementType, adPlacementSource, currencyCode, totalAmount, customPayload);

        public static void LogFullStateUpdate(Dictionary<string, object> fullUserStateAttributes) =>
            _sdk.LogFullStateUserUpdateEvent(fullUserStateAttributes, null);

        public static void LogPartialStateUpdate(Dictionary<string, object> partialUserAttributes) =>
            _sdk.LogPartialStateUserUpdateEvent(partialUserAttributes, null);

        public static void LogCustomEvent(string eventType, Dictionary<string, object> userEvent) =>
            _sdk.LogCustomEvent(eventType, null);

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
