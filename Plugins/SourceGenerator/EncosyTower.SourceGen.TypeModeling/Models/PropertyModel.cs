using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct PropertyModel : System.IEquatable<PropertyModel>
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string TypeFullName;
        public readonly string Accessibility;
        public readonly RefKind RefKind;
        public readonly bool IsStatic;
        public readonly bool IsIndexer;
        public readonly AccessorModel Getter;
        public readonly AccessorModel Setter;
        public readonly EquatableArray<ParameterModel> Parameters;
        public readonly EquatableArray<AttributeModel> Attributes;

        public PropertyModel(
            string name,
            string typeName,
            string typeFullName,
            string accessibility,
            RefKind refKind,
            bool isStatic,
            bool isIndexer,
            AccessorModel getter,
            AccessorModel setter,
            EquatableArray<ParameterModel> parameters,
            EquatableArray<AttributeModel> attributes)
        {
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            TypeFullName = typeFullName ?? string.Empty;
            Accessibility = accessibility ?? string.Empty;
            RefKind = refKind;
            IsStatic = isStatic;
            IsIndexer = isIndexer;
            Getter = getter;
            Setter = setter;
            Parameters = parameters;
            Attributes = attributes;
        }

        public bool Equals(PropertyModel other)
            => Name == other.Name
            && TypeFullName == other.TypeFullName
            && Accessibility == other.Accessibility
            && RefKind == other.RefKind
            && IsStatic == other.IsStatic
            && IsIndexer == other.IsIndexer
            && Getter.Equals(other.Getter)
            && Setter.Equals(other.Setter)
            && Parameters.Equals(other.Parameters)
            && Attributes.Equals(other.Attributes);

        public override bool Equals(object obj)
            => obj is PropertyModel other && Equals(other);

        public override int GetHashCode()
            => (int)HashValue.Combine(Name, TypeFullName, Accessibility, RefKind, IsStatic, IsIndexer, Getter, Setter);

        public static bool operator ==(PropertyModel left, PropertyModel right)
            => left.Equals(right);

        public static bool operator !=(PropertyModel left, PropertyModel right)
            => !left.Equals(right);
    }
}
