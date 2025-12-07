namespace EncosyTower.Collections
{
    public interface ITryAddKeyValue<in TKey, in TValue>
    {
        bool TryAdd(TKey key, TValue value);
    }
}
