#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Metica.Unity
{
    internal abstract class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        private SimpleDiskCache<TKey, TValue> _cache;

        public Cache(string name, string cacheFilePath, int maxEntries = 100)
        {
            _cache = new SimpleDiskCache<TKey, TValue> (name, cacheFilePath, maxEntries);
            _cache.Prepare();
        }

        public virtual void Clear()
        {
            _cache.Clear();
        }

        public virtual TValue Read(TKey key)
        {
            return _cache.Read(BuildKey(key));
        }

        public virtual void Save()
        {
            _cache.Save();
        }

        public virtual void Write(TKey key, TValue value, long timeToLive)
        {
            _cache.Write(BuildKey(key), value, timeToLive);
        }

        protected abstract TKey BuildKey(TKey key);
    }

    internal class OffersCache : Cache<string, List<Offer>>
    {
        public OffersCache(string name, string cacheFilePath, int maxEntries = 100)
            : base(name, cacheFilePath, maxEntries)
        {
            //if (Application.isEditor && !Application.isPlaying)
            //{
            //    MeticaLogger.LogWarning(() => "The offers cache will not be available in the editor");
            //    return;
            //}
        }

        protected override string BuildKey(string key)
        {
            return $"offers-{MeticaAPI.AppId}-{MeticaAPI.UserId}-{key}";
        }
    }
}