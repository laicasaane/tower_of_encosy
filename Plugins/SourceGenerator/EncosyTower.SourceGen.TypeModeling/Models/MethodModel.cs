using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct MethodModel : IEquatable<MethodModel>
    {
        public readonly string Name;
        public readonly string ReturnTypeName;
        public readonly string ReturnTypeFullName;
        public readonly string Accessibility;
        public readonly RefKind RefKind;
        public readonly bool ReturnsVoid;
        public readonly bool IsStatic;
        public readonly bool IsReadOnly;
        public readonly bool IsAbstract;
        public readonly bool IsVirtual;
        public readonly MethodKind MethodKind;
        public readonly EquatableArray<ParameterModel> Parameters;
        public readonly EquatableArray<string> TypeParameters;
        public readonly EquatableArray<AttributeModel> Attributes;

        public MethodModel(
              string name
            , string returnTypeName
            , string returnTypeFullName
            , string accessibility
            , RefKind refKind
            , bool returnsVoid
            , bool isStatic
            , bool isReadOnly
            , bool isAbstract
            , bool isVirtual
            , MethodKind methodKind
            , EquatableArray<ParameterModel> parameters
            , EquatableArray<string> typeParameters
            , EquatableArray<AttributeModel> attributes
        )
        {
            Name = name ?? string.Empty;
            ReturnTypeName = returnTypeName ?? string.Empty;
            ReturnTypeFullName = returnTypeFullName ?? string.Empty;
            Accessibility = accessibility ?? string.Empty;
            RefKind = refKind;
            ReturnsVoid = returnsVoid;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
            IsAbstract = isAbstract;
            IsVirtual = isVirtual;
            MethodKind = methodKind;
            Parameters = parameters;
            TypeParameters = typeParameters;
            Attributes = attributes;
        }

        public bool Equals(MethodModel other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(ReturnTypeFullName, other.ReturnTypeFullName, StringComparison.Ordinal)
            && string.Equals(Accessibility, other.Accessibility, StringComparison.Ordinal)
            && RefKind == other.RefKind
            && ReturnsVoid == other.ReturnsVoid
            && IsStatic == other.IsStatic
            && IsReadOnly == other.IsReadOnly
            && IsAbstract == other.IsAbstract
            && IsVirtual == other.IsVirtual
            && MethodKind == other.MethodKind
            && Parameters.Equals(other.Parameters)
            && TypeParameters.Equals(other.TypeParameters)
            && Attributes.Equals(other.Attributes)
            ;

        public override bool Equals(object obj)
            => obj is MethodModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Name, ReturnTypeFullName, Accessibility, RefKind, IsStatic, MethodKind, Parameters, TypeParameters).ToHashCode();

        public static bool operator ==(MethodModel left, MethodModel right)
            => left.Equals(right);

        public static bool operator !=(MethodModel left, MethodModel right)
            => left.Equals(right) == false;
    }
}
