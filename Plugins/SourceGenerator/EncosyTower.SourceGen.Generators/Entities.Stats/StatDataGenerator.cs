using System;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    [Generator]
    internal sealed class StatDataGenerator : IIncrementalGenerator
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
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      STAT_DATA_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax
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

        private static StatDataSpec ExtractSpec(
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

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = syntaxTree.GetHintName(syntax, structSymbol.ToFileName());

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatDataSpec {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
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

                var namespaces = StatGeneratorAPI.Namespaces.AsSpan();
                var typeNames = StatGeneratorAPI.TypeNames.AsSpan();
                var sizes = StatGeneratorAPI.Sizes.AsSpan();

                result.size = sizes[index];
                result.valueTypeNs = namespaces[index];
                result.valueType = types[index];
                result.valueTypeName = typeNames[index];
            }
            else if (firstArg.Kind == TypedConstantKind.Type && firstArg.Value is INamedTypeSymbol enumType)
            {
                if (enumType.TypeKind != TypeKind.Enum)
                {
                    return default;
                }

                var underlyingType = enumType.EnumUnderlyingType.ToFullNameNoGlobal();

                if (StatGeneratorAPI.EnumTypeMap.TryGetValue(underlyingType, out var valueTypeName) == false)
                {
                    return default;
                }

                result.isEnum = true;
                result.valueTypeName = valueTypeName;
                result.valueType = enumType.ToFullName();
                result.underlyingTypeName = underlyingType;
                enumType.EnumUnderlyingType.GetUnmanagedSize(ref result.size, token);
            }
            else
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            foreach (var namedArg in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

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
                p.PrintLine("using S = global::System;");
                p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
                p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
                p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
                p.PrintLine("using ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using ETL = global::EncosyTower.Logging;");
                p.PrintLine("using UM = global::Unity.Mathematics;");
                p.PrintEndLine();
                p.PrintLine("using UnityDebug = global::UnityEngine.Debug;");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , StatDataSpec candidate
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
            = new("SG_ENTITIES_STAT_DATA_UNKNOWN_0001"
                , "Stat Data Generator Error"
                , "This error indicates a bug in the Stat Data source generators. Error message: '{0}'."
                , STAT_DATA_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
