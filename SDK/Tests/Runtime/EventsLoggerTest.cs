using System.Collections.Generic;
using Metica.Unity;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class EventsLoggerTest
    {
        private const string testUserId = "testUser";
        private const string testApp = "testApp";
        private const string testKey = "testKey";

        [SetUp]
        public void Setup()
        {
            MeticaAPI.Initialise(testUserId, testApp, testKey, result => { Assert.True(result.Result);});
        }
        
        [Test]
        public void TestTheCommonAttributesOfTheLoggedEvents()
        {
            var logger = new GameObject().AddComponent<EventsLogger>();

            var eventType = "test";
            var eventData = new Dictionary<string, object> {{"userId", "rejected"}, {"key2", "value2"}};

            logger.LogCustomEvent(eventType, eventData);

            var recordedEvent = logger.EventsQueue[0];
            Assert.AreEqual( "test", recordedEvent["eventType"]);
            Assert.AreEqual( testUserId, recordedEvent["userId"]);
            Assert.AreEqual( testApp, recordedEvent["appId"]);
            Assert.AreEqual( "value2", recordedEvent["key2"]);
            Assert.NotNull( recordedEvent["eventTime"]);
            Assert.AreEqual( MeticaAPI.SDKVersion, recordedEvent["meticaUnitSdk"]);
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

            Assert.AreEqual(256, logger.EventsQueue.Count);
        }
    }
}