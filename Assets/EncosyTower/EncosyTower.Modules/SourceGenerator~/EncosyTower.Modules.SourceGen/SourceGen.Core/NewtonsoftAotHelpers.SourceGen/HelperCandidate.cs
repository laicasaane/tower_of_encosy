using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.NewtonsoftAotHelpers.SourceGen
{
    internal struct HelperCandidate
    {
        public TypeDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
        public ITypeSymbol baseType;
    }
}
