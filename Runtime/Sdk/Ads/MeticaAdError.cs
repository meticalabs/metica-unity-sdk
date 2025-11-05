// MeticaAd.cs

#nullable enable

using System;

namespace Metica.Ads
{
public class MeticaAdError
{
    public string message;
    public string? adUnitId;

    public MeticaAdError(string message, string? adUnitId)
    {
        this.message = message;
        this.adUnitId = adUnitId;
    }
}
}
