﻿using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MustHaveOnlyOneConstructor = new(
              id: "USER_DATA_VAULT_0001"
            , title: "Type must have only 01 constructor whose parameters are either UserDataStorageBase<T> or IUserDataAccess"
            , messageFormat: "Type \"{0}\" must have only 01 constructor whose parameters are either UserDataStorageBase<T> or IUserDataAccess"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type must have only 01 constructor whose parameters are either UserDataStorageBase<T> or IUserDataAccess"
        );

        public static readonly DiagnosticDescriptor ConstructorContainsUnsupportedType = new(
              id: "USER_DATA_VAULT_0002"
            , title: "Constructor parameter must be either UserDataStorageBase<T> or IUserDataAccess"
            , messageFormat: "Parameter \"{1}\" of constructor of type \"{0}\" must be either UserDataStorageBase<T> or IUserDataAccess"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Constructor parameter must be either UserDataStorageBase<T> or IUserDataAccess"
        );
    }
}
