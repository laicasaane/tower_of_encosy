using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct ConstructorModel : System.IEquatable<ConstructorModel>
    {
        public readonly string Accessibility;
        public readonly bool IsStatic;
        public readonly EquatableArray<ParameterModel> Parameters;

        public ConstructorModel(
            string accessibility,
            bool isStatic,
            EquatableArray<ParameterModel> parameters)
        {
            Accessibility = accessibility ?? string.Empty;
            IsStatic = isStatic;
            Parameters = parameters;
        }

        public bool Equals(ConstructorModel other)
            => Accessibility == other.Accessibility
            && IsStatic == other.IsStatic
            && Parameters.Equals(other.Parameters);

        public override bool Equals(object obj)
            => obj is ConstructorModel other && Equals(other);

        public override int GetHashCode()
            => (int)HashValue.Combine(Accessibility, IsStatic, Parameters);

        public static bool operator ==(ConstructorModel left, ConstructorModel right)
            => left.Equals(right);

        public static bool operator !=(ConstructorModel left, ConstructorModel right)
            => !left.Equals(right);
    }
}
