using System;
using UnityEngine;

namespace Metica.Ads
{
    public class ShowCallbackProxy : AndroidJavaProxy
    {
        private const string TAG = MeticaAds.TAG;
        
        public event Action<MeticaAd> AdShowSuccess;
        public event Action<MeticaAd, MeticaAdError> AdShowFailed;
        public event Action<MeticaAd> AdHidden;
        public event Action<MeticaAd> AdClicked;
        public event Action<MeticaAd> AdRewarded;
        public event Action<MeticaAd> AdRevenuePaid;
        
        public ShowCallbackProxy() 
            : base("com.metica.ads.MeticaAdsShowCallback")
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} ShowCallbackProxy created");
        }
        
        // Called when ad shows successfully - now receives MeticaAd object
        public void onAdShowSuccess(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowSuccess callback received for adUnitId={meticaAd.adUnitId}");
            AdShowSuccess?.Invoke(meticaAd);
        }
        
        // Called when ad fails to show - now receives MeticaAd object
        public void onAdShowFailed(AndroidJavaObject meticaAdObject, AndroidJavaObject meticaAdErrorObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            var meticaAdError = meticaAdErrorObject.ToMeticaAdError();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdShowFailed callback received for adUnitId={meticaAd.adUnitId}, error={meticaAdError}");
            AdShowFailed?.Invoke(meticaAd, meticaAdError);
        }
        
        // Called when ad is hidden (closed) - now receives MeticaAd object
        public void onAdHidden(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdHidden callback received for adUnitId={meticaAd.adUnitId}");
            AdHidden?.Invoke(meticaAd);
        }
        
        // Called when ad is clicked - now receives MeticaAd object
        public void onAdClicked(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdClicked callback received for adUnitId={meticaAd.adUnitId}");
            AdClicked?.Invoke(meticaAd);
        }
        
        // Called when ad provides a reward 
        public void onAdRewarded(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdRewarded callback received for adUnitId={meticaAd.adUnitId}");
            AdRewarded?.Invoke(meticaAd);
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
