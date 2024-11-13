using System;

namespace EncosyTower.Modules.UnionIds
{
    /// <summary>
    /// Place this attribute on a struct to indicate that it is a union id.
    /// </summary>
    /// <seealso cref="KindForUnionIdAttribute"/>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class UnionIdAttribute : Attribute
    {
        public UnionIdSize Size { get; set; }

        public string DisplayNameForId { get; set; }

        public string DisplayNameForKind { get; set; }

        public UnionIdKindSettings KindSettings { get; set; }

        public UnionIdAttribute()
        {
            Size = UnionIdSize.Auto;
            DisplayNameForId = "Id";
            DisplayNameForKind = "Kind";
            KindSettings = UnionIdKindSettings.None;
        }
    }

    /// <summary>
    /// Place this attribute on a struct decorated with [UnionId] to specify the kinds of values for the union id.
    /// </summary>
    /// <seealso cref="UnionIdAttribute"/>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class UnionIdKindAttribute : Attribute
    {
        public Type KindType { get; }

        public ulong Order { get; }

        public string DisplayName { get; }

        /// <param name="kindType">The type size must be 8 bytes or lesser.</param>
        public UnionIdKindAttribute(Type kindType, ulong order, string displayName = "")
        {
            KindType = kindType;
            Order = order;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Place this attribute on an enum or unmanaged value type
    /// to indicate it's a part of an union id.
    /// </summary>
    /// <remarks>
    /// The type size must be 8 bytes or lesser.
    /// </remarks>
    /// <seealso cref="UnionIdAttribute"/>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class KindForUnionIdAttribute : Attribute
    {
        public Type IdType { get; }

        public ulong Order { get; }

        public string DisplayName { get; }

        /// <param name="idType">The type that is decorated with [UnionId].</param>
        public KindForUnionIdAttribute(Type idType, ulong order, string displayName = "")
        {
            IdType = idType;
            Order = order;
            DisplayName = displayName;
        }
    }
}
