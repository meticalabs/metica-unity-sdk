// AndroidJavaObjectExtensions.cs

using UnityEngine;

namespace Metica.ADS
{
public static class AndroidJavaObjectExtensions
{
    public static MeticaAd ToMeticaAd(this AndroidJavaObject javaObject)
    {
        string adUnitId = javaObject.Get<string>("adUnitId");

        // Revenue is no longer nullable, can get directly
        double revenue = javaObject.Get<double>("revenue");

        string networkName = javaObject.Get<string>("networkName");
        string placementTag = javaObject.Get<string>("placementTag");
        string adFormat = javaObject.Get<string>("adFormat");

        return new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat);
    }
}
}
