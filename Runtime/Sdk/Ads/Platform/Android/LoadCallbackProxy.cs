using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Metica.Ads
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LoadCallbackProxy : AndroidJavaProxy
    {
        private const string TAG = MeticaAds.TAG;
        public event Action<MeticaAd> AdLoadSuccess;
        public event Action<MeticaAdError> AdLoadFailed;

        public LoadCallbackProxy()
            : base("com.metica.ads.MeticaAdsLoadCallback")
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} LoadCallbackProxy created");
        }

        // Called from Android when ad loads successfully - now receives MeticaAd object
        public void onAdLoadSuccess(AndroidJavaObject meticaAdObject)
        {
            var meticaAd = meticaAdObject.ToMeticaAd();
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadSuccess callback received for adUnitId={meticaAd.adUnitId}");
            AdLoadSuccess?.Invoke(meticaAd);
        }

        // Called from Android when ad load fails - now only receives error string
        public void onAdLoadFailed(AndroidJavaObject meticaAdErrorObject)
        {
            var meticaAdError = meticaAdErrorObject.ToMeticaAdError(); 
            MeticaAds.Log.LogDebug(() => $"{TAG} onAdLoadFailed callback received, error={meticaAdError}");
            AdLoadFailed?.Invoke(meticaAdError);
        }
    }
}
