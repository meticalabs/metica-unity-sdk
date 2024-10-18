using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metica.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using UnityEngine.TestTools;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class OffersE2eTest
    {
        String endpoint = Environment.GetEnvironmentVariable("ODS_ENDPOINT");
        String apiKey = Environment.GetEnvironmentVariable("E2E_TESTSAPP_API_KEY");
        String appId = "e2eTestsApp";

        [UnityTest]
        public IEnumerator Get_Offers()
        {
            var config = SdkConfig.Default();
            config.offersEndpoint = endpoint;
            config.networkTimeout = 5;

            string userId = Utils.RandomUserId();

            MeticaAPI.Initialise(userId, appId, apiKey, config, result => Assert.That(result.Result));
            MeticaLogger.CurrentLogLevel = LogLevel.Info;

            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();

            yield return new WaitForSeconds(3);

            OffersByPlacement offers = null;
            MeticaAPI.GetOffers(new[] { "mainMulti" }, result =>
            {
                Assert.That(result.Error == null);
                offers = result.Result;
            }, new Dictionary<string, object>()
            {
                { "lowPriorityOfferEligible", true },
                { "highPriorityOfferEligible", true }
            }, new DeviceInfo()
            {
                appVersion = "1.0.0",
                timezone = "UTC",
                locale = "en_US",
                store = "AppStore"
            });

            yield return new WaitForSeconds(3);

            var offersPlacement = offers.placements["mainMulti"];
            var lowP = offersPlacement.Find(o => o.offerId == "12205");

            Assert.That(lowP != null);
            Assert.That(lowP!.items.Count() == 2);
            Assert.That(lowP!.expirationTime != null);
            Assert.That(lowP!.price == 1.0);
            Assert.That(lowP!.metrics != null);
            Assert.That(lowP!.currencyId == "gem");
            Assert.That(lowP!.iap == null);
            Assert.That(lowP!.customPayload != null);
            Assert.That(lowP!.creativeId != null);

            var highP = offersPlacement.Find(o => o.offerId == "12206");

            Assert.That(highP != null);
            Assert.That(highP!.items.Count() == 2);
            Assert.That(highP!.expirationTime != null);
            Assert.That(highP!.price == 1.0);
            Assert.That(highP!.metrics != null);
            Assert.That(highP!.metrics.purchase != null);
            Assert.That(highP!.metrics.display != null);
            Assert.That(highP!.metrics.interaction != null);
            Assert.That(highP!.currencyId == "gem");
            Assert.That(highP!.iap == null);
            Assert.That(highP!.customPayload != null);
            Assert.That(highP!.creativeId != null);
        }

        /// <summary>
        /// Forces a 4xx response by sending a wrongly formatted timezone in the request
        /// </summary>
        [UnityTest]
        public IEnumerator Test_Error_Response()
        {
            var config = SdkConfig.Default();
            config.offersEndpoint = endpoint;
            config.networkTimeout = 5;
            // force a new cache
            config.offersCachePath = Path.GetTempFileName();

            string userId = Utils.RandomUserId();
            MeticaAPI.Initialise(userId, appId, apiKey, config, result => Assert.That(result.Result));
            MeticaLogger.CurrentLogLevel = LogLevel.Info;
            LogAssert.ignoreFailingMessages = true;

            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();

            yield return new WaitForSeconds(3);

            MeticaAPI.GetOffers(new[] { "main" }, result =>
            {
                Assert.That(result.Error != null);
            }, new Dictionary<string, object>()
            {
            }, new DeviceInfo()
            {
                appVersion = "1.0.0",
                timezone = "01:00",
                locale = "en_US",
                store = "AppStore"
            });

            yield return new WaitForSeconds(3);
        }
    }
}