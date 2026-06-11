using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    internal struct EnumTemplateSpec : IEquatable<EnumTemplateSpec>
    {
        private const string MEMBERS_FROM_ENUM_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMembersFromEnumAttribute";
        private const string MEMBER_FROM_TYPE_ATTRIBUTE = "global::EncosyTower.EnumExtensions.EnumTemplateMemberFromTypeAttribute";

        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string templateFullName;
        public string templateSimpleName;
        public string fileHintName;
        public Accessibility accessibility;
        public bool parentIsNamespace;
        public string namespaceName;
        public EquatableArray<string> containingTypes;
        public EquatableArray<TemplateMemberSpec> inlineMembers;

        public readonly bool IsValid => location.IsValid;

        public static EnumTemplateSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol templateSymbol)
            {
                return default;
            }

            if (templateSymbol.IsUnmanagedType == false || templateSymbol.IsUnboundGenericType)
            {
                return default;
            }

            var syntax = context.TargetNode;
            var location = LocationInfo.From(syntax.GetLocation());

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var containingTypes = templateSymbol.GetContainingTypes(token);
            var ns = templateSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;
            var templateFullName = templateSymbol.ToFullName();
            var attributes = templateSymbol.GetAttributes(MEMBERS_FROM_ENUM_ATTRIBUTE, MEMBER_FROM_TYPE_ATTRIBUTE, token);

            using var inlineMemberBuilder = ImmutableArrayBuilder<TemplateMemberSpec>.Rent();

            foreach (var attrib in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attrib == null || attrib.ConstructorArguments.Length < 2)
                {
                    continue;
                }

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol typeSymbol
                )
                {
                    continue;
                }

                var attribLocation = LocationInfo.From(
                    attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                    ?? Location.None
                );

                var candidate = TemplateMemberSpec.Extract(
                      typeSymbol
                    , templateFullName
                    , attrib
                    , typeSymbol.TypeKind == TypeKind.Enum
                    , attribLocation
                    , token
                );

                if (candidate.IsValid)
                {
                    inlineMemberBuilder.Add(candidate);
                }
            }

            return new EnumTemplateSpec {
                location = location,
                openingSource = openingSource,
                closingSource = closingSource,
                templateFullName = templateFullName,
                templateSimpleName = templateSymbol.Name,
                fileHintName = templateSymbol.ToFileName(),
                accessibility = templateSymbol.DeclaredAccessibility,
                parentIsNamespace = syntax.Parent is BaseNamespaceDeclarationSyntax,
                namespaceName = namespaceName,
                containingTypes = containingTypes,
                inlineMembers = new EquatableArray<TemplateMemberSpec>(inlineMemberBuilder.ToImmutable()),
            };
        }

        public readonly bool Equals(EnumTemplateSpec other)
            => string.Equals(templateFullName, other.templateFullName, StringComparison.Ordinal)
            && string.Equals(templateSimpleName, other.templateSimpleName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && containingTypes.Equals(other.containingTypes)
            && inlineMembers.Equals(other.inlineMembers)
            ;

        public readonly override bool Equals(object obj)
            => obj is EnumTemplateSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(templateFullName, templateSimpleName, namespaceName)
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(containingTypes.GetHashCode())
            .Add(inlineMembers.GetHashCode());

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SC = global::System.Collections;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETCol = global::EncosyTower.Collections;");
            p.PrintLine("using ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
