using System;
using UnityEngine;

namespace Metica.ADS
{
    public class BannerCallbackProxy : AndroidJavaProxy
    {
        private const string TAG = MeticaAds.TAG;

        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<string> AdLoadFailed;
        public event Action<MeticaAd> AdClicked;
        public event Action<MeticaAd> AdRevenuePaid;
        
        public BannerCallbackProxy() 
            : base("com.metica.ads.MeticaAdsBannerCallback")
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} BannerCallbackProxy created");
        }
        
        // Called from Android when ad loads successfully - now receives MeticaAd object
        public void onAdLoadSuccess(AndroidJavaObject meticaAdObject)
        {
            // Convert AndroidJavaObject to C# MeticaAd object
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
            AdLoadSuccess?.Invoke(meticaAd);
        }

        // Called from Android when ad load fails - now only receives error string
        public void onAdLoadFailed(string error)
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={error}");
            AdLoadFailed?.Invoke(error);
        }
        
        // Called when ad is clicked - now receives MeticaAd object
        public void onAdClicked(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdClicked callback received for adUnitId={meticaAd.adUnitId}");
            AdClicked?.Invoke(meticaAd);
        }
        
        // Called when revenue is paid for the ad 
        public void onAdRevenuePaid(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRevenuePaid callback received for adUnitId={meticaAd.adUnitId}");
            AdRevenuePaid?.Invoke(meticaAd);
        }
    }
}
