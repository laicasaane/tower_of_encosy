using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Data
{
    using static Helpers;

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

            if (typeSymbol.GetAttribute(DATA_ATTRIBUTE_METADATA) is null
                && typeSymbol.GetAttribute($"global::{DATA_ATTRIBUTE_METADATA}") is null
            )
            {
                var hasAttr = false;

                foreach (var attr in typeSymbol.GetAttributes())
                {
                    if (attr.AttributeClass is { } attrClass
                        && (attrClass.HasFullName("EncosyTower.Data.DataAttribute")
                            || attrClass.HasFullName("EncosyTower.Data.Data"))
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

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                {
                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE) == false)
                    {
                        continue;
                    }

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

                var origDef = namedType.OriginalDefinition;

                return origDef.HasFullName("System.Collections.Generic.List<T>")
                    || origDef.HasFullName("System.Collections.Generic.Dictionary<TKey, TValue>")
                    || origDef.HasFullName("System.Collections.Generic.HashSet<T>")
                    || origDef.HasFullName("System.Collections.Generic.Queue<T>")
                    || origDef.HasFullName("System.Collections.Generic.Stack<T>")
                    || origDef.HasFullName("System.Collections.Generic.IList<T>")
                    || origDef.HasFullName("System.Collections.Generic.ISet<T>")
                    || origDef.HasFullName("System.Collections.Generic.IReadOnlyList<T>")
                    || origDef.HasFullName("System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>")
                    || origDef.HasFullName("System.Collections.Generic.IDictionary<TKey, TValue>")
                    || origDef.HasFullName("System.ReadOnlyMemory<T>")
                    || origDef.HasFullName("System.Memory<T>")
                    || origDef.HasFullName("System.ReadOnlySpan<T>")
                    || origDef.HasFullName("System.Span<T>")
                    || origDef.HasFullName("EncosyTower.Collections.ListFast<T>")
                    ;
            }

            return false;
        }
    }
}
