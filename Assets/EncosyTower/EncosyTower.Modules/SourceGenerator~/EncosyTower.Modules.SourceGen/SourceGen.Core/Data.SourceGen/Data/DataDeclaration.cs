using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;

    public partial class DataDeclaration
    {
        public TypeDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string ClassName { get; }

        public bool IsMutable { get; }

        public bool WithoutPropertySetter { get; }

        public DataFieldPolicy FieldPolicy { get; }

        public bool IsSealed { get; }

        public bool HasBaseType => string.IsNullOrEmpty(BaseTypeName) == false;

        public string BaseTypeName { get; }

        public bool ReferenceUnityEngine { get; }

        public ImmutableArray<Order> Orders { get; }

        public ImmutableArray<FieldRef> FieldRefs { get; }

        public ImmutableArray<PropertyRef> PropRefs { get; }

        public ImmutableArray<string> OverrideEquals { get; }

        public ImmutableArray<DiagnosticInfo> Diagnostics { get; }

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
            IsSealed = Symbol.IsSealed || Symbol.IsValueType;

            var withoutId = WithoutId = Symbol.GetAttribute(DATA_WITHOUT_ID_ATTRIBUTE) != null;
            var mutableAttrib = Symbol.GetAttribute(DATA_MUTABLE_ATTRIBUTE);

            if (mutableAttrib != null)
            {
                IsMutable = true;

                var args = mutableAttrib.ConstructorArguments;

                if (args.Length > 0 && args[0].Kind == TypedConstantKind.Primitive)
                {
                    WithoutPropertySetter = (bool)args[0].Value;
                }
            }

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

            {
                var classNameSb = new StringBuilder(Syntax.Identifier.Text);

                if (syntax.TypeParameterList is TypeParameterListSyntax typeParamList
                    && typeParamList.Parameters.Count > 0
                )
                {
                    classNameSb.Append("<");

                    var typeParams = typeParamList.Parameters;
                    var last = typeParams.Count - 1;

                    for (var i = 0; i <= last; i++)
                    {
                        classNameSb.Append(typeParams[i].Identifier.Text);

                        if (i < last)
                        {
                            classNameSb.Append(", ");
                        }
                    }

                    classNameSb.Append(">");
                }

                ClassName = classNameSb.ToString();
            }

            foreach (var assembly in Symbol.ContainingModule.ReferencedAssemblySymbols)
            {
                if (assembly.ToDisplayString().StartsWith("UnityEngine,"))
                {
                    ReferenceUnityEngine = true;
                    break;
                }
            }

            var existingFields = new HashSet<string>();
            var existingProperties = new HashSet<string>();
            var existingOverrideEquals = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            using var orderBuilder = ImmutableArrayBuilder<Order>.Rent();
            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldRef>.Rent();
            using var propArrayBuilder = ImmutableArrayBuilder<PropertyRef>.Rent();
            using var overrideEqualsArrayBuilder = ImmutableArrayBuilder<string>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var allowOnlyPrivateOrInitSetter = IsMutable == false || WithoutPropertySetter;
            var equalityComparer = SymbolEqualityComparer.Default;
            var members = Symbol.GetMembers();

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    existingFields.Add(field.Name);

                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE) == false
                        && field.HasAttribute(JSON_INCLUDE_ATTRIBUTE) == false
                        && field.HasAttribute(JSON_PROPERTY_ATTRIBUTE) == false
                    )
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

                    if (field.TryGetAttribute(PROPERTY_TYPE_ATTRIBUTE, out var propertyTypeAttrib)
                        && propertyTypeAttrib.ConstructorArguments is { Length: > 0 } args
                        && args[0].Value is ITypeSymbol propTypeSymbol)
                    {
                        propertyType = propTypeSymbol;
                        implicitlyConvertible = true;
                    }
                    else
                    {
                        propertyType = field.Type;
                        implicitlyConvertible = false;
                    }

                    field.GatherForwardedAttributes(
                          semanticModel
                        , context.CancellationToken
                        , diagnosticBuilder
                        , out var propertyAttributes
                        , DiagnosticDescriptors.InvalidPropertyTargetedAttribute
                    );

                    var propertyName = field.ToPropertyName();

                    var fieldRef = new FieldRef {
                        Field = field,
                        FieldType = field.Type,
                        PropertyType = propertyType,
                        PropertyName = propertyName,
                        PropertyIsImplemented = existingProperties.Contains(propertyName),
                        ForwardedPropertyAttributes = propertyAttributes,
                        FieldCollection = GetFieldCollection(field.Type),
                        ImplicitlyConvertible = implicitlyConvertible,
                    };

                    var index = fieldArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index });

                    fieldArrayBuilder.Add(fieldRef);

                    if (withoutId == false && string.Equals(propertyName, "Id", StringComparison.Ordinal))
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
                    existingProperties.Add(property.Name);

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

                    var fieldName = property.ToFieldName();

                    var propRef = new PropertyRef {
                        Property = property,
                        FieldType = fieldType,
                        PropertyType = property.Type,
                        FieldName = fieldName,
                        FieldIsImplemented = existingFields.Contains(fieldName),
                        ForwardedFieldAttributes = fieldAttributes,
                        FieldCollection = GetFieldCollection(fieldType),
                        PropertyCollection = GetPropertyCollection(property.Type),
                        ImplicitlyConvertible = implicitlyConvertible,
                    };

                    var index = propArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index, isPropRef = true });

                    propArrayBuilder.Add(propRef);

                    if (withoutId == false && string.Equals(property.Name, "Id", StringComparison.Ordinal))
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
                FieldRefs = fieldArrayBuilder.ToImmutable();
            }
            else
            {
                FieldRefs = ImmutableArray<FieldRef>.Empty;
            }

            if (propArrayBuilder.Count > 0)
            {
                PropRefs = propArrayBuilder.ToImmutable();
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

        private static CollectionRef GetFieldCollection(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                return default;
            }

            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return new CollectionRef {
                    Kind = CollectionKind.Array,
                    ElementType = arrayType.ElementType,
                };
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

        private static CollectionRef GetPropertyCollection(ITypeSymbol typeSymbol)
        {
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

            if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var iSetType))
            {
                return new CollectionRef {
                    Kind = CollectionKind.HashSet,
                    ElementType = iSetType.TypeArguments[0],
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

            return default;
        }

        public abstract class MemberRef
        {
            public ITypeSymbol FieldType { get; set; }

            public ITypeSymbol PropertyType { get; set; }

            public CollectionRef FieldCollection { get; set; }

            public CollectionRef PropertyCollection { get; set; }

            public bool ImplicitlyConvertible { get; set; }
        }

        public class FieldRef : MemberRef
        {
            public IFieldSymbol Field { get; set; }

            public string PropertyName { get; set; }

            public bool PropertyIsImplemented { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedPropertyAttributes { get; set; }
        }

        public class PropertyRef : MemberRef
        {
            public IPropertySymbol Property { get; set; }

            public string FieldName { get; set; }

            public bool FieldIsImplemented { get; set; }

            public ImmutableArray<(string, AttributeInfo)> ForwardedFieldAttributes { get; set; }
        }

        public struct CollectionRef
        {
            public CollectionKind Kind { get; set; }

            public ITypeSymbol KeyType { get; set; }

            public ITypeSymbol ElementType { get; set; }
        }

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
    }
}
