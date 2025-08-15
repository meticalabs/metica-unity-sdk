using System;
using System.Threading.Tasks;

namespace Metica.SDK.Storage
{
    /// <summary>
    /// Facade for storage operations. Delegates work to a chosen storage strategy.
    /// Supports a default strategy and allows injection before first use.
    /// </summary>
    public class StorageManager
    {
        private IStorageStrategy _strategy;

        /// <summary>
        /// Initializes a new instance of Storage.
        /// Optionally accepts a custom strategy; otherwise, a default, volatile one is used.
        /// </summary>
        public StorageManager(IStorageStrategy strategy = null) =>
            _strategy = strategy ?? new InMemoryStorageStrategy();

        /// <summary>
        /// Sets or replaces the storage strategy before the first operation.
        /// Throws if the strategy has already been initialized.
        /// </summary>
        public void SetStrategy(IStorageStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            _strategy = strategy;
        }

        /// <summary>
        /// Saves a value for the given key using the current strategy.
        /// </summary>
        public async Task SaveAsync<T>(string key, T value) => await _strategy.SaveAsync(key, value);

        /// <summary>
        /// Loads a value for the given key using the current strategy.
        /// Returns default(T) if not found.
        /// </summary>
        public async Task<T> LoadAsync<T>(string key) => await _strategy.LoadAsync<T>(key);

        /// <summary>
        /// Checks if a value exists for the given key.
        /// </summary>
        public bool Exists(string key) => _strategy.Exists(key);

        /// <summary>
        /// Deletes the value associated with the given key.
        /// </summary>
        public void Delete(string key) => _strategy.Delete(key);
    }

}
