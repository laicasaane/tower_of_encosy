using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct FieldSymbol
    {
        private readonly IFieldSymbol _symbol;

        internal FieldSymbol(IFieldSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol?.Name ?? string.Empty;

        public string TypeName => _symbol?.Type.ToDisplayString(SymbolFormats.SimpleNoGlobal) ?? string.Empty;

        public string TypeFullName => _symbol?.Type.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public Accessibility Accessibility => _symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;

        public bool IsReadOnly => _symbol?.IsReadOnly ?? false;

        public bool IsStatic => _symbol?.IsStatic ?? false;

        public bool IsConst => _symbol?.IsConst ?? false;

        public object ConstantValue => _symbol?.ConstantValue;

        public bool IsImplicitlyDeclared => _symbol?.IsImplicitlyDeclared ?? false;

        public RefKind RefKind => _symbol?.RefKind ?? RefKind.None;

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

        public IFieldSymbol Symbol => _symbol;
    }
}
