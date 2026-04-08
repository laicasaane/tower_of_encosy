using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
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
                if (_symbol == null) return ImmutableArray<ParameterSymbol>.Empty;
                var builder = ImmutableArray.CreateBuilder<ParameterSymbol>(_symbol.Parameters.Length);
                foreach (var p in _symbol.Parameters)
                    builder.Add(new ParameterSymbol(p));
                return builder.ToImmutable();
            }
        }

        public ImmutableArray<string> TypeParameters
        {
            get
            {
                if (_symbol == null) return ImmutableArray<string>.Empty;
                var builder = ImmutableArray.CreateBuilder<string>(_symbol.TypeParameters.Length);
                foreach (var tp in _symbol.TypeParameters)
                    builder.Add(tp.Name);
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

        public IMethodSymbol Symbol => _symbol;
    }
}
