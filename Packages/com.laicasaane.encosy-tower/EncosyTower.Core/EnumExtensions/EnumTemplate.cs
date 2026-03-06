using System;

namespace EncosyTower.EnumExtensions
{
    public interface IEnumTemplate<T> where T : unmanaged, Enum { }

    /// <summary>
    /// Place on a struct whose name ends with either "_EnumTemplate" or "_Template"
    /// to indicate a template for generating an enum.
    /// </summary>
    /// <example>
    /// <code>
    /// [EnumTemplate]
    /// public readonly struct ProductType_EnumTemplate { }
    ///
    /// // Will generate an enum named 'ProductType'
    /// </code>
    /// </example>
    /// <seealso cref="EnumMembersForTemplateAttribute"/>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class EnumTemplateAttribute : Attribute
    {
    }

    /// <summary>
    /// Place on the struct decorated with [EnumTemplate]
    /// and declare from which enum type the members will be taken.
    /// </summary>
    /// <remarks>
    /// Members of <see cref="EnumType"/> will be added to the enum generated from [EnumTemplate].
    /// </remarks>
    /// <example>
    /// <code>
    /// [EnumTemplate]
    /// [EnumTemplateMembersFromEnum(typeof(FruitType)), 000]
    /// public readonly struct ProductType_EnumTemplate { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class EnumTemplateMembersFromEnumAttribute : Attribute
    {
        public Type EnumType { get; }

        public ulong Order { get; }

        /// <param name="enumType">To provide members for the enum generated from [EnumTemplate]</param>
        /// <param name="order"></param>
        public EnumTemplateMembersFromEnumAttribute(Type enumType, ulong order)
        {
            EnumType = enumType;
            Order = order;
        }
    }

    /// <summary>
    /// Place on the struct decorated with [EnumTemplate]
    /// and declare from which type the member will be made.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The name of <see cref="Type"/> will be used to made a member for the enum generated from [EnumTemplate].</item>
    /// <item>If <see cref="AlternateName"/> is provided, it will be used as the member name instead of the type name.</item>
    /// <item>Cannot use with unbound generic types.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [EnumTemplate]
    /// [EnumTemplateMemberFromType(typeof(CustomFruit&lt;int&gt;)), 500]
    /// public readonly struct ProductType_EnumTemplate { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class EnumTemplateMemberFromTypeAttribute : Attribute
    {
        public Type Type { get; }

        public ulong Order { get; }

        public string DisplayName { get; }

        public string AlternateName { get; }

        public EnumTemplateMemberFromTypeAttribute(
              Type type
            , ulong order
            , string displayName = ""
            , string alternateName = ""
        )
        {
            Type = type;
            Order = order;
            DisplayName = displayName;
            AlternateName = alternateName;
        }
    }

    /// <summary>
    /// Place on an enum whose members will be taken to
    /// add to the enum generated from [EnumTemplate].
    /// </summary>
    /// <example>
    /// <code>
    /// [EnumMembersForTemplate(typeof(ProductType_EnumTemplate), 000)]
    /// public enum FruitType { Apple, Orange }
    ///
    /// [EnumMembersForTemplate(typeof(ProductType_EnumTemplate), 100)]
    /// public enum GrainType { Rice, Wheat }
    /// </code>
    /// </example>
    /// <seealso cref="EnumTemplateAttribute"/>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
    public sealed class EnumMembersForTemplateAttribute : Attribute
    {
        public Type TemplateType { get; }

        public ulong Order { get; }

        public EnumMembersForTemplateAttribute(Type templateType, ulong order)
        {
            TemplateType = templateType;
            Order = order;
        }
    }

    /// <summary>
    /// Place on a type to indicate that its type name or an alias will be used to
    /// generate a member for the enum generated from [EnumTemplate].
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>If <see cref="AlternateName"/> is provided, it will be used instead of the type name.</item>
    /// <item>Cannot use with unbound generic types.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypeAsEnumMemberForTemplate(typeof(ProductType_EnumTemplate), 500)]
    /// public struct CustomFruit { }
    /// </code>
    /// </example>
    /// <seealso cref="EnumTemplateAttribute"/>
    [AttributeUsage(
          AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface
        , AllowMultiple = true
    )]
    public sealed class TypeAsEnumMemberForTemplateAttribute : Attribute
    {
        public Type TemplateType { get; }

        public ulong Order { get; }

        public string DisplayName { get; }

        public string AlternateName { get; }

        public TypeAsEnumMemberForTemplateAttribute(
              Type templateType
            , ulong order
            , string displayName = ""
            , string alternateName = ""
        )
        {
            TemplateType = templateType;
            Order = order;
            DisplayName = displayName;
            AlternateName = alternateName;
        }
    }
}

