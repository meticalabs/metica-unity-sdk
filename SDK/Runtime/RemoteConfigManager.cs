#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    public interface IRemoteConfigManager
    {
        void Init();

        void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback, List<string>? configKeys = null,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null
        );
    }

    internal class RemoteConfigCache : SimpleDiskCache<Dictionary<string, object>>
    {
        public RemoteConfigCache() : base("RemoteConfigCache")
        {
        }

        protected override long TtlInMinutes => MeticaAPI.Config.offersCacheTtlMinutes;
        protected override string CacheFilePath => MeticaAPI.Config.remoteConfigCachePath;
    }

    public class RemoteConfigManager : IRemoteConfigManager
    {
        private RemoteConfigCache _configCache;

        public void Init()
        {
            _configCache = new RemoteConfigCache();
            _configCache.Prepare();
        }

        public void GetConfig(MeticaSdkDelegate<Dictionary<string, object>> responseCallback,
            List<string>? configKeys = null,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null)
        {
            var cachedResult = _configCache.Read();
            if (cachedResult != null && !Application.isEditor)
            {
                MeticaLogger.LogDebug("Returning cached offers");

                responseCallback(SdkResultImpl<Dictionary<string, object>>.WithResult(cachedResult));
                return;
            }


            MeticaAPI.BackendOperations.CallRemoteConfigAPI(configKeys?.ToArray(), (sdkResult) =>
                {
                    if (sdkResult.Error != null)
                    {
                        MeticaLogger.LogError($"Error while fetching the remote config: {sdkResult.Error}");
                        responseCallback(SdkResultImpl<Dictionary<string, object>>.WithError(sdkResult.Error));
                    }
                    else
                    {
                        // cache the config
                        _configCache.Write(sdkResult.Result);
                        responseCallback(SdkResultImpl<Dictionary<string, object>>.WithResult(sdkResult.Result));
                    }
                },
                userProperties, deviceInfo);
        }
    }
}