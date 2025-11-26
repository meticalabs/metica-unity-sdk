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
        [DllImport("__Internal")]
        private static extern int ios_getConsentFlowUserGeography();
        [DllImport("__Internal")]
        private static extern bool ios_isMuted();
        [DllImport("__Internal")]
        private static extern void ios_showCmpForExistingUser();
        [DllImport("__Internal")]
        private static extern void ios_showMediationDebugger();

        public bool HasUserConsent()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.hasUserConsent method");
            var result = ios_hasUserConsent();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.hasUserConsent returned: {result}");
            return result;
        }

        public bool IsMuted() {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.isMuted method");
            var result = ios_isMuted();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.isMuted returned: {result}");
            return result;
        }

        public bool IsUserConsentSet()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.isUserConsentSet method");
            var result = ios_isUserConsentSet();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.isUserConsentSet returned: {result}");
            return result;
        }

        public void ShowCmpForExistingUser() {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.showCmpForExistingUser method");
            ios_showCmpForExistingUser();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.showCmpForExistingUser method called");
        }

        public void ShowMediationDebugger() {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.showMediationDebugger method");
            ios_showMediationDebugger();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.showMediationDebugger method called");
        }

        public MaxSdk.ConsentFlowUserGeography GetConsentFlowUserGeography()
        {
            MeticaAds.Log.LogDebug(() => $"{TAG} About to call iOS Max.getConsentFlowUserGeography method");
            var ordinal = ios_getConsentFlowUserGeography();
            MeticaAds.Log.LogDebug(() => $"{TAG} iOS Max.getConsentFlowUserGeography returned ordinal: {ordinal}");
            return (MaxSdkBase.ConsentFlowUserGeography)ordinal;
        }
    }
}
