using System.Threading.Tasks;

namespace Metica.SDK.Storage
{
    /// <summary>
    /// Defines the contract for a storage strategy.
    /// Concrete strategies are used to define how the data is saved by the storage system.
    /// </summary>
    public interface IStorageStrategy
    {
        /// <summary>
        /// Asynchronously saves a value associated with the given key.
        /// </summary>
        Task Save<T>(string key, T value);

        /// <summary>
        /// Asynchronously loads a value for the given key. Returns default(T) if not found.
        /// </summary>
        Task<T> Load<T>(string key);

        /// <summary>
        /// Checks if a value exists for the given key.
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Deletes the value associated with the given key.
        /// </summary>
        void Delete(string key);
    }
}