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
            long latency = javaObject.Get<long>("latency");

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
