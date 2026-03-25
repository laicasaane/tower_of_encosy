using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MustHaveOnlyOneConstructor = new(
              id: "USER_DATA_VAULT_0001"
            , title: "Type must have only 01 constructor whose parameters are either UserDataStoreBase<T> or IUserDataAccessor"
            , messageFormat: "Type \"{0}\" must have only 01 constructor whose parameters are either UserDataStoreBase<T> or IUserDataAccessor"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type must have only 01 constructor whose parameters are either UserDataStoreBase<T> or IUserDataAccessor"
        );

        public static readonly DiagnosticDescriptor ConstructorContainsUnsupportedType = new(
              id: "USER_DATA_VAULT_0002"
            , title: "Constructor parameter must be either UserDataStoreBase<T> or IUserDataAccessor"
            , messageFormat: "Parameter \"{1}\" of constructor of type \"{0}\" must be either UserDataStoreBase<T> or IUserDataAccessor"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Constructor parameter must be either UserDataStoreBase<T> or IUserDataAccessor"
        );

        public static readonly DiagnosticDescriptor MustNotBeAbstract = new(
              id: "USER_DATA_VAULT_0003"
            , title: "[UserDataAccessor] type must not be abstract"
            , messageFormat: "Type \"{0}\" is marked [UserDataAccessor] but is abstract and cannot be processed by the source generator"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A type marked with [UserDataAccessor] must not be abstract"
        );

        public static readonly DiagnosticDescriptor MustNotBeUnboundGenericType = new(
              id: "USER_DATA_VAULT_0004"
            , title: "[UserDataAccessor] type must not be an unbound generic type"
            , messageFormat: "Type \"{0}\" is marked [UserDataAccessor] but is an unbound generic type and cannot be processed by the source generator"
            , category: "UserDataVault"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A type marked with [UserDataAccessor] must not be an unbound generic type"
        );
    }
}
