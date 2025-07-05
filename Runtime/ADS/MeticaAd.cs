// MeticaAd.cs

using System;

namespace Metica.ADS
{
[Serializable]
public class MeticaAd
{
    public string adUnitId;
    public double revenue;
    public string networkName;
    public string placementTag;
    public string adFormat;
    public string creativeId;

    public MeticaAd(string adUnitId, double revenue = -1, string networkName = null, 
        string placementTag = null, string adFormat = null, string creativeId = null)
    {
        this.adUnitId = adUnitId;
        this.revenue = revenue;
        this.networkName = networkName;
        this.placementTag = placementTag;
        this.adFormat = adFormat;
        this.creativeId = creativeId;
    }
}
}
