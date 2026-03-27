using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Data
{
    using static EncosyTower.SourceGen.Common.Data.Common.Helpers;

    partial struct DataDeclaration
    {
        /// <summary>
        /// Extracts all data-type metadata from the annotated type symbol into a fully populated,
        /// cache-friendly <see cref="DataDeclaration"/>.
        /// Called once per type inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static DataDeclaration Extract(
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

            var semanticModel = context.SemanticModel;
            var syntaxTree = typeSyntax.SyntaxTree;
            var typeIdent = typeSyntax.Identifier.Text;
            var typeValidIdent = typeSymbol.ToValidIdentifier();
            var isValueType = typeSymbol.IsValueType;
            var isSealed = typeSymbol.IsSealed || isValueType;

            // ── TypeName (with type params if generic) ──────────────────────────────────────
            var typeNameSb = new StringBuilder(typeIdent);

            if (typeSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                typeNameSb.Append("<");
                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    typeNameSb.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        typeNameSb.Append(", ");
                    }
                }

                typeNameSb.Append(">");
            }

            var typeName = typeNameSb.ToString();
            var readOnlyTypeName = $"ReadOnly{typeName}";

            // ── Opening / closing source (namespace wrapper) ────────────────────────────────
            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            // ── hint / sourceFilePath ───────────────────────────────────────────────────────
            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  GENERATOR_NAME
                , typeSyntax
                , typeValidIdent
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  semanticModel.Compilation.AssemblyName
                , GENERATOR_NAME
                , typeValidIdent
            );

            // ── Attributes on the container type ───────────────────────────────────────────
            var hasSerializableAttribute = typeSymbol.HasAttribute(SERIALIZABLE_ATTRIBUTE);
            var hasGeneratePropertyBagAttribute = typeSymbol.HasAttribute(GENERATE_PROPERTY_BAG_ATTRIBUTE);
            var withoutId = typeSymbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE) != null;
            var withId = withoutId == false;

            // ── Mutability ──────────────────────────────────────────────────────────────────
            DetermineMutability(
                  typeSymbol
                , out var isMutable
                , out var withoutPropertySetters
                , out var withReadOnlyView
            );

            // ── DataFieldPolicy ─────────────────────────────────────────────────────────────
            var fieldPolicyAttrib = typeSymbol.GetAttribute(DATA_FIELD_POLICY_ATTRIBUTE);
            var fieldPolicy = DataFieldPolicy.Private;

            if (isMutable == false && fieldPolicyAttrib != null)
            {
                // Diagnostic emitted by DataDiagnosticAnalyzer — skip in generator, return invalid
                return default;
            }

            if (fieldPolicyAttrib != null)
            {
                var args = fieldPolicyAttrib.ConstructorArguments;
                if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
                {
                    fieldPolicy = (DataFieldPolicy)(int)args[0].Value;
                }
            }

            // ── Base type ───────────────────────────────────────────────────────────────────
            var baseTypeName = string.Empty;

            if (typeSymbol.BaseType is INamedTypeSymbol baseNamedTypeSymbol
                && baseNamedTypeSymbol.TypeKind == TypeKind.Class
                && baseNamedTypeSymbol.ImplementsInterface(IDATA)
            )
            {
                baseTypeName = baseNamedTypeSymbol.ToFullName();
            }

            // ── Accessibility keyword ───────────────────────────────────────────────────────
            var accessibilityKeyword = typeSymbol.DeclaredAccessibility.ToKeyword();

            // ── Location ────────────────────────────────────────────────────────────────────
            var locationInfo = LocationInfo.From(typeSymbol.Locations.IsEmpty
                ? Location.None
                : typeSymbol.Locations[0]);

            // ── Per-member analysis ─────────────────────────────────────────────────────────
            var existingFields = new HashSet<string>();
            var existingProperties = new Dictionary<string, bool>(); // name → isPublic
            var existingOverrideEquals = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var allowOnlyPrivateOrInitSetter = isMutable == false || withoutPropertySetters;
            var equalityComparer = SymbolEqualityComparer.Default;

            using var orderBuilder = ImmutableArrayBuilder<OrderData>.Rent();
            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldRefData>.Rent();
            using var propArrayBuilder = ImmutableArrayBuilder<PropRefData>.Rent();
            using var overrideEqualsBuilder = ImmutableArrayBuilder<string>.Rent();
            // Diagnostics from GatherForwardedAttributes are discarded; the analyzer handles them.
            using var throwawayDiagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var idPropertyTypeName = string.Empty;
            var hasGetHashCodeMethod = false;
            var hasEqualsMethod = false;
            var hasIEquatableMethod = false;

            var typeSb = new StringBuilder();
            var members = typeSymbol.GetMembers();

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field)
                {
                    existingFields.Add(field.Name);

                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE) == false)
                    {
                        continue;
                    }

                    if (isMutable == false && field.DeclaredAccessibility != Accessibility.Private)
                    {
                        // Diagnostic emitted by DataDiagnosticAnalyzer — skip member
                        continue;
                    }

                    ITypeSymbol propertyType;
                    bool implicitlyConvertible;
                    var fieldType = field.Type;

                    if (field.TryGetAttribute(PROPERTY_TYPE_ATTRIBUTE, out var propertyTypeAttrib)
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
                        , throwawayDiagnosticBuilder
                        , out ImmutableArray<AttributeInfo> propertyAttributes
                        , DiagnosticDescriptors.InvalidPropertyTargetedAttribute
                    );

                    var propertyName = field.ToPropertyName();
                    var collection = GetCollection(fieldType, isField: true);
                    var fieldEquality = GetEquality(fieldType);
                    var fieldConverter = string.Empty;
                    var propertyConverter = string.Empty;
                    var fieldEqualityComparer = string.Empty;

                    MakeConverters(
                          ref fieldConverter
                        , ref propertyConverter
                        , fieldType
                        , propertyType
                        , typeSymbol
                    );

                    MakeFieldComparer(
                          ref fieldEqualityComparer
                        , fieldType
                        , propertyType
                        , field
                        , typeSymbol
                    );

                    // Precompute type names
                    var fieldTypeName = GetFieldTypeName(fieldType, collection);
                    var fieldTypeOriginalFullName = fieldType.ToFullName();
                    var propertyTypeName = propertyType.ToFullName();

                    GetTypeNames(
                          propertyType
                        , collection
                        , out var mutablePropertyTypeName
                        , out var immutablePropertyTypeName
                        , out var samePropertyType
                    );

                    var typesAreDifferent = equalityComparer.Equals(fieldType, propertyType) == false;

                    // Equality precomputation
                    ITypeSymbol equalityFieldType;

                    if (fieldEquality.IsNullable)
                    {
                        equalityFieldType = fieldType.GetTypeFromNullable() ?? fieldType;
                    }
                    else
                    {
                        equalityFieldType = fieldType;
                    }

                    var fieldTypeFullNameForEquality = equalityFieldType.ToFullName();
                    var fieldTypeIsReferenceType = equalityFieldType.IsReferenceType;

                    // Forwarded property attribute syntaxes
                    using var attrSyntaxBuilder = ImmutableArrayBuilder<string>.Rent();

                    foreach (var attr in propertyAttributes)
                    {
                        attrSyntaxBuilder.Add(attr.GetSyntax().ToFullString());
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
                        forwardedPropertyAttributeSyntaxes = attrSyntaxBuilder.ToImmutable().AsEquatableArray(),
                    };

                    var fieldIndex = fieldArrayBuilder.Count;
                    orderBuilder.Add(new OrderData { index = fieldIndex, isPropRef = false });
                    fieldArrayBuilder.Add(fieldRefData);

                    if (withId && string.Equals(propertyName, "Id", StringComparison.Ordinal))
                    {
                        if (collection.Kind != CollectionKind.NotCollection)
                        {
                            // Diagnostic emitted by analyzer — skip
                        }
                        else
                        {
                            idPropertyTypeName = propertyType.ToFullName();
                        }
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    existingProperties[property.Name] = property.DeclaredAccessibility == Accessibility.Public;

                    if (allowOnlyPrivateOrInitSetter && property.SetMethod != null)
                    {
                        var setter = property.SetMethod;
                        // Only warn — handled by analyzer
                        _ = setter.IsInitOnly || setter.DeclaredAccessibility == Accessibility.Private;
                    }

                    var attribute = property.GetAttribute(DATA_PROPERTY_ATTRIBUTE);

                    if (attribute == null)
                    {
                        continue;
                    }

                    ITypeSymbol fieldType;
                    bool implicitlyConvertible;

                    if (attribute.ConstructorArguments.Length > 0
                        && attribute.ConstructorArguments[0].Value is ITypeSymbol fieldTypeSymbol
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
                        , throwawayDiagnosticBuilder
                        , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                        , DiagnosticDescriptors.InvalidFieldTargetedAttribute
                    );

                    var fieldName = property.ToPrivateFieldName();
                    var collection = GetCollection(fieldType, isField: true);
                    var propCollection = GetCollection(property.Type, isField: false);
                    var fieldEquality = GetEquality(fieldType);
                    var fieldConverter = string.Empty;
                    var propertyConverter = string.Empty;
                    var fieldEqualityComparer = string.Empty;

                    MakeConverters(
                          ref fieldConverter
                        , ref propertyConverter
                        , fieldType
                        , property.Type
                        , typeSymbol
                    );

                    MakeFieldComparer(
                          ref fieldEqualityComparer
                        , fieldType
                        , property.Type
                        , property
                        , typeSymbol
                    );

                    // Precompute type names
                    var fieldTypeName = GetFieldTypeName(fieldType, collection);
                    var propertyTypeName = property.Type.ToFullName();

                    GetTypeNames(
                          property.Type
                        , collection
                        , out var mutablePropertyTypeName
                        , out var immutablePropertyTypeName
                        , out var samePropertyType
                    );

                    var typesAreDifferent = equalityComparer.Equals(fieldType, property.Type) == false;

                    // Equality precomputation for PropRef uses GetFieldTypeName (not ToFullName)
                    ITypeSymbol equalityFieldType;

                    if (fieldEquality.IsNullable)
                    {
                        equalityFieldType = fieldType.GetTypeFromNullable() ?? fieldType;
                    }
                    else
                    {
                        equalityFieldType = fieldType;
                    }

                    // GetFieldTypeName for the equality-field-type with the same collection data
                    var equalityCollection = GetCollection(equalityFieldType, isField: true);
                    var fieldTypeDeclNameForEquality = GetFieldTypeName(equalityFieldType, equalityCollection);
                    var fieldTypeIsReferenceType = equalityFieldType.IsReferenceType;

                    // Forwarded field attributes (fullTypeName + syntax)
                    using var attrBuilder = ImmutableArrayBuilder<ForwardedFieldAttributeData>.Rent();

                    foreach (var (fullTypeName, attributeInfo) in fieldAttributes)
                    {
                        attrBuilder.Add(new ForwardedFieldAttributeData {
                            fullTypeName = fullTypeName,
                            attributeSyntax = attributeInfo.GetSyntax().ToFullString(),
                        });
                    }

                    var doesCreateProperty = property.HasAttribute(CREATE_PROPERTY_ATTRIBUTE);
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
                    };

                    var propIndex = propArrayBuilder.Count;
                    orderBuilder.Add(new OrderData { index = propIndex, isPropRef = true });
                    propArrayBuilder.Add(propRefData);

                    if (withId && string.Equals(property.Name, "Id", StringComparison.Ordinal))
                    {
                        if (propCollection.Kind != CollectionKind.NotCollection)
                        {
                            // Diagnostic emitted by analyzer — skip
                        }
                        else
                        {
                            idPropertyTypeName = property.Type.ToFullName();
                        }
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
                }
            }

            // ── Resolve ImplementedProperty / FieldIsImplemented post-scan ──────────────────
            var resolvedFieldRefs = fieldArrayBuilder.ToImmutable();

            for (var i = 0; i < resolvedFieldRefs.Length; i++)
            {
                var fr = resolvedFieldRefs[i];

                if (existingProperties.TryGetValue(fr.propertyName, out var isPublic))
                {
                    fr.hasImplementedProperty = true;
                    fr.isImplementedPropertyPublic = isPublic;
                    resolvedFieldRefs = resolvedFieldRefs.SetItem(i, fr);
                }
            }

            var resolvedPropRefs = propArrayBuilder.ToImmutable();

            for (var i = 0; i < resolvedPropRefs.Length; i++)
            {
                var pr = resolvedPropRefs[i];

                if (existingFields.Contains(pr.fieldName) != pr.fieldIsImplemented)
                {
                    pr.fieldIsImplemented = existingFields.Contains(pr.fieldName);
                    resolvedPropRefs = resolvedPropRefs.SetItem(i, pr);
                }
            }

            // ── Override-equals list (base classes that also implement IData) ────────────────
            if (string.IsNullOrEmpty(baseTypeName) == false)
            {
                var baseType = typeSymbol.BaseType;

                while (baseType != null)
                {
                    if (baseType.ImplementsInterface(IDATA)
                        && existingOverrideEquals.Contains(baseType) == false
                    )
                    {
                        overrideEqualsBuilder.Add(baseType.ToFullName());
                    }

                    baseType = baseType.BaseType;
                }
            }

            return new DataDeclaration {
                location = locationInfo,
                typeName = typeName,
                readOnlyTypeName = readOnlyTypeName,
                typeIdentifier = typeIdent,
                typeValidIdentifier = typeValidIdent,
                baseTypeName = baseTypeName,
                accessibilityKeyword = accessibilityKeyword,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                idPropertyTypeName = idPropertyTypeName,
                fieldPolicy = fieldPolicy,
                isMutable = isMutable,
                isValueType = isValueType,
                withoutPropertySetters = withoutPropertySetters,
                withReadOnlyView = withReadOnlyView,
                isSealed = isSealed,
                hasSerializableAttribute = hasSerializableAttribute,
                hasGeneratePropertyBagAttribute = hasGeneratePropertyBagAttribute,
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

        // ── Internal helpers (symbol-analysis, only used during extraction) ─────────────────

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

        private static Equality GetEquality(ITypeSymbol type)
        {
            var fieldEquality = type.DetermineEquality();

            if (IsIData(type))
            {
                fieldEquality = new Equality(EqualityStrategy.Equals, false, false);
            }
            else if (type.HasAttribute(UNION_ID_ATTRIBUTE))
            {
                fieldEquality = new Equality(EqualityStrategy.Operator, false, false);
            }

            return fieldEquality;
        }

        private static bool GetEquatable(ITypeSymbol type)
        {
            return IsIData(type)
                || type.HasAttribute(UNION_ID_ATTRIBUTE)
                || type.DetermineIEquatable()
                ;
        }

        private static CollectionRef GetCollection(ITypeSymbol typeSymbol, bool isField)
        {
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return new CollectionRef {
                    Kind = CollectionKind.Array,
                    ElementType = arrayType.ElementType,
                    IsElementEquatable = GetEquatable(arrayType.ElementType),
                };
            }

            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                return default;
            }

            if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemoryType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.ReadOnlyMemory,
                    ElementType = readMemoryType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(readMemoryType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memoryType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.Memory,
                    ElementType = memoryType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(memoryType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.ReadOnlySpan,
                    ElementType = readSpanType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(readSpanType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Array : CollectionKind.Span,
                    ElementType = spanType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(spanType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var iReadListType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.List : CollectionKind.ReadOnlyList,
                    ElementType = iReadListType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(iReadListType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var iListType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = iListType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(iListType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericFromContainingType(LIST_FAST_TYPE_T, 1, out var listFastType))
            {
                var isReadOnly = namedType.Name.Equals("ReadOnly");

                return new CollectionRef {
                    Kind = isField || isReadOnly == false ? CollectionKind.List : CollectionKind.ReadOnlyList,
                    ElementType = listFastType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(listFastType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = listType.TypeArguments[0],
                    IsElementEquatable = GetEquatable(listType.TypeArguments[0]),
                };
            }

            if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var iSetType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = iSetType.TypeArguments[0],
                    IsElementEquatable = iSetType.TypeArguments[0].DetermineIEquatable(),
                };
            }

            if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = hashSetType.TypeArguments[0],
                    IsElementEquatable = hashSetType.TypeArguments[0].DetermineIEquatable(),
                };
            }

            if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Queue,
                    ElementType = queueType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Stack,
                    ElementType = stackType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var iReadDictType))
            {
                return new CollectionRef {
                    Kind = isField ? CollectionKind.Dictionary : CollectionKind.ReadOnlyDictionary,
                    KeyType = iReadDictType.TypeArguments[0],
                    ElementType = iReadDictType.TypeArguments[1],
                    IsKeyEquatable = iReadDictType.TypeArguments[0].DetermineIEquatable(),
                    IsElementEquatable = iReadDictType.TypeArguments[1].DetermineIEquatable(),
                };
            }

            if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var iDictType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = iDictType.TypeArguments[0],
                    ElementType = iDictType.TypeArguments[1],
                    IsKeyEquatable = iDictType.TypeArguments[0].DetermineIEquatable(),
                    IsElementEquatable = iDictType.TypeArguments[1].DetermineIEquatable(),
                };
            }

            if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = dictType.TypeArguments[0],
                    ElementType = dictType.TypeArguments[1],
                    IsKeyEquatable = dictType.TypeArguments[0].DetermineIEquatable(),
                    IsElementEquatable = dictType.TypeArguments[1].DetermineIEquatable(),
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
        )
        {
            isMutable = false;
            withoutPropertySetters = false;
            withReadOnlyView = false;

            var attrib = symbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE);

            if (attrib == null)
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
            , ITypeSymbol fieldType
            , ITypeSymbol propertyType
            , ITypeSymbol containingTypeSymbol
        )
        {
            ITypeSymbol converterType = null;

            if (converterType is null)
            {
                if (TryGetConvertMethod(containingTypeSymbol, out var fc1, propertyType, fieldType, IsAny))
                {
                    fieldConverter = Make(containingTypeSymbol, fc1);
                }
                else if (TryGetConvertMethod(fieldType, out var fc2, propertyType, fieldType, IsPublic))
                {
                    fieldConverter = Make(fieldType, fc2);
                }
                else if (TryGetConvertMethod(propertyType, out var fc3, propertyType, fieldType, IsPublic))
                {
                    fieldConverter = Make(propertyType, fc3);
                }

                if (TryGetConvertMethod(containingTypeSymbol, out var pc1, fieldType, propertyType, IsAny))
                {
                    propertyConverter = Make(containingTypeSymbol, pc1);
                }
                else if (TryGetConvertMethod(propertyType, out var pc2, fieldType, propertyType, IsPublic))
                {
                    propertyConverter = Make(propertyType, pc2);
                }
                else if (TryGetConvertMethod(fieldType, out var pc3, fieldType, propertyType, IsPublic))
                {
                    propertyConverter = Make(fieldType, pc3);
                }

                return;
            }

            if (TryGetConvertMethod(converterType, out var fm, propertyType, fieldType, IsPublic))
            {
                fieldConverter = Make(converterType, fm);
            }

            if (TryGetConvertMethod(converterType, out var pm, fieldType, propertyType, IsPublic))
            {
                propertyConverter = Make(converterType, pm);
            }

            return;

            static string Make(ITypeSymbol type, IMethodSymbol method)
                => method.IsStatic
                    ? $"{type.ToFullName()}.{method.Name}"
                    : $"new {type.ToFullName()}().{method.Name}";

            static bool IsPublic(Accessibility v) => v == Accessibility.Public;

            static bool IsAny(Accessibility _) => true;
        }

        private static bool TryGetConvertMethod(
              ITypeSymbol converterType
            , out IMethodSymbol convertMethod
            , ITypeSymbol paramType
            , ITypeSymbol returnType
            , Predicate<Accessibility> validateAccessibility
        )
        {
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
                    if (ctor is not IMethodSymbol ctorM
                        || validateAccessibility(ctorM.DeclaredAccessibility) == false)
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

            var comparer = SymbolEqualityComparer.Default;
            var members = converterType.GetMembers("Convert");

            foreach (var m in members)
            {
                if (m is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 1)
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
        )
        {
            ITypeSymbol comparerType = null;

            if (targetSymbol.GetAttribute(DATA_COMPARER_ATTRIBUTE) is AttributeData comparerAttrib
                && comparerAttrib.ConstructorArguments.Length > 0
                && comparerAttrib.ConstructorArguments[0].Value is ITypeSymbol comparerTypeCandidate
            )
            {
                comparerType = comparerTypeCandidate;
            }

            if (comparerType is not null)
            {
                if (TryGetEqualsMethod(comparerType, out var em, fieldType, IsPublic))
                {
                    fieldEqualityComparer = Make(comparerType, em);
                }

                return;
            }

            if (TryGetEqualsMethod(containingTypeSymbol, out var em1, fieldType, IsAny))
            {
                fieldEqualityComparer = Make(containingTypeSymbol, em1);
            }
            else if (TryGetEqualsMethod(fieldType, out var em2, fieldType, IsPublic))
            {
                fieldEqualityComparer = Make(fieldType, em2);
            }
            else if (TryGetEqualsMethod(propertyType, out var em3, fieldType, IsPublic))
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
        )
        {
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

            var comparer = SymbolEqualityComparer.Default;
            var members = converterType.GetMembers("Equals");

            foreach (var m in members)
            {
                if (m is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
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

        // Temporary record used only during extraction — not stored in cache model
        private record struct CollectionRef(
              CollectionKind Kind
            , ITypeSymbol KeyType
            , ITypeSymbol ElementType
            , bool IsKeyEquatable
            , bool IsElementEquatable
        );
    }
}
