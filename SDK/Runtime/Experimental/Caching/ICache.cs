using Metica.Experimental.Core;
using System.Collections.Generic;

namespace Metica.Experimental.Caching
{
    public interface ICache<TKey, TValue>
    {
        ITimeSource _timeSource { get; }

        TValue Get(TKey key);
        void AddOrUpdate(TKey key, TValue value, long ttlSeconds);
        void AddOrUpdate(KeyValuePair<TKey, TValue>[] keyValuePairs, long ttlSeconds);
        void AddOrUpdate(Dictionary<TKey, TValue> entriesDictionary, long ttlSeconds);
        List<TValue> Get(TKey[] keys);
        Dictionary<TKey, TValue> GetAsDictionary(TKey[] keys);
    }
}
