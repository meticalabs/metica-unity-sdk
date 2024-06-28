using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Metica.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using UnityEngine.TestTools;


namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class IngestionE2eTest
    {
        // String endpoint = System.Environment.GetEnvironmentVariable("INGESTION_ENDPOINT");
        // String apiKey = System.Environment.GetEnvironmentVariable("E2E_TESTSAPP_API_KEY");
        String endpoint = "https://services-alb.dev.metica.com";
        String apiKey = "01HRW4KNEMYGS9FCEPW4ZVFX88";
        String appId = "e2eTestsApp";

        [UnityTest]
        public IEnumerator Send_Events()
        {
            var config = SdkConfig.Default();
            config.ingestionEndpoint = endpoint;
            config.networkTimeout = 5;

            string userId = Utils.RandomUserId();

            MeticaAPI.Initialise(userId, appId, apiKey, config, result => Assert.That(result.Result));

            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();

            yield return new WaitForSeconds(3);

            MeticaAPI.LogUserAttributes(new Dictionary<string, object>()
            {
                { "name", "test" }
            });

            var logger = ScriptingObjects.GetComponent<EventsLogger>();
            logger.FlushEvents();

            yield return new WaitForSeconds(3);
        }
    }
}