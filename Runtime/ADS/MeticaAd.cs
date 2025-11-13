// MeticaAd.cs

using System;
using UnityEngine;

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
        public long latency;

        public MeticaAd(string adUnitId, double revenue, string networkName,
            string placementTag, string adFormat, string creativeId, long latency)
        {
            this.adUnitId = adUnitId;
            this.revenue = revenue;
            this.networkName = networkName;
            this.placementTag = placementTag;
            this.adFormat = adFormat;
            this.creativeId = creativeId;
            this.latency = latency;
        }

        public static MeticaAd FromJson(string json)
        {
            return JsonUtility.FromJson<MeticaAd>(json);
        }
    }
}
