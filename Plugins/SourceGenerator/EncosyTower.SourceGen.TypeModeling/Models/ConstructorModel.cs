using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct ConstructorModel : IEquatable<ConstructorModel>
    {
        public readonly Accessibility Accessibility;
        public readonly bool IsStatic;
        public readonly EquatableArray<ParameterModel> Parameters;

        public ConstructorModel(
              Accessibility accessibility
            , bool isStatic
            , EquatableArray<ParameterModel> parameters
        )
        {
            Accessibility = accessibility;
            IsStatic = isStatic;
            Parameters = parameters;
        }

        public bool Equals(ConstructorModel other)
            => Accessibility == other.Accessibility
            && IsStatic == other.IsStatic
            && Parameters.Equals(other.Parameters)
            ;

        public override bool Equals(object obj)
            => obj is ConstructorModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Accessibility, IsStatic, Parameters).ToHashCode();

        public static bool operator ==(ConstructorModel left, ConstructorModel right)
            => left.Equals(right);

        public static bool operator !=(ConstructorModel left, ConstructorModel right)
            => left.Equals(right) == false;
    }
}
