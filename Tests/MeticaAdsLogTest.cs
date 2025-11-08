#nullable enable

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Metica;
using Metica.Ads;
using Metica.Core;
using UnityEngine;

namespace Tests
{
    [TestFixture]
    public class MeticaAdsLogTest
    {
        [Test(Description = "Verify that MeticaAds.Log uses updated logger after SetLogEnabled is called")]
        public void TestLogLevelChangeAffectsMeticaAds()
        {
            Task.Run(async () =>
            {
                await TestLogLevelChangeAffectsMeticaAds_Task();
            }).GetAwaiter().GetResult();
        }

        private async Task TestLogLevelChangeAffectsMeticaAds_Task()
        {
            // Reset static fields to ensure clean test
            MeticaSdk.ResetStaticFields();

            // First, force MeticaAds static constructor to run by accessing the class
            // This would previously capture the Log instance with Error level
            var initialLogAccess = MeticaAds.TAG;
            Assert.IsNotNull(initialLogAccess);

            // Get the initial logger - should be Error level
            var initialLog = Registry.Resolve<ILog>();
            Assert.IsNotNull(initialLog, "Initial logger should not be null");

            // Store reference to check if it's the same instance
            var initialLogHashCode = initialLog.GetHashCode();

            // Now enable debug logging
            MeticaSdk.SetLogEnabled(true);

            // Get the new logger from Registry
            var updatedLog = Registry.Resolve<ILog>();
            Assert.IsNotNull(updatedLog, "Updated logger should not be null");

            // Verify Registry has a different logger instance
            var updatedLogHashCode = updatedLog.GetHashCode();
            Assert.AreNotEqual(initialLogHashCode, updatedLogHashCode,
                "Registry should have a new logger instance after SetLogEnabled");

            // The key test: MeticaAds.Log should now return the updated logger
            // With the property implementation, it will always resolve from Registry
            // Previously with readonly field, it would still have the old instance
            var meticaAdsLog = GetMeticaAdsLogViaReflection();
            Assert.IsNotNull(meticaAdsLog, "MeticaAds.Log should not be null");

            // Verify MeticaAds is using the updated logger
            var meticaAdsLogHashCode = meticaAdsLog.GetHashCode();
            Assert.AreEqual(updatedLogHashCode, meticaAdsLogHashCode,
                "MeticaAds.Log should be using the updated logger from Registry");

            Debug.Log("Test passed: MeticaAds.Log correctly uses the updated logger after SetLogEnabled");
        }

        private ILog GetMeticaAdsLogViaReflection()
        {
            // Access the internal Log property via reflection for testing
            var meticaAdsType = typeof(MeticaAds);
            var logProperty = meticaAdsType.GetProperty("Log",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

            if (logProperty == null)
            {
                throw new Exception("Could not find Log property on MeticaAds");
            }

            var logValue = logProperty.GetValue(null) as ILog;
            if (logValue == null)
            {
                throw new Exception("Log property value is null or not ILog");
            }

            return logValue;
        }
    }
}