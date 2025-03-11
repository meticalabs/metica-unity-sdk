using System.Collections.Generic;

namespace Metica.Experimental.Caching
{
    internal abstract class CacheBase<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        protected class DataEntry
        {
            public long timeCreated;
            public int hits;
            public TValue value;
        }

        private Dictionary<TKey, DataEntry> _data = new();


        public virtual void AddValue(TKey key, TValue value, long ttlSeconds)
        {
            TKey transformedKey = TransformKey(key);
            // TODO : timeCreated with time source
            var entry = new DataEntry { timeCreated = 0, hits = 0, value = value };
            // TODO : check ttl
            if (_data.ContainsKey(transformedKey))
            {
                _data[transformedKey] = entry;
            }
            else
            {
                _data.Add(transformedKey, entry);
            }
        }

        public virtual TValue GetValue(TKey key)
        {
            TKey transformedKey = TransformKey(key);
            // TODO : check ttl
            if(_data.ContainsKey(transformedKey))
            {
                _data[transformedKey].hits++;
                return _data[transformedKey].value;
            }
            return null;
        }

        protected virtual TKey TransformKey(TKey key)
        {
            return key; // equality transform
        }

    }
}
