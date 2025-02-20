using System.Collections.Generic;
using Metica.Unity;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class OffersManagerTest
    {
        private class TestOps : IBackendOperations
        {
            public OffersByPlacement MockOffers { get; set; }
            public bool ShouldBeInvoked { get; set; }
            public bool FailOp { get; set; }
            public bool WasInvoked { get; set; }

            public void CallGetOffersAPI(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
                Dictionary<string, object> userProperties = null,
                DeviceInfo deviceInfo = null)
            {
                if (!ShouldBeInvoked) throw new AssertionException("Should not be called");
                WasInvoked = true;
                offersCallback.Invoke(!FailOp
                    ? SdkResultImpl<OffersByPlacement>.WithResult(MockOffers)
                    : SdkResultImpl<OffersByPlacement>.WithError("Test error"));
            }

            public void CallSubmitEventsAPI(ICollection<Dictionary<string, object>> events,
                MeticaSdkDelegate<string> callback)
            {
            }

            public void CallRemoteConfigAPI(string[] configKeys, MeticaSdkDelegate<RemoteConfig> responseCallback,
                Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
            {
                throw new AssertionException("Should not be called");
            }
        }

        [SetUp]
        public void Setup()
        {
            Utils.InitSdk();
        }

        [Test]
        public void TestCachedOffers()
        {
            TestOps testOps = new TestOps
            {
                FailOp = false,
                ShouldBeInvoked = true
            };
            testOps.MockOffers = new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    { "p1", new List<Offer> {
                        new()
                        {
                            offerId = "offerP1",
                        }
                    }},
                    { "p2", new List<Offer> {
                        new()
                        {
                            offerId = "offerP2",
                        }
                    }}
                }
            };

            MeticaAPI.BackendOperations = testOps;
            var offersManager = new OffersManager();

            // populates the cache
            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP1", result.Result.placements["p1"][0].offerId);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
            });

            testOps.ShouldBeInvoked = false;
            testOps.WasInvoked = false;
            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP1", result.Result.placements["p1"][0].offerId);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
            });
            
            testOps.ShouldBeInvoked = false;
            testOps.WasInvoked = false;
            offersManager.GetOffers(new[] { "p1" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP1", result.Result.placements["p1"][0].offerId);
            });
            
            testOps.ShouldBeInvoked = false;
            testOps.WasInvoked = false;
            offersManager.GetOffers(new[] { "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
            });
        }

        [Test]
        public void TestDisplayLogApplication()
        {
            TestOps testOps = new TestOps
            {
                FailOp = false,
                ShouldBeInvoked = true
            };
            testOps.MockOffers = new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    { "p1", new List<Offer> {
                        new()
                        {
                            offerId = "offerP1",
                            displayLimits = new List<DisplayLimit>
                            {
                                new()
                                {
                                    timeWindowInHours = 24, maxDisplayCount = 2
                                }
                            },
                            metrics = new OfferMetrics()
                            {
                                display = new DisplayMetric()
                                {
                                    userId = MeticaAPI.UserId,
                                    appId = MeticaAPI.AppId,
                                    meticaAttributes = new MeticaAttributes()
                                    {
                                        offer = new OfferVariant()
                                        {
                                            offerId = "offerP1",
                                            bundleId = "bundleP1",
                                            variantId = "variantP1",
                                        }
                                    }
                                }
                            }
                        }
                    }},
                    { "p2", new List<Offer> {
                        new()
                        {
                            offerId = "offerP2",
                        }
                    }}
                }
            };

            MeticaAPI.BackendOperations = testOps;
            var offersManager = new OffersManager();

            // fetches the offers once
            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP1", result.Result.placements["p1"][0].offerId);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
                
                // Assert.That(MeticaAPI.DisplayLog.GetEntriesForOffer("offerP1").Count == 1);
                // // because it has no display limits specified
                // Assert.That(MeticaAPI.DisplayLog.GetEntriesForOffer("offerP2").Count == 0);
            });
            
            testOps.ShouldBeInvoked = false;
            testOps.WasInvoked = false;
            
            // fetches again, this time from the cache
            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual("offerP1", result.Result.placements["p1"][0].offerId);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
            });
            
            // fetches one more time, again from the cache
            // offerP1 should not be returned at it reached the limit. offerP1 should be returned
            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.Null(result.Error);
                Assert.NotNull(result.Result);
                Assert.AreEqual(0, result.Result.placements["p1"].Count);
                Assert.AreEqual("offerP2", result.Result.placements["p2"][0].offerId);
            });
        }

        [Test]
        public void TestErrorsAreReported()
        {
            LogAssert.ignoreFailingMessages = true;
            TestOps testOps = new TestOps
            {
                FailOp = true,
                ShouldBeInvoked = true
            };

            MeticaAPI.BackendOperations = testOps;
            var offersManager = new OffersManager();

            offersManager.GetOffers(new[] { "p1", "p2" }, result =>
            {
                Assert.That(testOps.WasInvoked, Is.True);
                Assert.NotNull(result.Error);
            });
        }
    }
}