using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct ConstructorSymbol
    {
        private readonly IMethodSymbol _symbol;

        internal ConstructorSymbol(IMethodSymbol symbol)
        {
            _symbol = symbol;
        }

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public bool IsStatic => _symbol?.IsStatic ?? false;

        public int ParameterCount => _symbol?.Parameters.Length ?? 0;

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

        public IMethodSymbol Symbol => _symbol;
    }
}
