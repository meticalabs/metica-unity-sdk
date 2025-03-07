namespace Metica.Unity
{
    internal interface ICache<TKey, TValue>
    {
        public void Clear();
        public void Save();
        public TValue Read(TKey key);
        public void Write(TKey key, TValue value, long ttlSeconds);
    }
}
