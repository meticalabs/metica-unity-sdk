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
    public class DisplayLogTest
    {
        [SetUp]
        public void Setup()
        {
            Utils.InitSdk();
        }

        [Test]
        public void Log_AppendingEntries()
        {
            Utils.ConfigureDisplayLogPath(Path.GetTempFileName());
            
            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();
            
            displayLog.AppendDisplayLogs(CreateEntries(1));

            Assert.That(displayLog._displayLogs.ContainsKey(Utils.testOfferId));
        }

        [Test]
        public void Log_Restoration()
        {
            var tempFileName = Path.GetTempFileName();
            
            var entries = CreateEntries(10);
            var json = JsonConvert.SerializeObject(entries);
            File.WriteAllText(tempFileName, json);
            
            Utils.ConfigureDisplayLogPath(tempFileName);

            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();

            Debug.Log(MeticaAPI.Config.displayLogPath);
            
            Assert.That(displayLog._displayLogs.ContainsKey(Utils.testOfferId));
            Assert.That(displayLog._displayLogs[Utils.testOfferId].Count == 10);
        }

        [Test]
        public void TestTheEntriesAreFiltered()
        {
            var offers = new List<Offer>();
            
            for (int i = 0; i < 5; i++)
            {
                Offer offer = new Offer
                {
                    offerId = "offer" + i,
                    displayLimits = new List<DisplayLimit> { new() { timeWindowInHours = 1, maxDisplayCount = 1 } }
                };
                offers.Add(offer);
            }
            
            Utils.ConfigureDisplayLogPath(Path.GetTempFileName());
            
            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();
            
            displayLog.AppendDisplayLogs(Enumerable.Range(1, 100).Select(i => new DisplayLogEntry
            {
                displayedOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(3)).ToUnixTimeSeconds(),
                offerId = "offer0",
                placementId = Utils.testPlacementId,
            }).ToList());
            
            for (int i = 1; i < 5; i++)
            {
                displayLog.AppendDisplayLogs(new[]
                {
                    new DisplayLogEntry
                    {
                        displayedOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)).ToUnixTimeSeconds(),
                        offerId = $"offer{i}",
                        placementId = Utils.testPlacementId,
                    },
                    new DisplayLogEntry
                    {
                        displayedOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)).ToUnixTimeSeconds(),
                        offerId = $"offer{i}",
                        placementId = Utils.testPlacementId,
                    }
                });
            }

            List<Offer> filteredOffers = displayLog.FilterOffers(offers);

            Assert.That(filteredOffers.Count == 1);
            Assert.That(filteredOffers[0].offerId == "offer0");

            offers[1].displayLimits[0].maxDisplayCount = 2;
            filteredOffers = displayLog.FilterOffers(offers);

            Assert.That(filteredOffers.Count == 2);
            Assert.That(filteredOffers[1].offerId == "offer1");
        }

        [UnityTest]
        public IEnumerator Log_Persistence()
        {
            Utils.ConfigureDisplayLogPath(Path.GetTempFileName());

            var config = MeticaAPI.Config;
            config.displayLogFlushCadence = 1;
            MeticaAPI.Config = config;
            
            var displayLog = MeticaAPI.DisplayLog;
            displayLog.Awake();
            
            displayLog.AppendDisplayLogs(CreateEntries((int)(4 * MeticaAPI.Config.maxDisplayLogEntries)));

            yield return new WaitForSecondsRealtime(MeticaAPI.Config.displayLogFlushCadence);

            Assert.That(File.Exists(MeticaAPI.Config.displayLogPath));

            var json = File.ReadAllText(MeticaAPI.Config.displayLogPath);
            var entries = JsonConvert.DeserializeObject<List<DisplayLogEntry>>(json);

            Debug.Log($"got {entries.Count}");
            Assert.That(entries.Count == MeticaAPI.Config.maxDisplayLogEntries);
        }

        private List<DisplayLogEntry> CreateEntries(int numEntries)
        {
            var entries = new List<DisplayLogEntry>();

            for (var i = 0; i < numEntries; i++)
            {
                entries.Add(
                    new DisplayLogEntry()
                    {
                        displayedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - i * 1000,
                        offerId = Utils.testOfferId,
                        offerVariantId = Utils.testVariantId,
                        placementId = Utils.testPlacementId
                    }
                );
            }

            return entries;
        }

        
    }
}