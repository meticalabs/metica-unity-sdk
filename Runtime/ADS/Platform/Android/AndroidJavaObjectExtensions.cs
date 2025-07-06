// AndroidJavaObjectExtensions.cs

using UnityEngine;

namespace Metica.ADS
{
/// <summary>
/// Extension methods for converting AndroidJavaObject instances to C# objects.
/// </summary>
public static class AndroidJavaObjectExtensions
{
    /// <summary>
    /// Converts an AndroidJavaObject representing a MeticaAd to a C# MeticaAd object.
    /// </summary>
    /// <param name="javaObject">The AndroidJavaObject containing MeticaAd data from the Android side</param>
    /// <returns>A new MeticaAd instance with data extracted from the AndroidJavaObject</returns>
    /// <remarks>
    /// This method extracts the following properties from the AndroidJavaObject:
    /// - adUnitId: The unique identifier for the ad unit
    /// - revenue: The revenue value (non-nullable)
    /// - networkName: The name of the ad network (nullable)
    /// - placementTag: The placement tag for the ad (nullable)
    /// - adFormat: The format type of the ad (nullable)
    /// - creativeId: The creative identifier for the ad (nullable)
    /// </remarks>
    /// <exception cref="System.NullReferenceException">
    /// Thrown if javaObject is null or if required properties are missing
    /// </exception>
    public static MeticaAd ToMeticaAd(this AndroidJavaObject javaObject)
    {
        string adUnitId = javaObject.Get<string>("adUnitId");
        double revenue = javaObject.Get<double>("revenue");
        string networkName = javaObject.Get<string>("networkName");
        string placementTag = javaObject.Get<string>("placementTag");
        string adFormat = javaObject.Get<string>("adFormat");
        string creativeId = javaObject.Get<string>("creativeId");

        return new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId);
    }
    
    /// <summary>
    /// Converts a MeticaAd object to an AndroidJavaObject for Unity-Android interop.
    /// </summary>
    /// <param name="meticaAd">The MeticaAd object to convert</param>
    /// <returns>AndroidJavaObject representing the MeticaAd on the Android side</returns>
    public static AndroidJavaObject ToAndroidJavaObject(this MeticaAd meticaAd)
    {
        // Much cleaner - directly reference the MeticaAd class
        using var meticaAdClass = new AndroidJavaClass("com.metica.ads.MeticaAdKt");
        return meticaAdClass.CallStatic<AndroidJavaObject>("createMeticaAd", 
            meticaAd.adUnitId, 
            meticaAd.revenue, 
            meticaAd.networkName, 
            meticaAd.placementTag, 
            meticaAd.adFormat,
            meticaAd.creativeId);
    }
}
}
