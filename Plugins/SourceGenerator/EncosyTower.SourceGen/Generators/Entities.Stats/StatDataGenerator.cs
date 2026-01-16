using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal class StatDataGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatGeneratorAPI.SKIP_ATTRIBUTE;
        private const string STAT_DATA = "StatData";
        private const string STAT_DATA_ATTRIBUTE = $"global::{NAMESPACE}.StatDataAttribute";
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

            return node is StructDeclarationSyntax syntax
                && syntax.TypeParameterList is null
                && syntax.AttributeLists.Count > 0
                && syntax.GetAttribute(NAMESPACE, STAT_DATA) is AttributeSyntax attributeSyntax
                && attributeSyntax.ArgumentList is AttributeArgumentListSyntax argumentList
                && argumentList.Arguments.Count > 0
                ;
        }

        private static StatDataDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
                || syntax.GetAttribute(NAMESPACE, STAT_DATA) is not AttributeSyntax attributeSyntax
                || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                || argumentList.Arguments.Count < 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol is not INamedTypeSymbol structSymbol)
            {
                return default;
            }

            var attribute = symbol.GetAttribute(STAT_DATA_ATTRIBUTE);

            if (attribute == null || attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var fileTypeName = structSymbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, fileTypeName);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileTypeName);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var args = argumentList.Arguments;
            var result = new StatDataDefinition {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                location = syntax.GetLocation(),
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

                result.size = sizes[index];
                result.valueTypeName = typeName;
                result.valueFullTypeName = type;
            }
            else if (args[0].Expression is TypeOfExpressionSyntax typeOfExpr)
            {
                var candidate = semanticModel.GetSymbolInfo(typeOfExpr.Type, token).Symbol as INamedTypeSymbol;

                if (candidate.TypeKind != TypeKind.Enum)
                {
                    return default;
                }

                var underlyingType = candidate.EnumUnderlyingType.ToSimpleName();

                if (StatGeneratorAPI.EnumTypeMap.TryGetValue(underlyingType, out var valueTypeName) == false)
                {
                    return default;
                }

                result.isEnum = true;
                result.valueTypeName = valueTypeName;
                result.valueFullTypeName = candidate.ToFullName();
                result.underlyingTypeName = underlyingType;

                candidate.EnumUnderlyingType.GetUnmanagedSize(ref result.size);
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
                p.PrintLine("using Unity.Mathematics;");
                p.PrintLine($"using {StatGeneratorAPI.NAMESPACE};");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using UnityDebug = global::UnityEngine.Debug;");
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim _
            , StatDataDefinition candidate
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
            = new("SG_ENTITIES_STAT_DATA_01"
                , "Stat Data Generator Error"
                , "This error indicates a bug in the Stat Data source generators. Error message: '{0}'."
                , STAT_DATA_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
