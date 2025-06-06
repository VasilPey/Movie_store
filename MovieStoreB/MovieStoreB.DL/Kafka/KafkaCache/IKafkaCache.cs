namespace MovieStoreB.DL.Kafka.KafkaCache
{
    public interface IKafkaCache<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        IEnumerable<TValue> GetAll();

        bool TryGetValue(TKey key, out TValue value);
    }
}