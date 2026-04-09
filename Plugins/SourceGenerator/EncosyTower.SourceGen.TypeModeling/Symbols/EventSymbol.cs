using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
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

        public IEventSymbol Symbol => _symbol;
    }
}
