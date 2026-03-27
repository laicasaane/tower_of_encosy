using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    /// <summary>
    /// Cache-friendly, equatable replacement for the nested <c>DataDeclaration.Order</c> struct.
    /// Records the position of a member in the declared order and whether it is a PropRef or FieldRef.
    /// </summary>
    public struct OrderData : IEquatable<OrderData>
    {
        public int index;
        public bool isPropRef;

        public readonly bool Equals(OrderData other)
            => index == other.index
            && isPropRef == other.isPropRef;

        public readonly override bool Equals(object obj)
            => obj is OrderData other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(index, isPropRef);
    }
}
