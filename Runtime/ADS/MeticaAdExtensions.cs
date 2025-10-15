// MeticaAdExtensions.cs

using System.Collections.Generic;

namespace Metica.ADS
{
[System.Obsolete]
public static class MeticaAdExtensions
{
    public static MaxSdkBase.AdInfo ToAdInfo(this MeticaAd meticaAd)
    {
        // Create a dictionary with available MeticaAd values
        var adInfoDictionary = new Dictionary<string, object>
        {
            ["adUnitId"] = meticaAd.adUnitId ?? string.Empty,
            ["adFormat"] = meticaAd.adFormat ?? string.Empty,
            ["networkName"] = meticaAd.networkName ?? string.Empty,
            ["placement"] = meticaAd.placementTag ?? string.Empty,
            ["revenue"] = meticaAd.revenue,
            ["creativeId"] = meticaAd.creativeId,
            ["latencyMillis"] = meticaAd.latency,

            // Set defaults for unavailable fields
            ["networkPlacement"] = string.Empty,
            ["revenuePrecision"] = string.Empty,
            ["waterfallInfo"] = new Dictionary<string, object>(),
            ["dspName"] = string.Empty
        };

        return new MaxSdkBase.AdInfo(adInfoDictionary);
    }

    /// <summary>
    /// Converts MaxSdkBase.AdInfo to MeticaAd format for internal processing
    /// </summary>
    /// <param name="adInfo">The MAX SDK AdInfo object to convert</param>
    /// <returns>MeticaAd object with mapped values</returns>
    public static MeticaAd ToMeticaAd(this MaxSdkBase.AdInfo adInfo)
    {
        return new MeticaAd(
            adUnitId: adInfo.AdUnitIdentifier,
            revenue: adInfo.Revenue,
            networkName: adInfo.NetworkName,
            placementTag: string.IsNullOrWhiteSpace(adInfo.Placement) ? null : adInfo.Placement,
            adFormat: adInfo.AdFormat,
            creativeId: adInfo.CreativeIdentifier,
            latency: adInfo.LatencyMillis
        );
    }
}
}
