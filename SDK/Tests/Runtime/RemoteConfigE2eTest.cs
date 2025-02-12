using System;
using System.Collections;
using System.Collections.Generic;
using Metica.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using UnityEngine.TestTools;

namespace MeticaUnitySDK.SDK.Tests.Runtime
{
    [TestFixture]
    public class RemoteConfigE2eTest
    {
        private String endpoint = Environment.GetEnvironmentVariable("RC_ENDPOINT");

        private String apiKey = Environment.GetEnvironmentVariable("E2E_TESTSAPP_API_KEY");

        String appId = "e2eTestsApp";

        [UnityTest]
        public IEnumerator Get_RemoteConfig()
        {
            LogAssert.ignoreFailingMessages = true;
            var config = SdkConfig.Default();
            config.remoteConfigEndpoint = endpoint;
            config.networkTimeout = 5;

            string userId = Utils.RandomUserId();

            MeticaAPI.Initialise(userId, appId, apiKey, config, result => Assert.That(result.Result));
            MeticaLogger.CurrentLogLevel = LogLevel.Info;

            Dictionary<string, object> remoteConfig = null;
            MeticaAPI.GetConfig(result =>
            {
                Assert.That(result.Error == null);
                remoteConfig = result.Result;
            }, new List<string>(), new Dictionary<string, object>()
            {
                { "cond1", false },
                { "cond2", true },
                { "cond3", true }
            }, new DeviceInfo()
            {
                appVersion = "1.0.0",
                timezone = "UTC",
                locale = "en_US",
                store = "AppStore"
            });

            yield return new WaitForSeconds(3);

            Assert.That(remoteConfig["multipleConds"] as string == "value2");
        }
    }
}