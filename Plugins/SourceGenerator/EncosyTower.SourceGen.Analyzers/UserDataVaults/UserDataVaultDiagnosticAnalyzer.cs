using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.UserDataVaults
{
    /// <summary>
    /// Roslyn diagnostic analyzer that validates <c>[UserDataAccessor]</c>-attributed classes,
    /// reporting issues that were previously reported inside the source generator itself.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UserDataVaultDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string NAMESPACE = "EncosyTower.UserDataVaults";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string VAULT_ATTRIBUTE = $"global::{NAMESPACE}.UserDataVaultAttribute";
        public const string ACCESSOR_ATTRIBUTE = $"global::{NAMESPACE}.UserDataAccessorAttribute";
        public const string VAULT_ATTRIBUTE_METADATA = $"{NAMESPACE}.UserDataVaultAttribute";
        public const string ACCESSOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.UserDataAccessorAttribute";
        public const string IUSER_DATA = $"global::{NAMESPACE}.IUserData";
        public const string IUSER_DATA_ACCESSOR = $"global::{NAMESPACE}.IUserDataAccessor";
        public const string USER_DATA_STORE_BASE = $"global::{NAMESPACE}.UserDataStoreBase<";
        public const string ENCRYPTION_BASE = "EncryptionBase";
        public const string STRING_VAULT = "StringVault";
        public const string ILOGGER = "ILogger";
        public const string TASK_ARRAY_POOL = "global::System.Buffers.ArrayPool<UnityTask>";
        public const string STORE_ARGS = "UserDataStoreArgs";
        public const string COMPLETED_TASK = "UnityTasks.GetCompleted()";
        public const string WHEN_ALL_TASKS = "UnityTasks.WhenAll(tasks)";
        public const string NOT_NULL = "[NotNull]";
        public const string STRING_ID = "StringIdᐸstringᐳ";
        public const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataVaultGenerator\"";
        public const string HIDE_IN_CALL_STACK = "[HideInCallstack, StackTraceHidden]";

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustHaveOnlyOneConstructor
                , ConstructorContainsUnsupportedType
                , MustNotBeAbstract
                , MustNotBeUnboundGenericType
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol symbol
                || symbol.GetAttribute(ACCESSOR_ATTRIBUTE) is null
            )
            {
                return;
            }

            var symbolLocation = symbol.Locations.Length > 0
                ? symbol.Locations[0]
                : Location.None;

            if (symbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeAbstract
                    , symbolLocation
                    , symbol.Name
                ));
                return;
            }

            if (symbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeUnboundGenericType
                    , symbolLocation
                    , symbol.Name
                ));
                return;
            }

            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length > max)
                {
                    max = constructors[i].Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustHaveOnlyOneConstructor
                    , symbolLocation
                    , symbol.Name
                ));

                return;
            }

            var constructor = constructors[constructorIndex];

            foreach (var param in constructor.Parameters)
            {
                if (TryGetParam(param.Type, out _) == false)
                {
                    var location = param.Locations.Length > 0
                        ? param.Locations[0]
                        : Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                          ConstructorContainsUnsupportedType
                        , location
                        , symbol.Name
                        , param.Name
                    ));
                }
            }
        }

        public static bool TryGetParam(ITypeSymbol type, out ITypeSymbol argType)
        {
            argType = default;

            if (type.IsAbstract)
            {
                return false;
            }

            if (type is INamedTypeSymbol namedType
                && namedType.TryGetGenericType(USER_DATA_STORE_BASE, 1, out var baseType)
                && baseType.TypeArguments.Length == 1
                && baseType.TypeArguments[0].IsAbstract == false
            )
            {
                argType = baseType.TypeArguments[0];
                return true;
            }

            return type.Interfaces.DoesMatchInterface(IUSER_DATA_ACCESSOR)
                || type.AllInterfaces.DoesMatchInterface(IUSER_DATA_ACCESSOR);
        }
    }
}
