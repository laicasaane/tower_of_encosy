using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UserDataStores.SourceGen
{
    [Generator]
    internal class UserDataAccessProviderGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(UserDataAccessProviderGenerator);
        private const string ATTRIBUTE = "global::EncosyTower.Modules.UserDataStores.UserDataAccessProviderAttribute";
        private const string IUSER_DATA_ACCESS = "global::EncosyTower.Modules.UserDataStores.IUserDataAccess";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.UserDataStores.SkipSourceGenForAssemblyAttribute";

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
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(SKIP_ATTRIBUTE));

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
                && syntax.HasAttributeCandidate("EncosyTower.Modules.UserDataStores", "UserDataAccessProvider");
        }

        private static UserDataProviderCandidate GetProviderCandidate(
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

            var candidate = new UserDataProviderCandidate {
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
                    static x => x.Type.IsTypeNameCandidate("EncosyTower.Modules.UserDataStores", "IUserDataAccess"
                ));
        }

        public static INamedTypeSymbol GetDataAccessClass(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
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
            , UserDataProviderCandidate providerCandidate
            , ImmutableArray<INamedTypeSymbol> accessCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (compilationCandidate.compilation == null
                || providerCandidate.syntax == null
                || providerCandidate.symbol == null
                || accessCandidates.Length < 1
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var providerSyntax = providerCandidate.syntax;
                var accessDeclarations = new List<UserDataAccessDefinition>(accessCandidates.Length);
                var sb = new StringBuilder();
                var prefix = providerCandidate.prefix;
                var suffix = providerCandidate.suffix;

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
                var providerSymbol = providerCandidate.symbol;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                var declaration = new UserDataProviderDeclaration(
                      providerSyntax
                    , providerSymbol
                    , accessDeclarations
                );

                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , providerSyntax
                    , declaration.WriteCode()
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, providerSyntax, providerSymbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
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
                    , providerCandidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_USER_DATA_ACCESS_PROVIDER_01"
                , "UserDataAccessProvider Generator Error"
                , "This error indicates a bug in the UserDataAccessProvider source generators. Error message: '{0}'."
                , "EncosyTower.Modules.UserDataStores.UserDataAccessProviderAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
