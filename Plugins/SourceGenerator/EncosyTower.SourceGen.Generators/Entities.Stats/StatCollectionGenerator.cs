using System;
using System.Threading;
using EncosyTower.SourceGen.Common.Entities.Stats;
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
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_COLLECTION_ATTRIBUTE_METADATA = $"{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string GENERATOR_NAME = nameof(StatCollectionGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            // Only propagate the assembly-validity flag — not the full CompilationCandidateSlim.
            // This prevents the combined pipeline from re-running whenever unrelated compilation
            // details (referenced assemblies, nullable context, etc.) change.
            var isValidProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE).isValid);

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  STAT_COLLECTION_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax { TypeParameterList: null }
                , GetSemanticSymbolMatch
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(isValidProvider)
                .Where(static t => t.Right)
                .Select(static (t, _) => t.Left)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static StatCollectionDefinition GetSemanticSymbolMatch(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax)
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol structSymbol)
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var statSystemTypeSymbol = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

            if (statSystemTypeSymbol is null || statSystemTypeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE) == false)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var fileTypeName = structSymbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, fileTypeName);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileTypeName);
            var statSystemFullTypeName = statSystemTypeSymbol.ToFullName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatCollectionDefinition {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                statSystemFullTypeName = statSystemFullTypeName,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                location = LocationInfo.From(syntax.GetLocation()),
            };

            var args = attribute.ConstructorArguments;

            if (args.Length > 1 && args[1].Value is uint typeIdOffset)
            {
                result.typeIdOffset = typeIdOffset;
            }

            GetStatDataDefintions(syntax, token, ref result);

            if ((result.typeIdOffset + (ulong)result.statDataCollection.Count) > uint.MaxValue)
            {
                return default;
            }

            return result;

            void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintEndLine();
                p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using System;");
                p.PrintLine("using System.CodeDom.Compiler;");
                p.PrintLine("using System.Diagnostics;");
                p.PrintLine("using System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using System.Runtime.CompilerServices;");
                p.PrintLine("using System.Runtime.InteropServices;");
                p.PrintLine("using EncosyTower.Common;");
                p.PrintLine("using EncosyTower.Collections;");
                p.PrintLine("using EncosyTower.Conversion;");
                p.PrintLine("using EncosyTower.Logging;");
                p.PrintLine("using Unity.Collections;");
                p.PrintLine("using Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using Unity.Entities;");
                p.PrintLine("using Unity.Mathematics;");
                p.PrintLine("using UnityEngine;");
                p.PrintLine($"using {StatGeneratorAPI.NAMESPACE};");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintBeginLine("using StatSystem = ").Print(statSystemFullTypeName).PrintEndLine(";");
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

                    result.valueTypeName = types[index];
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
                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode()
                    , candidate.closingSource
                    , candidate.hintName
                    , candidate.sourceFilePath
                    , candidate.location.ToLocation()
                    , projectPath
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
                    , candidate.location.ToLocation()
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
