using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct ParameterSymbol
    {
        private readonly IParameterSymbol _symbol;

        internal ParameterSymbol(IParameterSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string TypeName => _symbol?.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal) ?? string.Empty;

        public string TypeFullName => _symbol?.Type.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public RefKind RefKind => _symbol?.RefKind ?? RefKind.None;

        public int Ordinal => _symbol?.Ordinal ?? 0;

        public bool HasDefaultValue => _symbol?.HasExplicitDefaultValue ?? false;

        public object DefaultValue => _symbol?.ExplicitDefaultValue;
    }
}
