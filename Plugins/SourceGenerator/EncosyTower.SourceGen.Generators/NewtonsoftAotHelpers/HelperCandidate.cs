using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    internal struct HelperCandidate
    {
        public TypeDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
        public ITypeSymbol baseType;
    }
}
