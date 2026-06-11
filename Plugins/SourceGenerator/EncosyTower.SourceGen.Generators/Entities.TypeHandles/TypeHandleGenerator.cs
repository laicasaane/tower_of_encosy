using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.TypeHandles
{
    [Generator]
    internal sealed class TypeHandleGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string HANDLE_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.TypeHandleAttribute";

        private const string I_BUFFER_ELEMENT_DATA = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA = "global::Unity.Entities.IComponentData";
        private const string I_SHARED_COMPONENT_DATA = "global::Unity.Entities.ISharedComponentData";

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_TYPE_HANDLES_UNKNOWN_0001"
            , title: "Type Handle Generator Error"
            , messageFormat: "This error indicates a bug in the Type Handle source generators. Error message: '{0}'."
            , category: "EncosyTower.Entities.TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: ""
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      HANDLE_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax
                        && syntax.TypeParameterList is null
                    , GetSemanticMatch
                )
                .Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (context, source) => {
                GenerateOutput(
                      context
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static TypeHandleSpec GetSemanticMatch(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax
                || context.TargetSymbol is not INamedTypeSymbol structSymbol
                || syntax.TypeParameterList is not null
            )
            {
                return default;
            }

            using var typeRefsBuilder = ImmutableArrayBuilder<TypeRefSpec>.Rent();
            var typeHash = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attribute in context.Attributes)
            {
                token.ThrowIfCancellationRequested();

                var args = attribute.ConstructorArguments;

                if (args.Length < 1 || args[0].Value is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (type.IsUnboundGenericType || type.IsUnmanagedType == false)
                {
                    continue;
                }

                var kind = GetHandleKind(type, token);

                if (kind == TypeKind.None)
                {
                    continue;
                }

                if (typeHash.Add(type))
                {
                    typeRefsBuilder.Add(new TypeRefSpec {
                        typeName = type.ToFullName(),
                        typeIdentifier = type.ToValidIdentifier(),
                        typeShortName = type.Name,
                        isReadOnly = args.Length > 1 && args[1].Value is true,
                        kind = kind,
                    });
                }
            }

            var typeRefs = typeRefsBuilder.ToImmutable().AsEquatableArray();

            if (typeRefs.Count == 0)
            {
                return default;
            }

            var syntaxTree = syntax.SyntaxTree;
            var fileTypeName = structSymbol.ToFileName();
            var hintName = syntaxTree.GetHintName(syntax, fileTypeName);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new TypeHandleSpec {
                structName = structSymbol.Name,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                typeRefs = typeRefs,
                location = LocationInfo.From(syntax.GetLocation()),
            };
        }

        private static TypeKind GetHandleKind(INamedTypeSymbol structSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var iface in structSymbol.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_ELEMENT_DATA))
                    return TypeKind.Buffer;

                if (iface.HasFullName(I_COMPONENT_DATA))
                    return TypeKind.Component;

                if (iface.HasFullName(I_SHARED_COMPONENT_DATA))
                    return TypeKind.SharedComponent;
            }

            return TypeKind.None;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , TypeHandleSpec candidate
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
                    , TypeHandleCodeWriter.Write(candidate)
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

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETETH = global::EncosyTower.Entities.TypeHandles;");
            p.PrintLine("using UC = global::Unity.Collections;");
            p.PrintLine("using UE = global::Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
