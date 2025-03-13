namespace Metica.Experimental.Caching
{
    public interface ICache<TKey, TValue>
    {
        TValue Get(TKey key);
        void AddOrUpdate(TKey key, TValue value, long ttlSeconds);
    }
}
