#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    
    internal class OffersCache : MonoBehaviour
    {
        private SimpleDiskCache<List<Offer>>? _cache;
            
        internal void Awake()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                MeticaLogger.LogWarning(() => "The offers cache will not be available in the editor");
                return;
            }

            _cache = new SimpleDiskCache<List<Offer>>("OffersCache", MeticaAPI.Config.offersCachePath);
            _cache.Prepare();
            DontDestroyOnLoad(this);
        }

        private void OnApplicationQuit()
        {
            _cache?.Save();
        }

        public void Clear()
        {
            _cache?.Clear();
        }
        
        public List<Offer>? Read(string placement)
        {
            List<Offer>? value = _cache?.Read(CacheKeyForPlacement(placement));
            MeticaLogger.LogDebug(() => value == null ? "<b>OFFER CACHE MISS</b>" : "<b>OFFER CACHE HIT</b>");
            return value;
        }

        public void Write(string placement, List<Offer> offers)
        {
            _cache?.Write(CacheKeyForPlacement(placement), offers, MeticaAPI.Config.offersCacheTtlMinutes * 60);
        }

        private static string CacheKeyForPlacement(string placement)
        {
            return $"offers-{MeticaAPI.AppId}-{MeticaAPI.UserId}-{placement}";
        }
    }
}