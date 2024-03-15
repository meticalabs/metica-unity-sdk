using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metica.Unity
{
    internal class EventsLogger : MonoBehaviour
    {
        const UInt16 MaxPendingEventsCount = 256;

        // store the events in a list and flush them to the server every X minutes
        private LinkedList<Dictionary<string, object>> _eventsList = new();

        public List<Dictionary<string, object>> EventsQueue => _eventsList.ToList();

        private Coroutine _logEventsRoutine;
        public float LogInterval { get; set; } = 60;

        void Start()
        {
            if (Application.isEditor)
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

        private void LogEvent(Dictionary<string, object> eventDetails)
        {
            _eventsList.AddFirst(eventDetails);
            if (_eventsList.Count > MaxPendingEventsCount)
            {
                _eventsList.RemoveLast();
            }
        }

        public void LogCustomEvent(Dictionary<string, object> eventDetails)
        {
            if (!eventDetails.ContainsKey("eventType"))
            {
                Debug.LogError("The event must contain an eventType key");
                return;
            }

            if (eventDetails["eventType"] is not string)
            {
                Debug.LogError("The eventType attribute must be a string");
                return;
            }

            var eventDict = CreateCommonEventAttributes(eventDetails["eventType"] as string);
            LogEvent(eventDict);
        }

        public void LogOfferDisplay(string offerId, string placementId)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferImpression");
            eventDict["meticaAttributes"] = GetOrCreateMeticaAttributes(offerId, placementId);
            LogEvent(eventDict);
        }


        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferInAppPurchase");
            eventDict["meticaAttributes"] = GetOrCreateMeticaAttributes(offerId, placementId);
            eventDict["currencyCode"] = currency;
            eventDict["totalAmount"] = amount;
            LogEvent(eventDict);
        }

        public void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferInteraction");
            eventDict["meticaAttributes"] = GetOrCreateMeticaAttributes(offerId, placementId);
            eventDict["interactionType"] = interactionType;
            LogEvent(eventDict);
        }

        public void LogUserAttributes(Dictionary<string, object> userAttributes)
        {
            var eventDict = CreateCommonEventAttributes("meticaUserStateUpdate");
            eventDict["userStateAttributes"] = userAttributes;
            LogEvent(eventDict);
        }

        private IEnumerator LogEventsRoutine()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(LogInterval);

                FlushEvents();

                // Clear the list after logging
                _eventsList.Clear();
            }
        }

        internal void FlushEvents()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("No internet connection, events will be submitted later");
                return;
            }

            var copyList = new List<Dictionary<string, object>>(_eventsList);
            _eventsList = new LinkedList<Dictionary<string, object>>();
            BackendOperations.CallSubmitEventsAPI(copyList, (result) =>
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

        private Dictionary<string, object> CreateCommonEventAttributes(string eventType)
        {
            return new Dictionary<string, object>()
            {
                { "userId", MeticaAPI.UserId },
                { "appId", MeticaAPI.AppId },
                { "eventType", eventType },
                { "eventTime", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "meticaUnitSdk", MeticaAPI.SDKVersion }
            };
        }


        private Dictionary<string, object> GetOrCreateMeticaAttributes(string offerId, string placementId)
        {
            var cachedOffers = MeticaAPI.OffersManager.GetCachedOffersByPlacement(placementId);
            var offerDetails = cachedOffers.Find(offer => offer.offerId == offerId);
            return (offerDetails == null)
                ? CreateMeticaAttributes(offerId, placementId)
                : CopyMetricsFromOfferDetails(offerDetails);
        }

        private Dictionary<string, object> CopyMetricsFromOfferDetails(Offer offerDetails)
        {
            return new Dictionary<string, object>()
            {
                {
                    "offer", new Dictionary<string, object>()
                    {
                        { "offerId", offerDetails.metrics.display.meticaAttributes.offer.offerId },
                        { "variantId", offerDetails.metrics.display.meticaAttributes.offer.variantId },
                        { "bundleId", offerDetails.metrics.display.meticaAttributes.offer.bundleId }
                    }
                },
                { "placementId", offerDetails.metrics.display.meticaAttributes.placementId }
            };
        }

        private Dictionary<string, object> CreateMeticaAttributes(
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
                        { "offerId", offerId },
                        { "variantId", variantId },
                        { "bundleId", bundleId }
                    }
                },
                { "placementId", placementId }
            };
        }
    }
}