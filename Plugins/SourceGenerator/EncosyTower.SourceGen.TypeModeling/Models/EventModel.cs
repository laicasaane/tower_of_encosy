using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct EventModel : IEquatable<EventModel>
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string TypeFullName;
        public readonly Accessibility Accessibility;
        public readonly bool IsStatic;
        public readonly EquatableArray<AttributeModel> Attributes;

        public EventModel(
              string name
            , string typeName
            , string typeFullName
            , Accessibility accessibility
            , bool isStatic
            , EquatableArray<AttributeModel> attributes
        )
        {
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            TypeFullName = typeFullName ?? string.Empty;
            Accessibility = accessibility;
            IsStatic = isStatic;
            Attributes = attributes;
        }

        public bool Equals(EventModel other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(TypeFullName, other.TypeFullName, StringComparison.Ordinal)
            && Accessibility == other.Accessibility
            && IsStatic == other.IsStatic
            && Attributes.Equals(other.Attributes)
            ;

        public override bool Equals(object obj)
            => obj is EventModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Name, TypeFullName, Accessibility, IsStatic, Attributes).ToHashCode();

        public static bool operator ==(EventModel left, EventModel right)
            => left.Equals(right);

        public static bool operator !=(EventModel left, EventModel right)
            => left.Equals(right) == false;
    }
}
