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
    public class SimpleDiskCacheTest
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
            var cache = new SimpleDiskCache<OffersByPlacement>("testCache", tempFileName);
            cache.Prepare();
            cache.Write("k1", new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    { "abc", new List<Offer>() }
                }
            }, 60_000);

            cache.Save();
            
            var newCache = new SimpleDiskCache<OffersByPlacement>("testCache", tempFileName);
            newCache.Prepare();
            
            Assert.That(newCache.Read("k1") != null);
            Assert.That(newCache.Read("k1")!.placements!["abc"]!.SequenceEqual(cache.Read("k1")!.placements!["abc"]));
        }

        [Test]
        public void TestRead()
        {
            var tempFileName = Path.GetTempFileName();
            var cache = new SimpleDiskCache<OffersByPlacement>("testCache", tempFileName);
            cache.Prepare();
            
            Assert.That(cache.Read("test") == null);

            cache.Write("test", new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    { "abc", new List<Offer>() }
                }
            }, 10_000);

            Assert.That(cache.Read("test") != null);
        }

        [Test]
        public void TestExpiration()
        {
            var tempFileName = Path.GetTempFileName();
            var cache = new SimpleDiskCache<OffersByPlacement>("testCache", tempFileName);
            cache.Prepare();

            var mockTimeSource = new Mock<ITimeSource>();
            MeticaAPI.TimeSource = mockTimeSource.Object;

            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(0);

            cache.Write("k", new OffersByPlacement()
            {
                placements = new Dictionary<string, List<Offer>>()
                {
                    { "abc", new List<Offer>() }
                }
            }, 10_000);

            Assert.That(cache.Read("k") != null);

            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(9_000);
            Assert.That(cache.Read("k") != null);

            mockTimeSource.Setup(t => t.EpochSeconds()).Returns(10_000);
            Assert.That(cache.Read("k") == null);
        }
    }
}