using System;
using System.Collections.Generic;

namespace Metica.Experimental.SDK
{
    public static class DictionaryExtensionMethods
    {
        public static void AddDictionary<T, S>(this Dictionary<T,S> dictionary, Dictionary<T,S> additionalDictionary, bool overwriteExistingKeys = false)
        {
            if(additionalDictionary == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var item in additionalDictionary)
            {
                if (dictionary.ContainsKey(item.Key))
                {
                    if (overwriteExistingKeys)
                    {
                        dictionary[item.Key] = item.Value;
                    }
                    else
                    {
                        throw new ArgumentException($"Duplicate key: {item.Key}");
                    }
                }
                else
                {
                    dictionary.Add(item.Key, item.Value); // lets exceptions bubble up
                }
            }
        }
    }
}
