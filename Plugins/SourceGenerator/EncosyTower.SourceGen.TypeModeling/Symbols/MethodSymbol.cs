using System.Collections.Immutable;
using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct MethodSymbol
    {
        private readonly IMethodSymbol _symbol;

        internal MethodSymbol(IMethodSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string ReturnTypeName => _symbol?.ReturnType.ToDisplayString(SymbolFormats.SimpleNoGlobal) ?? string.Empty;

        public string ReturnTypeFullName => _symbol?.ReturnType.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public RefKind RefKind => _symbol?.RefKind ?? RefKind.None;

        public bool ReturnsVoid => _symbol?.ReturnsVoid ?? false;

        public bool IsStatic => _symbol?.IsStatic ?? false;

        public bool IsReadOnly => _symbol?.IsReadOnly ?? false;

        public bool IsAbstract => _symbol?.IsAbstract ?? false;

        public bool IsVirtual => _symbol?.IsVirtual ?? false;

        public MethodKind MethodKind => _symbol?.MethodKind ?? MethodKind.Ordinary;

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

        public ImmutableArray<string> TypeParameters
        {
            get
            {
                if (_symbol == null)
                {
                    return ImmutableArray<string>.Empty;
                }

                var rawTypeParams = _symbol.TypeParameters;
                var typeParamCount = rawTypeParams.Length;

                using var builder = ImmutableArrayBuilder<string>.Rent();

                for (var i = 0; i < typeParamCount; i++)
                {
                    builder.Add(rawTypeParams[i].Name);
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
                if (attrs[i].AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) == fullyQualifiedName)
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
                if (attrs[i].AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) == fullyQualifiedName)
                {
                    return new AttributeSymbol(attrs[i]);
                }
            }

            return default;
        }

        public IMethodSymbol Symbol => _symbol;
    }
}
