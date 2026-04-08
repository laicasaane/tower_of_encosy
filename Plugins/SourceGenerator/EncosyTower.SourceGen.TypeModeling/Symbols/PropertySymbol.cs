using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
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
                if (_symbol == null) return ImmutableArray<ParameterSymbol>.Empty;
                var builder = ImmutableArray.CreateBuilder<ParameterSymbol>(_symbol.Parameters.Length);
                foreach (var p in _symbol.Parameters)
                    builder.Add(new ParameterSymbol(p));
                return builder.ToImmutable();
            }
        }

        public bool HasAttribute(string fullyQualifiedName)
        {
            if (_symbol == null) return false;
            foreach (var attr in _symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) == fullyQualifiedName)
                    return true;
            }
            return false;
        }

        public AttributeSymbol GetAttribute(string fullyQualifiedName)
        {
            if (_symbol == null) return default;
            foreach (var attr in _symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) == fullyQualifiedName)
                    return new AttributeSymbol(attr);
            }
            return default;
        }

        public IPropertySymbol Symbol => _symbol;
    }
}
