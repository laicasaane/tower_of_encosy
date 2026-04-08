using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct FieldModel : System.IEquatable<FieldModel>
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string TypeFullName;
        public readonly string Accessibility;
        public readonly bool IsReadOnly;
        public readonly bool IsStatic;
        public readonly bool IsConst;
        public readonly string ConstantValueText;
        public readonly RefKind RefKind;
        public readonly EquatableArray<AttributeModel> Attributes;

        public FieldModel(
            string name,
            string typeName,
            string typeFullName,
            string accessibility,
            bool isReadOnly,
            bool isStatic,
            bool isConst,
            string constantValueText,
            RefKind refKind,
            EquatableArray<AttributeModel> attributes)
        {
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            TypeFullName = typeFullName ?? string.Empty;
            Accessibility = accessibility ?? string.Empty;
            IsReadOnly = isReadOnly;
            IsStatic = isStatic;
            IsConst = isConst;
            ConstantValueText = constantValueText ?? string.Empty;
            RefKind = refKind;
            Attributes = attributes;
        }

        public bool Equals(FieldModel other)
            => Name == other.Name
            && TypeFullName == other.TypeFullName
            && Accessibility == other.Accessibility
            && IsReadOnly == other.IsReadOnly
            && IsStatic == other.IsStatic
            && IsConst == other.IsConst
            && ConstantValueText == other.ConstantValueText
            && RefKind == other.RefKind
            && Attributes.Equals(other.Attributes);

        public override bool Equals(object obj)
            => obj is FieldModel other && Equals(other);

        public override int GetHashCode()
            => (int)HashValue.Combine(Name, TypeFullName, Accessibility, IsReadOnly, IsStatic, IsConst, RefKind, Attributes);

        public static bool operator ==(FieldModel left, FieldModel right)
            => left.Equals(right);

        public static bool operator !=(FieldModel left, FieldModel right)
            => !left.Equals(right);
    }
}
