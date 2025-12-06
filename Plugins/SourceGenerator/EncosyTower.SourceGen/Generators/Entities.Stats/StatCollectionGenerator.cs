using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal class StatCollectionGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_DATA = $"StatData";
        private const string STAT_COLLECTION = $"StatCollection";
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string GENERATOR_NAME = nameof(StatCollectionGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidStructSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidStructSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is StructDeclarationSyntax syntax
                && syntax.TypeParameterList is null
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate(NAMESPACE, STAT_COLLECTION)
                ;
        }

        private static StatCollectionDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
                || syntax.GetAttribute(NAMESPACE, STAT_COLLECTION) is not AttributeSyntax attributeSyntax
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var structSymbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (structSymbol is not INamedTypeSymbol symbol)
            {
                return default;
            }

            var attribute = structSymbol.GetAttribute(STAT_COLLECTION_ATTRIBUTE);

            if (attribute == null)
            {
                return default;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = symbol.ToValidIdentifier();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, typeIdentifier);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatCollectionDefinition {
                typeName = symbol.Name,
                typeNamespace = symbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                location = syntax.GetLocation(),
            };

            GetStatDataDefintions(syntax, token, ref result);

            return result;

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintEndLine();
                p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using System;");
                p.PrintLine("using System.CodeDom.Compiler;");
                p.PrintLine("using System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using System.Runtime.CompilerServices;");
                p.PrintLine("using System.Runtime.InteropServices;");
                p.PrintLine("using EncosyTower.Logging;");
                p.PrintLine("using Unity.Entities;");
                p.PrintLine("using Unity.Mathematics;");
                p.PrintLine($"using {StatGeneratorAPI.NAMESPACE};");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            }

            static void GetStatDataDefintions(
                  StructDeclarationSyntax parentSyntax
                , CancellationToken token
                , ref StatCollectionDefinition statCollection
            )
            {
                using var arrayBuilder = ImmutableArrayBuilder<StatCollectionDefinition.StatDataDefinition>.Rent();

                foreach (var childNode in parentSyntax.ChildNodes())
                {
                    token.ThrowIfCancellationRequested();

                    var statData = GetStatDataDefinition(childNode);

                    if (statData.IsValid == false)
                    {
                        continue;
                    }

                    arrayBuilder.Add(statData);
                }

                statCollection.statDataCollection = arrayBuilder.ToImmutable();
            }

            static StatCollectionDefinition.StatDataDefinition GetStatDataDefinition(SyntaxNode node)
            {
                if (node is not StructDeclarationSyntax syntax
                    || syntax.TypeParameterList is not null
                    || syntax.GetAttribute(NAMESPACE, STAT_DATA) is not AttributeSyntax attributeSyntax
                    || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                    || argumentList.Arguments.Count < 1
                )
                {
                    return default;
                }

                var args = argumentList.Arguments;
                var result = new StatCollectionDefinition.StatDataDefinition {
                    typeName = syntax.Identifier.ValueText,
                    fieldName = syntax.Identifier.ValueText.ToPublicFieldName(),
                    singleValue = false,
                };

                if (args[0].Expression is MemberAccessExpressionSyntax memberAccessExpr
                    && memberAccessExpr.Expression is IdentifierNameSyntax identifierSyntax
                    && string.Equals(identifierSyntax.Identifier.ValueText, "StatVariantType")
                )
                {
                    if (Enum.TryParse(memberAccessExpr.Name.Identifier.Text, false, out StatVariantType variantType) == false)
                    {
                        return default;

                    }

                    var index = (int)variantType;
                    var types = StatGeneratorAPI.Types.AsSpan();

                    if ((uint)index >= (uint)types.Length)
                    {
                        return default;
                    }

                    var typeNames = StatGeneratorAPI.TypeNames.AsSpan();
                    var sizes = StatGeneratorAPI.Sizes.AsSpan();

                    var type = types[index];
                    var typeName = typeNames[index];

                    result.valueTypeName = typeName;
                }
                else if (args[0].Expression is TypeOfExpressionSyntax typeOfExpr)
                {
                    result.valueTypeName = typeOfExpr.Type.ToFullString();
                }
                else
                {
                    return default;
                }

                for (var i = 1; i < args.Count; i++)
                {
                    var arg = args[i];

                    if (arg.NameEquals is not NameEqualsSyntax nameEquals
                        || arg.Expression is not LiteralExpressionSyntax literalExpr2
                    )
                    {
                        continue;
                    }

                    switch (nameEquals.Name.Identifier.ValueText)
                    {
                        case "SingleValue":
                        {
                            result.singleValue = (bool)literalExpr2.Token.Value;
                            break;
                        }
                    }
                }

                return result;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim _
            , StatCollectionDefinition candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode()
                    , candidate.closingSource
                    , candidate.hintName
                    , candidate.sourceFilePath
                    , candidate.location
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
                    , candidate.location
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ENTITIES_STAT_COLLECTION_01"
                , "Stat Collection Generator Error"
                , "This error indicates a bug in the Stat Collection source generators. Error message: '{0}'."
                , STAT_COLLECTION_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
