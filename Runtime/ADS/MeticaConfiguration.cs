namespace Metica.ADS
{
    [System.Serializable]
    public struct MeticaConfiguration
    {
        public string ApiKey;
        public string AppId;
        public string UserId;
        [System.Obsolete]
        public string Version;
        public string BaseEndpoint;
    }
}
