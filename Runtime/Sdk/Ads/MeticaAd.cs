// MeticaAd.cs

#nullable enable

using System;
using Newtonsoft.Json;

namespace Metica.Ads
{
    public record MeticaAd(
        string adUnitId,
        double? revenue,
        string? networkName,
        string? placementTag,
        string? adFormat,
        string? creativeId,
        long? latency
    );
  
    public static class MeticaAdJson
    {
        public static MeticaAd FromJson(string json) =>
            JsonConvert.DeserializeObject<MeticaAd>(json)!;
    }

}
