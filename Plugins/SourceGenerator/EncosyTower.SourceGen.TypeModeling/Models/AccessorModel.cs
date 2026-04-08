using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct AccessorModel : IEquatable<AccessorModel>
    {
        public readonly bool Exists;
        public readonly string Accessibility;
        public readonly bool IsReadOnly;
        public readonly bool IsInitOnly;
        public readonly RefKind RefKind;

        public AccessorModel(
              bool exists
            , string accessibility
            , bool isReadOnly
            , bool isInitOnly
            , RefKind refKind
        )
        {
            Exists = exists;
            Accessibility = accessibility ?? string.Empty;
            IsReadOnly = isReadOnly;
            IsInitOnly = isInitOnly;
            RefKind = refKind;
        }

        public bool Equals(AccessorModel other)
            => Exists == other.Exists
            && string.Equals(Accessibility, other.Accessibility, StringComparison.Ordinal)
            && IsReadOnly == other.IsReadOnly
            && IsInitOnly == other.IsInitOnly
            && RefKind == other.RefKind
            ;

        public override bool Equals(object obj)
            => obj is AccessorModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Exists, Accessibility, IsReadOnly, IsInitOnly, RefKind).ToHashCode();

        public static bool operator ==(AccessorModel left, AccessorModel right)
            => left.Equals(right);

        public static bool operator !=(AccessorModel left, AccessorModel right)
            => left.Equals(right) == false;
    }
}
