using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Data
{
    using static Helpers;

    /// <summary>
    /// Roslyn diagnostic analyzer that validates <c>[Data]</c>-attributed types and reports
    /// issues that are intentionally excluded from the source generator itself to keep the
    /// incremental pipeline cache-friendly.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class DataDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  DiagnosticDescriptors.CannotDecorateImmutableDataWithFieldPolicyAttribute
                , DiagnosticDescriptors.ImmutableDataFieldMustBePrivate
                , DiagnosticDescriptors.OnlyPrivateOrInitOnlySetterIsAllowed
                , DiagnosticDescriptors.CollectionIsNotApplicableForProperty
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            // Only process [Data]-attributed types
            if (typeSymbol.GetAttribute(DATA_ATTRIBUTE_METADATA) is null
                && typeSymbol.GetAttribute($"global::{DATA_ATTRIBUTE_METADATA}") is null
            )
            {
                // Try matching by full qualified metadata name directly
                var hasAttr = false;

                foreach (var attr in typeSymbol.GetAttributes())
                {
                    if (attr.AttributeClass?.ToDisplayString() is { } fullName
                        && (fullName == "EncosyTower.Data.DataAttribute"
                            || fullName == "EncosyTower.Data.Data")
                    )
                    {
                        hasAttr = true;
                        break;
                    }
                }

                if (hasAttr == false)
                {
                    return;
                }
            }

            var typeLocation = typeSymbol.Locations.Length > 0
                ? typeSymbol.Locations[0]
                : Location.None;

            // ── DATA_0003: immutable + [DataFieldPolicy] ────────────────────────────────────
            var isMutable = typeSymbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE) is not null;
            var fieldPolicyAttrib = typeSymbol.GetAttribute(DATA_FIELD_POLICY_ATTRIBUTE);

            if (isMutable == false && fieldPolicyAttrib != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DiagnosticDescriptors.CannotDecorateImmutableDataWithFieldPolicyAttribute
                    , typeLocation
                    , typeSymbol.Name
                ));
            }

            // Determine setter restrictions
            var withoutPropertySetters = false;

            if (isMutable)
            {
                var mutableAttrib = typeSymbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE);

                if (mutableAttrib != null)
                {
                    var args = mutableAttrib.ConstructorArguments;

                    if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
                    {
                        var options = (DataMutableOptions)(int)args[0].Value;
                        withoutPropertySetters = options.HasFlag(DataMutableOptions.WithoutPropertySetters);
                    }
                }
            }

            var allowOnlyPrivateOrInitSetter = isMutable == false || withoutPropertySetters;
            var withoutId = typeSymbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE) is not null;

            // ── Analyze members ─────────────────────────────────────────────────────────────
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                {
                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE) == false)
                    {
                        continue;
                    }

                    // DATA_0004: immutable data field must be private
                    if (isMutable == false
                        && field.DeclaredAccessibility != Accessibility.Private
                    )
                    {
                        var fieldLocation = field.Locations.Length > 0
                            ? field.Locations[0]
                            : Location.None;

                        context.ReportDiagnostic(Diagnostic.Create(
                              DiagnosticDescriptors.ImmutableDataFieldMustBePrivate
                            , fieldLocation
                            , typeSymbol.Name
                        ));
                    }

                    // DATA_0006: [SerializeField] field named to generate "Id" property but is a collection
                    if (withoutId == false)
                    {
                        var propertyName = field.ToPropertyName();

                        if (string.Equals(propertyName, "Id", System.StringComparison.Ordinal))
                        {
                            if (IsCollectionType(field.Type))
                            {
                                var fieldLocation = field.Locations.Length > 0
                                    ? field.Locations[0]
                                    : Location.None;

                                context.ReportDiagnostic(Diagnostic.Create(
                                      DiagnosticDescriptors.CollectionIsNotApplicableForProperty
                                    , fieldLocation
                                    , field.Type.Name
                                    , "Id"
                                ));
                            }
                        }
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    // DATA_0005: only private or init-only setter allowed
                    if (allowOnlyPrivateOrInitSetter
                        && property.SetMethod is { } setter
                        && setter.IsInitOnly == false
                        && setter.DeclaredAccessibility != Accessibility.Private
                    )
                    {
                        var propLocation = property.Locations.Length > 0
                            ? property.Locations[0]
                            : Location.None;

                        context.ReportDiagnostic(Diagnostic.Create(
                              DiagnosticDescriptors.OnlyPrivateOrInitOnlySetterIsAllowed
                            , propLocation
                            , typeSymbol.Name
                        ));
                    }

                    // DATA_0006: [DataProperty] property named "Id" but is a collection
                    if (withoutId == false
                        && property.GetAttribute(DATA_PROPERTY_ATTRIBUTE) is not null
                        && string.Equals(property.Name, "Id", System.StringComparison.Ordinal)
                        && IsCollectionType(property.Type)
                    )
                    {
                        var propLocation = property.Locations.Length > 0
                            ? property.Locations[0]
                            : Location.None;

                        context.ReportDiagnostic(Diagnostic.Create(
                              DiagnosticDescriptors.CollectionIsNotApplicableForProperty
                            , propLocation
                            , property.Type.Name
                            , "Id"
                        ));
                    }
                }
            }
        }

        private static bool IsCollectionType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol)
            {
                return true;
            }

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType == false)
                {
                    return false;
                }

                var fullName = namedType.OriginalDefinition.ToDisplayString();

                return fullName is
                    "System.Collections.Generic.List<T>"
                    or "System.Collections.Generic.Dictionary<TKey, TValue>"
                    or "System.Collections.Generic.HashSet<T>"
                    or "System.Collections.Generic.Queue<T>"
                    or "System.Collections.Generic.Stack<T>"
                    or "System.Collections.Generic.IList<T>"
                    or "System.Collections.Generic.ISet<T>"
                    or "System.Collections.Generic.IReadOnlyList<T>"
                    or "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>"
                    or "System.Collections.Generic.IDictionary<TKey, TValue>"
                    or "System.ReadOnlyMemory<T>"
                    or "System.Memory<T>"
                    or "System.ReadOnlySpan<T>"
                    or "System.Span<T>"
                    or "EncosyTower.Collections.ListFast<T>"
                    ;
            }

            return false;
        }
    }
}
