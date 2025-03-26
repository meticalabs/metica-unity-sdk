using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metica.Experimental.SDK;
using Metica.Experimental.SDK.Model;

namespace Metica.Experimental.Unity
{
    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- //
    // Metica API for retro compatibility with previous SDK. //
    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- //

    /// <summary>
    /// # Metica API
    /// 
    /// ## How to upgrade to new SDK
    /// 
    /// If you're using a version of the SDK that is below 1.3.1 and you want to upgrade to 1.4.0
    /// you don't need to do much. In fact, this class, <see cref="MeticaAPI"/> should look familiar to you.
    /// This version of it is made for retro compatibility with the provious releases.
    /// 
    /// Where you find errors related to the type `OffersByPlacement`, add the following line at the top of the file:
    /// <code>
    /// using OffersByPlacement = Metica.Experimental.OfferResult;
    /// </code>
    /// Where you find errors related to the type `Log`, add the following line at the top of the file:
    /// <code>
    /// using MeticaLogger = Log;
    /// </code>
    /// That should be enough to keep you going with your project. However,
    /// note that there is a new way to use the SDK and it is recommended that you switch to it.
    /// 
    /// ## New way to access SDK methods
    /// 
    /// ### Initialization
    /// 
    /// Currently the initialization is done in Unity in the `MeticaUnitySdk`, linked to the `MeticaSdk` prefab,
    /// so make sure you have exactly one `MeticaSdk` prefab in your starting scene. This object will be marked
    /// as `DontDestroyOnLoad` so it will not be disposed of when a new scene is loaded.
    /// There is no need to call <see cref="Initialise(string, string, string, SdkConfig, MeticaAPI.MeticaSdkDelegate{bool})"/>
    /// (or other overload) anymore.
    /// 
    /// ### New way to use the SDK
    /// 
    /// 1. Get the sdk instance (after Unity's `Awake` phase):
    /// <code>
    /// IMeticaSdk _sdk = MeticaSdk.SDK;</IMeticaSdk>
    /// </code>
    /// 2. Call asynchronous methods:
    /// <code>
    /// var offerResult = await _sdk.GetOffersAsync(null);
    /// </code>
    /// </summary>
    public static class MeticaAPI
    {
        // Reference to the SDK
        private static IMeticaSdk _sdk = null;
        private static IMeticaSdk SDK { get => MeticaSdk.SDK; }

        // Fields
        public static string UserId { get => MeticaSdk.CurrentUserId; set => MeticaSdk.CurrentUserId = value; }
        public static string AppId { get => MeticaSdk.AppId; }
        public static string ApiKey { get => MeticaSdk.ApiKey; }
        public static bool Initialized { get; private set; } = true;
        public static SdkConfig Config { get; }

        //Methods
        [Obsolete("Please use any other overload of this method.")]
        public static void Initialise(string initialUserId, string appId, string apiKey, MeticaSdkDelegate<bool> initCallback)
            => Initialise(initialUserId, appId, apiKey, SdkConfig.Default(), initCallback);

        public static void Initialise(SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback) => Initialise(sdkConfig.initialUserId, sdkConfig.appId, sdkConfig.apiKey, sdkConfig, initCallback);
        public static void Initialise(string initialUserId, string appId, string apiKey, SdkConfig sdkConfig, MeticaSdkDelegate<bool> initCallback)
            => Log.Info(() => "There is no need to manually call 'Initialize'.");

