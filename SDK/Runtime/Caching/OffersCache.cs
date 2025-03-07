#nullable enable
using System.Collections.Generic;
using Metica.Unity;

namespace Metica.SDK.Caching
{

    internal class OffersCache : Cache<string, List<Offer>>
    {
        public OffersCache(string name, string cacheFilePath, int maxEntries = 100)
            : base(name, cacheFilePath, maxEntries)
        {
            //if (Application.isEditor && !Application.isPlaying)
            //{
            //    MeticaLogger.LogWarning(() => "The offers cache will not be available in the editor");
            //    return;
            //}
        }

        protected override string BuildKey(string key)
        {
            return $"offers-{MeticaAPI.AppId}-{MeticaAPI.UserId}-{key}";
        }
    }
}