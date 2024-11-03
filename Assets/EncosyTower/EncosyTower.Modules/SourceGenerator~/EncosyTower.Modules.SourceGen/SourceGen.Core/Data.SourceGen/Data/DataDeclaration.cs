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
                        Type = field.Type,
                        PropertyName = propertyName,
                        PropertyIsImplemented = existingProperties.Contains(propertyName),
                        ForwardedPropertyAttributes = propertyAttributes,
                    };

                    if (field.Type is IArrayTypeSymbol arrayType)
                    {
                        fieldRef.CollectionKind = CollectionKind.Array;
                        fieldRef.CollectionElementType = arrayType.ElementType;
                    }
                    else if (field.Type is INamedTypeSymbol namedType)
                    {
                        if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
                        {
                            fieldRef.CollectionKind = CollectionKind.List;
                            fieldRef.CollectionElementType = listType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
                        {
                            fieldRef.CollectionKind = CollectionKind.Dictionary;
                            fieldRef.CollectionKeyType = dictType.TypeArguments[0];
                            fieldRef.CollectionElementType = dictType.TypeArguments[1];
                        }
                        else if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
                        {
                            fieldRef.CollectionKind = CollectionKind.HashSet;
                            fieldRef.CollectionElementType = hashSetType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
                        {
                            fieldRef.CollectionKind = CollectionKind.Queue;
                            fieldRef.CollectionElementType = queueType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
                        {
                            fieldRef.CollectionKind = CollectionKind.Stack;
                            fieldRef.CollectionElementType = stackType.TypeArguments[0];
                        }
                    }

                    var index = fieldArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index });

                    fieldArrayBuilder.Add(fieldRef);

                    if (string.Equals(propertyName, "Id", StringComparison.Ordinal))
                    {
                        if (fieldRef.CollectionKind != CollectionKind.NotCollection)
                        {
                            context.ReportDiagnostic(
                                  DiagnosticDescriptors.CollectionIsNotApplicableForProperty
                                , field.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken)
                                , field.Type.ToDisplayString()
                                , "Id"
                            );
                        }
                        else
                        {
                            IdPropertyType = field.Type;
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

                    var checkAutoCollectionType = false;

                    if (attribute.ConstructorArguments.Length < 1
                        || attribute.ConstructorArguments[0].Value is not ITypeSymbol fieldType
                    )
                    {
                        fieldType = property.Type;
                        checkAutoCollectionType = true;
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
                        Type = fieldType,
                        FieldName = fieldName,
                        FieldIsImplemented = existingFields.Contains(fieldName),
                        ForwardedFieldAttributes = fieldAttributes,
                    };

                    if (checkAutoCollectionType && property.Type is INamedTypeSymbol namedType)
                    {
                        if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemoryType))
                        {
                            propRef.CollectionKind = CollectionKind.ReadOnlyMemory;
                            propRef.CollectionElementType = readMemoryType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memoryType))
                        {
                            propRef.CollectionKind = CollectionKind.Memory;
                            propRef.CollectionElementType = memoryType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
                        {
                            propRef.CollectionKind = CollectionKind.ReadOnlySpan;
                            propRef.CollectionElementType = readSpanType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
                        {
                            propRef.CollectionKind = CollectionKind.Span;
                            propRef.CollectionElementType = spanType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var readListType))
                        {
                            propRef.CollectionKind = CollectionKind.ReadOnlyList;
                            propRef.CollectionElementType = readListType.TypeArguments[0];
                        }
                        else if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var readDictType))
                        {
                            propRef.CollectionKind = CollectionKind.ReadOnlyDictionary;
                            propRef.CollectionKeyType = readDictType.TypeArguments[0];
                            propRef.CollectionElementType = readDictType.TypeArguments[1];
                        }
                    }

                    var index = propArrayBuilder.Count;
                    orderBuilder.Add(new Order { index = index, isPropRef = true });

                    propArrayBuilder.Add(propRef);

                    if (string.Equals(property.Name, "Id", StringComparison.Ordinal))
                    {
                        if (propRef.CollectionKind != CollectionKind.NotCollection)
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

        public abstract class MemberRef
        {
            public ITypeSymbol Type { get; set; }

            public CollectionKind CollectionKind { get; set; }

            public ITypeSymbol CollectionKeyType { get; set; }

            public ITypeSymbol CollectionElementType { get; set; }
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
