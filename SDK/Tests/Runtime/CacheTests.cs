using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

using Metica.Unity;
using Metica.SDK.Caching;

using MeticaUnitySDK.SDK.Tests.Runtime;

public class CacheTests
{
    private OffersCache offersCache;
    private RemoteConfigCache remoteConfigCache;

    [SetUp]
    public void SetUp()
    {
        // Instantiate OffersCache and RemoteConfigCache with required parameters
        string name = "TestCache";
        string cacheFilePath = Path.GetTempFileName();
        int maxEntries = 100;
        offersCache = new OffersCache(name, cacheFilePath, maxEntries);
        remoteConfigCache = new RemoteConfigCache(name, cacheFilePath, maxEntries);

        var mockTimeSource = new DummyTimeSource();
        MeticaAPI.TimeSource = mockTimeSource;
    }

    [Test]
    public void OffersCache_WriteAndReadOffer()
    {
        // Arrange
        string key = "TestOfferKey";
        Offer testOffer = new Offer
        {
            offerId = "testOfferId",
            price = 9.99,
            metrics = new OfferMetrics(),
            items = new List<Item> { new Item() },
            expirationTime = "2024-01-01T00:00:00.000Z",
            customPayload = new Dictionary<string, object> { { "key", "value" } },
            creativeId = "testCreativeId",
            creativeOverride = new Dictionary<string, object> { { "key", "value" } },
            iap = "testIap",
            currencyId = "testCurrencyId",
            displayLimits = new List<DisplayLimit> { new DisplayLimit() }
        };
        List<Offer> testOffers = new List<Offer> { testOffer };

        // Act
        offersCache.Write(key, testOffers, 60);
        List<Offer> retrievedOffers = (List<Offer>)offersCache.Read(key);

        // Assert
        Assert.IsNotNull(retrievedOffers);
        Assert.AreEqual(testOffer.offerId, retrievedOffers[0].offerId);
        Assert.AreEqual(testOffer.price, retrievedOffers[0].price);
    }

    [Test]
    public void OffersCache_WriteAndReadOffer_ExpiredTTL()
    {
        // Arrange
        string key = "TestOfferKey";
        Offer testOffer = new Offer
        {
            offerId = "testOfferId",
            price = 9.99,
            metrics = new OfferMetrics(),
            items = new List<Item> { new Item() },
            expirationTime = "2024-01-01T00:00:00.000Z",
            customPayload = new Dictionary<string, object> { { "key", "value" } },
            creativeId = "testCreativeId",
            creativeOverride = new Dictionary<string, object> { { "key", "value" } },
            iap = "testIap",
            currencyId = "testCurrencyId",
            displayLimits = new List<DisplayLimit> { new DisplayLimit() }
        };
        List<Offer> testOffers = new List<Offer> { testOffer };

        // Act
        offersCache.Write(key, testOffers, -1); // expired TTL
        List<Offer> retrievedOffers = (List<Offer>)offersCache.Read(key);

        // Assert
        Assert.IsNull(retrievedOffers);
    }

    [Test]
    public void OffersCache_Clear()
    {
        // Arrange
        string key = "TestOfferKey";
        Offer testOffer = new Offer
        {
            offerId = "testOfferId",
            price = 9.99,
            metrics = new OfferMetrics(),
            items = new List<Item> { new Item() },
            expirationTime = "2024-01-01T00:00:00.000Z",
            customPayload = new Dictionary<string, object> { { "key", "value" } },
            creativeId = "testCreativeId",
            creativeOverride = new Dictionary<string, object> { { "key", "value" } },
            iap = "testIap",
            currencyId = "testCurrencyId",
            displayLimits = new List<DisplayLimit> { new DisplayLimit() }
        };
        List<Offer> testOffers = new List<Offer> { testOffer };

        // Act
        offersCache.Write(key, testOffers, 60);
        offersCache.Clear();
        List<Offer> retrievedOffers = (List<Offer>)offersCache.Read(key);

        // Assert
        Assert.IsNull(retrievedOffers);
    }

    [Test]
    public void RemoteConfigCache_WriteAndReadConfig_ExpiredTTL()
    {
        // Arrange
        string key = "TestConfigKey";
        object testConfig = new { foo = "bar" };

        // Act
        remoteConfigCache.Write(key, testConfig, -1); // expired TTL
        object retrievedConfig = remoteConfigCache.Read(key);

        // Assert
        Assert.IsNull(retrievedConfig);
    }

    // Example test for RemoteConfigCache
    [Test]
    public void RemoteConfigCache_WriteAndReadConfig()
    {
        // Arrange
        string key = "TestConfigKey";
        object testConfig = new { foo = "bar" };

        // Act
        remoteConfigCache.Write(key, testConfig, 60);
        object retrievedConfig = remoteConfigCache.Read(key);

        // Assert
        Assert.IsNotNull(retrievedConfig);
        Assert.AreEqual(testConfig, retrievedConfig);
    }

    // Example test for RemoteConfigCache Clear
    [Test]
    public void RemoteConfigCache_Clear()
    {
        // Arrange
        string key = "TestConfigKey";
        object testConfig = new { foo = "bar" };

        // Act
        remoteConfigCache.Write(key, testConfig, 60);
        remoteConfigCache.Clear();
        object retrievedConfig = remoteConfigCache.Read(key);

        // Assert
        Assert.IsNull(retrievedConfig);
    }
}