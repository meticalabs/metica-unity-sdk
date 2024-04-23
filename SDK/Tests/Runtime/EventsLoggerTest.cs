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
        private const string TestUserId = "testUser";
        private const string TestApp = "testApp";
        private const string TestKey = "testKey";

        private const string testOfferId = "testOffer";
        private const string testBundleId = "testBundle";
        private const string testVariantId = "testVariant";
        private const string testPlacementId = "testPlacement";

        [SetUp]
        public void Setup()
        {
            MeticaAPI.Initialise(TestUserId, TestApp, TestKey, result => { Assert.That(result.Result); });
        }

        [Test]
        public void TestTheCommonAttributesOfTheCustomEvents()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "test";
            var eventData = new Dictionary<string, object> { { "userId", "rejected" }, { "key2", "value2" } };

            logger.LogCustomEvent(eventType, eventData);

            var recordedEvent = logger.EventsQueue[0];
            Assert.That( recordedEvent["eventType"], Is.EqualTo("test"));
            Assert.That( recordedEvent["key2"], Is.EqualTo("value2"));
            assertCommonAttributes("test", recordedEvent);
        }

        [Test]
        public void TestTheAttributesOfOfferPurchase()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(testPlacementId)).Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;
            
            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferPurchase(testOfferId, testPlacementId, 1.0, "USD");
            var recordedEvent = logger.EventsQueue[0];
            
            assertCommonAttributes("meticaOfferInAppPurchase", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(testPlacementId));
            Assert.That(meticaAttributes["totalAmount"], Is.EqualTo(1.0));
            Assert.That(meticaAttributes["currencyCode"], Is.EqualTo("USD"));
        }
        
        [Test]
        public void TestTheAttributesOfOfferDisplay()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(testPlacementId)).Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;
            
            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferDisplay(testOfferId, testPlacementId);
            
            var recordedEvent = logger.EventsQueue[0];
            
            assertCommonAttributes("meticaOfferImpression", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(testPlacementId));
        }

        
        [Test]
        public void TestTheAttributesOfOfferInteraction()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(testPlacementId)).Returns(createOfferCache());

            MeticaAPI.OffersManager = offersManager.Object;
            
            var logger = new GameObject().AddComponent<EventsLogger>();
            logger.LogOfferInteraction(testOfferId, testPlacementId, "click");
            
            var recordedEvent = logger.EventsQueue[0];
            
            assertCommonAttributes("meticaOfferInteraction", recordedEvent);

            var meticaAttributes = (Dictionary<string, object>)recordedEvent["meticaAttributes"];
            var offerDetails = (Dictionary<string, object>)meticaAttributes["offer"];
            Assert.That(offerDetails["bundleId"], Is.EqualTo(testBundleId));
            Assert.That(offerDetails["variantId"], Is.EqualTo(testVariantId));
            Assert.That(meticaAttributes["placementId"], Is.EqualTo(testPlacementId));
            Assert.That(meticaAttributes["interactionType"], Is.EqualTo("click"));
        }

        [Test]
        public void TestTheAttributesOfUserStateUpdate()
        {
            var offersManager = new Mock<IOffersManager>();
            offersManager.Setup(manager => manager.GetCachedOffersByPlacement(testPlacementId)).Returns(new List<Offer>());

            MeticaAPI.OffersManager = offersManager.Object;
            
            var logger = new GameObject().AddComponent<EventsLogger>();
            var userAttributes = new Dictionary<string, object>()
            {
                { "name", "test"},
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
            Assert.That(recordedEvent["userId"], Is.EqualTo(TestUserId));
            Assert.That(recordedEvent["appId"], Is.EqualTo(TestApp));
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
                                    offerId = testOfferId,
                                    bundleId = testBundleId,
                                    variantId = testVariantId
                                },
                                placementId = testPlacementId
                            }
                        }
                    }
                }
            };
        }
    }
}