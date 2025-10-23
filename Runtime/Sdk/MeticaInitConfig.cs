namespace Metica
{
/// <summary>
/// Configuration data required for initializing the Metica SDK.
/// Contains the essential parameters needed to authenticate and configure
/// the SDK for communication with Metica backend services.
/// </summary>
public class MeticaInitConfig
{
    /// <summary>
    /// The API key for authenticating with Metica services
    /// </summary>
    public string ApiKey { get; }
    
    /// <summary>
    /// The application ID for identifying this app in Metica systems
    /// </summary>
    public string AppId { get; }
    
    /// <summary>
    /// Unique identifier for the current user (for analytics and segmentation)
    /// </summary>
    public string UserId { get; }

    public MeticaInitConfig(string apiKey, string appId, string userId)
    {
        ApiKey = apiKey;
        AppId = appId;
        UserId = userId;
    }
}}
