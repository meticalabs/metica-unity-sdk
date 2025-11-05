// MeticaAd.cs

#nullable enable

using System;

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
}
