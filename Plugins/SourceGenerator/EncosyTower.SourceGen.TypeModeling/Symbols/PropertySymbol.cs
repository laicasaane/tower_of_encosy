using System.Collections.Immutable;
using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct PropertySymbol
    {
        private readonly IPropertySymbol _symbol;

        internal PropertySymbol(IPropertySymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string TypeName => _symbol?.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal) ?? string.Empty;

        public string TypeFullName => _symbol?.Type.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public RefKind RefKind => _symbol?.RefKind ?? RefKind.None;

        public bool IsStatic => _symbol?.IsStatic ?? false;

        public bool IsIndexer => _symbol?.IsIndexer ?? false;

        public bool IsReadOnly => _symbol?.IsReadOnly ?? false;

        public bool IsWriteOnly => _symbol?.IsWriteOnly ?? false;

        public AccessorSymbol Getter => new(_symbol?.GetMethod);

        public AccessorSymbol Setter => new(_symbol?.SetMethod);

        public ImmutableArray<ParameterSymbol> Parameters
        {
            get
            {
                if (_symbol == null)
                {
                    return ImmutableArray<ParameterSymbol>.Empty;
                }

                var rawParams = _symbol.Parameters;
                var paramCount = rawParams.Length;

                using var builder = ImmutableArrayBuilder<ParameterSymbol>.Rent();

                for (var i = 0; i < paramCount; i++)
                {
                    builder.Add(new ParameterSymbol(rawParams[i]));
                }

                return builder.ToImmutable();
            }
        }

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

        public IPropertySymbol Symbol => _symbol;
    }
}
