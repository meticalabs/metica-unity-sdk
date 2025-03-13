using Metica.Experimental.Core;
using System.Collections.Generic;

namespace Metica.Experimental.Caching
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        public ITimeSource _timeSource;

        protected class CacheEntry
        {
            /// <summary>
            /// When this entry was created.
            /// </summary>
            public long timeCreated;
            /// <summary>
            /// Time To Live in seconds.
            /// </summary>
            public long ttl;
            /// <summary>
            /// How many time this entry was "hit",, meaning it was requested and it was valid.
            /// </summary>
            public int hits;
            /// <summary>
            /// A value of type <see cref="TValue"/>
            /// </summary>
            public TValue value;

            public int maxHits = 100;

            public long ExpirationTime { get => timeCreated + ttl; }
        }

        private Dictionary<TKey, CacheEntry> _data = new();

        private const long GARBAGE_COLLECTION_INTERVAL_SECONDS = 10; // TODO : make this settable
        private long nextGarbageCollection = 0;

        public Cache(ITimeSource timeSource)
        {
            _timeSource = timeSource;
        }

        /// <summary>
        /// Add or update a new cache entry.
        /// </summary>
        /// <param name="key">Key for the cache entry.</param>
        /// <param name="value">Value for the cache entry.</param>
        /// <param name="ttlSeconds">Time To Live for the cache entry in seconds.</param>
        public virtual void AddOrUpdate(TKey key, TValue value, long ttlSeconds)
        {
            GarbageCollect();
            TKey transformedKey = TransformKey(key);
            var entry = new CacheEntry { timeCreated = _timeSource.EpochSeconds(), ttl = ttlSeconds, hits = 0, value = value };
            if (_data.ContainsKey(transformedKey))
            {
                _data[transformedKey] = entry;
            }
            else
            {
                _data.Add(transformedKey, entry);
            }
        }

        /// <summary>
        /// Retrieves multiple values with multiple keys, ignoring keys that aren't found.
        /// </summary>
        /// <param name="keys">List of keys.</param>
        /// <returns>A list of entries that correspond to the given keys, if found.</returns>
        public virtual List<TValue> Get(TKey[] keys)
        {
            GarbageCollect();
            List<TValue> result = new List<TValue>();
            for (int i = 0; i < keys.Length; i++)
            {
                TKey key = keys[i];
                TKey transformedKey = TransformKey(key);
                if(_data.ContainsKey(transformedKey))
                {
                    CacheEntry entry = _data[transformedKey];
                    entry.hits++;
                    result.Add(entry.value);
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a single value.
        /// </summary>
        /// <param name="key">Key of the cache entry</param>
        /// <returns>The value that corresponds to the key, if found.</returns>
        public virtual TValue Get(TKey key)
        {
            List<TValue> results = Get(new TKey[] { key });
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        protected virtual bool IsValid(CacheEntry entry)
        {
            bool hasHitMax = entry.maxHits > 0 && (entry.hits > entry.maxHits);
            bool hasExpired = entry.ExpirationTime < _timeSource.EpochSeconds();
            if (hasExpired || hasHitMax)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Collects the garbage, deleting all <see cref="CacheEntry"/>s that are not valid (expired or else).
        /// </summary>
        /// <remarks>
        /// This is user triggered when interacting with the cache but will only trigger when a specified time has elapsed.
        /// </remarks>
        protected void GarbageCollect()
        {
            if(nextGarbageCollection > _timeSource.EpochSeconds())
            {
                return;
            }
            nextGarbageCollection = _timeSource.EpochSeconds() + GARBAGE_COLLECTION_INTERVAL_SECONDS;
            List<TKey> marked = new List<TKey>();
            foreach (TKey k in _data.Keys)
            {
                if (!IsValid(_data[k]))
                {
                    marked.Add(k);
                }
            }
            for (int i = 0; i < marked.Count; i++)
            {
                _data.Remove(marked[i]);
            }
            marked.Clear();
        }

        protected virtual TKey TransformKey(TKey key)
        {
            return key; // equality transform
        }

    }
}
