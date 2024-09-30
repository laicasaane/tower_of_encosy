#if UNITY_MATHEMATICS && (UNITASK || UNITY_6000_0_OR_NEWER)

using Module.Core.TypeWrap;

namespace Module.Core.Pooling
{
    public readonly record struct PoolEntry<TId, TKey>(TId Id, TKey Key);

    [WrapRecord]
    internal readonly partial record struct TransformId(int _);
}

#endif
