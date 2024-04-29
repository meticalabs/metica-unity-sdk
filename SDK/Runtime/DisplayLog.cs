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
        // timestamp in epoch seconds
        public long displayedOn;
        public string offerId;
        public string offerVariantId;
        public string placementId;

        public static DisplayLogEntry Create(string offerId, string placementId, string variantId = null)
        {
            return new DisplayLogEntry()
            {
                displayedOn = DateTimeOffset.Now.ToUnixTimeSeconds(),
                offerId = offerId,
                placementId = placementId,
                offerVariantId = variantId
            };
        }
        
        // Compares instances based on the displayedOn attribute.
        public int CompareTo(DisplayLogEntry other)
        {
            return displayedOn.CompareTo(other.displayedOn);
        }
    }

    public class DisplayLog : MonoBehaviour
    {
        internal Dictionary<string, List<DisplayLogEntry>> _displayLogs;
        private Coroutine _saveLogRoutine;

        internal void Awake()
        {
            _displayLogs = LoadDisplayLog();
            _saveLogRoutine = StartCoroutine(SaveDisplayLogRoutine());
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                Debug.LogWarning("The displays log will not run in the editor");
            }
        }

        private void OnApplicationQuit()
        {
            PersistDisplayLog();
        }

        private IEnumerator SaveDisplayLogRoutine()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(MeticaAPI.Config.displayLogFlushCadence);

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
                    let timeWindowInSeconds = displayLimit.timeWindowInHours * 3600
                    let recentDisplayLogs =
                        displayLogList.Count(log => (currentTime - log.displayedOn) <= (long)timeWindowInSeconds)
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
            if (_displayLogs == null || _displayLogs.Count == 0) return;
            
            var entries = _displayLogs
                .SelectMany(pair => pair.Value)
                .OrderByDescending(entry => entry.displayedOn)
                .Take((int)MeticaAPI.Config.maxDisplayLogEntries)
                .ToList();

            var json = JsonConvert.SerializeObject(entries);
            File.WriteAllText(MeticaAPI.Config.displayLogPath, json);

            _displayLogs = CreateOffersIndex(entries);
        }

        private Dictionary<string, List<DisplayLogEntry>> LoadDisplayLog()
        {
            if (!File.Exists(MeticaAPI.Config.displayLogPath)) return new Dictionary<string, List<DisplayLogEntry>>();

            try
            {
                Debug.Log($"Loading log from {MeticaAPI.Config.displayLogPath}");
                var json = File.ReadAllText(MeticaAPI.Config.displayLogPath);
                var entries = JsonConvert.DeserializeObject<List<DisplayLogEntry>>(json);
                return CreateOffersIndex(entries);
            }
            catch (JsonSerializationException)
            {
                return new Dictionary<string, List<DisplayLogEntry>>();
            }
        }

        private Dictionary<string, List<DisplayLogEntry>> CreateOffersIndex(List<DisplayLogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return new Dictionary<string, List<DisplayLogEntry>>();
            }
            return entries.GroupBy(entry => entry.offerId)
                .ToDictionary(group => group.Key, group => group.ToList());
        }
    }
}