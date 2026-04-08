using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct EventSymbol
    {
        private readonly IEventSymbol _symbol;

        internal EventSymbol(IEventSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string TypeName => _symbol?.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal) ?? string.Empty;

        public string TypeFullName => _symbol?.Type.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public bool IsStatic => _symbol?.IsStatic ?? false;

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

        public IEventSymbol Symbol => _symbol;
    }
}
