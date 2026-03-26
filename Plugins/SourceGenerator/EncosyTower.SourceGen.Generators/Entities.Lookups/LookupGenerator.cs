using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    [Generator]
    internal sealed class LookupGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string LOOKUP_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.LookupAttribute";
        private const string GENERATOR_NAME = nameof(LookupGenerator);

        // Marker interfaces (fully qualified, without "global::" prefix for comparison)
        private const string I_BUFFER_LOOKUPS                      = $"{NAMESPACE}.IBufferLookups";
        private const string I_COMPONENT_LOOKUPS                   = $"{NAMESPACE}.IComponentLookups";
        private const string I_ENABLEABLE_BUFFER_LOOKUPS           = $"{NAMESPACE}.IEnableableBufferLookups";
        private const string I_ENABLEABLE_COMPONENT_LOOKUPS        = $"{NAMESPACE}.IEnableableComponentLookups";
        private const string I_PHYSICS_BUFFER_LOOKUPS              = $"{NAMESPACE}.IPhysicsBufferLookups";
        private const string I_PHYSICS_COMPONENT_LOOKUPS           = $"{NAMESPACE}.IPhysicsComponentLookups";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsEnableableComponentLookups";

        // Required ECS interfaces (with "global::" prefix as expected by InheritsFromInterface)
        private const string I_BUFFER_ELEMENT_DATA  = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA       = "global::Unity.Entities.IComponentData";
        private const string I_ENABLEABLE_COMPONENT = "global::Unity.Entities.IEnableableComponent";

        // Lookup RO/RW interfaces (short names — EncosyTower.Entities.Lookups is imported via PrintAdditionalUsings)
        private const string I_BUFFER_LOOKUP_RO                       = "IBufferLookupRO";
        private const string I_BUFFER_LOOKUP_RW                       = "IBufferLookupRW";
        private const string I_COMPONENT_LOOKUP_RO                    = "IComponentLookupRO";
        private const string I_COMPONENT_LOOKUP_RW                    = "IComponentLookupRW";
        private const string I_ENABLEABLE_BUFFER_LOOKUP_RO            = "IEnableableBufferLookupRO";
        private const string I_ENABLEABLE_BUFFER_LOOKUP_RW            = "IEnableableBufferLookupRW";
        private const string I_ENABLEABLE_COMPONENT_LOOKUP_RO         = "IEnableableComponentLookupRO";
        private const string I_ENABLEABLE_COMPONENT_LOOKUP_RW         = "IEnableableComponentLookupRW";
        private const string I_PHYSICS_BUFFER_LOOKUP_RO               = "IPhysicsBufferLookupRO";
        private const string I_PHYSICS_BUFFER_LOOKUP_RW               = "IPhysicsBufferLookupRW";
        private const string I_PHYSICS_COMPONENT_LOOKUP_RO            = "IPhysicsComponentLookupRO";
        private const string I_PHYSICS_COMPONENT_LOOKUP_RW            = "IPhysicsComponentLookupRW";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RO = "IPhysicsEnableableComponentLookupRO";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RW = "IPhysicsEnableableComponentLookupRW";

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_LOOKUPS_01"
            , title: "Lookup Generator Error"
            , messageFormat: "This error indicates a bug in the Lookup source generators. Error message: '{0}'."
            , category: "EncosyTower.Entities.Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: ""
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      LOOKUP_ATTRIBUTE_METADATA_NAME
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

        private static LookupDefinition GetSemanticMatch(
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

            var kind = GetLookupKind(structSymbol);

            if (kind == LookupKind.None)
            {
                return default;
            }

            GetLookupInterfaces(kind, out var interfaceLookupRO, out var interfaceLookupRW);
            GetRequiredEcsInterfaces(kind, out var ecsInterface1, out var ecsInterface2);

            using var typeRefsBuilder = ImmutableArrayBuilder<TypeRefModel>.Rent();
            var typeHash = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attribute in context.Attributes)
            {
                var args = attribute.ConstructorArguments;

                if (args.Length < 1 || args[0].Value is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (type.IsUnboundGenericType || type.IsUnmanagedType == false)
                {
                    continue;
                }

                if (type.InheritsFromInterface(ecsInterface1) == false)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(ecsInterface2) == false
                    && type.InheritsFromInterface(ecsInterface2) == false
                )
                {
                    continue;
                }

                var isReadOnly = args.Length > 1 && args[1].Value is true;

                if (typeHash.Add(type))
                {
                    typeRefsBuilder.Add(new TypeRefModel {
                        typeName = type.ToFullName(),
                        typeIdentifier = type.ToValidIdentifier(),
                        typeShortName = type.Name,
                        isReadOnly = isReadOnly,
                    });
                }
            }

            var typeRefs = typeRefsBuilder.ToImmutable().AsEquatableArray();

            if (typeRefs.Count == 0)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var fileTypeName = structSymbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, fileTypeName);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileTypeName);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: kind >= LookupKind.PhysicsBuffer
                    ? PrintAdditionalUsingsWithLatios
                    : PrintAdditionalUsings
            );

            return new LookupDefinition {
                structName = structSymbol.Name,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                interfaceLookupRO = interfaceLookupRO,
                interfaceLookupRW = interfaceLookupRW,
                kind = kind,
                typeRefs = typeRefs,
                location = LocationInfo.From(syntax.GetLocation()),
            };
        }

        private static LookupKind GetLookupKind(INamedTypeSymbol structSymbol)
        {
            foreach (var iface in structSymbol.AllInterfaces)
            {
                var full = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", string.Empty);

                if (string.Equals(full, I_BUFFER_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.Buffer;

                if (string.Equals(full, I_COMPONENT_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.Component;

                if (string.Equals(full, I_ENABLEABLE_BUFFER_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.EnableableBuffer;

                if (string.Equals(full, I_ENABLEABLE_COMPONENT_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.EnableableComponent;

                if (string.Equals(full, I_PHYSICS_BUFFER_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.PhysicsBuffer;

                if (string.Equals(full, I_PHYSICS_COMPONENT_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.PhysicsComponent;

                if (string.Equals(full, I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS, StringComparison.Ordinal))
                    return LookupKind.PhysicsEnableableComponent;
            }

            return LookupKind.None;
        }

        private static void GetLookupInterfaces(LookupKind kind, out string ro, out string rw)
        {
            switch (kind)
            {
                case LookupKind.Buffer:
                    ro = I_BUFFER_LOOKUP_RO;
                    rw = I_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.Component:
                    ro = I_COMPONENT_LOOKUP_RO;
                    rw = I_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.EnableableBuffer:
                    ro = I_ENABLEABLE_BUFFER_LOOKUP_RO;
                    rw = I_ENABLEABLE_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.EnableableComponent:
                    ro = I_ENABLEABLE_COMPONENT_LOOKUP_RO;
                    rw = I_ENABLEABLE_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsBuffer:
                    ro = I_PHYSICS_BUFFER_LOOKUP_RO;
                    rw = I_PHYSICS_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsComponent:
                    ro = I_PHYSICS_COMPONENT_LOOKUP_RO;
                    rw = I_PHYSICS_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsEnableableComponent:
                    ro = I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RO;
                    rw = I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RW;
                    return;

                default:
                    ro = string.Empty;
                    rw = string.Empty;
                    return;
            }
        }

        private static void GetRequiredEcsInterfaces(LookupKind kind, out string iface1, out string iface2)
        {
            switch (kind)
            {
                case LookupKind.Buffer:
                case LookupKind.PhysicsBuffer:
                    iface1 = I_BUFFER_ELEMENT_DATA;
                    iface2 = string.Empty;
                    return;

                case LookupKind.Component:
                case LookupKind.PhysicsComponent:
                    iface1 = I_COMPONENT_DATA;
                    iface2 = string.Empty;
                    return;

                case LookupKind.EnableableBuffer:
                    iface1 = I_BUFFER_ELEMENT_DATA;
                    iface2 = I_ENABLEABLE_COMPONENT;
                    return;

                case LookupKind.EnableableComponent:
                case LookupKind.PhysicsEnableableComponent:
                    iface1 = I_COMPONENT_DATA;
                    iface2 = I_ENABLEABLE_COMPONENT;
                    return;

                default:
                    iface1 = string.Empty;
                    iface2 = string.Empty;
                    return;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim _
            , LookupDefinition candidate
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

                var writer = LookupCodeWriter.GetWriter(candidate.kind);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , writer.Write(candidate)
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

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Entities.Lookups;");
            p.PrintLine("using Unity.Collections;");
            p.PrintLine("using Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
        private static void PrintAdditionalUsingsWithLatios(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Entities.Lookups;");
            p.PrintLine("using Latios.Psyshock;");
            p.PrintLine("using Unity.Collections;");
            p.PrintLine("using Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
