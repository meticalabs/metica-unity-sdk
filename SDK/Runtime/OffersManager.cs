#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable all NotAccessedField.Global
// ReSharper disable file UnusedMember.Local
namespace Metica.Unity
{
    internal interface IOffersManager
    {
        void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null);
    }

    public class OffersManager : IOffersManager
    {
        private const long CACHE_DURARION_SECONDS = 60;
        static readonly string[] FetchAllSentinel = Array.Empty<string>();

        public void GetOffers(string[] placements, MeticaSdkDelegate<OffersByPlacement> offersCallback,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null)
        {
            var resultOffers = new Dictionary<string, List<Offer>>();
            string[]? pendingFetch = null;

            if (placements != null && placements.Length > 0)
            {
                foreach (var p in placements)
                {
                    var cachedResult = MeticaAPI.OffersCache.Read(p);
                    if (cachedResult != null && cachedResult.Count > 0)
                    {
                        resultOffers.Add(p, cachedResult);
                    }
                }

                // select the placements that are requested but weren't found in the cache
                pendingFetch = placements.Where(key => !resultOffers.ContainsKey(key)).ToArray();
                if (!pendingFetch.Any())
                {
                    pendingFetch = null;
                }
            }
            else
            {
                pendingFetch = FetchAllSentinel;
            }

            if (pendingFetch != null)
            {
                MeticaAPI.BackendOperations.CallGetOffersAPI(pendingFetch, (sdkResult) =>
                    {
                        if (sdkResult.Error != null)
                        {
                            MeticaLogger.LogError(() => $"Error while fetching offers: {sdkResult.Error}");
                            offersCallback(SdkResultImpl<OffersByPlacement>.WithError(sdkResult.Error));
                            return;
                        }
                        else
                        {
                            foreach (var pair in sdkResult.Result.placements)
                            {
                                resultOffers.Add(pair.Key, pair.Value);
                                // persist the response and refresh the in-memory cache
                                MeticaAPI.OffersCache.Write(pair.Key, pair.Value, CACHE_DURARION_SECONDS);
                            }
                            
                            // filter out the offers that have exceeded their display limit
                            offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement() { placements = resultOffers }));
                        }
                    },
                    userProperties, deviceInfo);
            }
            else
            {
                offersCallback(SdkResultImpl<OffersByPlacement>.WithResult(new OffersByPlacement() { placements = resultOffers }));
            }
        }
        
        // Logs the display of offers by placement.
        // 
        // Parameters:
        //   offersByPlacement: A dictionary containing offers grouped by placement.
        //
        // Returns: void
        //private void LogDisplays(Dictionary<string, List<Offer>> offersByPlacement)
        //{
        //    var currentTime = MeticaAPI.TimeSource.EpochSeconds();
        //    var offerIds = new HashSet<String>();

        //    foreach (var entry in offersByPlacement)
        //    {
        //        var newEntries = from offer in entry.Value
        //            where !offerIds.Contains(offer.offerId) && offer.displayLimits != null && offer.metrics?.display?.meticaAttributes?.offer != null
        //            let displayLogEntry = DisplayLogEntry.Create(
        //                offerId: offer.offerId,
        //                placementId: entry.Key,
        //                variantId: offer.metrics.display.meticaAttributes.offer.variantId
        //            )
        //            select displayLogEntry;

        //        offerIds.UnionWith(from e in newEntries select e.offerId);
        //    }
        //}

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