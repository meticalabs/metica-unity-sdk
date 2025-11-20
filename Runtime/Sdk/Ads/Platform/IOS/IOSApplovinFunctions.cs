#nullable enable

using System;
using System.Runtime.InteropServices;
using Metica.Ads;

namespace Metica.Ads.IOS
{
    internal class IOSApplovinFunctions : MeticaApplovinFunctions
    {
        private const string TAG = MeticaAds.TAG;
        [DllImport("__Internal")]
        private static extern bool ios_hasUserConsent();
        [DllImport("__Internal")]
        private static extern bool ios_isUserConsentSet();

        public bool HasUserConsent()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.hasUserConsent method");
            var result = ios_hasUserConsent();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.hasUserConsent returned: {result}");
            return result;
        }

        public bool IsUserConsentSet()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.isUserConsentSet method");
            var result = ios_isUserConsentSet();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.isUserConsentSet returned: {result}");
            return result;
        }

        public MaxSdk.ConsentFlowUserGeography GetConsentFlowUserGeography()
        {
            return MaxSdk.ConsentFlowUserGeography.Unknown;
        }
    }
}
