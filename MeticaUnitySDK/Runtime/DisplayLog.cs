using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    // Represents the impressions or displays of offers.
    [Serializable]
    public struct DisplayLogEntry : IComparable<DisplayLogEntry>
    {
        public long displayedOn; // timestamp in epoch millis
        public string offerId;
        public string offerVariantId;
        public string placementId;

        // Compares instances based on the displayedOn attribute.
        public int CompareTo(DisplayLogEntry other)
        {
            return displayedOn.CompareTo(other.displayedOn);
        }
    }

    public class DisplayLog
    {
        private Dictionary<string, List<DisplayLogEntry>> _displayLogs;

        public void Init()
        {
            var filePath = Path.Combine(Application.persistentDataPath, "display_log.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _displayLogs = JsonConvert.DeserializeObject<Dictionary<string, List<DisplayLogEntry>>>(json);
            }
            else
            {
                _displayLogs = new Dictionary<string, List<DisplayLogEntry>>();
            }
        }

        public void AppendDisplayLogs(IEnumerable<DisplayLogEntry> displayLogEntries)
        {
            var perOffer = displayLogEntries.GroupBy(entry => entry.offerId)
                .ToDictionary(group => group.Key, group => group.ToList());
            foreach (var entry in perOffer)
            {
                if (_displayLogs.ContainsKey(entry.Key))
                {
                    _displayLogs[entry.Key].AddRange(entry.Value);
                }
                else
                {
                    _displayLogs.Add(entry.Key, entry.Value);
                }
            }

            var filePath = Path.Combine(Application.persistentDataPath, "display_log.json");
            var json = JsonConvert.SerializeObject(displayLogEntries);
            File.WriteAllText(filePath, json);
        }

        public List<Offer> FilterOffers(List<Offer> offers)
        {
            var filteredOffers = new List<Offer>();

            foreach (var offer in offers)
            {
                if (offer.displayLimits == null || !_displayLogs.ContainsKey(offer.offerId))
                {
                    filteredOffers.Add(offer);
                    continue;
                }

                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var displayLogList = _displayLogs[offer.offerId];
                var limitExceeded = (from displayLimit in offer.displayLimits
                    let timeWindowInHours = displayLimit.timeWindowInHours * 3600
                    let recentDisplayLogs =
                        displayLogList.Where(log => (currentTime - log.displayedOn) <= (long)timeWindowInHours).ToList()
                    where recentDisplayLogs.Count > (int)displayLimit.maxDisplayCount
                    select displayLimit).Any();

                if (!limitExceeded)
                {
                    filteredOffers.Add(offer);
                }
            }

            return filteredOffers;
        }
    }
}