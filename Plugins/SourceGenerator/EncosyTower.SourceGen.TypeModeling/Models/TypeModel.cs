using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct TypeModel : IEquatable<TypeModel>
    {
        public readonly string Name;
        public readonly string FullName;
        public readonly string Namespace;
        public readonly string Accessibility;
        public readonly TypeKind TypeKind;
        public readonly bool IsStatic;
        public readonly bool IsSealed;
        public readonly bool IsAbstract;
        public readonly bool IsReadOnly;
        public readonly bool IsRecord;
        public readonly bool IsGeneric;
        public readonly EquatableArray<string> Interfaces;
        public readonly EquatableArray<AttributeModel> Attributes;
        public readonly EquatableArray<FieldModel> Fields;
        public readonly EquatableArray<PropertyModel> Properties;
        public readonly EquatableArray<MethodModel> Methods;
        public readonly EquatableArray<ConstructorModel> Constructors;
        public readonly EquatableArray<EventModel> Events;

        public TypeModel(
              string name
            , string fullName
            , string @namespace
            , string accessibility
            , TypeKind typeKind
            , bool isStatic
            , bool isSealed
            , bool isAbstract
            , bool isReadOnly
            , bool isRecord
            , bool isGeneric
            , EquatableArray<string> interfaces
            , EquatableArray<AttributeModel> attributes
            , EquatableArray<FieldModel> fields
            , EquatableArray<PropertyModel> properties
            , EquatableArray<MethodModel> methods
            , EquatableArray<ConstructorModel> constructors
            , EquatableArray<EventModel> events
        )
        {
            Name = name ?? string.Empty;
            FullName = fullName ?? string.Empty;
            Namespace = @namespace ?? string.Empty;
            Accessibility = accessibility ?? string.Empty;
            TypeKind = typeKind;
            IsStatic = isStatic;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsReadOnly = isReadOnly;
            IsRecord = isRecord;
            IsGeneric = isGeneric;
            Interfaces = interfaces;
            Attributes = attributes;
            Fields = fields;
            Properties = properties;
            Methods = methods;
            Constructors = constructors;
            Events = events;
        }

        public bool Equals(TypeModel other)
            => string.Equals(FullName, other.FullName, StringComparison.Ordinal)
            && string.Equals(Accessibility, other.Accessibility, StringComparison.Ordinal)
            && TypeKind == other.TypeKind
            && IsStatic == other.IsStatic
            && IsSealed == other.IsSealed
            && IsAbstract == other.IsAbstract
            && IsReadOnly == other.IsReadOnly
            && IsRecord == other.IsRecord
            && IsGeneric == other.IsGeneric
            && Interfaces.Equals(other.Interfaces)
            && Attributes.Equals(other.Attributes)
            && Fields.Equals(other.Fields)
            && Properties.Equals(other.Properties)
            && Methods.Equals(other.Methods)
            && Constructors.Equals(other.Constructors)
            && Events.Equals(other.Events)
            ;

        public override bool Equals(object obj)
            => obj is TypeModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(FullName, Accessibility, TypeKind, IsStatic, Fields, Properties, Methods, Constructors).ToHashCode();

        public static bool operator ==(TypeModel left, TypeModel right)
            => left.Equals(right);

        public static bool operator !=(TypeModel left, TypeModel right)
            => left.Equals(right) == false;
    }
}
