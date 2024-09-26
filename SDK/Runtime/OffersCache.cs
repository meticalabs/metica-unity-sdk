namespace Metica.Unity
{
    internal class OffersCache : SimpleDiskCache<OffersByPlacement>
    {
        public OffersCache() : base("OffersCache")
        {
        }

        protected override long TtlInMinutes => MeticaAPI.Config.offersCacheTtlMinutes;
        protected override string CacheFilePath => MeticaAPI.Config.offersCachePath;
    }
}