using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    public struct TemplateCandidate
    {
        public StructDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
    }
}
