using System;
using EncosyTower.Conversion;
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
        /// <summary>
        /// The type that represents the kind.
        /// </summary>
        public Type KindType { get; }

        /// <summary>
        /// The order of the kind in the union id.
        /// </summary>
        public ulong Order { get; }

        /// <summary>
        /// The display name for the kind.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Indicates whether the kind type is an signed or unsigned integer.
        /// </summary>
        public bool Signed { get; }

        /// <summary>
        /// Indicates which ToString methods are available on <see cref="KindType"/>.
        /// </summary>
        /// <remarks>
        /// This option is used to optimize the source generator.
        /// </remarks>
        public ToStringMethods ToStringMethods { get; }

        /// <summary>
        /// Indicates whether TryParse methods that accept ReadOnlySpan&lt;char&gt;
        /// are available on <see cref="KindType"/>.
        /// </summary>
        /// <remarks>
        /// This option is used to optimize the source generator.
        /// </remarks>
        public TryParseMethodType TryParseSpan { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionIdKindAttribute"/> class.
        /// </summary>
        /// <param name="kindType">The type that represents the kind.</param>
        /// <param name="order">The order of the kind in the union id kinds.</param>
        /// <param name="displayName">The display name for the kind.</param>
        /// <param name="signed">Indicates whether the kind type is an signed or unsigned integer.</param>
        /// <param name="toStringMethods">
        /// Indicates which ToString methods are available on <see cref="KindType"/>.
        /// This option is used to optimize the source generator.
        /// </param>
        /// <param name="tryParseSpan">
        /// Indicates whether TryParse methods that accept ReadOnlySpan&lt;char&gt;
        /// are available on <see cref="KindType"/>.
        /// This option is used to optimize the source generator.
        /// </param>
        public UnionIdKindAttribute(
              Type kindType
            , ulong order
            , string displayName = ""
            , bool signed = false
            , ToStringMethods toStringMethods = ToStringMethods.Default
            , TryParseMethodType tryParseSpan = TryParseMethodType.None
        )
        {
            KindType = kindType;
            Order = order;
            DisplayName = displayName;
            Signed = signed;
            ToStringMethods = toStringMethods;
            TryParseSpan = tryParseSpan;
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
        /// <summary>
        /// The type that is annotated with [UnionId].
        /// </summary>
        public Type IdType { get; }

        /// <summary>
        /// The order of the kind in the union id.
        /// </summary>
        public ulong Order { get; }

        /// <summary>
        /// The display name for the kind.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Indicates whether the kind type is an signed or unsigned integer.
        /// </summary>
        public bool Signed { get; }

        /// <summary>
        /// Indicates which ToString methods are available on this type.
        /// </summary>
        /// <remarks>
        /// This option is used to optimize the source generator.
        /// </remarks>
        public ToStringMethods ToStringMethods { get; }

        /// <summary>
        /// Indicates whether TryParse methods that accept ReadOnlySpan&lt;char&gt; are available on this type.
        /// </summary>
        /// <remarks>
        /// This option is used to optimize the source generator.
        /// </remarks>
        public TryParseMethodType TryParseSpan { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KindForUnionIdAttribute"/> class.
        /// </summary>
        /// <param name="idType">The type that is annotated with [UnionId].</param>
        /// <param name="order">The order of the kind in the union id.</param>
        /// <param name="displayName">The display name for the kind.</param>
        /// <param name="signed">Indicates whether the kind type is an signed or unsigned integer.</param>
        /// <param name="toStringMethods">
        /// Indicates which ToString methods are available on this type.
        /// This option is used to optimize the source generator.
        /// </param>
        /// <param name="tryParseSpan">
        /// Indicates whether TryParse methods that accept ReadOnlySpan&lt;char&gt; are available on this type.
        /// This option is used to optimize the source generator.
        /// </param>
        public KindForUnionIdAttribute(
              Type idType
            , ulong order
            , string displayName = ""
            , bool signed = false
            , ToStringMethods toStringMethods = ToStringMethods.Default
            , TryParseMethodType tryParseSpan = TryParseMethodType.None
        )
        {
            IdType = idType;
            Order = order;
            DisplayName = displayName;
            Signed = signed;
            ToStringMethods = toStringMethods;
            TryParseSpan = tryParseSpan;
        }
    }
}
