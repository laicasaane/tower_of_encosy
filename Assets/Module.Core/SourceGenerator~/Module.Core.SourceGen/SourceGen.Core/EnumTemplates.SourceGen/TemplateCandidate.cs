using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Module.Core.EnumTemplates.SourceGen
{
    public struct TemplateCandidate
    {
        public StructDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
    }
}
