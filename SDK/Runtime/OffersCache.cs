using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    public class CachedOffersByPlacement
    {
        public OffersByPlacement offers;
        public DateTimeOffset cacheTime;
    }
    
    public class OffersCache
    {
        private CachedOffersByPlacement _cachedOffers;

        private bool IsOffersCacheValid() => _cachedOffers != null && _cachedOffers.offers != null;

        private bool IsOffersCacheUpToDate() =>
            IsOffersCacheValid() && (MeticaAPI.TimeSource.EpochSeconds() - _cachedOffers.cacheTime.ToUnixTimeSeconds()) < (60*MeticaAPI.Config.offersCacheTtlMinutes);
        
        public OffersCache()
        {
            try
            {
                // Ensure the file exists
                if (File.Exists(MeticaAPI.Config.offersCachePath))
                {
                    using (StreamReader reader = new StreamReader(MeticaAPI.Config.offersCachePath))
                    {
                        var content = reader.ReadToEnd();
                        _cachedOffers = JsonConvert.DeserializeObject<CachedOffersByPlacement>(content);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"Error while trying to load the offers cache: {e}");
            }
        }

        [CanBeNull]
        public OffersByPlacement Read()
        {
            var upToDate = IsOffersCacheUpToDate();
            return upToDate ? _cachedOffers.offers : null;
        }
        
        public void Write(OffersByPlacement offers)
        {
            try
            {
                _cachedOffers = new CachedOffersByPlacement()
                {
                    offers = offers,
                    cacheTime = DateTimeOffset.FromUnixTimeSeconds(MeticaAPI.TimeSource.EpochSeconds())
                };
                using (StreamWriter writer = new StreamWriter(MeticaAPI.Config.offersCachePath))
                {
                    writer.Write(JsonConvert.SerializeObject(_cachedOffers));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while trying to save the offers cache: {e}");
                throw;
            }
        }
    }
}