using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Module.Core.UserDataStores.SourceGen
{
    internal struct UserDataProviderCandidate
    {
        public ClassDeclarationSyntax syntax;
        public INamedTypeSymbol symbol;
        public string prefix;
        public string suffix;
    }
}
