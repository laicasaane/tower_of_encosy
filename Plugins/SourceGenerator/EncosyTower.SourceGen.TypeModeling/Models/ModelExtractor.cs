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

            var memberParts = ModelParts.Fields | ModelParts.Properties
                | ModelParts.Methods | ModelParts.Constructors | ModelParts.Events;

            var members = (parts & memberParts) != 0
                ? symbol.GetMembers()
                : ImmutableArray<ISymbol>.Empty;

            var interfaces = ExtractInterfaces(symbol, parts, token);
            var allInterfaces = ExtractAllInterfaces(symbol, parts, token);
            var containingTypes = ExtractContainingTypes(symbol, parts, token);
            var attributes = ExtractTypeAttributes(symbol, parts, token);
            var fields = ExtractFields(members, parts, options, token);
            var properties = ExtractProperties(members, parts, options, token);
            var methods = ExtractMethods(members, parts, options, token);
            var constructors = ExtractConstructors(members, parts, options, token);
            var events = ExtractEvents(members, parts, options, token);

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
                , isValueType: symbol.IsValueType
                , isEnum: symbol.TypeKind == TypeKind.Enum
                , isUnmanaged: symbol.IsUnmanagedType
                , isRefLikeType: symbol.IsRefLikeType
                , baseTypeName: symbol.BaseType?.ToDisplayString(SymbolFormats.FullyQualified)
                              ?? string.Empty
                , interfaces: interfaces
                , allInterfaces: allInterfaces
                , containingTypes: containingTypes
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

        private static EquatableArray<string> ExtractAllInterfaces(
              INamedTypeSymbol symbol
            , ModelParts parts
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.AllInterfaces) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var builder = ImmutableArrayBuilder<string>.Rent();
            var allInterfaces = symbol.AllInterfaces;
            var ifaceCount = allInterfaces.Length;

            for (var i = 0; i < ifaceCount; i++)
            {
                builder.Add(allInterfaces[i].ToDisplayString(SymbolFormats.FullyQualified));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<string> ExtractContainingTypes(
              INamedTypeSymbol symbol
            , ModelParts parts
            , CancellationToken token
        )
        {
            if ((parts & ModelParts.ContainingTypes) == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var innerBuilder = ImmutableArrayBuilder<string>.Rent();
            var containingType = symbol.ContainingType;

            while (containingType != null)
            {
                token.ThrowIfCancellationRequested();

                var keyword = containingType.TypeKind switch {
                    TypeKind.Interface => "interface",
                    TypeKind.Struct    => containingType.IsRecord ? "record struct" : "struct",
                    TypeKind.Class     => containingType.IsRecord ? "record class" : "class",
                    _                  => "class",
                };

                var accessibility = containingType.DeclaredAccessibility.ToKeyword();
                innerBuilder.Add($"{accessibility} partial {keyword} {containingType.Name}");
                containingType = containingType.ContainingType;
            }

            var innerToOuter = innerBuilder.ToImmutable();

            if (innerToOuter.Length == 0)
            {
                return default;
            }

            var outerToInner = new string[innerToOuter.Length];

            for (var i = 0; i < innerToOuter.Length; i++)
            {
                outerToInner[i] = innerToOuter[innerToOuter.Length - 1 - i];
            }

            return outerToInner.ToImmutableArray().AsEquatableArray();
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
                var shortName = attr.AttributeClass?.Name ?? string.Empty;

                using var ctorArgs = ImmutableArrayBuilder<AttributeCtorArgModel>.Rent();
                var ctorArguments = attr.ConstructorArguments;
                var ctorArgCount = ctorArguments.Length;

                for (var j = 0; j < ctorArgCount; j++)
                {
                    var ctorArg = ctorArguments[j];

                    if (ctorArg.Kind == TypedConstantKind.Type && ctorArg.Value is ITypeSymbol typeArg)
                    {
                        ctorArgs.Add(new AttributeCtorArgModel(
                              TypedConstantKind.Type
                            , typeArg.ToDisplayString(SymbolFormats.FullyQualified)
                            , default
                        ));
                    }
                    else if (ctorArg.Kind == TypedConstantKind.Array)
                    {
                        var elems = ctorArg.Values;
                        var elemCount = elems.Length;
                        using var elemBuilder = ImmutableArrayBuilder<string>.Rent();

                        for (var e = 0; e < elemCount; e++)
                        {
                            var elem = elems[e];

                            if (elem.Kind == TypedConstantKind.Type && elem.Value is ITypeSymbol elemTypeArg)
                            {
                                elemBuilder.Add(elemTypeArg.ToDisplayString(SymbolFormats.FullyQualified));
                            }
                            else
                            {
                                elemBuilder.Add(elem.Value?.ToString() ?? string.Empty);
                            }
                        }

                        ctorArgs.Add(new AttributeCtorArgModel(
                              TypedConstantKind.Array
                            , string.Empty
                            , elemBuilder.ToImmutable()
                        ));
                    }
                    else
                    {
                        ctorArgs.Add(new AttributeCtorArgModel(
                              ctorArg.Kind
                            , ctorArg.Value?.ToString() ?? string.Empty
                            , default
                        ));
                    }
                }

                using var namedArgs = ImmutableArrayBuilder<AttributeNamedArgModel>.Rent();
                var namedArguments = attr.NamedArguments;
                var namedArgCount = namedArguments.Length;

                for (var k = 0; k < namedArgCount; k++)
                {
                    var pair = namedArguments[k];
                    var argValue = pair.Value.Kind == TypedConstantKind.Type
                                   && pair.Value.Value is ITypeSymbol namedTypeArg
                        ? namedTypeArg.ToDisplayString(SymbolFormats.FullyQualified)
                        : pair.Value.Value?.ToString() ?? string.Empty;
                    namedArgs.Add(new AttributeNamedArgModel(pair.Key, pair.Value.Kind, argValue));
                }

                builder.Add(new AttributeModel(fullName, shortName, ctorArgs.ToImmutable(), namedArgs.ToImmutable()));
            }

            return builder.ToImmutable();
        }

        private static EquatableArray<FieldModel> ExtractFields(
              ImmutableArray<ISymbol> members
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
              ImmutableArray<ISymbol> members
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
              ImmutableArray<ISymbol> members
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
              ImmutableArray<ISymbol> members
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
              ImmutableArray<ISymbol> members
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
