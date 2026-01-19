using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial class UserDataVaultDeclaration
    {
        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public List<UserDataAccessorDefinition> AccessorDefs { get; }

        public UserDataVaultDeclaration(
              ClassDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , List<UserDataAccessorDefinition> accessorDefs
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            AccessorDefs = accessorDefs;
        }
    }
}
