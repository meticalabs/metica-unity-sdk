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
        private CachedOffersByPlacement _cachedOffers = null;

        public void Init()
        {
            // load offers from disk
            _cachedOffers = OffersCache.Read();
        }

        private bool IsOffersCacheValid() => _cachedOffers != null && _cachedOffers.offers != null;

        private bool IsOffersCacheUpToDate() =>
            IsOffersCacheValid() && (DateTime.Now - _cachedOffers.cacheTime).TotalHours < 2;

        public void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            // if the cache is recent, and not running inside the editor, return the cached offers
            if (IsOffersCacheUpToDate() && !Application.isEditor)
            {
                Debug.Log("Returning cached offers");
                offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(_cachedOffers.offers));
                return;
            }

            BackendOperations.CallGetOffersAPI(placements, (sdkResult) =>
                {
                    if (sdkResult.Error != null)
                    {
                        Debug.LogError($"Error while fetching offers: {sdkResult.Error}");
                        if (IsOffersCacheUpToDate())
                        {
                            Debug.Log("Returning cached offers as fallback");
                            offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(_cachedOffers.offers));
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
                        OffersCache.Write(new OffersByPlacement()
                        {
                            placements = sdkResult.Result.placements
                        });

                        _cachedOffers = new CachedOffersByPlacement()
                        {
                            offers = new OffersByPlacement()
                            {
                                placements = sdkResult.Result.placements
                            },
                            cacheTime = DateTime.Now
                        };

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
            return _cachedOffers.offers.placements.ContainsKey(placement)
                ? _cachedOffers.offers.placements[placement]
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
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var offerIds = new HashSet<String>();

            foreach (var entry in offersByPlacement)
            {
                var newEntries = from offer in entry.Value
                    where !offerIds.Contains(offer.offerId)
                    let displayLogEntry = new DisplayLogEntry()
                    {
                        displayedOn = currentTime,
                        offerId = offer.offerId,
                        offerVariantId = offer.metrics.display.meticaAttributes.offer.variantId,
                        placementId = entry.Key
                    }
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