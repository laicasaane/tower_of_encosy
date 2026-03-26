using System;
using System.Threading;
using EncosyTower.SourceGen.Common.Entities.Stats;
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
        private const string STAT_DATA_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.StatDataAttribute";
        private const string GENERATOR_NAME = nameof(StatDataGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      STAT_DATA_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax
                        && syntax.TypeParameterList is null
                    , GetSemanticSymbolMatch
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

        private static StatDataDefinition GetSemanticSymbolMatch(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
            )
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol structSymbol)
            {
                return default;
            }

            // ForAttributeWithMetadataName guarantees at least one matching attribute
            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
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

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatDataDefinition {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                location = LocationInfo.From(syntax.GetLocation()),
                singleValue = false,
            };

            var firstArg = attribute.ConstructorArguments[0];

            if (firstArg.Kind == TypedConstantKind.Enum && firstArg.Value is byte enumByte)
            {
                var index = (int)enumByte;
                var types = StatGeneratorAPI.Types.AsSpan();

                if ((uint)index >= (uint)types.Length)
                {
                    return default;
                }

                var typeNames = StatGeneratorAPI.TypeNames.AsSpan();
                var sizes = StatGeneratorAPI.Sizes.AsSpan();

                result.size = sizes[index];
                result.valueTypeName = typeNames[index];
                result.valueFullTypeName = types[index];
            }
            else if (firstArg.Kind == TypedConstantKind.Type
                && firstArg.Value is INamedTypeSymbol enumType)
            {
                if (enumType.TypeKind != TypeKind.Enum)
                {
                    return default;
                }

                var underlyingType = enumType.EnumUnderlyingType.ToSimpleName();

                if (StatGeneratorAPI.EnumTypeMap.TryGetValue(underlyingType, out var valueTypeName) == false)
                {
                    return default;
                }

                result.isEnum = true;
                result.valueTypeName = valueTypeName;
                result.valueFullTypeName = enumType.ToFullName();
                result.underlyingTypeName = underlyingType;

                enumType.EnumUnderlyingType.GetUnmanagedSize(ref result.size);
            }
            else
            {
                return default;
            }

            foreach (var namedArg in attribute.NamedArguments)
            {
                if (string.Equals(namedArg.Key, "SingleValue", StringComparison.Ordinal)
                    && namedArg.Value.Value is bool singleValue)
                {
                    result.singleValue = singleValue;
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