        /// <summary>
        /// Utility method to run task in a coroutine
        /// </summary>
        /// <typeparam name="T">Return type of the task that's processed in the callback.</typeparam>
        /// <param name="asyncTaskFunc">Any <see cref="Func{TResult}"/> with <see cref="Task"/>&lt;<typeparamref name="T"/>&gt; as result.</param>
        /// <param name="responseCallback">The callback that processes the returned value.</param>
        /// <param name="cancellationToken">A cancellation token for task cancellation.</param>
        /// <returns><see cref="IEnumerator"/></returns>
        private static IEnumerator RunAsyncTaskCoroutine<T>(Func<Task<T>> asyncTaskFunc, MeticaSdkDelegate<T> responseCallback, CancellationToken cancellationToken = default)
        {
            var task = asyncTaskFunc();
            yield return task.Await(cancellationToken);
            if (task.IsCompletedSuccessfully == false)
            {
                if (task.IsCanceled)
                {
                    Log.Info(() => "Task was cancelled");
                }
                else
                {
                    Log.Error(() => $"A task didn't complete successfully in {nameof(RunAsyncTaskCoroutine)}");
                }
                yield break;
            }
            responseCallback?.Invoke(new SdkResultImpl<T>().WithResult(task.Result));
        }

        /// <summary>
        /// Gets all or the StartConfigs with the given keys.
        /// </summary>
        /// <param name="responseCallback">Callback method to process the result as a dictionary.</param>
        /// <param name="configKeys">List of keys of required SmartConfigs. Leave to null or empty to get all SmartConfigs.</param>
        /// <param name="userProperties">Real-time user state data to override pre-ingested user state attributes, conforms to userStateAttributes.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be automatically created to retrieve information about the device.</param>
        /// <remarks>
        /// ## ROADMAP
        /// - TODO : make this obsolete and use a response callback with <see cref="ConfigResult"/> as in
        /// <see cref="GetConfigAsConfigResult(MeticaSdkDelegate{ConfigResult}, List{string}, Dictionary{string, object}, DeviceInfo)"/>
        /// </remarks>
        public static void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            CoroutineRunner.Instance.RunCoroutine(GetConfigAsDictionaryCoroutine(responseCallback, configKeys, userProperties, deviceInfo));
        }
        /// <summary>
        /// Gets all or the StartConfigs with the given keys.
        /// </summary>
        /// <param name="responseCallback">Callback method to process the result as a <see cref="ConfigResult"/>.</param>
        /// <param name="configKeys">List of keys of required SmartConfigs. Leave to null or empty to get all SmartConfigs.</param>
        /// <param name="userProperties">Real-time user state data to override pre-ingested user state attributes, conforms to userStateAttributes.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be automatically created to retrieve information about the device.</param>
        /// <remarks>
        /// ## ROADMAP
        /// - TODO [BREAKING CHANGE] : This will be renamed back to GetConfig and will be the only way to get the result.
        /// - TODO : we'll likely introduce static async versions for some or all of the methods as alternatives to the current static void versions.
        /// </remarks>
        public static void GetConfigAsConfigResult(MeticaSdkDelegate<ConfigResult> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            CoroutineRunner.Instance.RunCoroutine(GetConfigAsConfigResultCoroutine(responseCallback, configKeys, userProperties, deviceInfo));
        }

