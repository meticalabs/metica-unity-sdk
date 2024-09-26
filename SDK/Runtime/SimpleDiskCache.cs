using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Metica.Unity
{
    internal class CachedValue<T>
    {
        public T Data;
        public DateTimeOffset CacheTime;   
    }
    
    internal abstract class SimpleDiskCache<T> where T : class
    {
        private string _name;

        private CachedValue<T> _cachedData;

        protected abstract long TtlInMinutes { get; }
        
        private bool IsCacheValid => _cachedData != null && _cachedData.Data != null;

        private bool IsCacheUpToDate =>
            IsCacheValid && (MeticaAPI.TimeSource.EpochSeconds() - _cachedData.CacheTime.ToUnixTimeSeconds()) <
            (60 * TtlInMinutes);

        protected abstract string CacheFilePath { get; }

        protected SimpleDiskCache(string name)
        {
            _name = name;
        }
        
        internal void Prepare()
        {
            try
            {
                // Ensure the file exists
                if (File.Exists(CacheFilePath))
                {
                    using StreamReader reader = new StreamReader(CacheFilePath);
                    var content = reader.ReadToEnd();
                    _cachedData = JsonConvert.DeserializeObject<CachedValue<T>>(content);
                }
            }
            catch (Exception e)
            {
                MeticaLogger.LogError($"Error while trying to load the cache", e);
            }
        }

        [CanBeNull]
        public T Read()
        {
            return IsCacheUpToDate ? _cachedData.Data : null;
        }

        public void Write(T data)
        {
            try
            {
                _cachedData = new CachedValue<T>
                {
                    Data = data,
                    CacheTime = DateTimeOffset.FromUnixTimeSeconds(MeticaAPI.TimeSource.EpochSeconds())
                };
                
                MeticaLogger.LogInfo($"Writing {JsonConvert.SerializeObject(_cachedData)} at {CacheFilePath}");


                using StreamWriter writer = new StreamWriter(CacheFilePath);
                writer.Write(JsonConvert.SerializeObject(_cachedData));
            }
            catch (Exception e)
            {
                MeticaLogger.LogError($"Error while trying to save the cache {_name}", e);
                throw;
            }
        }

    }
}