using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal class StatSystemGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string GENERATOR_NAME = nameof(StatDataGenerator);

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

            return node is TypeDeclarationSyntax syntax
                && syntax.TypeParameterList is null
                && syntax.AttributeLists.Count > 0
                && syntax.GetAttribute(NAMESPACE, "StatSystem") is AttributeSyntax attributeSyntax
                && attributeSyntax.ArgumentList is AttributeArgumentListSyntax argumentList
                && argumentList.Arguments.Count > 0
                ;
        }

        private static StatSystemDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not TypeDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
                || syntax.GetAttribute(NAMESPACE, "StatSystem") is not AttributeSyntax attributeSyntax
                || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                || argumentList.Arguments.Count < 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol is not INamedTypeSymbol typeSymbol)
            {
                return default;
            }

            var attribute = symbol.GetAttribute(STAT_SYSTEM_ATTRIBUTE);

            if (attribute == null || attribute.ConstructorArguments.Length < 1)
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

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = typeSymbol.ToValidIdentifier();
            var fileTypeName = typeSymbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, fileTypeName);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileTypeName);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new StatSystemDefinition {
                typeName = typeSymbol.Name,
                typeNamespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                syntaxKeyword = syntax.Keyword.ValueText,
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                maxDataSize = Math.Max((int)maxDataSize, 1),
                maxUserDataSize = maxUserDataSize,
                isStatic = typeSymbol.IsStatic,
                location = syntax.GetLocation()
            };

            static void PrintAdditionalUsings(ref Printer p)
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
                p.PrintLine("using UnityEngine;");
                p.PrintLine("using Unity.Burst;");
                p.PrintLine("using Unity.Collections;");
                p.PrintLine("using Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using Unity.Entities;");
                p.PrintLine("using Unity.Mathematics;");
                p.PrintLine("using Unity.Jobs;");
                p.PrintLine($"using {StatGeneratorAPI.NAMESPACE};");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , StatSystemDefinition candidate
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
                    , candidate.WriteCode(compilation.references)
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
            = new("SG_ENTITIES_STAT_SYSTEM_01"
                , "Stat System Generator Error"
                , "This error indicates a bug in the Stat System source generators. Error message: '{0}'."
                , STAT_SYSTEM_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
