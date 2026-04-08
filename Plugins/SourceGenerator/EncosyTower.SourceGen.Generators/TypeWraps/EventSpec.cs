using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public struct EventSpec : IEquatable<EventSpec>
    {
        public string name;
        public string typeName;
        public int explicitInterfaceImplementationsLength;
        public bool isPublic;
        public bool isStatic;

        public readonly bool IsValid
            => string.IsNullOrEmpty(name) == false
            && string.IsNullOrEmpty(typeName) == false;

        public static EventSpec Create(IEventSymbol evt, INamedTypeSymbol fieldTypeSymbol)
        {
            return new EventSpec {
                name = evt.ToDisplayString(SymbolExtensions.QualifiedMemberNameWithGlobalPrefix),
                typeName = evt.Type.ToFullName(),
                isPublic = evt.DeclaredAccessibility == Accessibility.Public,
                isStatic = evt.IsStatic,
                explicitInterfaceImplementationsLength = evt.ExplicitInterfaceImplementations.Length,
            };
        }

        public readonly bool Equals(EventSpec other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal);

        public readonly override bool Equals(object obj)
            => obj is EventSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, typeName);
    }
}
