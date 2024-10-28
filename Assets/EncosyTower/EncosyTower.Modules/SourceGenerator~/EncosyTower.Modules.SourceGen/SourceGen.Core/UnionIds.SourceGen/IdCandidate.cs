using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UnionIds.SourceGen
{
    public struct IdCandidate
    {
        public StructDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
        public UnionIdSize size;
        public string displayNameForId;
        public string displayNameForKind;
        public UnionIdKindSettings kindSettings;
    }
}
