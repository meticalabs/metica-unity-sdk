using System.Collections;
using System.Collections.Generic;
using Metica.Unity;
using Moq;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using UnityEngine.TestTools;
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
                callback.Invoke(SdkResultImpl<string>.WithResult("OK"));
            }
        }

        [SetUp]
        public void Setup()
        {
            MeticaAPI.Initialise(Utils.TestUserId, Utils.TestApp, Utils.TestKey, result => { Assert.That(result.Result); });
        }

        [UnityTest]
        public IEnumerator TestTheEventsSubmission()
        {
            var config = MeticaAPI.Config;
            config.eventsLogFlushCadence = 1;
            MeticaAPI.Config = config;
            
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "testSubmission";
            var eventData = new Dictionary<string, object> { { "userId", "testSubmission" } };

            for (int i = 0; i < 10; i++)
                logger.LogCustomEvent(eventType, eventData);
            

            TestOps testOps = new TestOps();
            MeticaAPI.BackendOperations = testOps;

            yield return new WaitForSecondsRealtime(config.eventsLogFlushCadence );
            
            Assert.That(testOps.NumEvents == 10);
            Assert.That(testOps.SubmitInvoked);
            Assert.That(logger.EventsQueue.Count == 0);
        }

        [Test]
        public void TestTheCommonAttributesOfTheCustomEvents()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "test";
            var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };

            logger.LogCustomEvent(eventType, eventData);

            var recordedEvent = logger.EventsQueue[0];
            Assert.That(recordedEvent["eventType"], Is.EqualTo("test"));
            Assert.That(recordedEvent["key2"], Is.EqualTo("value2"));
            assertCommonAttributes("test", recordedEvent);
        }

        [Test]
        public void TestTheAttributesOfOfferPurchase()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(Utils.testPlacementId))
                .Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferPurchase(Utils.testOfferId, Utils.testPlacementId, 1.0, "USD");
            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes("meticaOfferInAppPurchase", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(Utils.testPlacementId));
            Assert.That(meticaAttributes["totalAmount"], Is.EqualTo(1.0));
            Assert.That(meticaAttributes["currencyCode"], Is.EqualTo("USD"));
        }

        [Test]
        public void TestTheAttributesOfOfferDisplay()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(Utils.testPlacementId))
                .Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferDisplay(Utils.testOfferId, Utils.testPlacementId);

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes("meticaOfferImpression", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(Utils.testPlacementId));
        }


        [Test]
        public void TestTheAttributesOfOfferInteraction()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(Utils.testPlacementId))
                .Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;

            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferInteraction(Utils.testOfferId, Utils.testPlacementId, "click");

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes("meticaOfferInteraction", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(Utils.testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(Utils.testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(Utils.testPlacementId));
            Assert.That(meticaAttributes["interactionType"], Is.EqualTo("click"));
        }

        [Test]
        public void TestTheAttributesOfUserStateUpdate()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(Utils.testPlacementId))
                .Returns(new List<Offer>());

            MeticaAPI.OffersManager = offersManager.Object;

            var logger = new GameObject().AddComponent<EventsLogger>();
            var userAttributes = new Dictionary<string, object>()
            {
                { "name", "test" },
                { "score", 123 },
            };
            logger.LogUserAttributes(userAttributes);

            var recordedEvent = logger.EventsQueue[0];

            assertCommonAttributes("meticaUserStateUpdate", recordedEvent);

            var stateAttributes = (Dictionary<string, object>)recordedEvent["userStateAttributes"];
            Assert.That(userAttributes, Is.EqualTo(stateAttributes));
        }


        private static void assertCommonAttributes(string eventType, Dictionary<string, object> recordedEvent)
        {
            Assert.That(recordedEvent["eventType"], Is.EqualTo(eventType));
            Assert.That(recordedEvent["userId"], Is.EqualTo(Utils.TestUserId));
            Assert.That(recordedEvent["appId"], Is.EqualTo(Utils.TestApp));
            Assert.That(recordedEvent["meticaUnitSdk"], Is.EqualTo(MeticaAPI.SDKVersion));
            Assert.That(recordedEvent["eventTime"] != null);
        }

        [Test]
        public void TestTheLimitOfTheEnqueuedEventsBuffer()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            for (int i = 0; i < 1000; i++)
            {
                var eventType = "test";
                var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };

                logger.LogCustomEvent(eventType, eventData);
            }

            Assert.That(logger.EventsQueue.Count, Is.EqualTo(256));
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