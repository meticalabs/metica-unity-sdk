namespace Metica.Ads
{
    /// <summary>
    /// Interface for AppLovin-specific functionality in the Metica SDK.
    /// Provides access to privacy settings and consent flow information.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface MeticaApplovinFunctions
    {
        /// <summary>
        /// Checks whether the user has provided consent for data collection.
        /// </summary>
        /// <returns>True if the user has consented, false otherwise</returns>
        bool HasUserConsent();

        /// <summary>
        /// Checks whether the user consent status has been explicitly set.
        /// </summary>
        /// <returns>True if consent status has been set (either granted or denied), false if not yet determined</returns>
        bool IsUserConsentSet();

        /// <summary>
        /// Gets the geographical location category of the user for consent flow purposes.
        /// </summary>
        /// <returns>The user's geographical category as determined by AppLovin's consent flow</returns>
        MaxSdkBase.ConsentFlowUserGeography GetConsentFlowUserGeography();
    }
}
