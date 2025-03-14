#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;

using Metica.Unity;

namespace Metica.SDK.Caching
{
    internal class CachedValue<T>
    {
        public T? Data;
        public long ExpiresOn;
    }

    internal class SimpleDiskCache<TKey, TValue> where TValue : class
    {
        private readonly string _name;
        private readonly string _cacheFilePath;

        // TODO : do we really need an OrderedDictionary?
        private readonly OrderedDictionary _cachedData;

        public SimpleDiskCache(string name, string cacheFilePath, int maxEntries = 100)
        {
            _name = name;
            _cacheFilePath = cacheFilePath;
            _cachedData = new OrderedDictionary(maxEntries);
        }

        internal void Load()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    using StreamReader reader = new StreamReader(_cacheFilePath);
                    var content = reader.ReadToEnd();
                    var savedDict = JsonConvert.DeserializeObject<Dictionary<string, CachedValue<TValue>>>(content) ?? new Dictionary<string, CachedValue<TValue>>();

                    foreach (var pair in savedDict)
                    {
                        _cachedData[pair.Key] = pair.Value;
                    }
                    reader.Close();
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
        
        public void Save()
        {
            using StreamWriter writer = new StreamWriter(_cacheFilePath);
            writer.Write(JsonConvert.SerializeObject(_cachedData));
        }

        public TValue? Read(TKey key)
        {
            var result = (CachedValue<TValue>?)_cachedData[key];
            return result?.ExpiresOn > MeticaAPI.TimeSource.EpochSeconds() ? result.Data : null;
        }

        public void Write(TKey key, TValue data, long ttlSeconds)
        {
            try
            {
                var cachedValue =  new CachedValue<TValue>
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

        public void Clear()
        {
            _cachedData.Clear();
        }

    }
}