        private static IEnumerator GetConfigAsDictionaryCoroutine(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            yield return RunAsyncTaskCoroutine(() => SDK.GetConfigsAsync(configKeys, userProperties, deviceInfo), (result) =>
            {
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, object>>().WithResult(result.Result.Configs));
            });
        }

        private static IEnumerator GetConfigAsConfigResultCoroutine(MeticaSdkDelegate<ConfigResult> responseCallback, List<string> configKeys = null, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            yield return RunAsyncTaskCoroutine(() => SDK.GetConfigsAsync(configKeys, userProperties, deviceInfo), (result) =>
            {
                responseCallback?.Invoke(new SdkResultImpl<ConfigResult>().WithResult(result.Result));
            });
        }

        /// <summary>
        /// Gets the placements with the given IDs. if placement is null or empty, all placements and offers will be fetched.
        /// </summary>
        /// <param name="placements">Ad array of placement IDs. If this field is null or empty, all placements and offers will be fetched.</param>
        /// <param name="responseCallback">A callback method that takes an <see cref="OfferResult"/> to process it when the data is received.</param>
        /// <param name="userProperties">Real-time user state data to override pre-ingested user state attributes, conforms to its userStateAttributes property.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be created automatically.</param>
        /// <seealso cref="GetOffers(string[], MeticaSdkDelegate{Dictionary{string, List{Offer}}}, Dictionary{string, object}, DeviceInfo)"/>
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
        public static void GetOffers(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            CoroutineRunner.Instance.RunCoroutine(GetOffersAsOfferResultCoroutine(placements, responseCallback, userProperties, deviceInfo));
        }
        /// <summary>
        /// Gets the placements with the given IDs. if placement is null or empty, all placements and offers will be fetched.
        /// </summary>
        /// <param name="placements">Ad array of placement IDs. If this field is null or empty, all placements and offers will be fetched.</param>
        /// <param name="responseCallback">A callback method that takes a <code>Dictionary<string, List<Offer>></code> to process it when the data is received.</param>
        /// <param name="userProperties">Real-time user state data to override pre-ingested user state attributes, conforms to its userStateAttributes property.</param>
        /// <param name="deviceInfo">A <see cref="DeviceInfo"/> object. If null, one will be created automatically.</param>
        /// <seealso cref="GetOffers(string[], MeticaSdkDelegate{OfferResult}, Dictionary{string, object}, DeviceInfo)"/>
        /// <remarks>
        /// Example call:
        /// <code>
        /// // Get all placements:
        /// MeticaAPI.GetOffers(null, MyOfferResultHandler, myUserProperties);
        /// </code>
        /// A handler example:
        /// <code>
        /// void MyOfferResultHandler(OfferResult offerResult)
        /// {
        ///     var placements = offerResult.Placements;
        ///     foreach(var pId in placements.Keys)
        ///     {
        ///         List<Offer> offers = placements[pId];
        ///         foreach(var offer in offers)
        ///             Log.Info(()=> $"{offer.offerId} :: {offer.currencyId}{offer.price}");
        ///     }
        /// }
        /// </code>
        /// ## ROADMAP
        /// 
        /// - TODO : This call will be removed in favour of <see cref="GetOffers(string[], MeticaSdkDelegate{OfferResult}, Dictionary{string, object}, DeviceInfo)"/>.
        /// </remarks>
        public static void GetOffersAsDictionary(String[] placements, MeticaSdkDelegate<Dictionary<string, List<Offer>>> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            CoroutineRunner.Instance.RunCoroutine(GetOffersAsDictionaryCoroutine(placements, responseCallback, userProperties, deviceInfo));
        }

        private static IEnumerator GetOffersAsOfferResultCoroutine(String[] placements, MeticaSdkDelegate<OfferResult> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            yield return RunAsyncTaskCoroutine(() => SDK.GetOffersAsync(placements, userProperties, deviceInfo), (result) =>
            {
                var offerResult = new OfferResult
                {
                    placements = result.Result.placements
                };
                responseCallback?.Invoke(new SdkResultImpl<OfferResult>().WithResult(offerResult));
            });
        }

        private static IEnumerator GetOffersAsDictionaryCoroutine(String[] placements, MeticaSdkDelegate<Dictionary<string, List<Offer>>> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            yield return RunAsyncTaskCoroutine(() => SDK.GetOffersAsync(placements, userProperties, deviceInfo), (result) =>
            {
                responseCallback?.Invoke(new SdkResultImpl<Dictionary<string, List<Offer>>>().WithResult(result.Result.placements));
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

        // =-=-=-=-=-=-=-=-=-=-=-=-=-= //
        // Related types from old SDK  //
        // =-=-=-=-=-=-=-=-=-=-=-=-=-= //

        [Obsolete]
        public delegate void MeticaSdkDelegate<T>(ISdkResult<T> result);

        [Obsolete]
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

        [Obsolete]
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
