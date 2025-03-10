#nullable enable
using UnityEngine;
using Metica.Unity;
using UnityEngine.UIElements;

namespace Metica.SDK.Caching
{
    // TODO : This should probably extend Cache<string, RemoteConfig> but for now this should work with the code in place.
    internal class RemoteConfigCache : Cache<string, object>
    {
        public RemoteConfigCache(string name, string cacheFilePath, int maxEntries = 100) : base(name, cacheFilePath, maxEntries)
        {
            //if (Application.isEditor && !Application.isPlaying)
            //{
            //    MeticaLogger.LogWarning(() => "The remote config cache will not be available in the editor");
            //    return;
            //}
        }

        protected override string TransformKey(string key)
        {
            return $"cfg-{MeticaAPI.AppId}-{MeticaAPI.UserId}-{key}";
        }
    }
}