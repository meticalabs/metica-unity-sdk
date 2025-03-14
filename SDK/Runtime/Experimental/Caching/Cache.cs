using Metica.Experimental.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metica.Experimental.Caching
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
    /// <ul>TODO - Some methods may be inefficient. Decide whether to optimize or warn users.</ul>
    /// </remarks>
    public class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        public ITimeSource _timeSource {  get; private set; }

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

        protected Dictionary<TKey, CacheEntry> _data = new();

        protected const long GARBAGE_COLLECTION_INTERVAL_SECONDS = 10; // TODO : make this settable
        protected long nextGarbageCollection = 0;

        public Cache(ITimeSource timeSource)
        {
            _timeSource = timeSource;
        }

        /// <inheritdoc/>
        public virtual void AddOrUpdate(TKey key, TValue value, long ttlSeconds = GARBAGE_COLLECTION_INTERVAL_SECONDS)
        {
            // We call the version with multiple values to have better control on when the garbage collection is called.
            Dictionary<TKey, TValue> entriesDictionary = new Dictionary<TKey, TValue> { {  key, value } };
            AddOrUpdate(entriesDictionary, ttlSeconds);
        }

        /// <inheritdoc/>
        public virtual void AddOrUpdate(Dictionary<TKey, TValue> entriesDictionary, long ttlSeconds = GARBAGE_COLLECTION_INTERVAL_SECONDS)
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
        }

        /// <inheritdoc/>
        public virtual TValue Get(TKey key)
        {
            // We call the version with multiple values to have better control on when the garbage collection is called.
            List<TValue> results = Get(new TKey[] { key });
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual List<TValue> Get(TKey[] keys)
        {
            GarbageCollect();
            if(keys ==  null)
            {
                return null;
            }
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

        /// <inheritdoc/>
        public virtual Dictionary<TKey, TValue> GetAllAsDictionary()
        {
            GarbageCollect();
            Dictionary<TKey,TValue> result = new Dictionary<TKey, TValue>();
            foreach (var k in _data.Keys)
            {
                result.Add(k, _data[k].value);
            }
            return result;
        }

        /// <inheritdoc/>
       public virtual Dictionary<TKey, TValue> GetAsDictionary(TKey[] keys)
        {
            GarbageCollect();
            if(keys == null)
            {
                return null;
            }
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Length; i++)
            {
                TKey key = keys[i];
                TKey transformedKey = TransformKey(key);
                if(_data.ContainsKey(transformedKey))
                {
                    CacheEntry entry = _data[transformedKey];
                    entry.hits++;
                    result.Add(key, entry.value);
                }
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
            List<TKey> absents = new List<TKey>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!_data.ContainsKey(keys[i]))
                {
                    absents.Add(keys[i]);
                }
            }
            return absents.ToArray();
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

        /// <inheritdoc/>
        public void Clear()
        {
            _data.Clear();
        }

        protected virtual TKey TransformKey(TKey key)
        {
            return key; // equality transform
        }

    }
}
