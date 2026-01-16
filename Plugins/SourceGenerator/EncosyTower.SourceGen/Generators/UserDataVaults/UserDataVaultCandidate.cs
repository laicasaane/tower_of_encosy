using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal struct UserDataVaultCandidate
    {
        public ClassDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
    }
}
