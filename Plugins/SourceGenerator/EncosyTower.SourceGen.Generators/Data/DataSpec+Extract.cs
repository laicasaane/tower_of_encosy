using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Data
{
    using static EncosyTower.SourceGen.Helpers.Data.Helpers;
    using ReturnTypeName = String;
    using ParamTypeName = String;
    using ConvertExpression = String;
    using EqualsExpression = String;

    partial struct DataSpec
    {
        public static DataSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax typeSyntax
                || context.TargetSymbol is not INamedTypeSymbol typeSymbol
            )
            {
                return default;
            }

            if (typeSyntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return default;
            }

            var dataAttribute = typeSymbol.GetAttribute(DATA_ATTRIBUTE, token);

            if (dataAttribute == null)
            {
                return default;
            }

            DetermineMutability(
                  typeSymbol
                , out var isMutable
                , out var withoutPropertySetters
                , out var withReadOnlyView
                , token
            );

            token.ThrowIfCancellationRequested();

            var fieldPolicyAttrib = typeSymbol.GetAttribute(DATA_FIELD_POLICY_ATTRIBUTE, token);
            var fieldPolicy = DataFieldPolicy.Private;

            if (isMutable == false && fieldPolicyAttrib != null)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var syntaxTree = typeSyntax.SyntaxTree;
            var typeIdentifier = typeSyntax.Identifier.Text;
            var typeValidIdentifier = typeSymbol.ToValidIdentifier();
            var typeNameBuilder = new StringBuilder(typeIdentifier);
            var converterMap = new Dictionary<ReturnTypeName, Dictionary<ParamTypeName, ConvertExpression>>(StringComparer.Ordinal);
            var comparerMap = new Dictionary<ParamTypeName, EqualsExpression>(StringComparer.Ordinal);

            foreach (var kvp in dataAttribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                switch (kvp.Key)
                {
                    case "Converters":
                    {
                        if (kvp.Value.Kind == TypedConstantKind.Array && kvp.Value.Values.Length > 0)
                        {
                            foreach (var tc in kvp.Value.Values)
                            {
                                if (tc.Value is ITypeSymbol type)
                                {
                                    RegisterConvertMethods(type, converterMap, token);
                                }
                            }
                        }
                        break;
                    }

                    case "Comparers":
                    {
                        if (kvp.Value.Kind == TypedConstantKind.Array && kvp.Value.Values.Length > 0)
                        {
                            foreach (var tc in kvp.Value.Values)
                            {
                                if (tc.Value is ITypeSymbol type)
                                {
                                    RegisterEqualsMethods(type, comparerMap, token);
                                }
                            }
                        }
                        break;
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            if (typeSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                typeNameBuilder.Append("<");
                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    typeNameBuilder.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        typeNameBuilder.Append(", ");
                    }
                }

                typeNameBuilder.Append(">");
            }

            var typeName = typeNameBuilder.ToString();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var hintName = syntaxTree.GetHintName(typeSyntax, typeValidIdentifier);

            if (fieldPolicyAttrib != null)
            {
                var args = fieldPolicyAttrib.ConstructorArguments;

                if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
                {
                    fieldPolicy = (DataFieldPolicy)(int)args[0].Value;
                }
            }

            var baseTypeName = string.Empty;

            if (typeSymbol.BaseType is INamedTypeSymbol baseNamedTypeSymbol
                && baseNamedTypeSymbol.TypeKind == TypeKind.Class
                && baseNamedTypeSymbol.HasAttribute(DATA_ATTRIBUTE, token)
            )
            {
                baseTypeName = baseNamedTypeSymbol.ToFullName();
            }

            var withoutId = typeSymbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE, token) != null;
            var withId = withoutId == false;
            var locationInfo = LocationInfo.From(typeSymbol.Locations.IsEmpty
                ? Location.None
                : typeSymbol.Locations[0]);

            var existingFields = new HashSet<string>();
            var existingProperties = new Dictionary<string, bool>(); // name → isPublic
            var existingOverrideEquals = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var allowOnlyPrivateOrInitSetter = isMutable == false || withoutPropertySetters;
            var equalityComparer = SymbolEqualityComparer.Default;

            token.ThrowIfCancellationRequested();

            using var orderBuilder = ImmutableArrayBuilder<OrderData>.Rent();
            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldRefData>.Rent();
            using var propArrayBuilder = ImmutableArrayBuilder<PropRefData>.Rent();
            using var overrideEqualsBuilder = ImmutableArrayBuilder<string>.Rent();

            var members = typeSymbol.GetMembers();
            var hasGetHashCodeMethod = false;
            var hasEqualsMethod = false;
            var hasIEquatableMethod = false;
            var idPropertyTypeName = string.Empty;

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field)
                {
                    existingFields.Add(field.Name);

                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE, token) == false)
                    {
                        continue;
                    }

                    if (isMutable == false && field.DeclaredAccessibility != Accessibility.Private)
                    {
                        continue;
                    }

                    ITypeSymbol propertyType;
                    bool implicitlyConvertible;
                    var fieldType = field.Type;

                    if (field.TryGetAttribute(PROPERTY_TYPE_ATTRIBUTE, out var propertyTypeAttrib, token)
                        && propertyTypeAttrib.ConstructorArguments is { Length: > 0 } propertyTypeArgs
                        && propertyTypeArgs[0].Value is ITypeSymbol propTypeSymbol
                    )
                    {
                        propertyType = propTypeSymbol;
                        implicitlyConvertible = true;
                    }
                    else
                    {
                        propertyType = fieldType;
                        implicitlyConvertible = false;
                    }

                    field.GatherForwardedAttributes(
                          semanticModel
                        , token
                        , out ImmutableArray<(string, AttributeInfo)> propertyAttributes
                    );

                    var propertyName = field.ToPropertyName();
                    var collection = GetCollection(fieldType, isField: true, token);
                    var fieldEquality = GetEquality(fieldType, token);
                    var fieldConverter = string.Empty;
                    var propertyConverter = string.Empty;
                    var fieldEqualityComparer = string.Empty;

                    ITypeSymbol converterType = null;

                    if (propertyTypeAttrib != null && propertyTypeAttrib.ConstructorArguments.Length > 1)
                    {
                        if (propertyTypeAttrib.ConstructorArguments[1].Value is ITypeSymbol foundConverter)
                        {
                            converterType = foundConverter;
                        }
                    }

                    MakeConverters(
                          ref fieldConverter
                        , ref propertyConverter
                        , converterType
                        , fieldType
                        , propertyType
                        , typeSymbol
                        , token
                    );

                    MakeFieldComparer(
                          ref fieldEqualityComparer
                        , fieldType
                        , propertyType
                        , field
                        , typeSymbol
                        , token
                    );

                    var fieldTypeName = GetFieldTypeName(fieldType, collection);
                    var fieldTypeOriginalFullName = fieldType.ToFullName();
                    var propertyTypeName = propertyType.ToFullName();

                    TryGetConverterAndComparer(
                          converterMap
                        , comparerMap
                        , fieldTypeOriginalFullName
                        , propertyTypeName
                        , ref fieldConverter
                        , ref propertyConverter
                        , ref fieldEqualityComparer
                    );

                    GetTypeNames(
                          propertyType
                        , collection
                        , out var mutablePropertyTypeName
                        , out var immutablePropertyTypeName
                        , out var samePropertyType
                    );

                    var typesAreDifferent = equalityComparer.Equals(fieldType, propertyType) == false;
                    ITypeSymbol equalityFieldType;

                    if (fieldEquality.IsNullable)
                    {
                        equalityFieldType = fieldType.GetTypeFromNullable() ?? fieldType;
                    }
                    else
                    {
                        equalityFieldType = fieldType;
                    }

                    token.ThrowIfCancellationRequested();

                    var fieldTypeFullNameForEquality = equalityFieldType.ToFullName();
                    var fieldTypeIsReferenceType = equalityFieldType.IsReferenceType;
                    using var attrBuilder = ImmutableArrayBuilder<ForwardedAttributeData>.Rent();

                    foreach (var (fullTypeName, attributeInfo) in propertyAttributes)
                    {
                        token.ThrowIfCancellationRequested();

                        attrBuilder.Add(new ForwardedAttributeData {
                            fullTypeName = fullTypeName,
                            syntax = attributeInfo.GetSyntax().ToFullString(),
                        });
                    }

                    token.ThrowIfCancellationRequested();

                    field.GatherAttributes(semanticModel, token, out var attributes);

                    ForwardedAttributeData? manualAuthoringAttribute = null;
                    ForwardedAttributeData? converterAttribute = null;

                    foreach (var (fullyTypeName, attributeInfo) in attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        switch (fullyTypeName)
                        {
                            case DATA_MANUAL_AUTHORING_ATTRIBUTE:
                                manualAuthoringAttribute = new ForwardedAttributeData {
                                    fullTypeName = fullyTypeName,
                                    syntax = attributeInfo.GetSyntax().ToFullString(),
                                };
                                break;

                            case DATA_AUTHORING_CONVERTER_ATTRIBUTE:
                                converterAttribute = new ForwardedAttributeData {
                                    fullTypeName = fullyTypeName,
                                    syntax = attributeInfo.GetSyntax().ToFullString(),
                                };
                                break;
                        }
                    }

                    var fieldRefData = new FieldRefData {
                        location = LocationInfo.From(field.Locations.IsEmpty ? Location.None : field.Locations[0]),
                        fieldName = field.Name,
                        propertyName = propertyName,
                        fieldTypeName = fieldTypeName,
                        fieldTypeOriginalFullName = fieldTypeOriginalFullName,
                        propertyTypeName = propertyTypeName,
                        mutablePropertyTypeName = mutablePropertyTypeName,
                        immutablePropertyTypeName = immutablePropertyTypeName,
                        samePropertyType = samePropertyType,
                        typesAreDifferent = typesAreDifferent,
                        implicitlyConvertible = implicitlyConvertible,
                        hasImplementedProperty = false,   // resolved after all members are scanned
                        isImplementedPropertyPublic = false,
                        propertyConverter = propertyConverter,
                        fieldConverter = fieldConverter,
                        fieldEqualityComparer = fieldEqualityComparer,
                        fieldCollection = ToFieldCollectionData(collection),
                        fieldEquality = fieldEquality,
                        fieldTypeFullNameForEquality = fieldTypeFullNameForEquality,
                        fieldTypeIsReferenceType = fieldTypeIsReferenceType,
                        forwardedPropertyAttributes = attrBuilder.ToImmutable().AsEquatableArray(),
                        manualAuthoringAttribute = manualAuthoringAttribute,
                        converterAttribute = converterAttribute,
                    };

                    var fieldIndex = fieldArrayBuilder.Count;
                    orderBuilder.Add(new OrderData { index = fieldIndex, isPropRef = false });
                    fieldArrayBuilder.Add(fieldRefData);

                    if (withId && collection.Kind == CollectionKind.NotCollection
                        && string.Equals(propertyName, "Id", StringComparison.Ordinal)
                    )
                    {
                        idPropertyTypeName = propertyType.ToFullName();
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    existingProperties[property.Name] = property.DeclaredAccessibility == Accessibility.Public;

                    if (allowOnlyPrivateOrInitSetter && property.SetMethod != null)
                    {
                        var setter = property.SetMethod;

                        if (setter.IsInitOnly == false && setter.DeclaredAccessibility != Accessibility.Private)
                        {
                            continue;
                        }
                    }

                    if (property.GetAttribute(DATA_PROPERTY_ATTRIBUTE, token) is not { } dataPropertyAttribute)
                    {
                        continue;
                    }

                    ITypeSymbol fieldType;
                    bool implicitlyConvertible;

                    if (dataPropertyAttribute.ConstructorArguments.Length > 0
                        && dataPropertyAttribute.ConstructorArguments[0].Value is ITypeSymbol fieldTypeSymbol
                    )
                    {
                        fieldType = fieldTypeSymbol;
                        implicitlyConvertible = true;
                    }
                    else
                    {
                        fieldType = property.Type;
                        implicitlyConvertible = false;
                    }

                    property.GatherForwardedAttributes(
                          semanticModel
                        , token
                        , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                    );

                    var fieldName = property.ToPrivateFieldName();
                    var collection = GetCollection(fieldType, isField: true, token);
                    var propCollection = GetCollection(property.Type, isField: false, token);
                    var fieldEquality = GetEquality(fieldType, token);
                    var fieldConverter = string.Empty;
                    var propertyConverter = string.Empty;
                    var fieldEqualityComparer = string.Empty;

                    ITypeSymbol converterType = null;

                    if (dataPropertyAttribute != null && dataPropertyAttribute.ConstructorArguments.Length > 1)
                    {
                        if (dataPropertyAttribute.ConstructorArguments[1].Value is ITypeSymbol foundConverter)
                        {
                            converterType = foundConverter;
                        }
                    }

                    MakeConverters(
                          ref fieldConverter
                        , ref propertyConverter
                        , converterType
                        , fieldType
                        , property.Type
                        , typeSymbol
                        , token
                    );

                    MakeFieldComparer(
                          ref fieldEqualityComparer
                        , fieldType
                        , property.Type
                        , property
                        , typeSymbol
                        , token
                    );

                    var fieldTypeName = GetFieldTypeName(fieldType, collection);
                    var propertyTypeName = property.Type.ToFullName();

                    TryGetConverterAndComparer(
                          converterMap
                        , comparerMap
                        , fieldTypeName
                        , propertyTypeName
                        , ref fieldConverter
                        , ref propertyConverter
                        , ref fieldEqualityComparer
                    );

                    GetTypeNames(
                          property.Type
                        , collection
                        , out var mutablePropertyTypeName
                        , out var immutablePropertyTypeName
                        , out var samePropertyType
                    );

                    var typesAreDifferent = equalityComparer.Equals(fieldType, property.Type) == false;
                    ITypeSymbol equalityFieldType;

                    if (fieldEquality.IsNullable)
                    {
                        equalityFieldType = fieldType.GetTypeFromNullable() ?? fieldType;
                    }
                    else
                    {
                        equalityFieldType = fieldType;
                    }

                    token.ThrowIfCancellationRequested();

                    var equalityCollection = GetCollection(equalityFieldType, isField: true, token);
                    var fieldTypeDeclNameForEquality = GetFieldTypeName(equalityFieldType, equalityCollection);
                    var fieldTypeIsReferenceType = equalityFieldType.IsReferenceType;
                    using var attrBuilder = ImmutableArrayBuilder<ForwardedAttributeData>.Rent();

                    foreach (var (fullTypeName, attributeInfo) in fieldAttributes)
                    {
                        token.ThrowIfCancellationRequested();

                        attrBuilder.Add(new ForwardedAttributeData {
                            fullTypeName = fullTypeName,
                            syntax = attributeInfo.GetSyntax().ToFullString(),
                        });
                    }

                    token.ThrowIfCancellationRequested();

                    property.GatherAttributes(semanticModel, token, out var attributes);

                    ForwardedAttributeData? manualAuthoringAttribute = null;
                    ForwardedAttributeData? converterAttribute = null;

                    foreach (var (fullyTypeName, attributeInfo) in attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        switch (fullyTypeName)
                        {
                            case DATA_MANUAL_AUTHORING_ATTRIBUTE:
                                manualAuthoringAttribute = new ForwardedAttributeData {
                                    fullTypeName = fullyTypeName,
                                    syntax = attributeInfo.GetSyntax().ToFullString(),
                                };
                                break;

                            case DATA_AUTHORING_CONVERTER_ATTRIBUTE:
                                converterAttribute = new ForwardedAttributeData {
                                    fullTypeName = fullyTypeName,
                                    syntax = attributeInfo.GetSyntax().ToFullString(),
                                };
                                break;
                        }
                    }

                    var doesCreateProperty = property.HasAttribute(CREATE_PROPERTY_ATTRIBUTE, token);
                    var fieldIsImplemented = existingFields.Contains(fieldName);

                    var propRefData = new PropRefData {
                        location = LocationInfo.From(property.Locations.IsEmpty ? Location.None : property.Locations[0]),
                        propertyName = property.Name,
                        fieldName = fieldName,
                        fieldTypeName = fieldTypeName,
                        propertyTypeName = propertyTypeName,
                        mutablePropertyTypeName = mutablePropertyTypeName,
                        immutablePropertyTypeName = immutablePropertyTypeName,
                        samePropertyType = samePropertyType,
                        typesAreDifferent = typesAreDifferent,
                        implicitlyConvertible = implicitlyConvertible,
                        fieldIsImplemented = fieldIsImplemented,
                        isPropertyPublic = property.DeclaredAccessibility == Accessibility.Public,
                        doesCreateProperty = doesCreateProperty,
                        propertyConverter = propertyConverter,
                        fieldConverter = fieldConverter,
                        fieldEqualityComparer = fieldEqualityComparer,
                        fieldCollection = ToFieldCollectionData(collection),
                        fieldEquality = fieldEquality,
                        fieldTypeDeclNameForEquality = fieldTypeDeclNameForEquality,
                        fieldTypeIsReferenceType = fieldTypeIsReferenceType,
                        forwardedFieldAttributes = attrBuilder.ToImmutable().AsEquatableArray(),
                        manualAuthoringAttribute = manualAuthoringAttribute,
                        converterAttribute = converterAttribute,
                    };

                    var propIndex = propArrayBuilder.Count;
                    orderBuilder.Add(new OrderData { index = propIndex, isPropRef = true });
                    propArrayBuilder.Add(propRefData);

                    if (withId && propCollection.Kind == CollectionKind.NotCollection
                         && string.Equals(property.Name, "Id", StringComparison.Ordinal)
                    )
                    {
                        idPropertyTypeName = property.Type.ToFullName();
                    }

                    continue;
                }

                if (member is IMethodSymbol method)
                {
                    if (method.Name == "GetHashCode" && method.Parameters.Length == 0)
                    {
                        hasGetHashCodeMethod = true;
                        continue;
                    }

                    if (method.Name == "Equals"
                        && method.Parameters.Length == 1
                        && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    )
                    {
                        var param = method.Parameters[0];

                        if (param.Type.SpecialType == SpecialType.System_Object)
                        {
                            hasEqualsMethod = true;
                            continue;
                        }

                        if (equalityComparer.Equals(typeSymbol, param.Type))
                        {
                            hasIEquatableMethod = true;
                            continue;
                        }

                        existingOverrideEquals.Add(param.Type);
                    }

                    continue;
                }
            }

            token.ThrowIfCancellationRequested();

            var resolvedFieldRefs = fieldArrayBuilder.ToImmutable();

            for (var i = 0; i < resolvedFieldRefs.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var fr = resolvedFieldRefs[i];

                if (existingProperties.TryGetValue(fr.propertyName, out var isPublic))
                {
                    fr.hasImplementedProperty = true;
                    fr.isImplementedPropertyPublic = isPublic;
                    resolvedFieldRefs = resolvedFieldRefs.SetItem(i, fr);
                }
            }

            token.ThrowIfCancellationRequested();

            var resolvedPropRefs = propArrayBuilder.ToImmutable();

            for (var i = 0; i < resolvedPropRefs.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var pr = resolvedPropRefs[i];

                if (existingFields.Contains(pr.fieldName) != pr.fieldIsImplemented)
                {
                    pr.fieldIsImplemented = existingFields.Contains(pr.fieldName);
                    resolvedPropRefs = resolvedPropRefs.SetItem(i, pr);
                }
            }

            if (string.IsNullOrEmpty(baseTypeName) == false)
            {
                token.ThrowIfCancellationRequested();

                var baseType = typeSymbol.BaseType;

                while (baseType != null)
                {
                    token.ThrowIfCancellationRequested();

                    if (baseType.HasAttribute(DATA_ATTRIBUTE, token)
                        && existingOverrideEquals.Contains(baseType) == false
                    )
                    {
                        overrideEqualsBuilder.Add(baseType.ToFullName());
                    }

                    baseType = baseType.BaseType;
                }
            }

            return new DataSpec {
                location = locationInfo,
                typeName = typeName,
                readOnlyTypeName = $"ReadOnly{typeName}",
                typeIdentifier = typeIdentifier,
                typeValidIdentifier = typeValidIdentifier,
                baseTypeName = baseTypeName,
                accessibilityKeyword = typeSymbol.DeclaredAccessibility.ToKeyword(),
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                idPropertyTypeName = idPropertyTypeName,
                fieldPolicy = fieldPolicy,
                isMutable = isMutable,
                isValueType = typeSymbol.IsValueType,
                withoutPropertySetters = withoutPropertySetters,
                withReadOnlyView = withReadOnlyView,
                isSealed = typeSymbol.IsSealed || typeSymbol.IsValueType,
                hasSerializableAttribute = typeSymbol.HasAttribute(SERIALIZABLE_ATTRIBUTE, token),
                hasGeneratePropertyBagAttribute = typeSymbol.HasAttribute(GENERATE_PROPERTY_BAG_ATTRIBUTE, token),
                hasGetHashCodeMethod = hasGetHashCodeMethod,
                hasEqualsMethod = hasEqualsMethod,
                hasIEquatableMethod = hasIEquatableMethod,
                withoutId = withoutId,
                orders = orderBuilder.ToImmutable().AsEquatableArray(),
                fieldRefs = resolvedFieldRefs.AsEquatableArray(),
                propRefs = resolvedPropRefs.AsEquatableArray(),
                overrideEquals = overrideEqualsBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void TryGetConverterAndComparer(
              Dictionary<ReturnTypeName, Dictionary<ParamTypeName, ConvertExpression>> converterMap
            , Dictionary<ParamTypeName, EqualsExpression> comparerMap
            , string fieldTypeName
            , string propertyTypeName
            , ref string fieldConverter
            , ref string propertyConverter
            , ref string fieldEqualityComparer
        )
        {
            if (string.IsNullOrEmpty(fieldEqualityComparer))
            {
                if (comparerMap.TryGetValue(fieldTypeName, out var foundComparer))
                {
                    fieldEqualityComparer = foundComparer;
                }
            }

            if (string.IsNullOrEmpty(fieldConverter))
            {
                if (converterMap.TryGetValue(fieldTypeName, out var dict)
                    && dict.TryGetValue(propertyTypeName, out var foundConverter)
                )
                {
                    fieldConverter = foundConverter;
                }
            }

            if (string.IsNullOrEmpty(propertyConverter))
            {
                if (converterMap.TryGetValue(propertyTypeName, out var dict)
                    && dict.TryGetValue(fieldTypeName, out var foundConverter)
                )
                {
                    propertyConverter = foundConverter;
                }
            }
        }

        private static FieldCollectionData ToFieldCollectionData(in CollectionRef collection)
        {
            return new FieldCollectionData {
                kind = collection.Kind,
                elementTypeName = collection.ElementType?.ToFullName() ?? string.Empty,
                keyTypeName = collection.KeyType?.ToFullName() ?? string.Empty,
                isElementEquatable = collection.IsElementEquatable,
                isKeyEquatable = collection.IsKeyEquatable,
            };
        }

        private static Equality GetEquality(ITypeSymbol type, CancellationToken token)
        {
            var fieldEquality = type.DetermineEquality();

            if (type.HasAttribute(DATA_ATTRIBUTE, token))
            {
                fieldEquality = new Equality(EqualityStrategy.Equals, false, false);
            }
            else if (type.HasAttribute(UNION_ID_ATTRIBUTE, token))
            {
                fieldEquality = new Equality(EqualityStrategy.Operator, false, false);
            }

            return fieldEquality;
        }

        private static bool GetEquatable(ITypeSymbol type, CancellationToken token)
        {
            return type.HasAttribute(DATA_ATTRIBUTE, token)
                || type.HasAttribute(UNION_ID_ATTRIBUTE, token)
                || type.DetermineIEquatable(token)
                ;
        }

        private static CollectionRef GetCollection(ITypeSymbol typeSymbol, bool isField, CancellationToken token)
        {
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return new CollectionRef {
                    Kind = CollectionKind.Array,
                    ElementType = arrayType.ElementType,
                    IsElementEquatable = GetEquatable(arrayType.ElementType, token),
                };
            }

            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                return default;
            }

            if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemoryType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.ReadOnlyMemory,
                    ElementType = readMemoryType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(readMemoryType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memoryType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.Memory,
                    ElementType = memoryType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(memoryType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.ReadOnlySpan,
                    ElementType = readSpanType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(readSpanType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.Span,
                    ElementType = spanType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(spanType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var iReadListType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.List : CollectionKind.ReadOnlyList,
                    ElementType = iReadListType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(iReadListType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var iListType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = iListType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(iListType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericFromContainingType(LIST_FAST_TYPE_T, 1, out var listFastType, token))
            {
                var isReadOnly = namedType.Name.Equals("ReadOnly");

                return new CollectionRef {
                    Kind = isField || isReadOnly == false ? CollectionKind.List : CollectionKind.ReadOnlyList,
                    ElementType = listFastType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(listFastType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = listType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(listType.TypeArguments[0], token),
                };
            }

            if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var iSetType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = iSetType.TypeArguments[0],
                    IsElementEquatable = iSetType.TypeArguments[0].DetermineIEquatable(token),
                };
            }

            if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = hashSetType.TypeArguments[0],
                    IsElementEquatable = hashSetType.TypeArguments[0].DetermineIEquatable(token),
                };
            }

            if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Queue,
                    ElementType = queueType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Stack,
                    ElementType = stackType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var iReadDictType, token))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Dictionary : CollectionKind.ReadOnlyDictionary,
                    KeyType = iReadDictType.TypeArguments[0],
                    ElementType = iReadDictType.TypeArguments[1],
                    IsKeyEquatable = iReadDictType.TypeArguments[0].DetermineIEquatable(token),
                    IsElementEquatable = iReadDictType.TypeArguments[1].DetermineIEquatable(token),
                };
            }

            if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var iDictType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = iDictType.TypeArguments[0],
                    ElementType = iDictType.TypeArguments[1],
                    IsKeyEquatable = iDictType.TypeArguments[0].DetermineIEquatable(token),
                    IsElementEquatable = iDictType.TypeArguments[1].DetermineIEquatable(token),
                };
            }

            if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType, token))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = dictType.TypeArguments[0],
                    ElementType = dictType.TypeArguments[1],
                    IsKeyEquatable = dictType.TypeArguments[0].DetermineIEquatable(token),
                    IsElementEquatable = dictType.TypeArguments[1].DetermineIEquatable(token),
                };
            }

            return default;
        }

        private static string GetFieldTypeName(ITypeSymbol typeSymbol, in CollectionRef collection)
        {
            if (TryGetFieldTypeName(collection, out var result) == false)
            {
                result = typeSymbol.ToFullName();
            }

            return result;
        }

        private static bool TryGetFieldTypeName(in CollectionRef collection, out string result)
        {
            var (kind, keyType, elementType, _, _) = collection;

            switch (kind)
            {
                case CollectionKind.ReadOnlyMemory:
                case CollectionKind.Memory:
                case CollectionKind.ReadOnlySpan:
                case CollectionKind.Span:
                case CollectionKind.Array:
                {
                    result = $"{elementType.ToFullName()}[]";
                    return true;
                }

                case CollectionKind.ReadOnlyList:
                case CollectionKind.List:
                {
                    result = $"{LIST_TYPE_T}{elementType.ToFullName()}>";
                    return true;
                }

                case CollectionKind.HashSet:
                {
                    result = $"{HASH_SET_TYPE_T}{elementType.ToFullName()}>";
                    return true;
                }

                case CollectionKind.Stack:
                {
                    result = $"{STACK_TYPE_T}{elementType.ToFullName()}>";
                    return true;
                }

                case CollectionKind.Queue:
                {
                    result = $"{QUEUE_TYPE_T}{elementType.ToFullName()}>";
                    return true;
                }

                case CollectionKind.ReadOnlyDictionary:
                case CollectionKind.Dictionary:
                {
                    var keyTypeFullName = keyType.ToFullName();
                    var valueTypeFullName = elementType.ToFullName();
                    result = $"{DICTIONARY_TYPE_T}{keyTypeFullName}, {valueTypeFullName}>";
                    return true;
                }

                default:
                {
                    result = string.Empty;
                    return false;
                }
            }
        }

        private static void GetTypeNames(
              ITypeSymbol typeSymbol
            , in CollectionRef collection
            , out string mutableTypeName
            , out string immutableTypeName
            , out bool sameType
        )
        {
            sameType = false;

            var (kind, keyType, elementType, _, _) = collection;

            switch (kind)
            {
                case CollectionKind.Array:
                {
                    mutableTypeName = $"{elementType.ToFullName()}[]";
                    immutableTypeName = $"global::System.ReadOnlyMemory<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.List:
                {
                    mutableTypeName = $"global::System.Collections.Generic.List<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::EncosyTower.Collections.ListFast<{elementType.ToFullName()}>.ReadOnly";
                    break;
                }

                case CollectionKind.Dictionary:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Dictionary<{keyType.ToFullName()}, {elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyDictionary<{keyType.ToFullName()}, {elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.HashSet:
                {
                    mutableTypeName = $"global::System.Collections.Generic.HashSet<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Queue:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Queue<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
                    break;
                }

                case CollectionKind.Stack:
                {
                    mutableTypeName = $"global::System.Collections.Generic.Stack<{elementType.ToFullName()}>";
                    immutableTypeName = $"global::System.Collections.Generic.IReadOnlyCollection<{elementType.ToFullName()}>";
                    break;
                }

                default:
                {
                    mutableTypeName = typeSymbol.ToFullName();
                    immutableTypeName = typeSymbol.ToFullName();
                    sameType = true;
                    break;
                }
            }
        }

        private static void DetermineMutability(
              ITypeSymbol symbol
            , out bool isMutable
            , out bool withoutPropertySetters
            , out bool withReadOnlyView
            , CancellationToken token
        )
        {
            isMutable = false;
            withoutPropertySetters = false;
            withReadOnlyView = false;

            if (symbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE, token) is not { } attrib)
            {
                return;
            }

            isMutable = true;

            var args = attrib.ConstructorArguments;

            if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
            {
                var options = (DataMutableOptions)(int)args[0].Value;
                withoutPropertySetters = options.HasFlag(DataMutableOptions.WithoutPropertySetters);
                withReadOnlyView = options.HasFlag(DataMutableOptions.WithReadOnlyStruct);
            }
        }

        private static void MakeConverters(
              ref string fieldConverter
            , ref string propertyConverter
            , ITypeSymbol converterType
            , ITypeSymbol fieldType
            , ITypeSymbol propertyType
            , ITypeSymbol containingTypeSymbol
            , CancellationToken token
        )
        {
            if (converterType is null)
            {
                if (TryGetConvertMethod(containingTypeSymbol, out var fc1, propertyType, fieldType, IsAny, token))
                {
                    fieldConverter = Make(containingTypeSymbol, fc1);
                }
                else if (TryGetConvertMethod(fieldType, out var fc2, propertyType, fieldType, IsPublic, token))
                {
                    fieldConverter = Make(fieldType, fc2);
                }
                else if (TryGetConvertMethod(propertyType, out var fc3, propertyType, fieldType, IsPublic, token))
                {
                    fieldConverter = Make(propertyType, fc3);
                }

                if (TryGetConvertMethod(containingTypeSymbol, out var pc1, fieldType, propertyType, IsAny, token))
                {
                    propertyConverter = Make(containingTypeSymbol, pc1);
                }
                else if (TryGetConvertMethod(propertyType, out var pc2, fieldType, propertyType, IsPublic, token))
                {
                    propertyConverter = Make(propertyType, pc2);
                }
                else if (TryGetConvertMethod(fieldType, out var pc3, fieldType, propertyType, IsPublic, token))
                {
                    propertyConverter = Make(fieldType, pc3);
                }

                return;
            }

            if (TryGetConvertMethod(converterType, out var fm, propertyType, fieldType, IsPublic, token))
            {
                fieldConverter = Make(converterType, fm);
            }

            if (TryGetConvertMethod(converterType, out var pm, fieldType, propertyType, IsPublic, token))
            {
                propertyConverter = Make(converterType, pm);
            }

            return;

            static string Make(ITypeSymbol type, IMethodSymbol method)
                => method.IsStatic
                    ? $"{type.ToFullName()}.{method.Name}"
                    : $"new {type.ToFullName()}().{method.Name}";

            static bool IsPublic(Accessibility v)
                => v == Accessibility.Public;

            static bool IsAny(Accessibility _)
                => true;
        }

        private static void RegisterConvertMethods(
              ITypeSymbol converterType
            , Dictionary<ReturnTypeName, Dictionary<ParamTypeName, ConvertExpression>> converterMap
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (converterType.IsAbstract)
            {
                return;
            }

            if (converterType is INamedTypeSymbol { IsUnboundGenericType: true })
            {
                return;
            }

            if (converterType.IsValueType == false)
            {
                var ctors = converterType.GetMembers(".ctor");
                IMethodSymbol ctorMethod = null;

                foreach (var ctor in ctors)
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is not IMethodSymbol ctorM
                        || ctorM.DeclaredAccessibility != Accessibility.Public
                    )
                    {
                        continue;
                    }

                    if (ctorM.Parameters.Length == 0)
                    {
                        ctorMethod = ctorM;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    return;
                }
            }

            token.ThrowIfCancellationRequested();

            var containerTypeName = converterType.ToFullName();
            var members = converterType.GetMembers("Convert");

            foreach (var m in members)
            {
                token.ThrowIfCancellationRequested();

                if (m is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 1
                    || method.IsGenericMethod
                )
                {
                    continue;
                }

                var returnTypeName = method.ReturnType.ToFullName();
                var paramTypeName = method.Parameters[0].Type.ToFullName();

                if (converterMap.TryGetValue(returnTypeName, out var dict) == false)
                {
                    converterMap[returnTypeName] = dict = new(StringComparer.Ordinal);
                }

                if (dict.ContainsKey(paramTypeName) == false)
                {
                    dict[paramTypeName] = method.IsStatic
                        ? $"{containerTypeName}.{method.Name}"
                        : $"new {containerTypeName}().{method.Name}";
                }
            }
        }

        private static void RegisterEqualsMethods(
              ITypeSymbol converterType
            , Dictionary<ParamTypeName, EqualsExpression> comparerMap
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (converterType.IsAbstract)
            {
                return;
            }

            if (converterType is INamedTypeSymbol { IsUnboundGenericType: true })
            {
                return;
            }

            if (converterType.IsValueType == false)
            {
                var ctors = converterType.GetMembers(".ctor");
                IMethodSymbol ctorMethod = null;

                foreach (var ctor in ctors)
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is not IMethodSymbol ctorM
                        || ctorM.DeclaredAccessibility != Accessibility.Public
                    )
                    {
                        continue;
                    }

                    if (ctorM.Parameters.Length == 0)
                    {
                        ctorMethod = ctorM;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    return;
                }
            }

            token.ThrowIfCancellationRequested();

            var members = converterType.GetMembers("Equals");
            var comparer = SymbolEqualityComparer.Default;

            foreach (var m in members)
            {
                token.ThrowIfCancellationRequested();

                if (m is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 2
                    || method.IsGenericMethod
                )
                {
                    continue;
                }

                var parameters = method.Parameters;
                var p0 = parameters[0].Type;
                var p1 = parameters[1].Type;

                if (comparer.Equals(p0, p1) == false)
                {
                    continue;
                }

                var typeName = p0.ToFullName();

                if (comparerMap.ContainsKey(typeName) == false)
                {
                    comparerMap[typeName] = method.IsStatic
                        ? $"{converterType.ToFullName()}.{method.Name}"
                        : $"new {converterType.ToFullName()}().{method.Name}";
                }
            }
        }

        private static bool TryGetConvertMethod(
              ITypeSymbol converterType
            , out IMethodSymbol convertMethod
            , ITypeSymbol paramType
            , ITypeSymbol returnType
            , Predicate<Accessibility> validateAccessibility
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (converterType.IsAbstract)
            {
                convertMethod = null;
                return false;
            }

            if (converterType is INamedTypeSymbol { IsUnboundGenericType: true })
            {
                convertMethod = null;
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var ctors = converterType.GetMembers(".ctor");
                IMethodSymbol ctorMethod = null;

                foreach (var ctor in ctors)
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is not IMethodSymbol ctorM
                        || validateAccessibility(ctorM.DeclaredAccessibility) == false
                    )
                    {
                        continue;
                    }

                    if (ctorM.Parameters.Length == 0)
                    {
                        ctorMethod = ctorM;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    convertMethod = null;
                    return false;
                }
            }

            token.ThrowIfCancellationRequested();

            var comparer = SymbolEqualityComparer.Default;
            var members = converterType.GetMembers("Convert");

            foreach (var m in members)
            {
                token.ThrowIfCancellationRequested();

                if (m is not IMethodSymbol method
                    || validateAccessibility(method.DeclaredAccessibility) == false
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                if (comparer.Equals(method.ReturnType, returnType) == false)
                {
                    continue;
                }

                if (comparer.Equals(method.Parameters[0].Type, paramType) == false)
                {
                    if (method.IsGenericMethod == false || method.TypeParameters.Length > 1)
                    {
                        continue;
                    }
                }

                convertMethod = method;
                return true;
            }

            convertMethod = default;
            return false;
        }

        private static void MakeFieldComparer(
              ref string fieldEqualityComparer
            , ITypeSymbol fieldType
            , ITypeSymbol propertyType
            , ISymbol targetSymbol
            , ITypeSymbol containingTypeSymbol
            , CancellationToken token
        )
        {
            ITypeSymbol comparerType = null;

            if (targetSymbol.GetAttribute(DATA_COMPARER_ATTRIBUTE, token) is AttributeData comparerAttrib
                && comparerAttrib.ConstructorArguments.Length > 0
                && comparerAttrib.ConstructorArguments[0].Value is ITypeSymbol comparerTypeCandidate
            )
            {
                comparerType = comparerTypeCandidate;
            }

            if (comparerType is not null)
            {
                if (TryGetEqualsMethod(comparerType, out var em, fieldType, IsPublic, token))
                {
                    fieldEqualityComparer = Make(comparerType, em);
                    return;
                }
            }

            if (TryGetEqualsMethod(containingTypeSymbol, out var em1, fieldType, IsAny, token))
            {
                fieldEqualityComparer = Make(containingTypeSymbol, em1);
            }
            else if (TryGetEqualsMethod(fieldType, out var em2, fieldType, IsPublic, token))
            {
                fieldEqualityComparer = Make(fieldType, em2);
            }
            else if (TryGetEqualsMethod(propertyType, out var em3, fieldType, IsPublic, token))
            {
                fieldEqualityComparer = Make(propertyType, em3);
            }

            return;

            static string Make(ITypeSymbol type, IMethodSymbol method)
                => method.IsStatic
                    ? $"{type.ToFullName()}.{method.Name}"
                    : $"new {type.ToFullName()}().{method.Name}";


            static bool IsPublic(Accessibility v) => v == Accessibility.Public;

            static bool IsAny(Accessibility _) => true;
        }

        private static bool TryGetEqualsMethod(
              ITypeSymbol converterType
            , out IMethodSymbol equalsMethod
            , ITypeSymbol paramType
            , Predicate<Accessibility> validateAccessibility
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (converterType.IsAbstract)
            {
                equalsMethod = null;
                return false;
            }

            if (converterType is INamedTypeSymbol { IsUnboundGenericType: true })
            {
                equalsMethod = null;
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var ctors = converterType.GetMembers(".ctor");
                IMethodSymbol ctorMethod = null;

                foreach (var ctor in ctors)
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is not IMethodSymbol ctorM
                        || validateAccessibility(ctorM.DeclaredAccessibility) == false
                    )
                    {
                        continue;
                    }

                    if (ctorM.Parameters.Length == 0)
                    {
                        ctorMethod = ctorM;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    equalsMethod = null;
                    return false;
                }
            }

            token.ThrowIfCancellationRequested();

            var comparer = SymbolEqualityComparer.Default;
            var members = converterType.GetMembers("Equals");

            foreach (var m in members)
            {
                token.ThrowIfCancellationRequested();

                if (m is not IMethodSymbol method
                    || validateAccessibility(method.DeclaredAccessibility) == false
                    || method.Parameters.Length != 2
                )
                {
                    continue;
                }

                var parameters = method.Parameters;
                var p0 = parameters[0].Type;
                var p1 = parameters[1].Type;

                if (comparer.Equals(p0, p1) == false)
                {
                    continue;
                }

                if (comparer.Equals(p0, paramType) == false
                    || comparer.Equals(p1, paramType) == false
                )
                {
                    if (method.IsGenericMethod == false || method.TypeParameters.Length > 1)
                    {
                        continue;
                    }
                }

                equalsMethod = method;
                return true;
            }

            equalsMethod = default;
            return false;
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETCE = global::EncosyTower.Collections.Extensions;");
            p.PrintLine("using ETD = global::EncosyTower.Data;");
            p.PrintLine("using ETDA = global::EncosyTower.Data.Authoring;");
            p.PrintLine("using ETDSG = global::EncosyTower.Data.SourceGen;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UP = global::Unity.Properties;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private record struct CollectionRef(
              CollectionKind Kind
            , ITypeSymbol KeyType
            , ITypeSymbol ElementType
            , bool IsKeyEquatable
            , bool IsElementEquatable
        );
    }
}
