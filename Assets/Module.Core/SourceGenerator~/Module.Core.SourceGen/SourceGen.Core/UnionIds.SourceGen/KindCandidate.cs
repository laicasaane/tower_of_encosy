using Microsoft.CodeAnalysis;

namespace Module.Core.UnionIds.SourceGen
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
