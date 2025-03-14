#if UNITY_MATHEMATICS && (UNITASK || UNITY_6000_0_OR_NEWER)

using EncosyTower.TypeWraps;

namespace EncosyTower.Pooling
{
    public readonly record struct PoolEntry<TId, TKey>(TId Id, TKey Key);

    [WrapRecord]
    internal readonly partial record struct TransformId(int _);
}

#endif
