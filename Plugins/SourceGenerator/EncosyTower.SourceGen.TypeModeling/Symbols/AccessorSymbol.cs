using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct AccessorSymbol
    {
        private readonly IMethodSymbol _symbol;

        internal AccessorSymbol(IMethodSymbol symbol)
        {
            _symbol = symbol;
        }

        public bool Exists => _symbol != null;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public bool IsReadOnly => _symbol?.IsReadOnly ?? false;

        public bool IsInitOnly => _symbol?.IsInitOnly ?? false;

        public RefKind RefKind => _symbol?.RefKind ?? RefKind.None;
    }
}
