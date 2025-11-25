using System;
using System.Collections.Generic;
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
        private const string STAT_DATA_ATTRIBUTE = $"global::{NAMESPACE}.StatDataAttribute";
        private const string STAT_VALUE_TYPE_ATTRIBUTE = $"global::{NAMESPACE}.StatVariantTypeAttribute";
        private const string ISTAT_VALUE_ATTRIBUTE = $"global::{NAMESPACE}.IStatVariant";
        private const string ISTAT_VALUE_PAIR_ATTRIBUTE = $"global::{NAMESPACE}.IStatVariantPair";
        private const string GENERATOR_NAME = nameof(StatDataGenerator);

        private static readonly Dictionary<string, string> s_enumTypeMap = new(StringComparer.Ordinal) {
            { "sbyte", "SByte" },
            { "byte", "Byte" },
            { "short", "Short" },
            { "ushort", "UShort" },
            { "int", "Int" },
            { "uint", "UInt" },
            { "long", "Long" },
            { "ulong", "ULong" },
        };

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
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate(NAMESPACE, "StatData")
                ;
        }

        private static StatDataDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax syntax
                || syntax.GetAttribute(NAMESPACE, "StatData") is not AttributeSyntax attributeSyntax
                || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                || argumentList.Arguments.Count < 1
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

            var attribute = structSymbol.GetAttribute(STAT_DATA_ATTRIBUTE);

            if (attribute == null || attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var args = argumentList.Arguments;
            var result = new StatDataDefinition {
                syntax = syntax,
                typeName = symbol.Name,
                typeNamespace = symbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = symbol.ToValidIdentifier(),
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
                var namespaces = StatGeneratorAPI.Namespaces.AsSpan();

                var type = types[index];
                var typeName = typeNames[index];
                var ns = namespaces[index];
                var hasNs = string.IsNullOrEmpty(ns) == false;

                result.valueTypeName = typeName;
                result.valueFullTypeName = hasNs ? $"{ns}.{type}" : type;
            }
            else if (args[0].Expression is TypeOfExpressionSyntax typeOfExpr)
            {
                var candidate = semanticModel.GetSymbolInfo(typeOfExpr.Type, token).Symbol as INamedTypeSymbol;

                if (candidate.TypeKind != TypeKind.Enum)
                {
                    return default;
                }

                var underlyingType = candidate.EnumUnderlyingType.ToSimpleName();

                if (s_enumTypeMap.TryGetValue(underlyingType, out var valueTypeName) == false)
                {
                    return default;
                }

                result.isEnum = true;
                result.valueTypeName = valueTypeName;
                result.valueFullTypeName = candidate.ToFullName();
                result.underlyingTypeName = underlyingType;
            }

            if (args.Count > 1 && args[1].Expression is LiteralExpressionSyntax literalExpr2)
            {
                result.singleValue = (bool)literalExpr2.Token.Value;
            }

            return result;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
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

                var syntax = candidate.syntax;
                var syntaxTree = syntax.SyntaxTree;

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , candidate.WriteCode()
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, candidate.typeIdentifier)
                    , syntaxTree.GetGeneratedSourceFilePath(compilation.assemblyName, GENERATOR_NAME)
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
                    , candidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ENTITIES_STATS_01"
                , "Stat Data Generator Error"
                , "This error indicates a bug in the Stat Data source generators. Error message: '{0}'."
                , $"{NAMESPACE}.StatDataAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
