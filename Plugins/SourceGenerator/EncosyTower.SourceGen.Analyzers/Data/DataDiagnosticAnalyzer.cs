using System.Collections.Immutable;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Data
{
    using static EncosyTower.SourceGen.Helpers.Data.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class DataDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor CannotDecorateImmutableDataWithFieldPolicyAttribute = new(
              id: "SG_DATA_0001"
            , title: "Cannot decorate immutable data with [DataFieldPolicy] attribute"
            , messageFormat: "\"{0}\" is immutable thus cannot be decorated with [DataFieldPolicy] attribute (are you missing [DataMutable] attribute?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The data type must already decorated with [DataMutable] to be able have [DataFieldPolicy]."
        );

        public static readonly DiagnosticDescriptor ImmutableDataFieldMustBePrivate = new(
              id: "SG_DATA_0002"
            , title: "Fields of immutable data must be private"
            , messageFormat: "\"{0}\" is immutable thus its fields must be private (are you missing [DataMutable] attribute?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The data type must already decorated with [DataMutable] to be able have non-private fields."
        );

        public static readonly DiagnosticDescriptor OnlyPrivateOrInitOnlySetterIsAllowed = new(
              id: "SG_DATA_0003"
            , title: "Only private setter or init-only setter is allowed because the type is either immutable or decorated with [DataMutable(withoutPropertySetter: true)]"
            , messageFormat: "Only private setter or init-only setter is allowed because \"{0}\" is either immutable or decorated with [DataMutable(withoutPropertySetter: true)]"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Use private setter, or init-only setter, or decorate the type with [DataMutable(withoutPropertySetter: false)]."
        );

        public static readonly DiagnosticDescriptor CollectionIsNotApplicableForProperty = new(
              id: "SG_DATA_0004"
            , title: "Collection type is not applicable for the property"
            , messageFormat: "Type \"{0}\" is a collection thus it is not applicable for the \"{1}\" property"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Collection type is not applicable for the property."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  CannotDecorateImmutableDataWithFieldPolicyAttribute
                , ImmutableDataFieldMustBePrivate
                , OnlyPrivateOrInitOnlySetterIsAllowed
                , CollectionIsNotApplicableForProperty
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var  token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            if (HasDataAttribute(typeSymbol, token) == false)
            {
                return;
            }

            var typeLocation = typeSymbol.Locations.Length > 0
                ? typeSymbol.Locations[0]
                : Location.None;

            var isMutable = typeSymbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE, token) is not null;
            var withoutId = typeSymbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE, token) is not null;
            var fieldPolicyAttrib = typeSymbol.GetAttribute(DATA_FIELD_POLICY_ATTRIBUTE, token);

            if (isMutable == false && fieldPolicyAttrib != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      CannotDecorateImmutableDataWithFieldPolicyAttribute
                    , typeLocation
                    , typeSymbol.Name
                ));
            }

            var withoutPropertySetters = ReadWithoutPropertySettersFlag(typeSymbol, isMutable, token);
            var allowOnlyPrivateOrInitSetter = isMutable == false || withoutPropertySetters;

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                switch (member)
                {
                    case IFieldSymbol field:
                        AnalyzeField(context, typeSymbol, field, isMutable, withoutId);
                        break;

                    case IPropertySymbol property:
                        AnalyzeProperty(context, typeSymbol, property, allowOnlyPrivateOrInitSetter, withoutId);
                        break;
                }
            }
        }

        private static bool HasDataAttribute(INamedTypeSymbol typeSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (typeSymbol.GetAttribute(DATA_ATTRIBUTE, token) is not null
                || typeSymbol.GetAttribute($"global::{DATA_ATTRIBUTE}", token) is not null
            )
            {
                return true;
            }

            foreach (var attr in typeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attr.AttributeClass is { } attrClass
                    && (attrClass.HasFullName("EncosyTower.Data.DataAttribute", token)
                        || attrClass.HasFullName("EncosyTower.Data.Data", token))
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ReadWithoutPropertySettersFlag(
              INamedTypeSymbol typeSymbol
            , bool isMutable
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (isMutable == false)
            {
                return false;
            }

            var mutableAttrib = typeSymbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE, token);

            if (mutableAttrib == null)
            {
                return false;
            }

            var args = mutableAttrib.ConstructorArguments;

            if (args.Length == 0 || args[0].Kind != TypedConstantKind.Enum)
            {
                return false;
            }

            var options = (DataMutableOptions)(int)args[0].Value;
            return options.HasFlag(DataMutableOptions.WithoutPropertySetters);
        }

        private static void AnalyzeField(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , IFieldSymbol field
            , bool isMutable
            , bool withoutId
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE, token) == false)
            {
                return;
            }

            if (isMutable == false
                && field.DeclaredAccessibility != Accessibility.Private
            )
            {
                var fieldLocation = field.Locations.Length > 0
                    ? field.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      ImmutableDataFieldMustBePrivate
                    , fieldLocation
                    , typeSymbol.Name
                ));
            }

            if (withoutId)
            {
                return;
            }

            var propertyName = field.ToPropertyName();

            if (string.Equals(propertyName, "Id", System.StringComparison.Ordinal)
                && IsCollectionType(field.Type)
            )
            {
                var fieldLocation = field.Locations.Length > 0
                    ? field.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      CollectionIsNotApplicableForProperty
                    , fieldLocation
                    , field.Type.Name
                    , "Id"
                ));
            }
        }

        private static void AnalyzeProperty(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , IPropertySymbol property
            , bool allowOnlyPrivateOrInitSetter
            , bool withoutId
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

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
                      OnlyPrivateOrInitOnlySetterIsAllowed
                    , propLocation
                    , typeSymbol.Name
                ));
            }

            if (withoutId == false
                && property.GetAttribute(DATA_PROPERTY_ATTRIBUTE, token) is not null
                && string.Equals(property.Name, "Id", System.StringComparison.Ordinal)
                && IsCollectionType(property.Type)
            )
            {
                var propLocation = property.Locations.Length > 0
                    ? property.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      CollectionIsNotApplicableForProperty
                    , propLocation
                    , property.Type.Name
                    , "Id"
                ));
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
