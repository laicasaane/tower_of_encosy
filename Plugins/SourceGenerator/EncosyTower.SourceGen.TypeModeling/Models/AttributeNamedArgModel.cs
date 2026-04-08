using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct AttributeNamedArgModel : System.IEquatable<AttributeNamedArgModel>
    {
        public readonly string Name;
        public readonly string Value;

        public AttributeNamedArgModel(string name, string value)
        {
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public bool Equals(AttributeNamedArgModel other)
            => Name == other.Name && Value == other.Value;

        public override bool Equals(object obj)
            => obj is AttributeNamedArgModel other && Equals(other);

        public override int GetHashCode()
            => (int)HashValue.Combine(Name, Value);

        public static bool operator ==(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => !left.Equals(right);
    }
}
