#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Metica.Unity
{
    public interface IRemoteConfigManager
    {
        void GetConfig(List<string>? configKeys,
            MeticaSdkDelegate<Dictionary<string, object>> responseCallback,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null
        );
    }

    public class RemoteConfigManager : IRemoteConfigManager
    {
        static readonly string[] FetchAllSentinel = Array.Empty<string>();
        
        public void GetConfig(
            List<string>? configKeys,
            MeticaSdkDelegate<Dictionary<string, object>> responseCallback,
            Dictionary<string, object>? userProperties = null, DeviceInfo? deviceInfo = null)
        {
            var resultConfig = new Dictionary<string, object>();
            string[]? pendingFetch = null;

            if (configKeys != null && configKeys.Count > 0)
            {
                foreach (var key in configKeys)
                {
                    var value = MeticaAPI.RemoteConfigCache.Read(key);
                    if (value != null)
                    {
                        resultConfig.Add(key, value);
                    }
                }

                pendingFetch = configKeys.Where(key => !resultConfig.ContainsKey(key)).ToArray();
                if (!pendingFetch.Any())
                {
                    pendingFetch = null;
                }
            }
            else
            {
                pendingFetch = FetchAllSentinel;
            }

            if (pendingFetch != null)
            {
                MeticaAPI.BackendOperations.CallRemoteConfigAPI(pendingFetch.ToArray(), (sdkResult) =>
                    {
                        if (sdkResult.Error != null)
                        {
                            MeticaLogger.LogError(() => $"Error while fetching the remote config: {sdkResult.Error}");
                            responseCallback(SdkResultImpl<Dictionary<string, object>>.WithError(sdkResult.Error));
                        }
                        else
                        {
                            // cache the config
                            foreach (var pair in sdkResult.Result.Config)
                            {
                                resultConfig.Add(pair.Key, pair.Value);
                                MeticaAPI.RemoteConfigCache.Write(pair.Key, pair.Value, sdkResult.Result.CacheDurationSecs);
                            }
                        }
                    },
                    userProperties, deviceInfo);
            }

            responseCallback(SdkResultImpl<Dictionary<string, object>>.WithResult(resultConfig));
        }
    }
}