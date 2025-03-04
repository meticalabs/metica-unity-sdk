using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metica.Unity
{
    public class EventsLogger : MonoBehaviour
    {
        // store the events in a list and flush them to the server every X minutes
        private LinkedList<Dictionary<string, object>> _eventsList = new();

        public List<Dictionary<string, object>> EventsQueue => _eventsList.ToList();

        private Coroutine _logEventsRoutine;

        #region Unity Lifecycle

        void Start()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                MeticaLogger.LogWarning(() => "EventsLogger is not supported in the editor");
                return;
            }

            _logEventsRoutine = StartCoroutine(LogEventsRoutine());
        }

        private void OnDestroy()
        {
            if (_logEventsRoutine != null)
                StopCoroutine(_logEventsRoutine);
        }

        private void OnApplicationQuit()
        {
            FlushEvents();
        }

        private void AppendCustomPayload(Dictionary<string, object> attributes, Dictionary<string, object> customAttributes)
        {
            if(customAttributes == null || attributes == null)
            {
                // TODO : warn!
                return;
            }
            if (!attributes.ContainsKey(Constants.CustomPayload))
            {
                attributes.Add(Constants.CustomPayload, customAttributes);
            }
        }

        #endregion Unity Lifecycle

        private void LogEvent(Dictionary<string, object> eventDetails, Dictionary<string, object> customPayload = null)
        {
            if (customPayload != null && customPayload.Count > 0)
            {
                AppendCustomPayload(eventDetails, customPayload);
            }
            _eventsList.AddFirst(eventDetails);
            if (_eventsList.Count > MeticaAPI.Config.maxPendingLoggedEvents)
            {
                _eventsList.RemoveLast();
            }
        }

        #region Install & Login Events

        public void LogInstall(Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.Install);
            LogEvent(attributes, customPayload);
        }

        public void LogLogin(string newCurrentUserId = null, Dictionary<string, object> customPayload = null)
        {
            if(newCurrentUserId !=  null)
            {
                MeticaAPI.UserId = newCurrentUserId;
            }
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.Login);
            LogEvent(attributes, customPayload);
        }

        #endregion Install & Login Events

        #region Offer Impression

        public void LogOfferDisplay(string offerId, string placementId, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferImpression);
            attributes[Constants.MeticaAttributes] = GetOrCreateMeticaAttributes(offerId, placementId);
            LogEvent(attributes, customPayload);
        }

        public void LogOfferDisplayWithProductId(string productId, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferImpression);
            attributes[Constants.ProductId] = productId;
            LogEvent(attributes, customPayload);
        }

        #endregion Offer Impression

        #region Offer Purchase

        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferInAppPurchase);
            attributes[Constants.CurrencyCode] = currency;
            attributes[Constants.TotalAmount] = amount;
            var meticaAttributes = GetOrCreateMeticaAttributes(offerId, placementId);
            attributes[Constants.MeticaAttributes] = meticaAttributes;
            LogEvent(attributes, customPayload);
        }

        public void LogOfferPurchaseWithProductId(string productId, double amount, string currency, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary <string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferInAppPurchase);
            attributes[Constants.CurrencyCode] = currency;
            attributes[Constants.TotalAmount] = amount;
            attributes[Constants.ProductId] = productId;
            LogEvent(attributes, customPayload);
        }

        #endregion Offer Purchase

        #region Offer Interaction

        public void LogOfferInteraction(string offerId, string placementId, string interactionType, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferInteraction);
            attributes[Constants.InteractionType] = interactionType;
            var meticaAttributes = GetOrCreateMeticaAttributes(offerId, placementId);
            attributes[Constants.MeticaAttributes] = meticaAttributes;
            LogEvent(attributes, customPayload);
        }

        public void LogOfferInteractionWithProductId(string productId, string interactionType, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.OfferInteraction);
            attributes[Constants.ProductId] = productId;
            attributes[Constants.InteractionType] = interactionType;
            LogEvent(attributes, customPayload);
        }

        #endregion Offer Interaction

        #region Ad Revenue

        public void LogAdRevenue(double amount, string currencyCode, string adPlacement = null, string adPlacementType = null, string adPlacementSource = null, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.AdRevenue);
            attributes[Constants.CurrencyCode] = currencyCode;
            attributes[Constants.TotalAmount] = amount;
            attributes[Constants.AdPlacement] = adPlacement;
            attributes[Constants.AdPlacementType] = adPlacementType;
            attributes[Constants.AdPlacementSource] = adPlacementSource;
            LogEvent(attributes, customPayload);
        }

        #endregion Ad Revenue

        #region State Update

        [Obsolete("Please use LogFullStateUpdate")]
        public void LogUserAttributes(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
            => LogFullStateUpdate(userAttributes, customPayload);

        public void LogFullStateUpdate(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.FullStateUpdate);
            attributes[Constants.UserStateAttributes] = userAttributes;
            LogEvent(attributes, customPayload);
        }

        // TODO: rename this method to reflect new API naming
        public void LogPartialUserAttributes(Dictionary<string, object> userAttributes, Dictionary<string, object> customPayload = null)
        {
            var attributes = new Dictionary<string, object>();
            AddCommonEventAttributes(attributes, EventTypes.PartialStateUpdate);
            attributes[Constants.UserStateAttributes] = userAttributes;
            LogEvent(attributes, customPayload);
        }

        #endregion State Update

        #region Custom Event

        [Obsolete("Please use LogCustomEvent(string eventType, Dictionary<string, object> eventDetails)")]
        public void LogCustomEvent(string eventType, Dictionary<string, object> eventDetails, bool reuseDictionary)
            => LogCustomEvent(eventType, eventDetails);

        public void LogCustomEvent(string eventType, Dictionary<string, object> eventDetails)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                MeticaLogger.LogError(() => "The event type must be specified");
                return;
            }

            var attributes = new Dictionary<string, object>(eventDetails);
            AddCommonEventAttributes(attributes, eventType);
            LogEvent(attributes, customPayload: null);
        }

        #endregion Custom Event

        private IEnumerator LogEventsRoutine()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(MeticaAPI.Config.eventsLogFlushCadence);

                FlushEvents();

                // Clear the list after logging
                _eventsList.Clear();
            }
        }

        public void FlushEvents()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                MeticaLogger.LogWarning(() => "No internet connection, events will be submitted later");
                MeticaAPI.Config.eventsSubmissionDelegate?.Invoke(SdkResultImpl<Int32>.WithError("No internet connect"));
                return;
            }

            var copyList = _eventsList;
            _eventsList = new LinkedList<Dictionary<string, object>>();

            if (copyList.Count == 0)
            {
                return;
            }

            MeticaAPI.BackendOperations.CallSubmitEventsAPI(copyList, (result) =>
            {
                if (result.Error != null)
                {
                    var message = $"Error while submitting events: {result.Error}";
                    MeticaLogger.LogError(() => message);
                    MeticaAPI.Config.eventsSubmissionDelegate?.Invoke(SdkResultImpl<Int32>.WithError(message));
                }
            });
        }

        private static void AddCommonEventAttributes(Dictionary<string, object> attributes, string eventType)
        {
            attributes[Constants.UserId] = MeticaAPI.UserId;
            attributes.Add(Constants.AppId, MeticaAPI.AppId);
            // attributes.Add(Constants.EventId, Ulid.NewUlid().ToString());
            attributes[Constants.EventId] = Guid.NewGuid().ToString();
            attributes[Constants.EventType] = eventType;
            attributes[Constants.EventTime] = MeticaAPI.TimeSource.EpochSeconds() * 1000;
            attributes[Constants.MeticaUnitySdk] = MeticaAPI.SDKVersion;
        }


        private static Dictionary<string, object> GetOrCreateMeticaAttributes(string offerId, string placementId)
        {
            var cachedOffers = MeticaAPI.OffersCache.Read(placementId);
            var offerDetails = cachedOffers?.Find(offer => offer.offerId == offerId);
            return (offerDetails == null)
                ? CreateMeticaAttributes(offerId, placementId)
                : CreateMeticaAttributes(offerDetails.metrics.display.meticaAttributes.offer.offerId,
                    offerDetails.metrics.display.meticaAttributes.placementId,
                    offerDetails.metrics.display.meticaAttributes.offer.variantId,
                    offerDetails.metrics.display.meticaAttributes.offer.bundleId
                );
        }

        private static Dictionary<string, object> CreateMeticaAttributes(string offerId,
            string placementId,
            string variantId = null,
            string bundleId = null)
        {
            return new Dictionary<string, object>()
            {
                {
                    "offer", new Dictionary<string, object>()
                    {
                        { Constants.OfferId, offerId },
                        { Constants.VariantId, variantId },
                        { Constants.BundleId, bundleId }
                    }
                },
                { Constants.PlacementId, placementId }
            };
        }
    }
}