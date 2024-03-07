using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    internal class EventsLogger : MonoBehaviour
    {
        const UInt16 MaxPendingEventsCount = 256;
        
        // store the events in a list and flush them to the server every X minutes
        private LinkedList<Dictionary<string, object>> _eventsList = new();
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

        public void LogEvent(Dictionary<string, object> eventDetails)
        {
            _eventsList.AddFirst(eventDetails);
            if (_eventsList.Count > MaxPendingEventsCount)
            {
                _eventsList.RemoveLast();
            }
        }

        public void LogOfferDisplay(string offerId, string placementId)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferImpression");
            eventDict["meticaAttributes"] = CreateMeticaAttributes(offerId, placementId);
            LogEvent(eventDict);
        }


        public void LogOfferPurchase(string offerId, string placementId, double amount, string currency)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferInAppPurchase");
            eventDict["meticaAttributes"] = CreateMeticaAttributes(offerId, placementId);
            eventDict["currencyCode"] = currency;
            eventDict["totalAmount"] = amount;
            LogEvent(eventDict);
        }

        public void LogOfferInteraction(string offerId, string placementId, string interactionType)
        {
            var eventDict = CreateCommonEventAttributes("meticaOfferInteraction");
            eventDict["meticaAttributes"] = CreateMeticaAttributes(offerId, placementId);
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

        private void FlushEvents()
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
                { "userId", MeticaAPI.Context.userId },
                { "appId", MeticaAPI.Context.appId },
                { "eventType", eventType },
                { "eventTime", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "meticaUnitSdk", MeticaAPI.SDKVersion }
            };
        }

        // TODO: add missing variantId and bundleId
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