using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    public struct KindCandidate
    {
        public INamedTypeSymbol kindSymbol;
        public INamedTypeSymbol idSymbol;
        public AttributeData attributeData;
        public ulong order;
        public string displayName;
        public bool signed;
    }
}
