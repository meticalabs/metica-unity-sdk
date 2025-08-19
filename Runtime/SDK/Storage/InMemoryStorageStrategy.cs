using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metica.SDK.Storage
{
    /// <summary>
    /// In-memory storage strategy, useful for testing or temporary storage.
    /// </summary>
    public class InMemoryStorageStrategy : IStorageStrategy
    {
        private readonly Dictionary<string, object> _store = new();

        /// <inheritdoc/>
        /// <remarks>
        /// Note that this operation is synchronous but keeps the async-compatible signature for coherence.
        /// </remarks>
        public Task SaveAsync<T>(string key, T value)
        {
            _store[key] = value;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Note that this operation is synchronous but keeps the async-compatible signature for coherence.
        /// </remarks>
        public Task<T> LoadAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var value))
                return Task.FromResult((T)value);
            else
                return Task.FromResult(default(T));
        }
        
        /// <inheritdoc/>
        public bool Exists(string key) => _store.ContainsKey(key);

        /// <inheritdoc/>
        public void Delete(string key) => _store.Remove(key);
    }
}
