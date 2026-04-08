using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
{
    public static class TypeSymbolExtensions
    {
        public static TypeSymbol ToTypeSymbol(this INamedTypeSymbol symbol)
        {
            return new TypeSymbol(symbol);
        }
    }
}
