using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UserDataStores.SourceGen
{
    internal struct UserDataProviderCandidate
    {
        public ClassDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
        public string prefix;
        public string suffix;
    }
}
