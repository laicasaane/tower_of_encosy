using System.Collections.Immutable;
using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
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

        public IMethodSymbol Symbol => _symbol;
    }
}
