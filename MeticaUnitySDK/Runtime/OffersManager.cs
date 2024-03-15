using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local
namespace Metica.Unity
{
    // TODO: add a history of the delivered offers
    internal class OffersManager
    {
        private CachedOffersByPlacement _cachedOffers = null;
        
        public void Init()
        {
            // load offers from disk
            _cachedOffers = OffersCache.Read();
        }

        private bool IsOffersCacheValid() => _cachedOffers != null && _cachedOffers.offers != null;

        private bool IsOffersCacheUpToDate() =>
            IsOffersCacheValid() && (new DateTime() - _cachedOffers.cacheTime).TotalHours < 2;

        public void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object> userProperties = null, DeviceInfo deviceInfo = null)
        {
            Debug.Log("Fetching offers from the server");

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
                        offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(IsOffersCacheValid()
                            ? _cachedOffers.offers
                            : new OffersByPlacement()));
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
                            cacheTime = new DateTime()
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

        internal List<Offer> GetCachedOffersByPlacement(string placement)
        {
            return _cachedOffers.offers.placements[placement];
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