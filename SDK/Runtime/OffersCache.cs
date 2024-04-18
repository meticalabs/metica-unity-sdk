using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Metica.Unity
{
    public class CachedOffersByPlacement
    {
        public OffersByPlacement offers;
        public DateTime cacheTime;

        public static CachedOffersByPlacement Empty()
        {
            return new CachedOffersByPlacement()
            {
                offers = new OffersByPlacement()
                {
                    placements = new Dictionary<string, List<Offer>>()
                }
            };
        }
    }
    
    public class OffersCache
    {
        private static readonly string FilePath = Application.persistentDataPath + "/metica-offers.json";

        public static CachedOffersByPlacement Read()
        {
            try
            {
                // Ensure the file exists
                if (File.Exists(FilePath))
                {
                    using (StreamReader reader = new StreamReader(FilePath))
                    {
                        var content = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<CachedOffersByPlacement>(content);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"Error while trying to load the offers cache: {e}");
            }

            return CachedOffersByPlacement.Empty();
        }

        public static void Write(OffersByPlacement offers)
        {
            try
            {
                var cachedOffers = new CachedOffersByPlacement()
                {
                    offers = offers,
                    cacheTime = new DateTime()
                };
                using (StreamWriter writer = new StreamWriter(FilePath))
                {
                    writer.Write(JsonConvert.SerializeObject(cachedOffers));
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