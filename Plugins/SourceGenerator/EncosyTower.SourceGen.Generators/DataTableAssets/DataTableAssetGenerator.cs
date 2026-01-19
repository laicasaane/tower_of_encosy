using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    using static EncosyTower.SourceGen.Generators.DataTableAssets.Helpers;

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
                .Where(static t => t.Left.Right.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE));

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
                    static x => x.Type.IsTypeNameCandidate("EncosyTower.Databases", "IDataTableAsset")
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
                        var result = new DataTableAssetRef {
                            Syntax = classSyntax,
                            Symbol = symbol,
                            IdType = typeArguments[0],
                            DataType = typeArguments[1],
                        };

                        if (typeArguments.Length > 2)
                        {
                            result.ConvertedIdType = typeArguments[2];
                        }

                        return result;
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

                var fileTypeName = declaration.TypeRef.Symbol.ToFileName();

                var hintName = syntaxTree.GetGeneratedSourceFileName(
                      GENERATOR_NAME
                    , syntax
                    , fileTypeName
                );

                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                      compilation.Assembly.Name
                    , GENERATOR_NAME
                    , fileTypeName
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , declaration.WriteCode()
                    , hintName
                    , sourceFilePath
                );
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
                , "EncosyTower.Databases.DataTableAsset<TDataId, TData>"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
