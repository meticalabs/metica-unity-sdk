using System;
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

        public Task SaveAsync<T>(string key, T value)
        {
            _store[key] = value;
            return Task.CompletedTask;
        }

        public Task<T> LoadAsync<T>(string key) => Task.FromResult((T)_store[key]);
        
        public bool Exists(string key) => _store.ContainsKey(key);

        public void Delete(string key) => _store.Remove(key);
    }
}