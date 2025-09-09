using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    public struct KindCandidate
    {
        public INamedTypeSymbol typeSymbol;
        public INamedTypeSymbol templateSymbol;
        public AttributeData attributeData;
        public ulong order;
        public bool enumMembers;
    }
}
