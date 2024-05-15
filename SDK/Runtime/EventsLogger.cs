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

        void Start()
        {
            if (Application.isEditor && !Application.isPlaying) 
            {
                Debug.LogWarning("EventsLogger is not supported in the editor");
                return;
            }

            _logEventsRoutine = StartCoroutine(LogEventsRoutine());
        }

        ~EventsLogger()
        {
            if (_logEventsRoutine != null)
                StopCoroutine(_logEventsRoutine);
        }
        
        private void OnApplicationQuit()
        {
            FlushEvents();
        }

        private void LogEvent(Dictionary<string, object> eventDetails)
        {
            _eventsList.AddFirst(eventDetails);
            if (_eventsList.Count > MeticaAPI.Config.maxPendingLoggedEvents)
            {
                _eventsList.RemoveLast();
            }
        }

        public void LogCustomEvent(string eventType, Dictionary<string, object> eventDetails)
        {
            if (eventType == null)
            {
                Debug.LogError("The event type must be specified");
                return;
            }

            var commonDict = CreateCommonEventAttributes(eventType);
            var merged = commonDict
                .Concat(
                    eventDetails.Where(it => !commonDict.ContainsKey(it.Key))
                )
                .ToDictionary(it => it.Key, it => it.Value);

            LogEvent(merged);
        }

        public void LogOfferDisplay(string offerId, string placementId)
        {
            var eventDict = CreateCommonEventAttributes(EventTypes.OfferImpression);
            eventDict[Constants.MeticaAttributes] = GetOrCreateMeticaAttributes(offerId, placementId);
            LogEvent(eventDict);
        }

        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            var eventDict = CreateCommonEventAttributes(EventTypes.OfferInAppPurchase);
            var meticaAttributes = GetOrCreateMeticaAttributes(offerId, placementId);
            meticaAttributes[Constants.CurrencyCode] = currency;
            meticaAttributes[Constants.TotalAmount] = amount;
            eventDict[Constants.MeticaAttributes] = meticaAttributes;
            LogEvent(eventDict);
        }

        public void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            var eventDict = CreateCommonEventAttributes(EventTypes.OfferInteraction);
            var meticaAttributes = GetOrCreateMeticaAttributes(offerId, placementId);
            meticaAttributes[Constants.InteractionType] = interactionType;
            eventDict[Constants.MeticaAttributes] = meticaAttributes;
            LogEvent(eventDict);
        }

        public void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            var eventDict = CreateCommonEventAttributes(EventTypes.UserStateUpdate);
            eventDict[Constants.UserStateAttributes] = userAttributes;
            LogEvent(eventDict);
        }

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
                Debug.LogWarning("No internet connection, events will be submitted later");
                return;
            }

            var copyList = _eventsList;
            _eventsList = new LinkedList<Dictionary<string, object>>();
            MeticaAPI.BackendOperations.CallSubmitEventsAPI(copyList, (result) =>
            {
                if (result.Error != null)
                {
                    Debug.LogError($"Error while submitting events: {result.Error}");
                }
                else
                {
                    Debug.Log("Events submitted successfully");
                }
            });
        }

        private static Dictionary<string, object> CreateCommonEventAttributes(string eventType)
        {
            return new Dictionary<string, object>()
            {
                { Constants.UserId, MeticaAPI.UserId },
                { Constants.AppId, MeticaAPI.AppId },
                { Constants.EventId, Guid.NewGuid().ToString() },
                { Constants.EventType, eventType },
                { Constants.EventTime, DateTimeOffset.FromUnixTimeSeconds(MeticaAPI.TimeSource.EpochSeconds()).ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { Constants.MeticaUnitySdk, MeticaAPI.SDKVersion }
            };
        }


        private static Dictionary<string, object> GetOrCreateMeticaAttributes(string offerId, string placementId)
        {
            var cachedOffers = MeticaAPI.OffersManager.GetCachedOffersByPlacement(placementId);
            var offerDetails = cachedOffers.Find(offer => offer.offerId == offerId);
            return (offerDetails == null)
                ? CreateMeticaAttributes(offerId, placementId)
                : CopyMetricsFromOfferDetails(offerDetails);
        }

        private static Dictionary<string, object> CopyMetricsFromOfferDetails(Offer offerDetails)
        {
            return new Dictionary<string, object>()
            {
                {
                    "offer", new Dictionary<string, object>()
                    {
                        { Constants.OfferId, offerDetails.metrics.display.meticaAttributes.offer.offerId },
                        { Constants.VariantId, offerDetails.metrics.display.meticaAttributes.offer.variantId },
                        { Constants.BundleId, offerDetails.metrics.display.meticaAttributes.offer.bundleId }
                    }
                },
                { Constants.PlacementId, offerDetails.metrics.display.meticaAttributes.placementId }
            };
        }

        private static Dictionary<string, object> CreateMeticaAttributes(
            string offerId,
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