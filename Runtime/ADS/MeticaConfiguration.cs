using System.Collections.Generic;

namespace Metica.ADS
{
    [System.Serializable]
    public struct MeticaConfiguration
    {
        /// <summary>
        /// Metica API Key
        /// </summary>
        public string ApiKey;
        /// <summary>
        /// Metica App ID
        /// </summary>
        public string AppId;
        [System.Obsolete]
        public string UserId;
        [System.Obsolete]
        public string Version;
        public string BaseEndpoint;
        /// <summary>
        /// Allows adding custom API keys or other values.
        /// TODO: https://linear.app/metica/issue/MET-4645/custom-additional-api-keys
        /// </summary>
        public List<KeyMap> CustomKeys;

        public struct KeyMap
        {
            public string Key;
            public string Value;
        }
    }
}
