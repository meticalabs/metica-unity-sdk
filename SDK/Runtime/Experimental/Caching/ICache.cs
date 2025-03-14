using Metica.Experimental.Core;
using System.Collections.Generic;

namespace Metica.Experimental.Caching
{
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// An <see cref="ITimeSource"/> used for cache TTL.
        /// </summary>
        ITimeSource _timeSource { get; }

        /// <summary>
        /// Retrieves a single value.
        /// </summary>
        /// <param name="key">Key of the cache entry</param>
        /// <returns>The value that corresponds to the key, if found.</returns>
        TValue Get(TKey key);

        /// <summary>
        /// Add or update a new cache entry.
        /// </summary>
        /// <param name="key">Key for the cache entry.</param>
        /// <param name="value">Value for the cache entry.</param>
        /// <param name="ttlSeconds">Time To Live for the cache entry in seconds.</param>
        void AddOrUpdate(TKey key, TValue value, long ttlSeconds);

        /// <summary>
        /// Add or update multiple new cache entries at once.
        /// </summary>
        /// <param name="keyValuePairs">Array of <see cref="KeyValuePair"/>s for the cache entries.</param>
        /// <param name="ttlSeconds">Time To Live for all the cache entries in seconds.</param>
        void AddOrUpdate(KeyValuePair<TKey, TValue>[] keyValuePairs, long ttlSeconds);

        /// <summary>
        /// Add or update multiple new cache entries at once.
        /// </summary>
        /// <param name="entriesDictionary">Dictionary new entries for the cache.</param>
        /// <param name="ttlSeconds">Time To Live for all the cache entries in seconds.</param>
        void AddOrUpdate(Dictionary<TKey, TValue> entriesDictionary, long ttlSeconds);

        /// <summary>
        /// Retrieves multiple values with multiple keys, ignoring keys that aren't found.
        /// </summary>
        /// <param name="keys">An array of keys.</param>
        /// <returns>A list of entries that correspond to the given keys, if found.</returns>
        List<TValue> Get(TKey[] keys);
        
        /// <summary>
        /// Retrieves multiple values with multiple keys, ignoring those that aren't found.
        /// </summary>
        /// <param name="keys">An array of keys.</param>
        /// <returns>A dictionary including keys and values of the found entries.</returns>
        /// <remarks>Normally only the values would be returned but this method comes to rescue situations where maintaining the association between keys and values is useful.</remarks>
        Dictionary<TKey, TValue> GetAsDictionary(TKey[] keys);
    }
}
