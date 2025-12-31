using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Data.Data
{
    using static EncosyTower.SourceGen.Generators.Data.Helpers;

    public partial class DataDeclaration
    {
        public TypeDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string TypeName { get; }

        public string ReadOnlyTypeName { get; }

        public string TypeIdentifier { get; }

        public bool IsMutable { get; }

        public bool IsValueType { get; }

        public bool WithoutPropertySetters { get; }

        public bool WithReadOnlyView { get; }

        public DataFieldPolicy FieldPolicy { get; }

        public bool IsSealed { get; }

        public bool HasBaseType => string.IsNullOrEmpty(BaseTypeName) == false;

        public string BaseTypeName { get; }

        public ImmutableArray<Order> Orders { get; }

        public ImmutableArray<FieldRef> FieldRefs { get; }

        public ImmutableArray<PropertyRef> PropRefs { get; }

        public ImmutableArray<string> OverrideEquals { get; }

        public ImmutableArray<DiagnosticInfo> Diagnostics { get; }

        public bool HasSerializableAttribute { get; }

        public bool HasGeneratePropertyBagAttribute { get; }

        public bool HasGetHashCodeMethod { get; }

        public bool HasEqualsMethod { get; }

        public bool HasIEquatableMethod { get; }

        public ITypeSymbol IdPropertyType { get; }

        public bool WithoutId { get; }

        public bool IsValid { get; }

        public DataDeclaration(
              SourceProductionContext context
            , TypeDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , SemanticModel semanticModel
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            TypeIdentifier = Syntax.Identifier.Text;
            IsSealed = Symbol.IsSealed || Symbol.IsValueType;
            IsValueType = Symbol.IsValueType;
            HasSerializableAttribute = Symbol.HasAttribute(SERIALIZABLE_ATTRIBUTE);
            HasGeneratePropertyBagAttribute = Symbol.HasAttribute(GENERATE_PROPERTY_BAG_ATTRIBUTE);
            WithoutId = Symbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE) != null;

            var withId = WithoutId == false;

            DetermineMutability(
                  symbol
                , out var isMutable
                , out var withoutPropertySetters
                , out var withReadOnlyView
            );

            IsMutable = isMutable;
            WithoutPropertySetters = withoutPropertySetters;
            WithReadOnlyView = withReadOnlyView;

            var fieldPolicyAttrib = Symbol.GetAttribute(DATA_FIELD_POLICY_ATTRIBUTE);

            if (IsMutable == false && fieldPolicyAttrib != null)
            {
                context.ReportDiagnostic(
                      DiagnosticDescriptors.CannotDecorateImmutableDataWithFieldPolicyAttribute
                    , fieldPolicyAttrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                    , Symbol.Name
                );
                return;
            }

            if (fieldPolicyAttrib != null)
            {
                var args = fieldPolicyAttrib.ConstructorArguments;

                if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
                {
                    FieldPolicy = (DataFieldPolicy)args[0].Value;
                }
            }

            if (Symbol.BaseType is INamedTypeSymbol baseNamedTypeSymbol
                && baseNamedTypeSymbol.TypeKind == TypeKind.Class
                && baseNamedTypeSymbol.ImplementsInterface(IDATA)
            )
            {
                BaseTypeName = baseNamedTypeSymbol.ToFullName();
            }
            else
            {
                BaseTypeName = string.Empty;
            }

            var typeNameSb = new StringBuilder();

            {
                typeNameSb.Append(TypeIdentifier);

                if (syntax.TypeParameterList is TypeParameterListSyntax typeParamList
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

                TypeName = typeNameSb.ToString();
                ReadOnlyTypeName = $"ReadOnly{TypeName}";
            }

            var existingFields = new HashSet<string>();
            var existingProperties = new Dictionary<string, IPropertySymbol>();
            var existingOverrideEquals = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            using var orderBuilder = ImmutableArrayBuilder<Order>.Rent();
            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldRef>.Rent();
            using var propArrayBuilder = ImmutableArrayBuilder<PropertyRef>.Rent();
            using var overrideEqualsArrayBuilder = ImmutableArrayBuilder<string>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var allowOnlyPrivateOrInitSetter = IsMutable == false || WithoutPropertySetters;
            var equalityComparer = SymbolEqualityComparer.Default;
            var members = Symbol.GetMembers();

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    existingFields.Add(field.Name);

                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE) == false)
                    {
                        continue;
                    }

                    if (IsMutable == false && field.DeclaredAccessibility != Accessibility.Private)
                    {
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.ImmutableDataFieldMustBePrivate
                            , field.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken)
                            , Symbol.Name
                        );
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
                        , context.CancellationToken
                        , diagnosticBuilder
                        , out ImmutableArray<AttributeInfo> propertyAttributes
                        , DiagnosticDescriptors.InvalidPropertyTargetedAttribute
                    );

                    var propertyName = field.ToPropertyName();
                    var fieldEquality = fieldType.DetermineEquality();

                    if (fieldType.InheritsFromInterface(IDATA))
                    {
                        fieldEquality = new Equality(EqualityStrategy.Equals, false, false);
                    }
                    else if (fieldType.HasAttribute(UNION_ID_ATTRIBUTE))
                    {
                        fieldEquality = new Equality(EqualityStrategy.Operator, false, false);
                    }

                    var fieldRef = new FieldRef {
                        Field = field,
                        FieldType = fieldType,
                        FieldEquality = fieldEquality,
                        PropertyType = propertyType,
                        PropertyName = propertyName,
                        ForwardedPropertyAttributes = propertyAttributes,
                        FieldCollection = GetCollection(fieldType),
                        ImplicitlyConvertible = implicitlyConvertible,
                    };

                    MakeConverters(fieldRef, field, symbol);
                    MakeFieldComparer(fieldRef, field, symbol);

                    var index = fieldArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index });

                    fieldArrayBuilder.Add(fieldRef);

                    if (withId && string.Equals(propertyName, "Id", StringComparison.Ordinal))
                    {
                        if (fieldRef.PropertyCollection.Kind != CollectionKind.NotCollection)
                        {
                            context.ReportDiagnostic(
                                  DiagnosticDescriptors.CollectionIsNotApplicableForProperty
                                , field.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken)
                                , propertyType.ToDisplayString()
                                , "Id"
                            );
                        }
                        else
                        {
                            IdPropertyType = propertyType;
                        }
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    existingProperties.Add(property.Name, property);

                    if (allowOnlyPrivateOrInitSetter && property.SetMethod != null)
                    {
                        var setter = property.SetMethod;

                        if (setter.IsInitOnly == false && setter.DeclaredAccessibility != Accessibility.Private)
                        {
                            context.ReportDiagnostic(
                                  DiagnosticDescriptors.OnlyPrivateOrInitOnlySetterIsAllowed
                                , property.SetMethod.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken)
                                , Symbol.Name
                            );
                        }
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
                        , context.CancellationToken
                        , diagnosticBuilder
                        , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                        , DiagnosticDescriptors.InvalidFieldTargetedAttribute
                    );

                    var fieldName = property.ToPrivateFieldName();
                    var fieldEquality = fieldType.DetermineEquality();

                    if (fieldType.InheritsFromInterface(IDATA))
                    {
                        fieldEquality = new Equality(EqualityStrategy.Equals, false, false);
                    }
                    else if (fieldType.HasAttribute(UNION_ID_ATTRIBUTE))
                    {
                        fieldEquality = new Equality(EqualityStrategy.Operator, false, false);
                    }

                    var propRef = new PropertyRef {
                        Property = property,
                        FieldType = fieldType,
                        FieldEquality = fieldEquality,
                        PropertyType = property.Type,
                        FieldName = fieldName,
                        FieldIsImplemented = existingFields.Contains(fieldName),
                        ForwardedFieldAttributes = fieldAttributes,
                        FieldCollection = GetCollection(fieldType),
                        PropertyCollection = GetCollection(property.Type),
                        ImplicitlyConvertible = implicitlyConvertible,
                        DoesCreateProperty = property.HasAttribute(CREATE_PROPERTY_ATTRIBUTE),
                    };

                    MakeConverters(propRef, property, symbol);
                    MakeFieldComparer(propRef, property, symbol);

                    var index = propArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index, isPropRef = true });

                    propArrayBuilder.Add(propRef);

                    if (withId && string.Equals(property.Name, "Id", StringComparison.Ordinal))
                    {
                        if (propRef.PropertyCollection.Kind != CollectionKind.NotCollection)
                        {
                            context.ReportDiagnostic(
                                  DiagnosticDescriptors.CollectionIsNotApplicableForProperty
                                , property.SetMethod.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken)
                                , property.Type.ToDisplayString()
                                , "Id"
                            );
                        }
                        else
                        {
                            IdPropertyType = property.Type;
                        }
                    }

                    continue;
                }

                if (member is IMethodSymbol method)
                {
                    if (method.Name == "GetHashCode" && method.Parameters.Length == 0)
                    {
                        HasGetHashCodeMethod = true;
                        continue;
                    }

                    if (method.Name == "Equals" && method.Parameters.Length == 1
                        && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    )
                    {
                        var param = method.Parameters[0];

                        if (param.Type.SpecialType == SpecialType.System_Object)
                        {
                            HasEqualsMethod = true;
                            continue;
                        }

                        if (equalityComparer.Equals(Symbol, param.Type))
                        {
                            HasIEquatableMethod = true;
                            continue;
                        }

                        existingOverrideEquals.Add(param.Type);
                    }
                }
            }

            if (HasBaseType)
            {
                var baseType = Symbol.BaseType;

                while (baseType != null)
                {
                    if (baseType.ImplementsInterface(IDATA)
                        && existingOverrideEquals.Contains(baseType) == false
                    )
                    {
                        overrideEqualsArrayBuilder.Add(baseType.ToFullName());
                    }

                    baseType = baseType.BaseType;
                }
            }

            if (orderBuilder.Count > 0)
            {
                Orders = orderBuilder.ToImmutable();
            }
            else
            {
                Orders = ImmutableArray<Order>.Empty;
            }

            if (fieldArrayBuilder.Count > 0)
            {
                var fieldRefs = FieldRefs = fieldArrayBuilder.ToImmutable();

                foreach (var fieldRef in fieldRefs)
                {
                    if (existingProperties.TryGetValue(fieldRef.PropertyName, out var prop))
                    {
                        fieldRef.ImplementedProperty = prop;
                    }
                }
            }
            else
            {
                FieldRefs = ImmutableArray<FieldRef>.Empty;
            }

            if (propArrayBuilder.Count > 0)
            {
                var propRefs = PropRefs = propArrayBuilder.ToImmutable();

                foreach (var propRef in propRefs)
                {
                    propRef.FieldIsImplemented = existingFields.Contains(propRef.FieldName);
                }
            }
            else
            {
                PropRefs = ImmutableArray<PropertyRef>.Empty;
            }

            if (diagnosticBuilder.Count > 0)
            {
                Diagnostics = diagnosticBuilder.ToImmutable();
            }
            else
            {
                Diagnostics = ImmutableArray<DiagnosticInfo>.Empty;
            }

            if (overrideEqualsArrayBuilder.Count > 0)
            {
                OverrideEquals = overrideEqualsArrayBuilder.ToImmutable();
            }
            else
            {
                OverrideEquals = ImmutableArray<string>.Empty;
            }

            IsValid = true;
        }

        public static IDataType? GetIDataType(StringBuilder sb, ITypeSymbol type)
        {
            if (type.InheritsFromInterface(IDATA) == false)
            {
                return null;
            }

            DetermineMutability(
                  type
                , out var fieldTypeIsMutable
                , out _
                , out var fieldTypeWithReadOnlyView
            );

            GetTypeNames(
                  sb
                , type
                , fieldTypeIsMutable
                , fieldTypeWithReadOnlyView
                , out var fieldTypeName
                , out var fieldReadOnlyTypeName
            );

            return new IDataType(
                  fieldTypeIsMutable
                , fieldTypeWithReadOnlyView
                , fieldTypeName
                , fieldReadOnlyTypeName
            );
        }

        private static void GetTypeNames(
              StringBuilder sb
            , ITypeSymbol type
            , bool isMutable
            , bool withReadOnlyView
            , out string typeName
            , out string readOnlyTypeName
        )
        {
            sb.Clear();
            sb.Append("global::")
                .Append(type.ContainingNamespace.ToDisplayString())
                .Append(".");

            GetTypeName(type, sb, 0);
            typeName = sb.ToString();

            if (isMutable && withReadOnlyView)
            {
                sb.Clear();
                sb.Append("global::")
                    .Append(type.ContainingNamespace.ToDisplayString())
                    .Append(".ReadOnly");

                GetTypeName(type, sb, 0);
                readOnlyTypeName = sb.ToString();
            }
            else
            {
                readOnlyTypeName = string.Empty;
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

            if (attrib != null)
            {
                isMutable = true;

                var args = attrib.ConstructorArguments;

                if (args.Length > 0 && args[0].Kind == TypedConstantKind.Enum)
                {
                    var options = (DataMutableOptions)args[0].Value;
                    withoutPropertySetters = options.HasFlag(DataMutableOptions.WithoutPropertySetters);
                    withReadOnlyView = options.HasFlag(DataMutableOptions.WithReadOnlyStruct);
                }
            }
        }

        private static void GetTypeName(ITypeSymbol symbol, StringBuilder sb, int level)
        {
            if (symbol is not INamedTypeSymbol namedType)
            {
                sb.Append(symbol.ToSimpleName());
                return;
            }

            if (level < 1)
            {
                sb.Append(symbol.Name);
            }
            else
            {
                sb.Append(symbol.ToFullName());
            }

            var typeArgs = namedType.TypeArguments;

            if (typeArgs.Length > 0)
            {
                sb.Append('<');

                for (int i = 0; i < typeArgs.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    var typeArg = typeArgs[i];

                    if (typeArg is INamedTypeSymbol namedTypeArg)
                    {
                        GetTypeName(namedTypeArg, sb, level + 1);
                    }
                    else
                    {
                        sb.Append(typeArg.Name);
                    }
                }

                sb.Append('>');
            }
        }

        private static CollectionRef GetCollection(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return new CollectionRef {
                    Kind = CollectionKind.Array,
                    ElementType = arrayType.ElementType,
                };
            }

            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                return default;
            }

            if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemoryType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.ReadOnlyMemory,
                    ElementType = readMemoryType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memoryType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Memory,
                    ElementType = memoryType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.ReadOnlySpan,
                    ElementType = readSpanType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Span,
                    ElementType = spanType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var iReadListType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.ReadOnlyList,
                    ElementType = iReadListType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var iListType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = iListType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(LIST_FAST_TYPE_T, 1, out var listFastType, out var listFastFullName))
            {
                var isReadOnly = listFastFullName.EndsWith(">.ReadOnly");

                return new CollectionRef {
                    Kind = isReadOnly ? CollectionKind.ReadOnlyList : CollectionKind.List,
                    ElementType = listFastType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.List,
                    ElementType = listType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var iSetType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = iSetType.TypeArguments[0],
                };
            }

            if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = hashSetType.TypeArguments[0],
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
                    Kind = CollectionKind.ReadOnlyDictionary,
                    KeyType = iReadDictType.TypeArguments[0],
                    ElementType = iReadDictType.TypeArguments[1],
                };
            }

            if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var iDictType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = iDictType.TypeArguments[0],
                    ElementType = iDictType.TypeArguments[1],
                };
            }

            if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.Dictionary,
                    KeyType = dictType.TypeArguments[0],
                    ElementType = dictType.TypeArguments[1],
                };
            }

            return default;
        }

        private static void MakeConverters(
              MemberRef memberRef
            , ISymbol targetSymbol
            , ITypeSymbol containingTypeSymbol
        )
        {
            ITypeSymbol converterType;

            if (targetSymbol.GetAttribute(DATA_CONVERTER_ATTRIBUTE) is AttributeData attrib
                && attrib.ConstructorArguments.Length > 0
                && attrib.ConstructorArguments[0].Value is ITypeSymbol converterTypeCandidate
            )
            {
                converterType = converterTypeCandidate;
            }
            else
            {
                converterType = null;
            }

            if (converterType is not null)
            {
                if (TryGetConvertMethod(converterType, out var fieldConvertMethod, memberRef.PropertyType, memberRef.FieldType))
                {
                    memberRef.FieldConverter = Make(converterType, fieldConvertMethod);
                }

                if (TryGetConvertMethod(converterType, out var propConvertMethod, memberRef.FieldType, memberRef.PropertyType))
                {
                    memberRef.PropertyConverter = Make(converterType, propConvertMethod);
                }

                return;
            }

            if (TryGetConvertMethod(containingTypeSymbol, out var fieldConvertMethod1, memberRef.PropertyType, memberRef.FieldType))
            {
                memberRef.FieldConverter = Make(containingTypeSymbol, fieldConvertMethod1);
            }
            else if (TryGetConvertMethod(memberRef.FieldType, out var fieldConvertMethod2, memberRef.PropertyType, memberRef.FieldType))
            {
                memberRef.FieldConverter = Make(memberRef.FieldType, fieldConvertMethod2);
            }
            else if (TryGetConvertMethod(memberRef.PropertyType, out var fieldConvertMethod3, memberRef.PropertyType, memberRef.FieldType))
            {
                memberRef.FieldConverter = Make(memberRef.PropertyType, fieldConvertMethod3);
            }

            if (TryGetConvertMethod(containingTypeSymbol, out var propConvertMethod1, memberRef.FieldType, memberRef.PropertyType))
            {
                memberRef.PropertyConverter = Make(containingTypeSymbol, propConvertMethod1);
            }
            else if (TryGetConvertMethod(memberRef.PropertyType, out var propConvertMethod2, memberRef.FieldType, memberRef.PropertyType))
            {
                memberRef.PropertyConverter = Make(memberRef.PropertyType, propConvertMethod2);
            }
            else if (TryGetConvertMethod(memberRef.FieldType, out var propConvertMethod3, memberRef.FieldType, memberRef.PropertyType))
            {
                memberRef.PropertyConverter = Make(memberRef.FieldType, propConvertMethod3);
            }

            return;

            static string Make(ITypeSymbol type, IMethodSymbol method)
            {
                if (method.IsStatic)
                {
                    return $"{type.ToFullName()}.{method.Name}";
                }
                else
                {
                    return $"new {type.ToFullName()}().{method.Name}";
                }
            }
        }

        private static bool TryGetConvertMethod(
              ITypeSymbol converterType
            , out IMethodSymbol convertMethod
            , ITypeSymbol paramType
            , ITypeSymbol returnType
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
                    if (ctor is not IMethodSymbol method
                        || method.DeclaredAccessibility != Accessibility.Public
                    )
                    {
                        continue;
                    }

                    if (method.Parameters.Length == 0)
                    {
                        ctorMethod = method;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    convertMethod = null;
                    return false;
                }
            }

            var members = converterType.GetMembers("Convert");
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                if (comparer.Equals(method.ReturnType, returnType) == false
                    || comparer.Equals(method.Parameters[0].Type, paramType) == false
                )
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    convertMethod = method;
                    return true;
                }
                else
                {
                    convertMethod = method;
                    return true;
                }
            }

            convertMethod = default;
            return false;
        }

        private static void MakeFieldComparer(
              MemberRef memberRef
            , ISymbol targetSymbol
            , ITypeSymbol containingTypeSymbol
        )
        {
            ITypeSymbol comparerType;

            if (targetSymbol.GetAttribute(DATA_COMPARER_ATTRIBUTE) is AttributeData attrib
                && attrib.ConstructorArguments.Length > 0
                && attrib.ConstructorArguments[0].Value is ITypeSymbol comparerTypeCandidate
            )
            {
                comparerType = comparerTypeCandidate;
            }
            else
            {
                comparerType = null;
            }

            if (comparerType is not null)
            {
                if (TryGetEqualsMethod(comparerType, out var equalsMethod, memberRef.FieldType))
                {
                    memberRef.FieldEqualityComparer = Make(comparerType, equalsMethod);
                }
            }
            else if (TryGetEqualsMethod(containingTypeSymbol, out var equalsMethod1, memberRef.FieldType))
            {
                memberRef.FieldEqualityComparer = Make(containingTypeSymbol, equalsMethod1);
            }
            else if (TryGetEqualsMethod(memberRef.FieldType, out var equalsMethod2, memberRef.FieldType))
            {
                memberRef.FieldEqualityComparer = Make(memberRef.FieldType, equalsMethod2);
            }
            else if (TryGetEqualsMethod(memberRef.PropertyType, out var equalsMethod3, memberRef.FieldType))
            {
                memberRef.FieldEqualityComparer = Make(memberRef.PropertyType, equalsMethod3);
            }

            return;

            static string Make(ITypeSymbol type, IMethodSymbol method)
            {
                if (method.IsStatic)
                {
                    return $"{type.ToFullName()}.{method.Name}";
                }
                else
                {
                    return $"new {type.ToFullName()}().{method.Name}";
                }
            }
        }

        private static bool TryGetEqualsMethod(
              ITypeSymbol converterType
            , out IMethodSymbol equalsMethod
            , ITypeSymbol paramType
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
                    if (ctor is not IMethodSymbol method
                        || method.DeclaredAccessibility != Accessibility.Public
                    )
                    {
                        continue;
                    }

                    if (method.Parameters.Length == 0)
                    {
                        ctorMethod = method;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    equalsMethod = null;
                    return false;
                }
            }

            var members = converterType.GetMembers("Equals");
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 2
                )
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (method.ReturnType.IsUnmanagedType == false
                    || method.ReturnType.ToFullName() != "bool"
                    || comparer.Equals(parameters[0].Type, paramType) == false
                    || comparer.Equals(parameters[1].Type, paramType) == false
                )
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    equalsMethod = method;
                    return true;
                }
                else
                {
                    equalsMethod = method;
                    return true;
                }
            }

            equalsMethod = default;
            return false;
        }

        public record struct IDataType(
              bool IsMutable
            , bool WithReadOnlyView
            , string TypeName
            , string ReadOnlyTypeName
        );

        public abstract class MemberRef
        {
            public ITypeSymbol FieldType { get; set; }

            public ITypeSymbol PropertyType { get; set; }

            public CollectionRef FieldCollection { get; set; }

            public CollectionRef PropertyCollection { get; set; }

            public Equality FieldEquality { get; set; }

            public string FieldConverter { get; set; }

            public string PropertyConverter { get; set; }

            public string FieldEqualityComparer { get; set; }

            public bool ImplicitlyConvertible { get; set; }
        }

        public class FieldRef : MemberRef
        {
            public IFieldSymbol Field { get; set; }

            public string PropertyName { get; set; }

            public IPropertySymbol ImplementedProperty { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedPropertyAttributes { get; set; }
        }

        public class PropertyRef : MemberRef
        {
            public IPropertySymbol Property { get; set; }

            public string FieldName { get; set; }

            public bool FieldIsImplemented { get; set; }

            public ImmutableArray<(string, AttributeInfo)> ForwardedFieldAttributes { get; set; }

            public bool DoesCreateProperty { get; set; }
        }

        public record struct CollectionRef(
              CollectionKind Kind
            , ITypeSymbol KeyType
            , ITypeSymbol ElementType
        );

        public struct Order
        {
            public int index;
            public bool isPropRef;
        }

        public enum DataFieldPolicy
        {
            Private = 0,
            Internal,
            Public,
        }

        [Flags]
        public enum DataMutableOptions
        {
            Default = 0,
            WithoutPropertySetters = 1 << 0,
            WithReadOnlyStruct = 1 << 1,
        }
    }
}
