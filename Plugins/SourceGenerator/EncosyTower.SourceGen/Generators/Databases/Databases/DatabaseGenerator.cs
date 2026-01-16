using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    [Generator]
    public class DatabaseGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var databaseRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidDatabaseSyntax,
                transform: GetDatabaseRefSemanticMatch
            ).Where(static t => t is { });

            var combined = databaseRefProvider.Collect()
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidDatabaseSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is TypeDeclarationSyntax typeSyntax
                && typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword) == false
                && IsSupportedTypeSyntax(typeSyntax)
                && typeSyntax.AttributeLists.Count > 0
                && typeSyntax.HasAttributeCandidate("EncosyTower.Databases", "Database");
        }

        public static DatabaseRef GetDatabaseRefSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(DATABASES_NAMESPACE, SKIP_ATTRIBUTE) == false
                || context.Node is not TypeDeclarationSyntax typeSyntax
                || typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
                || IsSupportedTypeSyntax(typeSyntax) == false
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(typeSyntax, token);
            var attribute = symbol.GetAttribute(DATABASE_ATTRIBUTE);

            if (attribute == null)
            {
                return null;
            }

            return new DatabaseRef(
                  typeSyntax
                , symbol
                , attribute
            );
        }

        private static bool IsSupportedTypeSyntax(TypeDeclarationSyntax syntax)
            => syntax.IsKind(SyntaxKind.ClassDeclaration) || syntax.IsKind(SyntaxKind.StructDeclaration);

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ImmutableArray<DatabaseRef> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;
            var assemblyName = compilation.Assembly.Name;

            foreach (var candidate in candidates)
            {
                try
                {
                    var declaration = new DatabaseDeclaration(context, candidate);
                    var databaseRef = declaration.DatabaseRef;
                    var syntaxTree = candidate.Syntax.SyntaxTree;
                    var tables = databaseRef.Tables;

                    if (tables.Length < 1)
                    {
                        continue;
                    }

                    // Database
                    {
                        var fileTypeName = candidate.Symbol.ToFileName();

                        var databaseHintName = syntaxTree.GetGeneratedSourceFileName(
                              GENERATOR_NAME
                            , candidate.Syntax
                            , fileTypeName
                        );

                        var databaseSourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                              assemblyName
                            , GENERATOR_NAME
                            , fileTypeName
                        );

                        var printer = Printer.DefaultLarge;

                        {
                            printer.PrintEndLine();
                            printer.Print("#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS").PrintEndLine();
                            printer.Print("#define __ENCOSY_NO_VALIDATION__").PrintEndLine();
                            printer.Print("#else").PrintEndLine();
                            printer.Print("#define __ENCOSY_VALIDATION__").PrintEndLine();
                            printer.Print("#endif").PrintEndLine();
                            printer.PrintEndLine();
                        }

                        context.OutputSource(
                              outputSourceGenFiles
                            , databaseRef.Syntax
                            , declaration.WriteCode(tables)
                            , databaseHintName
                            , databaseSourceFilePath
                            , printer
                        );
                    }
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(
                          s_errorDescriptor
                        , candidate.Attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , e.ToUnityPrintableString()
                    );
                }
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("DATABASE_UNKNOWN_0001"
                , "Database Generator Error"
                , "This error indicates a bug in the Database source generators. Error message: '{0}'."
                , "DatabaseGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
