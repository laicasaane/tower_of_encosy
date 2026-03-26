using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for types involved in the
    /// <c>[EnumTemplate]</c> / <c>[EnumMembersForTemplate]</c> / <c>[TypeAsEnumMemberForTemplate]</c>
    /// attribute system.
    /// <para>
    /// Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator itself prevents unnecessary regeneration of source files when only
    /// an error (and not the surrounding valid code) has changed.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class EnumTemplateAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string ENUM_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumTemplateAttribute";
        private const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumMembersForTemplateAttribute";
        private const string TYPE_AS_MEMBER_ATTRIBUTE = $"global::{NAMESPACE}.TypeAsEnumMemberForTemplateAttribute";
        private const string MEMBERS_FROM_ENUM_ATTRIBUTE = $"global::{NAMESPACE}.EnumTemplateMembersFromEnumAttribute";
        private const string MEMBER_FROM_TYPE_ATTRIBUTE = $"global::{NAMESPACE}.EnumTemplateMemberFromTypeAttribute";

        public static readonly DiagnosticDescriptor NotEndWithTemplateSuffix = new(
              id: "ENUM_TEMPLATE_0001"
            , title: "Not end with template suffix"
            , messageFormat: "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
        );

        public static readonly DiagnosticDescriptor NotSupportUnderlyingType = new(
              id: "ENUM_TEMPLATE_0002"
            , title: "Not support underlying type"
            , messageFormat: "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
        );

        public static readonly DiagnosticDescriptor TypeAlreadyDeclared = new(
              id: "ENUM_TEMPLATE_0003"
            , title: "Type has already been declared"
            , messageFormat: "Type \"{0}\" has already been declared"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type has already been declared"
        );

        public static readonly DiagnosticDescriptor MustSpecifyTypeAndOrder = new(
              id: "ENUM_TEMPLATE_0004"
            , title: "Must specify type and order"
            , messageFormat: "Must specify type and order"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Must specify type and order"
        );

        public static readonly DiagnosticDescriptor MustBeTypeOfExpression = new(
              id: "ENUM_TEMPLATE_0005"
            , title: "First argument must be a type-of expression"
            , messageFormat: "First argument must be a type-of expression"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "First argument must be a type-of expression"
        );

        public static readonly DiagnosticDescriptor SameMemberIsIgnored = new(
              id: "ENUM_TEMPLATE_0006"
            , title: "A member of the same name will be ignored"
            , messageFormat: "Member \"{0}\" is ignored because it already exists in another source \"{1}\""
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A member of the same name will be ignored"
        );

        public static readonly DiagnosticDescriptor NotSupportUnboundGenericType = new(
              id: "ENUM_TEMPLATE_0007"
            , title: "Unbound generic type not supported"
            , messageFormat: "\"{0}\" must be a non-generic type or a closed generic type"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Only non-generic types or fully closed generic types are supported"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  NotEndWithTemplateSuffix
                , NotSupportUnderlyingType
                , TypeAlreadyDeclared
                , MustSpecifyTypeAndOrder
                , MustBeTypeOfExpression
                , SameMemberIsIgnored
                , NotSupportUnboundGenericType
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            // Validate template structs annotated with [EnumTemplate].
            if (typeSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE))
            {
                AnalyzeTemplateStruct(context, typeSymbol);
                return;
            }

            // Validate types annotated with external membership attributes.
            var attrib = typeSymbol.GetAttribute(ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE);

            if (attrib != null)
            {
                AnalyzeMembershipAttribute(context, typeSymbol, attrib, isEnumMembers: true);
                return;
            }

            attrib = typeSymbol.GetAttribute(TYPE_AS_MEMBER_ATTRIBUTE);

            if (attrib != null)
            {
                AnalyzeMembershipAttribute(context, typeSymbol, attrib, isEnumMembers: false);
            }
        }

        private static void AnalyzeTemplateStruct(SymbolAnalysisContext context, INamedTypeSymbol templateSymbol)
        {
            // Rule 0001: naming convention.
            var name = templateSymbol.Name;

            if (name.IndexOf("_EnumTemplate", System.StringComparison.Ordinal) <= 0
                && name.IndexOf("_Template", System.StringComparison.Ordinal) <= 0
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      NotEndWithTemplateSuffix
                    , templateSymbol.Locations[0]
                ));
            }

            // Validate inline membership attributes on the template struct.
            var attributes = templateSymbol.GetAttributes(MEMBERS_FROM_ENUM_ATTRIBUTE, MEMBER_FROM_TYPE_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                if (attrib == null)
                {
                    continue;
                }

                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? templateSymbol.Locations[0];

                if (attrib.ConstructorArguments.Length < 2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MustSpecifyTypeAndOrder, location));
                    continue;
                }

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol memberType
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(MustBeTypeOfExpression, location));
                    continue;
                }

                if (memberType.TypeKind == TypeKind.Enum)
                {
                    ValidateEnumUnderlyingType(context, memberType, location);
                }
                else if (memberType.IsUnboundGenericType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          NotSupportUnboundGenericType
                        , location
                        , memberType.ToDisplayString()
                    ));
                }
            }
        }

        private static void AnalyzeMembershipAttribute(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , AttributeData attrib
            , bool isEnumMembers
        )
        {
            var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                ?? typeSymbol.Locations[0];

            if (attrib.ConstructorArguments.Length < 2)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustSpecifyTypeAndOrder, location));
                return;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol templateSymbol
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(MustBeTypeOfExpression, location));
                return;
            }

            // Validate the template target has the required attribute.
            if (templateSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE) == false)
            {
                return;
            }

            if (isEnumMembers)
            {
                ValidateEnumUnderlyingType(context, typeSymbol, location);
            }
            else if (typeSymbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      NotSupportUnboundGenericType
                    , location
                    , typeSymbol.ToDisplayString()
                ));
            }
        }

        private static void ValidateEnumUnderlyingType(
              SymbolAnalysisContext context
            , INamedTypeSymbol enumSymbol
            , Location location
        )
        {
            if (enumSymbol.EnumUnderlyingType is not INamedTypeSymbol underlyingType
                || IsSupportedEnum(underlyingType.SpecialType) == false
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(NotSupportUnderlyingType, location));
            }
        }

        internal static bool IsSupportedEnum(SpecialType type)
        {
            return type switch {
                SpecialType.System_Byte => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_UInt64 => true,
                _ => false,
            };
        }
    }
}
