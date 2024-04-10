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
    internal struct DisplayLogEntry : IComparable<DisplayLogEntry>
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

    internal class DisplayLog
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
            var json = JsonConvert.SerializeObject(_displayLogs);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Filters the input offers by applying the display limits against the display log. 
        /// </summary>
        /// <param name="offers">the input offers</param>
        /// <returns>All the offers that haven't exceeded their display limit</returns>
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
                        displayLogList.Count(log => (currentTime - log.displayedOn) <= (long)timeWindowInHours)
                    where recentDisplayLogs > (int)displayLimit.maxDisplayCount
                    select displayLimit).Any();

                if (!limitExceeded)
                {
                    filteredOffers.Add(offer);
                }
            }

            return filteredOffers;
        }

        public List<DisplayLogEntry> GetEntriesForOffer(string offerId)
        {
            return _displayLogs.TryGetValue(offerId, out var displayLog) ? displayLog : new List<DisplayLogEntry>();
        }
    }
}