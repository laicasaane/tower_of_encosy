using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public static class ModelExtractor
    {
        public static TypeModel ToModel(
              this INamedTypeSymbol symbol
            , CancellationToken token
            , ModelOptions options = default
        )
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
                  name: symbol.Name
                , fullName: symbol.ToDisplayString(SymbolFormats.FullyQualified)
                , @namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty
                , accessibility: symbol.DeclaredAccessibility.ToKeyword()
                , typeKind: symbol.TypeKind
                , isStatic: symbol.IsStatic
                , isSealed: symbol.IsSealed
                , isAbstract: symbol.IsAbstract
                , isReadOnly: symbol.IsReadOnly
                , isRecord: symbol.IsRecord
                , isGeneric: symbol.IsGenericType
                , interfaces: interfaces
                , attributes: attributes
                , fields: fields
                , properties: properties
                , methods: methods
                , constructors: constructors
                , events: events
            );
        }

        private static EquatableArray<string> ExtractInterfaces(
              INamedTypeSymbol symbol
            , ModelParts parts
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Interfaces) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<string>.Rent();
            var interfaces = symbol.Interfaces;
            var ifaceCount = interfaces.Length;

            for (var i = 0; i < ifaceCount; i++)
            {
                builder.Add(interfaces[i].ToDisplayString(SymbolFormats.FullyQualified));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<AttributeModel> ExtractTypeAttributes(
              INamedTypeSymbol symbol
            , ModelParts parts
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Attributes) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();
            return ExtractAttributes(symbol.GetAttributes(), token);
        }

        private static EquatableArray<AttributeModel> ExtractAttributes(
              ImmutableArray<AttributeData> attrData
            , CancellationToken token
        )
        {
            if (attrData.IsDefaultOrEmpty)
            {
                return default;
            }

            using var builder = ImmutableArrayBuilder<AttributeModel>.Rent();
            var attrCount = attrData.Length;

            for (var i = 0; i < attrCount; i++)
            {
                var attr = attrData[i];
                token.ThrowIfCancellationRequested();

                var fullName = attr.AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

                using var ctorArgs = ImmutableArrayBuilder<string>.Rent();
                var ctorArguments = attr.ConstructorArguments;
                var ctorArgCount = ctorArguments.Length;

                for (var j = 0; j < ctorArgCount; j++)
                {
                    var ctorArg = ctorArguments[j];

                    if (ctorArg.Kind == TypedConstantKind.Type && ctorArg.Value is ITypeSymbol typeArg)
                    {
                        ctorArgs.Add(typeArg.ToDisplayString(SymbolFormats.FullyQualified));
                    }
                    else
                    {
                        ctorArgs.Add(ctorArg.Value?.ToString() ?? string.Empty);
                    }
                }

                using var namedArgs = ImmutableArrayBuilder<AttributeNamedArgModel>.Rent();
                var namedArguments = attr.NamedArguments;
                var namedArgCount = namedArguments.Length;

                for (var k = 0; k < namedArgCount; k++)
                {
                    var pair = namedArguments[k];
                    namedArgs.Add(new AttributeNamedArgModel(pair.Key, pair.Value.Value?.ToString() ?? string.Empty));
                }

                builder.Add(new AttributeModel(fullName, ctorArgs.ToImmutable(), namedArgs.ToImmutable()));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<FieldModel> ExtractFields(
              INamedTypeSymbol symbol
            , ModelParts parts
            , ModelOptions options
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Fields) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<FieldModel>.Rent();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = members[i];

                if (member is not IFieldSymbol field)
                {
                    continue;
                }

                if (options.IncludeCompilerGenerated == false && field.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (options.IncludeNonPublic == false && field.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(field.GetAttributes(), token)
                    : default;

                builder.Add(new FieldModel(
                      name: field.Name
                    , typeName: field.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal)
                    , typeFullName: field.Type.ToDisplayString(SymbolFormats.FullyQualified)
                    , accessibility: field.DeclaredAccessibility.ToKeyword()
                    , isReadOnly: field.IsReadOnly
                    , isStatic: field.IsStatic
                    , isConst: field.IsConst
                    , constantValueText: field.ConstantValue?.ToString() ?? string.Empty
                    , refKind: field.RefKind
                    , attributes: attrs
                ));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<PropertyModel> ExtractProperties(
              INamedTypeSymbol symbol
            , ModelParts parts
            , ModelOptions options
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Properties) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<PropertyModel>.Rent();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = members[i];

                if (member is not IPropertySymbol prop)
                {
                    continue;
                }

                if (options.IncludeCompilerGenerated == false && prop.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (options.IncludeNonPublic == false && prop.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(prop.GetAttributes(), token)
                    : default;

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                var propParams = prop.Parameters;
                var propParamCount = propParams.Length;

                for (var j = 0; j < propParamCount; j++)
                {
                    paramBuilder.Add(BuildParameterModel(propParams[j]));
                }

                builder.Add(new PropertyModel(
                      name: prop.Name
                    , typeName: prop.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal)
                    , typeFullName: prop.Type.ToDisplayString(SymbolFormats.FullyQualified)
                    , accessibility: prop.DeclaredAccessibility.ToKeyword()
                    , refKind: prop.RefKind
                    , isStatic: prop.IsStatic
                    , isIndexer: prop.IsIndexer
                    , getter: BuildAccessorModel(prop.GetMethod)
                    , setter: BuildAccessorModel(prop.SetMethod)
                    , parameters: paramBuilder.ToImmutable()
                    , attributes: attrs
                ));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<MethodModel> ExtractMethods(
              INamedTypeSymbol symbol
            , ModelParts parts
            , ModelOptions options
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Methods) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<MethodModel>.Rent();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = members[i];

                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                if (options.IncludeCompilerGenerated == false && method.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (options.IncludeNonPublic == false && method.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(method.GetAttributes(), token)
                    : default;

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                var methodParams = method.Parameters;
                var methodParamCount = methodParams.Length;

                for (var j = 0; j < methodParamCount; j++)
                {
                    paramBuilder.Add(BuildParameterModel(methodParams[j]));
                }

                using var typeParamBuilder = ImmutableArrayBuilder<string>.Rent();
                var typeParams = method.TypeParameters;
                var typeParamCount = typeParams.Length;

                for (var k = 0; k < typeParamCount; k++)
                {
                    typeParamBuilder.Add(typeParams[k].Name);
                }

                builder.Add(new MethodModel(
                      name: method.Name
                    , returnTypeName: method.ReturnType.ToDisplayString(SymbolFormats.SimpleNoGlobal)
                    , returnTypeFullName: method.ReturnType.ToDisplayString(SymbolFormats.FullyQualified)
                    , accessibility: method.DeclaredAccessibility.ToKeyword()
                    , refKind: method.RefKind
                    , returnsVoid: method.ReturnsVoid
                    , isStatic: method.IsStatic
                    , isReadOnly: method.IsReadOnly
                    , isAbstract: method.IsAbstract
                    , isVirtual: method.IsVirtual
                    , methodKind: method.MethodKind
                    , parameters: paramBuilder.ToImmutable()
                    , typeParameters: typeParamBuilder.ToImmutable()
                    , attributes: attrs
                ));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<ConstructorModel> ExtractConstructors(
              INamedTypeSymbol symbol
            , ModelParts parts
            , ModelOptions options
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Constructors) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<ConstructorModel>.Rent();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = members[i];

                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.MethodKind != MethodKind.Constructor)
                {
                    continue;
                }

                if (options.IncludeCompilerGenerated == false && method.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (options.IncludeNonPublic == false && method.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                using var paramBuilder = ImmutableArrayBuilder<ParameterModel>.Rent();
                var methodParams = method.Parameters;
                var methodParamCount = methodParams.Length;

                for (var j = 0; j < methodParamCount; j++)
                {
                    paramBuilder.Add(BuildParameterModel(methodParams[j]));
                }

                builder.Add(new ConstructorModel(
                      accessibility: method.DeclaredAccessibility.ToKeyword()
                    , isStatic: method.IsStatic
                    , parameters: paramBuilder.ToImmutable()
                ));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<EventModel> ExtractEvents(
              INamedTypeSymbol symbol
            , ModelParts parts
            , ModelOptions options
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.Events) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<EventModel>.Rent();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = members[i];

                if (member is not IEventSymbol ev)
                {
                    continue;
                }

                if (options.IncludeCompilerGenerated == false && ev.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (options.IncludeNonPublic == false && ev.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var attrs = (parts & ModelParts.Attributes) != 0
                    ? ExtractAttributes(ev.GetAttributes(), token)
                    : default;

                builder.Add(new EventModel(
                      name: ev.Name
                    , typeName: ev.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal)
                    , typeFullName: ev.Type.ToDisplayString(SymbolFormats.FullyQualified)
                    , accessibility: ev.DeclaredAccessibility.ToKeyword()
                    , isStatic: ev.IsStatic
                    , attributes: attrs
                ));
            }

            return builder.ToImmutable();
        }

        private static AccessorModel BuildAccessorModel(IMethodSymbol accessor)
        {
            if (accessor == null)
            {
                return new AccessorModel(false, string.Empty, false, false, RefKind.None);
            }

            return new AccessorModel(
                  exists: true
                , accessibility: accessor.DeclaredAccessibility.ToKeyword()
                , isReadOnly: accessor.IsReadOnly
                , isInitOnly: accessor.IsInitOnly
                , refKind: accessor.RefKind
            );
        }

        private static ParameterModel BuildParameterModel(IParameterSymbol p)
        {
            return new ParameterModel(
                  name: p.Name
                , typeName: p.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal)
                , typeFullName: p.Type.ToDisplayString(SymbolFormats.FullyQualified)
                , refKind: p.RefKind
                , ordinal: p.Ordinal
                , hasDefaultValue: p.HasExplicitDefaultValue
                , defaultValueText: p.HasExplicitDefaultValue
                    ? (p.ExplicitDefaultValue?.ToString() ?? string.Empty)
                    : string.Empty
            );
        }
    }
}
