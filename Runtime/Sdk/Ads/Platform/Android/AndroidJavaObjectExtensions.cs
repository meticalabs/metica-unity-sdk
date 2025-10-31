using System;
using Metica;
using UnityEngine;

namespace Metica.Ads
{
    /// <summary>
    /// Extension methods for converting AndroidJavaObject instances to C# objects.
    /// </summary>
    public static class AndroidJavaObjectExtensions
    {
        /// <summary>
        /// Converts a Java Double object (AndroidJavaObject) to a nullable C# double.
        /// </summary>
        /// <param name="javaObject">The AndroidJavaObject representing a Java Double, or null</param>
        /// <returns>A nullable double value, or null if the javaObject is null</returns>
        public static double? ToNullableDouble(this AndroidJavaObject javaObject)
        {
            if (javaObject == null)
            {
                return null;
            }
            return javaObject.Call<double>("doubleValue");
        }

        /// <summary>
        /// Converts an AndroidJavaObject representing a MeticaAd to a C# MeticaAd object.
        /// </summary>
        /// <param name="javaObject">The AndroidJavaObject containing MeticaAd data from the Android side</param>
        /// <returns>A new MeticaAd instance with data extracted from the AndroidJavaObject</returns>
        /// <remarks>
        /// This method extracts the following properties from the AndroidJavaObject:
        /// - adUnitId: The unique identifier for the ad unit
        /// - revenue: The revenue value (nullable)
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
            var adUnitId = javaObject.Call<string>("getAdUnitId");
            var revenue = javaObject.Call<AndroidJavaObject>("getRevenue").ToNullableDouble();
            var networkName = javaObject.Call<string>("getNetworkName");
            var placementTag = javaObject.Call<string>("getPlacementTag");
            var adFormat = javaObject.Call<string>("getAdFormat");
            var creativeId = javaObject.Call<string>("getCreativeId");
            var latency = javaObject.Call<long>("getLatency");

            return new MeticaAd(adUnitId, revenue, networkName, placementTag, adFormat, creativeId, latency);
        }
        public static MeticaAdError ToMeticaAdError(this AndroidJavaObject javaObject)
        {
            var message = javaObject.Call<string>("getMessage");
            return new MeticaAdError(message);
        }
        
    public static MeticaSmartFloors ToMeticaSmartFloors(this AndroidJavaObject javaObject)
        {
        var userGroupJavaObject = javaObject.Call<AndroidJavaObject>("getUserGroup");
        var userGroupName = userGroupJavaObject.Call<string>("name");
        var userGroup = (MeticaUserGroup)System.Enum.Parse(typeof(MeticaUserGroup), userGroupName);

        var isSuccess = javaObject.Call<bool>("isSuccess");

        return new MeticaSmartFloors(userGroup, isSuccess);
        }
    }
}
