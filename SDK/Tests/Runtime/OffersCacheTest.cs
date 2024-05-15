// ReSharper disable once RedundantUsingDirective
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metica.Unity;
using Moq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class OffersCacheTest
    {
        [SetUp]
        public void Setup()
        {
            Utils.InitSdk();
        }
        
        [Test]
        public void TestLoadingCacheFile()
        {
            var tempFileName = Path.GetTempFileName();
            Utils.ConfigureOffersCacheLogPath(tempFileName);

            var cache = new OffersCache();
            cache.Write(new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    {"abc", new List<Offer>()}
                }
            });
            
            var newCache = new OffersCache();
            Assert.That(newCache.Read() != null);
            Assert.That(newCache.Read()!.placements!["abc"]!.SequenceEqual(cache.Read()!.placements!["abc"]));
        }
        
        [Test]
        public void TestRead()
        {
            var tempFileName = Path.GetTempFileName();
            Utils.ConfigureOffersCacheLogPath(tempFileName);

            var cache = new OffersCache();
            Assert.That(cache.Read() == null);
            
            cache.Write(new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    {"abc", new List<Offer>()}
                }
            });
            
            Assert.That(cache.Read() != null);
        }
        
        [Test]
        public void TestExpiration()
        {
            var tempFileName = Path.GetTempFileName();
            Utils.ConfigureOffersCacheLogPath(tempFileName);

            var mockTimeSource = new Mock<ITimeSource>();
            MeticaAPI.TimeSource = mockTimeSource.Object;
            
            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(0);
            
            var cache = new OffersCache();
            
            cache.Write(new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    {"abc", new List<Offer>()}
                }
            });
            
            Assert.That(cache.Read() != null);
            
            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(MeticaAPI.Config.offersCacheTtlMinutes*60 - 1);
            Assert.That(cache.Read() != null);
            
            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(MeticaAPI.Config.offersCacheTtlMinutes*60 + 1);
            Assert.That(cache.Read() == null);
        }
    }
}