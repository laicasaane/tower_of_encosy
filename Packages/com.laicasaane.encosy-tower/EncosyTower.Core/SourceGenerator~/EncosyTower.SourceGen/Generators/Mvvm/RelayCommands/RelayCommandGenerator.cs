﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.RelayCommands
{
    [Generator]
    public class RelayCommandGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = MvvmGeneratorHelpers.NAMESPACE;
        public const string SKIP_ATTRIBUTE = MvvmGeneratorHelpers.SKIP_ATTRIBUTE;

        public const string ATTRIBUTE = "RelayCommand";
        public const string INPUT_NAMESPACE = $"{NAMESPACE}.Input";
        public const string INTERFACE = $"global::{NAMESPACE}.ComponentModel.IObservableObject";
        public const string GENERATOR_NAME = nameof(RelayCommandGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => {
                    return MvvmGeneratorHelpers.IsClassSyntaxMatchByAttribute(
                        node, token, SyntaxKind.MethodDeclaration, INPUT_NAMESPACE, ATTRIBUTE
                    );
                },
                transform: static (syntaxContext, token) => {
                    return MvvmGeneratorHelpers.GetClassSemanticMatch(syntaxContext, token, INTERFACE);
                }
            ).Where(static t => t is { });

            var combined = candidateProvider
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

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ClassDeclarationSyntax candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = candidate.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = new RelayCommandDeclaration(candidate, semanticModel, context.CancellationToken);

                if (declaration.IsValid == false)
                {
                    return;
                }

                var hintName = syntaxTree.GetGeneratedSourceFileName(
                      GENERATOR_NAME
                    , candidate
                    , declaration.Symbol.ToValidIdentifier()
                );

                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                      compilation.Assembly.Name
                    , GENERATOR_NAME
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate
                    , declaration.WriteCode()
                    , hintName
                    , sourceFilePath
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
                    , candidate.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_RELAY_COMMAND_01"
                , "Relay Command Generator Error"
                , "This error indicates a bug in the RelayCommand source generators. Error message: '{0}'."
                , $"{NAMESPACE}.IObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
