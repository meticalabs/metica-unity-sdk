using System.Collections.Generic;

namespace Metica.Core.Caching
{
    /// <summary>
    /// Basic caching system with a set of methods to add and retrieve data. It also handles time-to-live and hit-counts.
    /// <seealso cref="ICache{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <remarks>
    /// <b>Roadmap</b>
    /// <ul>TODO - Implement persistency</ul>
    /// <ul>TODO - Implement tests</ul>
    /// </remarks>
    public class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
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
            /// How many times this entry was "hit", meaning it was requested and it was valid.
            /// </summary>
            public int hits;
            /// <summary>
            /// A value of type <see cref="TValue"/>
            /// </summary>
            public TValue value;

            public int maxHits = 100;

            public long ExpirationTime { get => timeCreated + ttl; }
        }

        public ITimeSource _timeSource {  get; private set; }
        protected Dictionary<int, CacheEntry> _data = new();

        protected const long DefaultTTLSeconds = 60;

        protected readonly long _garbageCollectionIntervalSeconds;
        protected long _nextGarbageCollection = 0;

        /// <summary>
        /// Create an instance of <see cref="Cache{TKey, TValue}"/>.
        /// </summary>
        /// <param name="timeSource">An implementation of <see cref="ITimeSource"/></param>
        /// <param name="gcTimeoutSeconds">Minimum time interval between garbage collection.
        /// Note that in this case garbage collection means clearing the cache from expired/invalid cache entries.</param>
        public Cache(ITimeSource timeSource, long gcTimeoutSeconds)
        {
            _timeSource = timeSource;
            _garbageCollectionIntervalSeconds = gcTimeoutSeconds;
        }

        /// <inheritdoc/>
        public virtual void AddOrUpdate(TKey key, TValue value, long ttlSeconds = DefaultTTLSeconds)
        {
            // We call the version with multiple values to have better control on when the garbage collection is called.
            Dictionary<TKey, TValue> entriesDictionary = new Dictionary<TKey, TValue> { {  key, value } };
            AddOrUpdate(entriesDictionary, ttlSeconds);
        }

        /// <inheritdoc/>
        public virtual void AddOrUpdate(Dictionary<TKey, TValue> entriesDictionary, long ttlSeconds = DefaultTTLSeconds)
        {
            GarbageCollect();
            if(entriesDictionary == null)
            {
                return;
            }
            foreach (KeyValuePair<TKey, TValue> pair in entriesDictionary)
            {
                TKey key = pair.Key;
                TValue value = pair.Value;
                int hash = key.GetHashCode();
                var entry = new CacheEntry { timeCreated = _timeSource.EpochSeconds(), ttl = ttlSeconds, hits = 0, value = value };
                if (_data.ContainsKey(hash))
                {
                    _data[hash] = entry;
                }
                else
                {
                    _data.Add(hash, entry);
                }
            }
        }

        /// <inheritdoc/>
        public virtual TValue Get(TKey key)
        {
            // We call the version with multiple values to have better control on when the garbage collection is called.
            List<TValue> results = GetMultiple(new TKey[] { key });
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual List<TValue> GetMultiple(TKey[] keys)
        {
            GarbageCollect();
            if(keys == null)
            {
                return null;
            }
            List<TValue> result = new List<TValue>();
            for (int i = 0; i < keys.Length; i++)
            {
                TKey key = keys[i];
                int hash = key.GetHashCode();
                if(_data.ContainsKey(hash))
                {
                    CacheEntry entry = _data[hash];
                    entry.hits++;
                    result.Add(entry.value);
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public virtual List<TValue> GetAll()
        {
            GarbageCollect();
            List <TValue> result = new List<TValue>();
            foreach (var entry in _data.Values)
            {
                result.Add(entry.value);
            }
            return result;
        }

        /// <summary>
        /// Utility method to find what keys are missing in the current cached entries.
        /// </summary>
        /// <param name="keys">A list of keys to look up.</param>
        /// <returns>A list of keys that weren't found.</returns>
        public TKey[] GetMissingKeys(TKey[] keys)
        {
            if (keys == null)
            {
                return null;
            }

            List<TKey> missing = new List<TKey>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!_data.ContainsKey(keys[i].GetHashCode()))
                {
                    missing.Add(keys[i]);
                }
            }
            return missing.ToArray();
        }

        /// <summary>
        /// Checks validity of a <see cref="CacheEntry"/>.
        /// </summary>
        /// <param name="entry">Checked entry.</param>
        /// <returns><c>true</c> if the <paramref name="entry"/> is valid, <c>false</c> otherwise.</returns>
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
            if(_nextGarbageCollection > _timeSource.EpochSeconds())
            {
                return;
            }
            _nextGarbageCollection = _timeSource.EpochSeconds() + _garbageCollectionIntervalSeconds;
            List<int> expired = new List<int>();
            foreach (int k in _data.Keys)
            {
                if (!IsValid(_data[k]))
                {
                    expired.Add(k);
                }
            }
            for (int i = 0; i < expired.Count; i++)
            {
                _data.Remove(expired[i]);
            }
            expired.Clear();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _data.Clear();
        }
    }
}
