using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.EnumTemplates.SourceGen
{
    public struct TemplateCandidate
    {
        public StructDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
    }
}
