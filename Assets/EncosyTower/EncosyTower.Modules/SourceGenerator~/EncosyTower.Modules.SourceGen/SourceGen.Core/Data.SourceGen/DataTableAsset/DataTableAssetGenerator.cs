using System;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;

    [Generator]
    public class DataTableAssetGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var dataRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsClassSyntaxMatch,
                transform: GetSemanticMatch
            ).Where(static t => t is { });

            var combined = dataRefProvider
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.IsValidCompilation(SKIP_ATTRIBUTE));

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

        public static bool IsClassSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax classSyntax
                && classSyntax.HasModifier(SyntaxKind.AbstractKeyword) == false
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0
                && classSyntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameCandidate("EncosyTower.Modules.Data", "IDataTableAsset")
                );
        }

        public static DataTableAssetRef GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
            )
            {
                return null;
            }

            if (classSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(classSyntax, token);

            if (symbol.IsAbstract)
            {
                return null;
            }

            var baseType = symbol.BaseType;

            while (baseType != null)
            {
                var typeArguments = baseType.TypeArguments;

                if (typeArguments.Length >= 2)
                {
                    var fullName = baseType.ToFullName();

                    if (fullName.StartsWith(DATA_TABLE_ASSET))
                    {
                        return new DataTableAssetRef {
                            Syntax = classSyntax,
                            Symbol = symbol,
                            IdType = typeArguments[0],
                            DataType = typeArguments[1],
                        };
                    }
                }

                baseType = baseType.BaseType;
            }

            return null;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , DataTableAssetRef candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var syntax = candidate.Syntax;

            try
            {
                if (candidate.IdType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                {
                    context.ReportDiagnostic(
                          DiagnosticDescriptors.MustBeApplicableForTypeArgument
                        , candidate.Syntax
                        , candidate.IdType.Name
                        , "TDataId"
                    );
                    return;
                }

                if (candidate.DataType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                {
                    context.ReportDiagnostic(
                          DiagnosticDescriptors.MustBeApplicableForTypeArgument
                        , candidate.Syntax
                        , candidate.IdType.Name
                        , "TData"
                    );
                    return;
                }

                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = syntax.SyntaxTree;
                var declaration = new DataTableAssetDeclaration(candidate);

                if (declaration.GetIdMethodIsImplemented)
                {
                    return;
                }

                var source = declaration.WriteCode();
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, DATA_TABLE_ASSET_GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(DATA_TABLE_ASSET_GENERATOR_NAME, syntax, declaration.TypeRef.Symbol.ToValidIdentifier())
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_DATA_TABLE_ASSET_01"
                , "Data Table Asset Generator Error"
                , "This error indicates a bug in the Data Table Asset source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Data.DataTableAsset<TDataId, TData>"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
