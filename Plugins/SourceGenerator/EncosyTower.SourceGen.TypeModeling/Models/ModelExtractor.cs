using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public static class ModelExtractor
    {
        public static TypeModel ToModel(
            this INamedTypeSymbol symbol,
            CancellationToken token,
            ModelOptions options = default)
        {
            token.ThrowIfCancellationRequested();

            var parts = options.Parts == 0 ? ModelParts.All : options.Parts;

            var interfaces = ExtractInterfaces(symbol, parts, token);
            var attributes = ExtractTypeAttributes(symbol, parts, token);
            var fields = ExtractFields(symbol, parts, options, token);
            var properties = ExtractProperties(symbol, parts, options, token);
            var methods = ExtractMethods(symbol, parts, options, token);
            var constructors = ExtractConstructors(symbol, parts, options, token);
            var events = ExtractEvents(symbol, parts, options, token);

            return new TypeModel(
                name: symbol.Name,
                fullName: symbol.ToDisplayString(SymbolFormats.FullyQualified),
                @namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                accessibility: symbol.DeclaredAccessibility.ToKeyword(),
                typeKind: symbol.TypeKind,
                isStatic: symbol.IsStatic,
                isSealed: symbol.IsSealed,
                isAbstract: symbol.IsAbstract,
                isReadOnly: symbol.IsReadOnly,
                isRecord: symbol.IsRecord,
                isGeneric: symbol.IsGenericType,
                interfaces: interfaces,
                attributes: attributes,
                fields: fields,
                properties: properties,
                methods: methods,
                constructors: constructors,
                events: events
            );
        }

        private static EquatableArray<string> ExtractInterfaces(
            INamedTypeSymbol symbol,
            ModelParts parts,
            CancellationToken token)
        {
            if ((parts & ModelParts.Interfaces) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<string>.Rent();
            foreach (var iface in symbol.Interfaces)
            {
                builder.Add(iface.ToDisplayString(SymbolFormats.FullyQualified));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<AttributeModel> ExtractTypeAttributes(
            INamedTypeSymbol symbol,
            ModelParts parts,
            CancellationToken token)
        {
            if ((parts & ModelParts.Attributes) == 0)
                return default;

            token.ThrowIfCancellationRequested();
            return ExtractAttributes(symbol.GetAttributes(), token);
        }

        private static EquatableArray<AttributeModel> ExtractAttributes(
            ImmutableArray<AttributeData> attrData,
            CancellationToken token)
        {
            if (attrData.IsDefaultOrEmpty)
                return default;

            using var builder = ImmutableArrayBuilder<AttributeModel>.Rent();
            foreach (var attr in attrData)
            {
                token.ThrowIfCancellationRequested();

                var fullName = attr.AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

                using var ctorArgs = ImmutableArrayBuilder<string>.Rent();
                foreach (var arg in attr.ConstructorArguments)
                {
                    ctorArgs.Add(arg.Value?.ToString() ?? string.Empty);
                }

                using var namedArgs = ImmutableArrayBuilder<AttributeNamedArgModel>.Rent();
                foreach (var pair in attr.NamedArguments)
                {
                    namedArgs.Add(new AttributeNamedArgModel(pair.Key, pair.Value.Value?.ToString() ?? string.Empty));
                }

                builder.Add(new AttributeModel(fullName, ctorArgs.ToImmutable(), namedArgs.ToImmutable()));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<FieldModel> ExtractFields(
            INamedTypeSymbol symbol,
            ModelParts parts,
            ModelOptions options,
            CancellationToken token)
        {
            if ((parts & ModelParts.Fields) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<FieldModel>.Rent();
            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IFieldSymbol field) continue;
                if (!options.IncludeCompilerGenerated && field.IsImplicitlyDeclared) continue;
                if (!options.IncludeNonPublic && field.DeclaredAccessibility != Accessibility.Public) continue;

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(field.GetAttributes(), token)
                    : default;

                builder.Add(new FieldModel(
                    name: field.Name,
                    typeName: field.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal),
                    typeFullName: field.Type.ToDisplayString(SymbolFormats.FullyQualified),
                    accessibility: field.DeclaredAccessibility.ToKeyword(),
                    isReadOnly: field.IsReadOnly,
                    isStatic: field.IsStatic,
                    isConst: field.IsConst,
                    constantValueText: field.ConstantValue?.ToString() ?? string.Empty,
                    refKind: field.RefKind,
                    attributes: attrs
                ));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<PropertyModel> ExtractProperties(
            INamedTypeSymbol symbol,
            ModelParts parts,
            ModelOptions options,
            CancellationToken token)
        {
            if ((parts & ModelParts.Properties) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<PropertyModel>.Rent();
            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol prop) continue;
                if (!options.IncludeCompilerGenerated && prop.IsImplicitlyDeclared) continue;
                if (!options.IncludeNonPublic && prop.DeclaredAccessibility != Accessibility.Public) continue;

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(prop.GetAttributes(), token)
                    : default;

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                foreach (var p in prop.Parameters)
                {
                    paramBuilder.Add(BuildParameterModel(p));
                }

                builder.Add(new PropertyModel(
                    name: prop.Name,
                    typeName: prop.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal),
                    typeFullName: prop.Type.ToDisplayString(SymbolFormats.FullyQualified),
                    accessibility: prop.DeclaredAccessibility.ToKeyword(),
                    refKind: prop.RefKind,
                    isStatic: prop.IsStatic,
                    isIndexer: prop.IsIndexer,
                    getter: BuildAccessorModel(prop.GetMethod),
                    setter: BuildAccessorModel(prop.SetMethod),
                    parameters: paramBuilder.ToImmutable(),
                    attributes: attrs
                ));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<MethodModel> ExtractMethods(
            INamedTypeSymbol symbol,
            ModelParts parts,
            ModelOptions options,
            CancellationToken token)
        {
            if ((parts & ModelParts.Methods) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<MethodModel>.Rent();
            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method) continue;
                if (method.MethodKind != MethodKind.Ordinary) continue;
                if (!options.IncludeCompilerGenerated && method.IsImplicitlyDeclared) continue;
                if (!options.IncludeNonPublic && method.DeclaredAccessibility != Accessibility.Public) continue;

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(method.GetAttributes(), token)
                    : default;

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                foreach (var p in method.Parameters)
                {
                    paramBuilder.Add(BuildParameterModel(p));
                }

                using var typeParamBuilder = ImmutableArrayBuilder<string>.Rent();
                foreach (var tp in method.TypeParameters)
                {
                    typeParamBuilder.Add(tp.Name);
                }

                builder.Add(new MethodModel(
                    name: method.Name,
                    returnTypeName: method.ReturnType.ToDisplayString(SymbolFormats.SimpleNoGlobal),
                    returnTypeFullName: method.ReturnType.ToDisplayString(SymbolFormats.FullyQualified),
                    accessibility: method.DeclaredAccessibility.ToKeyword(),
                    refKind: method.RefKind,
                    returnsVoid: method.ReturnsVoid,
                    isStatic: method.IsStatic,
                    isReadOnly: method.IsReadOnly,
                    isAbstract: method.IsAbstract,
                    isVirtual: method.IsVirtual,
                    methodKind: method.MethodKind,
                    parameters: paramBuilder.ToImmutable(),
                    typeParameters: typeParamBuilder.ToImmutable(),
                    attributes: attrs
                ));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<ConstructorModel> ExtractConstructors(
            INamedTypeSymbol symbol,
            ModelParts parts,
            ModelOptions options,
            CancellationToken token)
        {
            if ((parts & ModelParts.Constructors) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<ConstructorModel>.Rent();
            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method) continue;
                if (method.MethodKind != MethodKind.Constructor) continue;
                if (!options.IncludeCompilerGenerated && method.IsImplicitlyDeclared) continue;
                if (!options.IncludeNonPublic && method.DeclaredAccessibility != Accessibility.Public) continue;

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                foreach (var p in method.Parameters)
                {
                    paramBuilder.Add(BuildParameterModel(p));
                }

                builder.Add(new ConstructorModel(
                    accessibility: method.DeclaredAccessibility.ToKeyword(),
                    isStatic: method.IsStatic,
                    parameters: paramBuilder.ToImmutable()
                ));
            }
            return builder.ToImmutable();
        }

        private static EquatableArray<EventModel> ExtractEvents(
            INamedTypeSymbol symbol,
            ModelParts parts,
            ModelOptions options,
            CancellationToken token)
        {
            if ((parts & ModelParts.Events) == 0)
                return default;

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<EventModel>.Rent();
            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IEventSymbol ev) continue;
                if (!options.IncludeCompilerGenerated && ev.IsImplicitlyDeclared) continue;
                if (!options.IncludeNonPublic && ev.DeclaredAccessibility != Accessibility.Public) continue;

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(ev.GetAttributes(), token)
                    : default;

                builder.Add(new EventModel(
                    name: ev.Name,
                    typeName: ev.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal),
                    typeFullName: ev.Type.ToDisplayString(SymbolFormats.FullyQualified),
                    accessibility: ev.DeclaredAccessibility.ToKeyword(),
                    isStatic: ev.IsStatic,
                    attributes: attrs
                ));
            }
            return builder.ToImmutable();
        }

        private static AccessorModel BuildAccessorModel(IMethodSymbol accessor)
        {
            if (accessor == null)
                return new AccessorModel(false, string.Empty, false, false, RefKind.None);

            return new AccessorModel(
                exists: true,
                accessibility: accessor.DeclaredAccessibility.ToKeyword(),
                isReadOnly: accessor.IsReadOnly,
                isInitOnly: accessor.IsInitOnly,
                refKind: accessor.RefKind
            );
        }

        private static ParameterModel BuildParameterModel(IParameterSymbol p)
        {
            return new ParameterModel(
                name: p.Name,
                typeName: p.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal),
                typeFullName: p.Type.ToDisplayString(SymbolFormats.FullyQualified),
                refKind: p.RefKind,
                ordinal: p.Ordinal,
                hasDefaultValue: p.HasExplicitDefaultValue,
                defaultValueText: p.HasExplicitDefaultValue ? (p.ExplicitDefaultValue?.ToString() ?? string.Empty) : string.Empty
            );
        }
    }
}
