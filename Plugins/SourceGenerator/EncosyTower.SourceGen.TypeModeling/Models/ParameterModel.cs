using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct ParameterModel : System.IEquatable<ParameterModel>
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string TypeFullName;
        public readonly RefKind RefKind;
        public readonly int Ordinal;
        public readonly bool HasDefaultValue;
        public readonly string DefaultValueText;

        public ParameterModel(
            string name,
            string typeName,
            string typeFullName,
            RefKind refKind,
            int ordinal,
            bool hasDefaultValue,
            string defaultValueText)
        {
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            TypeFullName = typeFullName ?? string.Empty;
            RefKind = refKind;
            Ordinal = ordinal;
            HasDefaultValue = hasDefaultValue;
            DefaultValueText = defaultValueText ?? string.Empty;
        }

        public bool Equals(ParameterModel other)
            => Name == other.Name
            && TypeFullName == other.TypeFullName
            && RefKind == other.RefKind
            && Ordinal == other.Ordinal
            && HasDefaultValue == other.HasDefaultValue
            && DefaultValueText == other.DefaultValueText;

        public override bool Equals(object obj)
            => obj is ParameterModel other && Equals(other);

        public override int GetHashCode()
            => (int)HashValue.Combine(Name, TypeFullName, RefKind, Ordinal, HasDefaultValue, DefaultValueText);

        public static bool operator ==(ParameterModel left, ParameterModel right)
            => left.Equals(right);

        public static bool operator !=(ParameterModel left, ParameterModel right)
            => !left.Equals(right);
    }
}
