#nullable enable

using UnityEngine;

namespace Metica.Ads.Android
{
    internal class AndroidApplovinFunctions : MeticaApplovinFunctions
    {
        private const string TAG = MeticaAds.TAG;
        private readonly AndroidJavaObject _maxObject;

        public AndroidApplovinFunctions(AndroidJavaObject maxObject)
        {
            _maxObject = maxObject;
        }

        public bool HasUserConsent()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android Max.hasUserConsent method");
            var result = _maxObject.Call<bool>("hasUserConsent");
            MeticaAds.Log.LogDebug(() => $"{TAG} Android Max.hasUserConsent returned: {result}");
            return result;
        }

        public bool IsUserConsentSet()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android Max.isUserConsentSet method");
            var result = _maxObject.Call<bool>("isUserConsentSet");
            MeticaAds.Log.LogDebug(() => $"{TAG} Android Max.isUserConsentSet returned: {result}");
            return result;
        }

        public MaxSdkBase.ConsentFlowUserGeography GetConsentFlowUserGeography()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call Android Max.getConsentFlowUserGeography method");
            var geographyObject = _maxObject.Call<AndroidJavaObject>("getConsentFlowUserGeography");
            var ordinal = geographyObject.Call<int>("ordinal");
            MeticaAds.Log.LogDebug(() => $"{TAG} Android Max.getConsentFlowUserGeography returned ordinal: {ordinal}");
            return (MaxSdkBase.ConsentFlowUserGeography)ordinal;
        }
    }
}
