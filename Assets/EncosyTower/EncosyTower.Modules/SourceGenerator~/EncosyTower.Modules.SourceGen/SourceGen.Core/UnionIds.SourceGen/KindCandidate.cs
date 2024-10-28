using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.UnionIds.SourceGen
{
    public struct KindCandidate
    {
        public INamedTypeSymbol kindSymbol;
        public INamedTypeSymbol idSymbol;
        public AttributeData attributeData;
        public ulong order;
        public string displayName;
    }
}
