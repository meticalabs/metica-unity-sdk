namespace Metica.Ads.UnityPlayer
{
    internal class UnityPlayerApplovinFunctions : MeticaApplovinFunctions
    {
        public bool HasUserConsent()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock HasUserConsent called - returning false");
            return false;
        }

        public bool IsUserConsentSet()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock IsUserConsentSet called - returning false");
            return false;
        }

        public MaxSdk.ConsentFlowUserGeography GetConsentFlowUserGeography()
        {
            MeticaAds.Log.LogDebug(() => "[MeticaAds Unity] Mock GetConsentFlowUserGeography called - returning Unknown");
            return MaxSdk.ConsentFlowUserGeography.Unknown;
        }
    }
}
