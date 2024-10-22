#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;

namespace Metica.Unity
{
    internal class CachedValue<T>
    {
        public T? Data;
        public long ExpiresOn;
    }

    internal class SimpleDiskCache<T> where T : class
    {
        private readonly string _name;
        private readonly string _cacheFilePath;

        private readonly OrderedDictionary _cachedData;

        public SimpleDiskCache(string name, string cacheFilePath, int maxEntries = 100)
        {
            _name = name;
            _cacheFilePath = cacheFilePath;
            _cachedData = new OrderedDictionary(maxEntries);
        }

        internal void Prepare()
        {
            try
            {
                // Ensure the file exists
                if (File.Exists(_cacheFilePath))
                {
                    using StreamReader reader = new StreamReader(_cacheFilePath);
                    var content = reader.ReadToEnd();
                    var savedDict = JsonConvert.DeserializeObject<Dictionary<string, CachedValue<T>>>(content) ?? new Dictionary<string, CachedValue<T>>();

                    foreach (var pair in savedDict)
                    {
                        _cachedData[pair.Key] = pair.Value;
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(_cacheFilePath);
                }
                catch (Exception)
                {
                    MeticaLogger.LogError(() => $"Error while removing the existing cache file", e);
                }
            }
        }

        internal void Clear()
        {
            _cachedData.Clear();
        }
        
        internal void Save()
        {
            using StreamWriter writer = new StreamWriter(_cacheFilePath);
            writer.Write(JsonConvert.SerializeObject(_cachedData));
        }

        public T? Read(string key)
        {
            var result = (CachedValue<T>?)_cachedData[key];
            return result?.ExpiresOn > MeticaAPI.TimeSource.EpochSeconds() ? result.Data : null;
        }

        public void Write(string key, T data, long ttlSeconds)
        {
            try
            {
                var cachedValue =  new CachedValue<T>
                {
                    Data = data,
                    ExpiresOn = MeticaAPI.TimeSource.EpochSeconds() + ttlSeconds
                };

                if (_cachedData.Contains(key))
                {
                    _cachedData[key] = cachedValue;
                }
                else
                {
                    _cachedData.Add(key, cachedValue);
                }
            }
            catch (Exception e)
            {
                MeticaLogger.LogError(() => $"Error while trying to save the cache {_name}", e);
            }
        }
    }
}