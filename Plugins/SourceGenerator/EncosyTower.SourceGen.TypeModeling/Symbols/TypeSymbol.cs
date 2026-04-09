using System.Collections.Immutable;
using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct TypeSymbol
    {
        private readonly INamedTypeSymbol _symbol;

        internal TypeSymbol(INamedTypeSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string FullName => _symbol?.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public string Namespace => _symbol?.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public TypeKind TypeKind => _symbol?.TypeKind ?? TypeKind.Unknown;

        public bool IsStatic => _symbol?.IsStatic ?? false;

        public bool IsSealed => _symbol?.IsSealed ?? false;

        public bool IsAbstract => _symbol?.IsAbstract ?? false;

        public bool IsReadOnly => _symbol?.IsReadOnly ?? false;

        public bool IsRecord => _symbol?.IsRecord ?? false;

        public bool IsUnmanaged => _symbol?.IsUnmanagedType ?? false;

        public bool IsGeneric => _symbol?.IsGenericType ?? false;

        public bool IsEnum => _symbol?.TypeKind == TypeKind.Enum;

        public bool IsValueType => _symbol?.IsValueType ?? false;

        public bool IsRefLikeType => _symbol?.IsRefLikeType ?? false;

        public FieldSymbolEnumerable Fields => new(_symbol?.GetMembers() ?? ImmutableArray<ISymbol>.Empty);

        public PropertySymbolEnumerable Properties => new(_symbol?.GetMembers() ?? ImmutableArray<ISymbol>.Empty);

        public MethodSymbolEnumerable Methods => new(_symbol?.GetMembers() ?? ImmutableArray<ISymbol>.Empty);

        public EventSymbolEnumerable Events => new(_symbol?.GetMembers() ?? ImmutableArray<ISymbol>.Empty);

        public ConstructorSymbolEnumerable Constructors => new(_symbol?.GetMembers() ?? ImmutableArray<ISymbol>.Empty);

        public bool HasAttribute(string fullyQualifiedName)
        {
            if (_symbol == null)
            {
                return false;
            }

            var attrs = _symbol.GetAttributes();
            var attrCount = attrs.Length;

            for (var i = 0; i < attrCount; i++)
            {
                if (SymbolNameMatching.HasFullName(attrs[i].AttributeClass, fullyQualifiedName))
                {
                    return true;
                }
            }

            return false;
        }

        public AttributeSymbol GetAttribute(string fullyQualifiedName)
        {
            if (_symbol == null)
            {
                return default;
            }

            var attrs = _symbol.GetAttributes();
            var attrCount = attrs.Length;

            for (var i = 0; i < attrCount; i++)
            {
                if (SymbolNameMatching.HasFullName(attrs[i].AttributeClass, fullyQualifiedName))
                {
                    return new AttributeSymbol(attrs[i]);
                }
            }

            return default;
        }

        public bool ImplementsInterface(string fullyQualifiedName)
        {
            if (_symbol == null)
            {
                return false;
            }

            var ifaces = _symbol.AllInterfaces;
            var ifaceCount = ifaces.Length;

            for (var i = 0; i < ifaceCount; i++)
            {
                if (SymbolNameMatching.HasFullName(ifaces[i], fullyQualifiedName))
                {
                    return true;
                }
            }

            return false;
        }

        public TypeSymbol BaseType => _symbol?.BaseType != null ? new TypeSymbol(_symbol.BaseType) : default;

        public INamedTypeSymbol Symbol => _symbol;
    }
}
