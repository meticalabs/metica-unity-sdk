// ReSharper disable once RedundantUsingDirective

using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using Metica.Unity;
using Moq;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class EventsLoggerTest
    {
        private class TestOps : IBackendOperations
        {
            public bool SubmitInvoked { get; private set; }
            public int NumEvents { get; private set; }
            public bool FailOp { get; set; }

            public void CallGetOffersAPI(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
                Dictionary<string, object> userProperties = null,
                DeviceInfo deviceInfo = null)
            {
                throw new AssertionException("Should not be called");
            }

            public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
                MeticaSdkDelegate<string> callback)
            {
                SubmitInvoked = true;
                NumEvents += events.Count;

                callback.Invoke(!FailOp
                    ? SdkResultImpl<string>.WithResult("OK")
                    : SdkResultImpl<string>.WithError("Test error"));
            }

            public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback, Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
            {
                throw new AssertionException("Should not be called");
            }

            public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<Dictionary<string, object>> responseCallback, Dictionary<string, object> userProperties = null,
                DeviceInfo deviceInfo = null)
            {
                throw new AssertionException("Should not be called");
            }
        }

        [SetUp]
        public void Setup()
        {
            Utils.InitSdk();
        }

        [UnityTest]
        public IEnumerator TestTheEventsSubmission()
        {
            var errorCallbackCalled = false;
            var config = MeticaAPI.Config;
            config.eventsLogFlushCadence = 1;
            config.eventsSubmissionDelegate = result => errorCallbackCalled = true;
            MeticaAPI.Config = config;

            TestOps testOps = new TestOps();
            MeticaAPI.BackendOperations = testOps;

            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "testSubmission";
            var eventData = new Dictionary<string, object> { { "userId", "testSubmission" } };

            for (int i = 0; i < 10; i++)
                logger.LogCustomEvent(eventType, eventData, false);

            yield return new WaitForSecondsRealtime(config.eventsLogFlushCadence);
            Assert.That(testOps.NumEvents == 10);
            Assert.That(testOps.SubmitInvoked);
            Assert.That(logger.EventsQueue.Count == 0);
            
            testOps = new TestOps
            {
                FailOp = true
            };
            MeticaAPI.BackendOperations = testOps;
            MeticaLogger.CurrentLogLevel = LogLevel.Off;

            logger.LogCustomEvent(eventType, eventData, false);
            yield return new WaitForSecondsRealtime(config.eventsLogFlushCadence);

            Assert.That(errorCallbackCalled);
        }

        [Test]
        public void TestTheCommonAttributesOfTheCustomEvents()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "test";
            var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };

            logger.LogCustomEvent(eventType, eventData, false);

            var recordedEvent = logger.EventsQueue[0];
            Assert.That(recordedEvent[Constants.EventType], Is.EqualTo(eventType));
            Assert.That(recordedEvent["key2"], Is.EqualTo("value2"));
            assertCommonAttributes("test", recordedEvent);
        }

        [Test]
        public void TestTheAttributesOfOfferPurchase()
        {
            MeticaAPI.OffersCache.Write(Utils.testPlacementId, createOfferCache());

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferPurchase(Utils.testOfferId, Utils.testPlacementId, 1.0, "USD");
            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes(EventTypes.OfferInAppPurchase, recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent[Constants.MeticaAttributes];
            var offerDetails = (Dictionary<string, object>)meticaAttributes[Constants.Offer];
            Assert.That(recordedEvent[Constants.TotalAmount], Is.EqualTo(1.0));
            Assert.That(recordedEvent[Constants.CurrencyCode], Is.EqualTo("USD"));
            Assert.That(offerDetails[Constants.BundleId], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails[Constants.VariantId], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes[Constants.PlacementId], Is.EqualTo(Utils.testPlacementId));
        }

        [Test]
        public void TestTheAttributesOfOfferDisplay()
        {
            MeticaAPI.OffersCache.Write(Utils.testPlacementId, createOfferCache());

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferDisplay(Utils.testOfferId, Utils.testPlacementId);

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes(EventTypes.OfferImpression, recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent[Constants.MeticaAttributes];
            var offerDetails = (Dictionary<string, object>)meticaAttributes[Constants.Offer];
            Assert.That(offerDetails[Constants.BundleId], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails[Constants.VariantId], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes[Constants.PlacementId], Is.EqualTo(Utils.testPlacementId));
        }


        [Test]
        public void TestTheAttributesOfOfferInteraction()
        {
            MeticaAPI.OffersCache.Write(Utils.testPlacementId, createOfferCache());

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferInteraction(Utils.testOfferId, Utils.testPlacementId, "click");

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes(EventTypes.OfferInteraction, recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent[Constants.MeticaAttributes];
            var offerDetails = (Dictionary<string, object>)meticaAttributes[Constants.Offer];
            Assert.That(recordedEvent[Constants.InteractionType], Is.EqualTo("click"));
            Assert.That(offerDetails[Constants.BundleId], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails[Constants.VariantId], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes[Constants.PlacementId], Is.EqualTo(Utils.testPlacementId));
        }

        // TODO: add test for partial state update and rename the following to TestTheAttributesOfFullStateUpdate
        [Test]
        public void TestTheAttributesOfUserStateUpdate()
        {
            MeticaAPI.OffersCache.Write(Utils.testPlacementId, new List<Offer>());

            var logger = new GameObject().AddComponent<EventsLogger>();
            var userAttributes = new Dictionary<string, object>()
            {
                { "name", "test" },
                { "score", 123 },
            };
            logger.LogUserAttributes(userAttributes);

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes(EventTypes.UserStateUpdate, recordedEvent);

            var stateAttributes = (Dictionary<string, object>)recordedEvent[Constants.UserStateAttributes];
            Assert.That(userAttributes, Is.EqualTo(stateAttributes));
        }

        [Test]
        public void TestTheLimitOfTheEnqueuedEventsBuffer()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            for (int i = 0; i < 1000; i++)
            {
                var eventType = "test";
                var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };

                logger.LogCustomEvent(eventType, eventData, false);
            }

            Assert.That(logger.EventsQueue.Count, Is.EqualTo(256));
        }

        [Test]
        public void TestTheReuseOfTheDictionaryInstance()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "test";
            var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };
            var copyDict = new Dictionary<string, object>(eventData);

            logger.LogCustomEvent(eventType, eventData, false);

            Assert.That(copyDict, Is.EqualTo(eventData));

            logger.LogCustomEvent(eventType, eventData, true);
            Assert.That(copyDict, Is.Not.EqualTo(eventData));
        }

        private static void assertCommonAttributes(string eventType, Dictionary<string, object> recordedEvent)
        {
            Assert.That(recordedEvent[Constants.EventId], Is.Not.Null);
            Assert.That(recordedEvent[Constants.EventType], Is.EqualTo(eventType));
            Assert.That(recordedEvent[Constants.UserId], Is.EqualTo(Utils.TestUserId));
            Assert.That(recordedEvent[Constants.AppId], Is.EqualTo(Utils.TestApp));
            Assert.That(recordedEvent[Constants.MeticaUnitySdk], Is.EqualTo(MeticaAPI.SDKVersion));
            Assert.That(recordedEvent[Constants.EventTime] != null);
        }


        private List<Offer> createOfferCache()
        {
            return new List<Offer>()
            {
                new()
                {
                    offerId = "testOffer",
                    metrics = new OfferMetrics()
                    {
                        display = new DisplayMetric()
                        {
                            meticaAttributes = new MeticaAttributes()
                            {
                                offer = new OfferVariant()
                                {
                                    offerId = Utils.testOfferId,
                                    bundleId = Utils.testBundleId,
                                    variantId = Utils.testVariantId
                                },
                                placementId = Utils.testPlacementId
                            }
                        }
                    }
                }
            };
        }
    }
}