namespace Metica.Experimental.Caching
{
    public interface ICache<TKey, TValue>
    {
        TValue GetValue(TKey key);
        void AddValue(TKey key, TValue value, long ttlSeconds);
    }
}
