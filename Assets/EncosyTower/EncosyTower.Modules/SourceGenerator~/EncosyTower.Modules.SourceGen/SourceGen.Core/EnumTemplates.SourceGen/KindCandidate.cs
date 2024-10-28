using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.EnumTemplates.SourceGen
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
