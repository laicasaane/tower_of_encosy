namespace EncosyTower.SourceGen.Tests.EnumExtensions;

internal static class EnumExtensionsAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class EnumExtensionsForAttribute : System.Attribute
            {
                public EnumExtensionsForAttribute(System.Type enumType) { }
                public System.Type EnumType { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public sealed class EnumTemplateAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class EnumTemplateMembersFromEnumAttribute : System.Attribute
            {
                public EnumTemplateMembersFromEnumAttribute(System.Type enumType, ulong order) { }
                public System.Type EnumType { get; }
                public ulong Order { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class EnumTemplateMemberFromTypeAttribute : System.Attribute
            {
                public EnumTemplateMemberFromTypeAttribute(
                      System.Type type
                    , ulong order
                    , string displayName = ""
                    , string alternateName = ""
                ) { }
                public System.Type Type { get; }
                public ulong Order { get; }
                public string DisplayName { get; }
                public string AlternateName { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = true)]
            public sealed class EnumMembersForTemplateAttribute : System.Attribute
            {
                public EnumMembersForTemplateAttribute(System.Type templateType, ulong order) { }
                public System.Type TemplateType { get; }
                public ulong Order { get; }
            }

            [System.AttributeUsage(
                  System.AttributeTargets.Class
                | System.AttributeTargets.Struct
                | System.AttributeTargets.Enum
                | System.AttributeTargets.Interface
                , AllowMultiple = true
            )]
            public sealed class TypeAsEnumMemberForTemplateAttribute : System.Attribute
            {
                public TypeAsEnumMemberForTemplateAttribute(
                      System.Type templateType
                    , ulong order
                    , string displayName = ""
                    , string alternateName = ""
                ) { }
                public System.Type TemplateType { get; }
                public ulong Order { get; }
                public string DisplayName { get; }
                public string AlternateName { get; }
            }
        }

        namespace EncosyTower.PolyEnumStructs
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
            public sealed class PolyEnumStructAttribute : System.Attribute
            {
                public bool SortFieldsBySize { get; set; }
                public bool AutoEquatable { get; set; }
                public bool WithEnumExtensions { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
            public sealed class EnumCaseIgnoreAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class PolyEnumFactoryForAttribute : System.Attribute
            {
                public PolyEnumFactoryForAttribute(System.Type type) { }
                public System.Type Type { get; }
            }
        }

        namespace EncosyTower.UnionIds
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public sealed class UnionIdAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class UnionIdKindAttribute : System.Attribute
            {
                public UnionIdKindAttribute(
                      System.Type kindType
                    , ulong order
                    , string name = ""
                    , string displayName = ""
                    , bool signed = false
                ) { }
                public System.Type KindType { get; }
                public ulong Order { get; }
                public string Name { get; }
                public string DisplayName { get; }
                public bool Signed { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Enum)]
            public sealed class KindForUnionIdAttribute : System.Attribute
            {
                public KindForUnionIdAttribute(
                      System.Type idType
                    , ulong order
                    , string name = ""
                    , string displayName = ""
                    , bool signed = false
                ) { }
                public System.Type IdType { get; }
                public ulong Order { get; }
                public string Name { get; }
                public string DisplayName { get; }
                public bool Signed { get; }
            }
        }
        """;
}
