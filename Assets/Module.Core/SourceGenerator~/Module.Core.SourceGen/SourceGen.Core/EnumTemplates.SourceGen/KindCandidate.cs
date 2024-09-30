using Microsoft.CodeAnalysis;

namespace Module.Core.EnumTemplates.SourceGen
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
