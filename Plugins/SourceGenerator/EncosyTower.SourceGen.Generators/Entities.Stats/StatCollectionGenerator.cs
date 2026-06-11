using System;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal sealed class StatCollectionGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_DATA = "StatData";
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_COLLECTION_ATTRIBUTE_METADATA = $"{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  STAT_COLLECTION_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax { TypeParameterList: null }
                , ExtractSpec
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

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

        private static StatCollectionSpec ExtractSpec(
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


            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol statSystemTypeSymbol
                || statSystemTypeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = syntaxTree.GetHintName(syntax, structSymbol.ToFileName());
            var statSystemFullTypeName = statSystemTypeSymbol.ToFullName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatCollectionSpec {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                statSystemFullTypeName = statSystemFullTypeName,
                hintName = hintName,
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
                p.PrintLine("using S = global::System;");
                p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
                p.PrintLine("using SD = global::System.Diagnostics;");
                p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
                p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
                p.PrintLine("using ET = global::EncosyTower.Common;");
                p.PrintLine("using ETCol = global::EncosyTower.Collections;");
                p.PrintLine("using ETCon = global::EncosyTower.Conversion;");
                p.PrintLine("using ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using ETL = global::EncosyTower.Logging;");
                p.PrintLine("using UC = global::Unity.Collections;");
                p.PrintLine("using UCLU = global::Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using UECS = global::Unity.Entities;");
                p.PrintLine("using UM = global::Unity.Mathematics;");
                p.PrintLine("using UE = global::UnityEngine;");
                p.PrintEndLine();
                p.PrintBeginLine("using StatSystem = ").Print(statSystemFullTypeName).PrintEndLine(";");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }

            static void GetStatDataDefintions(
                  StructDeclarationSyntax parentSyntax
                , CancellationToken token
                , ref StatCollectionSpec statCollection
            )
            {
                token.ThrowIfCancellationRequested();

                using var arrayBuilder = ImmutableArrayBuilder<StatCollectionSpec.StatDataSpec>.Rent();

                foreach (var childNode in parentSyntax.ChildNodes())
                {
                    token.ThrowIfCancellationRequested();

                    var statData = GetStatDataDefinition(childNode, token);

                    if (statData.IsValid == false)
                    {
                        continue;
                    }

                    arrayBuilder.Add(statData);
                }

                statCollection.statDataCollection = arrayBuilder.ToImmutable();
            }

            static StatCollectionSpec.StatDataSpec GetStatDataDefinition(SyntaxNode node, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();

                if (node is not StructDeclarationSyntax syntax
                    || syntax.TypeParameterList is not null
                    || syntax.GetAttribute(NAMESPACE, STAT_DATA, token) is not AttributeSyntax attributeSyntax
                    || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                    || argumentList.Arguments.Count < 1
                )
                {
                    return default;
                }

                var args = argumentList.Arguments;
                var result = new StatCollectionSpec.StatDataSpec {
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
                    var namespaces = StatGeneratorAPI.Namespaces.AsSpan();

                    if ((uint)index >= (uint)types.Length)
                    {
                        return default;
                    }

                    result.valueTypeNamespace = namespaces[index];
                    result.valueType = types[index];
                }
                else if (args[0].Expression is TypeOfExpressionSyntax typeOfExpr)
                {
                    result.valueType = typeOfExpr.Type.ToFullString();
                }
                else
                {
                    return default;
                }

                token.ThrowIfCancellationRequested();

                for (var i = 1; i < args.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

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
            , CompilationInfo compilation
            , StatCollectionSpec candidate
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
                var assemblyName = compilation.assemblyName;
                var hintName = candidate.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode()
                    , candidate.closingSource
                    , candidate.hintName
                    , sourceFilePath
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
            = new("SG_ENTITIES_STAT_COLLECTION_UNKNOWN_0001"
                , "Stat Collection Generator Error"
                , "This error indicates a bug in the Stat Collection source generators. Error message: '{0}'."
                , STAT_COLLECTION_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
