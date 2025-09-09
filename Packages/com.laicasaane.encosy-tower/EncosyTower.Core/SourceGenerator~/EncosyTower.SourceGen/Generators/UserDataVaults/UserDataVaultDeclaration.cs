﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial class UserDataVaultDeclaration
    {
        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public List<UserDataAccessDefinition> AccessDefs { get; }

        public UserDataVaultDeclaration(
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
