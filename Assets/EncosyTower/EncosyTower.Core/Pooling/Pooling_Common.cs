#if UNITY_MATHEMATICS && (UNITASK || UNITY_6000_0_OR_NEWER)

namespace EncosyTower.Pooling
{
    public readonly record struct PoolEntry<TId, TKey>(TId Id, TKey Key);
}

#endif
