using System.Collections.Generic;

namespace Metica.Core.Caching
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
        /// <param name="entriesDictionary">Dictionary new entries for the cache.</param>
        /// <param name="ttlSeconds">Time To Live for all the cache entries in seconds.</param>
        void AddOrUpdate(Dictionary<TKey, TValue> entriesDictionary, long ttlSeconds);

        /// <summary>
        /// Retrieves multiple values with multiple keys, ignoring keys that aren't found.
        /// </summary>
        /// <param name="keys">An array of keys.</param>
        /// <returns>A list of entries that correspond to the given keys, if found.</returns>
        List<TValue> GetMultiple(TKey[] keys);
        
        /// <summary>
        /// Gets all currently cached values.
        /// </summary>
        /// <returns>List of all currently cached values.</returns>
        List<TValue> GetAll();

        /// <summary>
        /// Clears all data.
        /// </summary>
        void Clear();
    }
}
