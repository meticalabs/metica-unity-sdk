using UnityEngine;

namespace Metica.Unity
{
    public class RemoteConfigCache : MonoBehaviour
    {
        private SimpleDiskCache<object> _cache;

        internal void Awake()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                MeticaLogger.LogWarning(() => "The offers cache will not be available in the editor");
                return;
            }

            _cache = new SimpleDiskCache<object>("RemoteConfigCache", MeticaAPI.Config.remoteConfigCachePath);
            _cache.Prepare();
            DontDestroyOnLoad(this);
        }

        private void OnApplicationQuit()
        {
            _cache.Save();
        }

        public object? Read(string configKey)
        {
            return _cache.Read(GetCacheKey(configKey));
        }

        public void Write(string configKey, object value, long ttlSeconds)
        {
            _cache.Write(GetCacheKey(configKey), value, ttlSeconds);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private string GetCacheKey(string configKey)
        {
            return $"cfg-{MeticaAPI.AppId}-{MeticaAPI.UserId}-{configKey}";
        }
    }
}