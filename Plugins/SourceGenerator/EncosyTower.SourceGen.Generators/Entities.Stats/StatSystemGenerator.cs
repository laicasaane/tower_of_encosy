using System;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal sealed class StatSystemGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.StatSystemAttribute";
        private const string GENERATOR_NAME = nameof(StatSystemGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      STAT_SYSTEM_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is TypeDeclarationSyntax syntax
                        && syntax.TypeParameterList is null
                    , ExtractSpec
                )
                .Where(static t => t.IsValid);

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

        private static StatSystemSpec ExtractSpec(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
            )
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var args = attribute.ConstructorArguments;

            if (args[0].Value is not byte maxDataSize)
            {
                return default;
            }

            int maxUserDataSize;

            if (args.Length > 1 && args[1].Value is byte userDataSize)
            {
                if (userDataSize > 2)
                {
                    maxUserDataSize = 4;
                }
                else if (userDataSize > 1)
                {
                    maxUserDataSize = 2;
                }
                else
                {
                    maxUserDataSize = 1;
                }
            }
            else
            {
                maxUserDataSize = 1;
            }

            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = typeSymbol.ToValidIdentifier();
            var fileTypeName = typeSymbol.ToFileName();
            var hintName = syntaxTree.GetHintName(syntax, fileTypeName);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new StatSystemSpec {
                typeName = typeSymbol.Name,
                typeNamespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                syntaxKeyword = syntax.Keyword.ValueText,
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                maxDataSize = Math.Max((int)maxDataSize, 1),
                maxUserDataSize = maxUserDataSize,
                isStatic = typeSymbol.IsStatic,
                location = LocationInfo.From(syntax.GetLocation())
            };

            static void PrintAdditionalUsings(ref Printer p)
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
                p.PrintLine("using ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using UE = UnityEngine;");
                p.PrintLine("using UB = Unity.Burst;");
                p.PrintLine("using UC = global::Unity.Collections;");
                p.PrintLine("using UCLU = global::Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using UECS = global::Unity.Entities;");
                p.PrintLine("using UM = global::Unity.Mathematics;");
                p.PrintLine("using UJ = Unity.Jobs;");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , StatSystemSpec candidate
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
                    , candidate.WriteCode(compilation.references)
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
            = new("SG_ENTITIES_STAT_SYSTEM_UNKNOWN_0001"
                , "Stat System Generator Error"
                , "This error indicates a bug in the Stat System source generators. Error message: '{0}'."
                , STAT_SYSTEM_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
