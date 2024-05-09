using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local
namespace Metica.Unity
{
    public interface IOffersManager
    {
        void Init();

        void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null);

        List<Offer> GetCachedOffersByPlacement(string placement);
    }

    // TODO: add a history of the delivered offers
    public class OffersManager : IOffersManager
    {
        private OffersCache _offersCache;

        public void Init()
        {
            // load offers from disk
            _offersCache = new OffersCache();
        }

        public void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            // if the cache is recent, and not running inside the editor, return the cached offers
            var cachedResult = _offersCache.Read();
            if (cachedResult != null && !Application.isEditor)
            {
                Debug.Log("Returning cached offers");
                offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(cachedResult));
                return;
            }

            MeticaAPI.BackendOperations.CallGetOffersAPI(placements, (sdkResult) =>
                {
                    if (sdkResult.Error != null)
                    {
                        Debug.LogError($"Error while fetching offers: {sdkResult.Error}");
                        var cachedResult = _offersCache.Read();
                        if (cachedResult != null)
                        {
                            Debug.Log("Returning cached offers as fallback");
                            offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(cachedResult));
                        }
                        else
                        {
                            Debug.LogError("Failed to fetch offers from the server");
                            offersCallback(SdkResultImpl<OffersByPlacement>.WithError(sdkResult.Error));
                        }
                    }
                    else
                    {
                        // persist the response and refresh the in-memory cache
                        _offersCache.Write(new OffersByPlacement()
                        {
                            placements = sdkResult.Result.placements
                        });

                        // filter out the offers that have exceeded their display limit
                        var filteredDictionary = sdkResult.Result.placements.ToDictionary(
                            offersByPlacement => offersByPlacement.Key,
                            offersByPlacement => MeticaAPI.DisplayLog.FilterOffers(offersByPlacement.Value));

                        LogDisplays(filteredDictionary);

                        offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement()
                        {
                            placements = filteredDictionary
                        }));
                    }
                },
                userProperties, deviceInfo);
        }

        public List<Offer> GetCachedOffersByPlacement(string placement)
        {
            var res = _offersCache.Read();

            return res != null && res.placements.ContainsKey(placement)
                ? res.placements[placement]
                : new List<Offer>();
        }

        // Logs the display of offers by placement.
        // 
        // Parameters:
        //   offersByPlacement: A dictionary containing offers grouped by placement.
        //
        // Returns: void
        private void LogDisplays(Dictionary<string, List<Offer>> offersByPlacement)
        {
            var currentTime = MeticaAPI.TimeSource.EpochSeconds();
            var offerIds = new HashSet<String>();

            foreach (var entry in offersByPlacement)
            {
                var newEntries = from offer in entry.Value
                    where !offerIds.Contains(offer.offerId)
                    let displayLogEntry = DisplayLogEntry.Create(
                        offerId: offer.offerId,
                        placementId: entry.Key,
                        variantId: offer.metrics.display.meticaAttributes.offer.variantId
                    )
                    select displayLogEntry;

                MeticaAPI.DisplayLog.AppendDisplayLogs(newEntries);

                offerIds.UnionWith(from e in newEntries select e.offerId);
            }
        }

        private bool isSupportedPlatform()
        {
            var supportedPlatforms = new List<RuntimePlatform>
            {
                RuntimePlatform.Android,
                RuntimePlatform.IPhonePlayer,
                RuntimePlatform.OSXEditor,
                RuntimePlatform.OSXPlayer,
            };
            return supportedPlatforms.Contains(Application.platform);
        }
    }
}