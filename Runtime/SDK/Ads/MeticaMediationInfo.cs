using System.Collections.Generic;

namespace Metica.Ads
{
/// <summary>
/// Configuration info for ad mediation services.
/// Pairs a mediation type with its corresponding API key to initialize
/// the ad network properly.
/// </summary>
public class MeticaMediationInfo
{
    /// <summary>
    /// The ad network you're using for mediation
    /// </summary>
    public MeticaMediationType MediationType { get; }

    /// <summary>
    /// The API key for the specified mediation service.
    /// For MeticaMediationType.MAX, find this at https://dash.applovin.com/o/account?r=3#keys
    /// under "SDK Key"
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Optional key-value pairs for additional configuration
    /// parameters specific to the mediation network. Defaults to an empty dictionary.
    /// </summary>
    public Dictionary<string, string> ExtraInformation { get; }

    public MeticaMediationInfo(
        MeticaMediationType mediationType,
        string key,
        Dictionary<string, string> extraInformation = null)
    {
        MediationType = mediationType;
        Key = key;
        ExtraInformation = extraInformation ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Supported ad mediation networks.
    /// Each type represents a different ad mediation service that can be
    /// configured with MeticaMediationInfo.
    /// </summary>
    public enum MeticaMediationType
    {
        /// <summary>
        /// AppLovin MAX mediation platform.
        /// Requires an SDK key from the AppLovin dashboard.
        /// </summary>
        MAX,
        // ADMOB, // Google AdMob - implementation pending
    }
}
}
