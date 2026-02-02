using System;
using EncosyTower.Conversion;

namespace EncosyTower.UnionIds
{
    public interface IUnionId<TBase>
    {
    }

    public interface IUnionId<TBase, TUnionId> : IUnionId<TBase>
        , IToDisplayString
        , IEquatable<TUnionId> , IComparable<TUnionId>
        , ITryParse<TUnionId>, ITryParseSpan<TUnionId>
        where TUnionId : IUnionId<TBase, TUnionId>
    {
    }

    public interface ISerializableUnionId<TBase, TUnionId, TSerializable>
        : ITryConvert<TUnionId>, IToDisplayString
        , IEquatable<TSerializable>, IComparable<TSerializable>
        where TUnionId : IUnionId<TBase, TUnionId>
    {
    }
}
