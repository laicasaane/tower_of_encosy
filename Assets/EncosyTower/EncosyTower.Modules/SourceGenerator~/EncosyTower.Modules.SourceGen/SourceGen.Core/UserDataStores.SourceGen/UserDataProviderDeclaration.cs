using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UserDataStores.SourceGen
{
    internal partial class UserDataProviderDeclaration
    {
        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public List<UserDataAccessDefinition> AccessDefs { get; }

        public UserDataProviderDeclaration(
              ClassDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , List<UserDataAccessDefinition> accessDefs
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            AccessDefs = accessDefs;
        }
    }
}
