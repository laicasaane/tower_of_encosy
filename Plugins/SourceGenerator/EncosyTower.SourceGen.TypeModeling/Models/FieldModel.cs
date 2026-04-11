using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct FieldModel : IEquatable<FieldModel>
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string TypeFullName;
        public readonly Accessibility Accessibility;
        public readonly bool IsReadOnly;
        public readonly bool IsStatic;
        public readonly bool IsConst;
        public readonly string ConstantValueText;
        public readonly bool HasConstantValue;
        public readonly ulong ConstantValueNumeric;
        public readonly RefKind RefKind;
        public readonly EquatableArray<AttributeModel> Attributes;

        public FieldModel(
              string name
            , string typeName
            , string typeFullName
            , Accessibility accessibility
            , bool isReadOnly
            , bool isStatic
            , bool isConst
            , string constantValueText
            , bool hasConstantValue
            , ulong constantValueNumeric
            , RefKind refKind
            , EquatableArray<AttributeModel> attributes
        )
        {
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            TypeFullName = typeFullName ?? string.Empty;
            Accessibility = accessibility;
            IsReadOnly = isReadOnly;
            IsStatic = isStatic;
            IsConst = isConst;
            ConstantValueText = constantValueText ?? string.Empty;
            HasConstantValue = hasConstantValue;
            ConstantValueNumeric = constantValueNumeric;
            RefKind = refKind;
            Attributes = attributes;
        }

        public bool Equals(FieldModel other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(TypeFullName, other.TypeFullName, StringComparison.Ordinal)
            && Accessibility == other.Accessibility
            && IsReadOnly == other.IsReadOnly
            && IsStatic == other.IsStatic
            && IsConst == other.IsConst
            && string.Equals(ConstantValueText, other.ConstantValueText, StringComparison.Ordinal)
            && HasConstantValue == other.HasConstantValue
            && ConstantValueNumeric == other.ConstantValueNumeric
            && RefKind == other.RefKind
            && Attributes.Equals(other.Attributes)
            ;

        public override bool Equals(object obj)
            => obj is FieldModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Name, TypeFullName, Accessibility, IsReadOnly, IsStatic, IsConst, RefKind, Attributes)
            .Add(HasConstantValue)
            .Add(ConstantValueNumeric)
            .ToHashCode();

        public static bool operator ==(FieldModel left, FieldModel right)
            => left.Equals(right);

        public static bool operator !=(FieldModel left, FieldModel right)
            => left.Equals(right) == false;
    }
}
