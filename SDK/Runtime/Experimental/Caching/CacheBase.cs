using Metica.Experimental.Core;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Metica.Experimental.Caching
{
    internal abstract class CacheBase<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        protected ITimeSource _timeSource;

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

        private const long GARBAGE_COLLECTION_INTERVAL_SECONDS = 10;
        private long nextGarbageCollection = 0;

        protected CacheBase(ITimeSource timeSource)
        {
            _timeSource = timeSource;
        }

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

        public virtual TValue Get(TKey key)
        {
            GarbageCollect();
            TKey transformedKey = TransformKey(key);
            if(_data.ContainsKey(transformedKey))
            {
                CacheEntry entry = _data[transformedKey];
                if (!IsValid(entry))
                {
                    _data.Remove(transformedKey);
                    return null;
                }
                entry.hits++;
                return entry.value;
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
