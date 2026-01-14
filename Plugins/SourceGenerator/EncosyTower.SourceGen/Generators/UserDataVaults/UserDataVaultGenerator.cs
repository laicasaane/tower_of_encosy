using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    [Generator]
    internal class UserDataVaultGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(UserDataVaultGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var providerClassProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchProviderClass
                , transform: GetProviderCandidate
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var accessProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchDataAccessClass
                , transform: GetDataAccessClass
            ).Where(static t => t is { });

            var combined = providerClassProvider
                .Combine(accessProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsSyntaxMatchProviderClass(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax syntax
                && syntax.HasAttributeCandidate(NAMESPACE, "UserDataVault");
        }

        private static UserDataVaultCandidate GetProviderCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
                || symbol.GetAttribute(ATTRIBUTE) is not AttributeData attrib
            )
            {
                return default;
            }

            var candidate = new UserDataVaultCandidate {
                syntax = syntax,
                symbol = symbol,
            };

            var args = attrib.NamedArguments;

            foreach (var arg in args)
            {
                switch (arg.Key)
                {
                    case "Prefix":
                    {
                        if (arg.Value.Value is string stringVal)
                        {
                            candidate.prefix = stringVal;
                        }
                        break;
                    }

                    case "Suffix":
                    {
                        if (arg.Value.Value is string stringVal)
                        {
                            candidate.suffix = stringVal;
                        }
                        break;
                    }
                }
            }

            return candidate;
        }

        public static bool IsSyntaxMatchDataAccessClass(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax syntax
                && syntax.BaseList != null
                && syntax.BaseList.Types.Count > 0
                && syntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameCandidate(NAMESPACE, "IUserDataAccess"
                ));
        }

        public static INamedTypeSymbol GetDataAccessClass(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            if (context.SemanticModel.Compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE) == false
                || context.Node is not ClassDeclarationSyntax syntax
                || syntax.BaseList == null
                || syntax.BaseList.Types.Count < 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null || symbol.IsAbstract || symbol.IsUnboundGenericType)
            {
                return default;
            }

            if (symbol.Interfaces.DoesMatchInterface(IUSER_DATA_ACCESS)
                || symbol.AllInterfaces.DoesMatchInterface(IUSER_DATA_ACCESS)
            )
            {
                return symbol;
            }

            return default;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , UserDataVaultCandidate vaultCandidate
            , ImmutableArray<INamedTypeSymbol> accessCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (compilationCandidate.compilation == null
                || vaultCandidate.syntax == null
                || vaultCandidate.symbol == null
                || accessCandidates.Length < 1
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var providerSyntax = vaultCandidate.syntax;
                var accessDeclarations = new List<UserDataAccessDefinition>(accessCandidates.Length);
                var sb = new StringBuilder();
                var prefix = vaultCandidate.prefix;
                var suffix = vaultCandidate.suffix;

                for (var i = 0; i < accessCandidates.Length; i++)
                {
                    var symbol = accessCandidates[i];

                    if (symbol == null)
                    {
                        continue;
                    }

                    var accessDeclaration = new UserDataAccessDefinition(
                        context, providerSyntax, symbol, prefix, suffix, sb
                    );

                    if (accessDeclaration.IsValid)
                    {
                        accessDeclarations.Add(accessDeclaration);
                    }
                }

                if (accessDeclarations.Count < 1)
                {
                    return;
                }

                accessDeclarations.Sort(static (x, y) => {
                    return string.Compare(x.Symbol.Name, y.Symbol.Name, StringComparison.Ordinal);
                });

                var syntaxTree = providerSyntax.SyntaxTree;
                var providerSymbol = vaultCandidate.symbol;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                var declaration = new UserDataVaultDeclaration(
                      providerSyntax
                    , providerSymbol
                    , accessDeclarations
                );

                SourceGenHelpers.ProjectPath = projectPath;

                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      providerSyntax
                    , context.CancellationToken
                    , out var openingSource
                    , out var closingSource
                    , printAdditionalUsings: compilationCandidate.references.unitask
                        ? PrintUsingUniTask : PrintUsingAwaitable
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , openingSource
                    , declaration.WriteCode()
                    , closingSource
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, providerSyntax, providerSymbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
                    , providerSyntax.GetLocation()
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , vaultCandidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }

            return;

            static void PrintUsingUniTask(ref Printer p)
            {
                p.PrintEndLine();
                p.PrintLine("using UnityTask = global::Cysharp.Threading.Tasks.UniTask;");
                p.PrintLine("using UnityTaskᐸboolᐳ = global::Cysharp.Threading.Tasks.UniTask<bool>;");
                PrintAdditionalUsings(ref p);
            }

            static void PrintUsingAwaitable(ref Printer p)
            {
                p.PrintEndLine();
                p.PrintLine("using UnityTask = global::UnityEngine.Awaitable;");
                p.PrintLine("using UnityTaskᐸboolᐳ = global::UnityEngine.Awaitable<bool>;");
                PrintAdditionalUsings(ref p);
            }

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintLine("using CancellationToken = global::System.Threading.CancellationToken;");
                p.PrintLine("using EncryptionBase = global::EncosyTower.Encryption.EncryptionBase;");
                p.PrintLine("using ExcludeFromCodeCoverageAttribute = global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute;");
                p.PrintLine("using GeneratedCodeAttribute = global::System.CodeDom.Compiler.GeneratedCodeAttribute;");
                p.PrintLine("using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;");
                p.PrintLine("using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;");
                p.PrintLine("using NotNullAttribute = global::System.Diagnostics.CodeAnalysis.NotNullAttribute;");
                p.PrintLine("using SpanᐸIUserDataᐳ = global::System.Span<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using StringIdᐸstringᐳ = global::EncosyTower.StringIds.StringId<string>;");
                p.PrintLine("using StringVault = global::EncosyTower.StringIds.StringVault;");
                p.PrintLine("using ThrowHelper = global::EncosyTower.Debugging.ThrowHelper;");
                p.PrintLine("using UnityTasks = global::EncosyTower.Tasks.UnityTasks;");
                p.PrintLine("using UserDataStorageArgs = global::EncosyTower.UserDataVaults.UserDataStorageArgs;");
                p.PrintEndLine();

                p.PrintLine("using ICopyToSpanᐸIUserDataᐳ = global::EncosyTower.Collections.ICopyToSpan<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IDeinitializable = global::EncosyTower.Initialization.IDeinitializable;");
                p.PrintLine("using IDisposable = global::System.IDisposable;");
                p.PrintLine("using IEnumerable = global::System.Collections.IEnumerable;");
                p.PrintLine("using IEnumerator = global::System.Collections.IEnumerator;");
                p.PrintLine("using IEnumerableᐸIUserDataᐳ = global::System.Collections.Generic.IEnumerable<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IEnumeratorᐸIUserDataᐳ = global::System.Collections.Generic.IEnumerator<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IInitializable = global::EncosyTower.Initialization.IInitializable;");
                p.PrintLine("using ILogger = global::EncosyTower.Logging.ILogger;");
                p.PrintLine("using IReadOnlyListᐸIUserDataᐳ = global::System.Collections.Generic.IReadOnlyList<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using ITryCopyToSpanᐸIUserDataᐳ = global::EncosyTower.Collections.ITryCopyToSpan<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IUserData = global::EncosyTower.UserDataVaults.IUserData;");
                p.PrintLine("using IUserDataAccess = global::EncosyTower.UserDataVaults.IUserDataAccess;");
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_USER_DATA_VAULT_01"
                , "UserDataVault Generator Error"
                , "This error indicates a bug in the UserDataVault source generators. Error message: '{0}'."
                , $"{NAMESPACE}.UserDataVaultAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
