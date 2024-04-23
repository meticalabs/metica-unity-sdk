using System;
using System.Collections;
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

    public class DisplayLog : MonoBehaviour
    {
        private const UInt16 MaxEntriesInLog = 256;

        private Dictionary<string, List<DisplayLogEntry>> _displayLogs;
        private Coroutine _saveLogRoutine;

        private string DisplayLogPath => Path.Combine(Application.persistentDataPath, "display_log.json");

        public void Init()
        {
            _displayLogs = LoadDisplayLog();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                Debug.LogWarning("The displays log will not run in the editor");
                return;
            }

            _saveLogRoutine = StartCoroutine(SaveDisplayLogRoutine());
        }

        private void OnApplicationQuit()
        {
            PersistDisplayLog();
        }

        private IEnumerator SaveDisplayLogRoutine()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(60);

                PersistDisplayLog();
            }
        }

        private void OnDestroy()
        {
            if (_saveLogRoutine != null)
                StopCoroutine(_saveLogRoutine);
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


        private void PersistDisplayLog()
        {
            var result = _displayLogs
                .SelectMany(pair => pair.Value)
                .OrderByDescending(entry => entry.displayedOn)
                .Take(MaxEntriesInLog)
                .ToList();

            var json = JsonConvert.SerializeObject(result);
            File.WriteAllText(DisplayLogPath, json);
        }

        private Dictionary<string, List<DisplayLogEntry>> LoadDisplayLog()
        {
            if (!File.Exists(DisplayLogPath)) return new Dictionary<string, List<DisplayLogEntry>>();

            var json = File.ReadAllText(DisplayLogPath);
            var entries = JsonConvert.DeserializeObject<List<DisplayLogEntry>>(json);
            return entries.GroupBy(entry => entry.offerId)
                .ToDictionary(group => group.Key, group => group.ToList());
        }
    }
}