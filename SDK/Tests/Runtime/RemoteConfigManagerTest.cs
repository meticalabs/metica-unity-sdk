using System.Collections.Generic;
using Metica.Unity;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class RemoteConfigManagerTest
    {
        private class TestOps : IBackendOperations
        {
            public RemoteConfig MockRemoteConfig { get; set; }
            public bool ShouldBeInvoked { get; set; }
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
                throw new AssertionException("Should not be called");
            }

            public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback,
                Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
            {
                if (!ShouldBeInvoked) throw new AssertionException("Should not be called");
                responseCallback.Invoke(!FailOp
                    ? SdkResultImpl<RemoteConfig>.WithResult(MockRemoteConfig)
                    : SdkResultImpl<RemoteConfig>.WithError("Test error"));
            }
        }

        [SetUp]
        public void Setup()
        {
            Utils.InitSdk();
        }

        [Test]
        public void TestCaching()
        {
            TestOps testOps = new TestOps();
            MeticaAPI.BackendOperations = testOps;
            RemoteConfigManager remoteConfigManager = new RemoteConfigManager();

            testOps.ShouldBeInvoked = true;
            testOps.MockRemoteConfig = new RemoteConfig(
                config: new Dictionary<string, object>()
                {
                    { "testKey1", "testValue" },
                    { "testKey2", "testValue2" }
                },
                cacheDurationSecs: 100);

            var mockTimeSource = new DummyTimeSource();
            mockTimeSource.SetValue(0);

            MeticaAPI.TimeSource = mockTimeSource;

            // first call to populate the cache
            remoteConfigManager.GetConfig(new List<string>() { "testKey1", "testKey2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("testValue", result.Result["testKey1"]);
                Assert.AreEqual("testValue2", result.Result["testKey2"]);
            });

            // testKey1 and testKey2 should be served from the cache
            mockTimeSource.SetValue(90);
            testOps.ShouldBeInvoked = false;
            remoteConfigManager.GetConfig(new List<string>() { "testKey1", "testKey2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("testValue", result.Result["testKey1"]);
                Assert.AreEqual("testValue2", result.Result["testKey2"]);
            });
            
            // testKey1 and testKey2 should be served from the cache while testKey3 will result in a call
            mockTimeSource.SetValue(99);
            testOps.ShouldBeInvoked = true;
            testOps.MockRemoteConfig = new RemoteConfig(
                config: new Dictionary<string, object>()
                {
                    { "testKey3", "testValue3" },
                },
                cacheDurationSecs: 100);
            remoteConfigManager.GetConfig(new List<string>() { "testKey1", "testKey2", "testKey3" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("testValue", result.Result["testKey1"]);
                Assert.AreEqual("testValue2", result.Result["testKey2"]);
                Assert.AreEqual("testValue3", result.Result["testKey3"]);
            });
            
            // testKey1 and testKey2 should have expired from the cache and result in a call
            mockTimeSource.SetValue(100);
            testOps.MockRemoteConfig = new RemoteConfig(
                config: new Dictionary<string, object>()
                {
                    { "testKey1", "testValue" },
                    { "testKey2", "testValue2" }
                },
                cacheDurationSecs: 100);
            testOps.ShouldBeInvoked = true;
            remoteConfigManager.GetConfig(new List<string>() { "testKey1", "testKey2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("testValue", result.Result["testKey1"]);
                Assert.AreEqual("testValue2", result.Result["testKey2"]);
            });
        }
    }
}