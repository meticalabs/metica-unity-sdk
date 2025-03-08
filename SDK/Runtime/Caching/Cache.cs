using Metica.Unity;

namespace Metica.SDK.Caching
{
    internal abstract class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        // TODO : not ideal to have a specific implementation inside a generic one.
        private SimpleDiskCache<TKey, TValue> _cache;

        public Cache(string name, string cacheFilePath, int maxEntries = 100)
        {
            _cache = new SimpleDiskCache<TKey, TValue>(name, cacheFilePath, maxEntries);
            _cache.Load();
        }

        public virtual void Clear()
        {
            _cache.Clear();
        }

        public virtual TValue Read(TKey key)
        {
            return _cache.Read(TransformKey(key));
        }

        /// <summary>
        /// Saves the current cached data to disk.
        /// Must be called manually.
        /// </summary>
        public virtual void Save()
        {
            _cache.Save();
        }

        public virtual void Write(TKey key, TValue value, long timeToLive)
        {
            _cache.Write(TransformKey(key), value, timeToLive);
        }

        protected abstract TKey TransformKey(TKey key);


    }
}
