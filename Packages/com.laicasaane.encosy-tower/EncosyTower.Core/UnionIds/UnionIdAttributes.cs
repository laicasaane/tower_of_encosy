using System;
using EncosyTower.Serialization;

namespace EncosyTower.UnionIds
{
    /// <summary>
    /// Place this attribute on a struct to indicate that it is a union id.
    /// </summary>
    /// <seealso cref="KindForUnionIdAttribute"/>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class UnionIdAttribute : Attribute
    {
        /// <summary>
        /// The size of the union id in the string representation.
        /// </summary>
        public UnionIdSize Size { get; set; } = UnionIdSize.Auto;

        /// <summary>
        /// The display name for the id part in the string representation.
        /// </summary>
        /// <remarks>
        /// Default is <c>"Id"</c>.
        /// </remarks>
        public string DisplayNameForId { get; set; } = "Id";

        /// <summary>
        /// The display name for the kind part in the string representation.
        /// </summary>
        /// <remarks>
        /// Default is <c>"Kind"</c>.
        /// </remarks>
        public string DisplayNameForKind { get; set; } = "Kind";

        /// <summary>
        /// The character used to separate the kind and id parts in the string representation.
        /// </summary>
        /// <remarks>
        /// Default is <c>-</c>.
        /// </remarks>
        public char Separator { get; set; } = '-';

        /// <summary>
        /// Settings that control the behavior of the union id kinds.
        /// </summary>
        public UnionIdKindSettings KindSettings { get; set; } = UnionIdKindSettings.None;

        /// <summary>
        /// Settings that control the behavior of the parsable struct converter.
        /// </summary>
        public ParsableStructConverterSettings ConverterSettings { get; set; } = ParsableStructConverterSettings.None;
    }

    /// <summary>
    /// Place this attribute on a struct annotateed with [UnionId] to specify the kinds of values for the union id.
    /// </summary>
    /// <seealso cref="UnionIdAttribute"/>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class UnionIdKindAttribute : Attribute
    {
        public Type KindType { get; }

        public ulong Order { get; }

        public string DisplayName { get; }

        /// <summary>
        /// Indicates whether the kind type is an signed or unsigned integer.
        /// </summary>
        public bool Signed { get; }

        /// <param name="kindType">The type size must be 8 bytes or lesser.</param>
        public UnionIdKindAttribute(Type kindType, ulong order, string displayName = "", bool signed = false)
        {
            KindType = kindType;
            Order = order;
            DisplayName = displayName;
            Signed = signed;
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

        /// <summary>
        /// Indicates whether the kind type is an signed or unsigned integer.
        /// </summary>
        public bool Signed { get; }

        /// <param name="idType">The type that is annotated with [UnionId].</param>
        public KindForUnionIdAttribute(Type idType, ulong order, string displayName = "", bool signed = false)
        {
            IdType = idType;
            Order = order;
            DisplayName = displayName;
            Signed = signed;
        }
    }
}
