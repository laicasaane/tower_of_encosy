using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    public struct KindCandidate
    {
        public INamedTypeSymbol kindSymbol;
        public INamedTypeSymbol idSymbol;
        public AttributeData attributeData;
        public string displayName;
        public ulong order;
        public ToStringMethods toStringMethods;
        public bool signed;
        public TryParseMethodType tryParseSpan;
    }
}
