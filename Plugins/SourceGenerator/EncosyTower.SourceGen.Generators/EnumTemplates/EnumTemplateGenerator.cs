using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    [Generator]
    internal class EnumTemplateGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string ENUM_TEMPLATE_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumTemplateAttribute";
        public const string ENUM_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumTemplateAttribute";
        public const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumMembersForTemplateAttribute";
        public const string TYPE_AS_MEMBER_ATTRIBUTE = $"global::{NAMESPACE}.TypeAsEnumMemberForTemplateAttribute";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumMembersForTemplateAttribute";
        private const string TYPE_AS_MEMBER_ATTRIBUTE_METADATA = $"{NAMESPACE}.TypeAsEnumMemberForTemplateAttribute";
        public const string GENERATOR_NAME = nameof(EnumTemplateGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var templateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_TEMPLATE_ATTRIBUTE_METADATA
                    , static (node, _) => node is StructDeclarationSyntax
                    , EnumTemplateCandidate.Extract
                )
                .Where(static t => t.IsValid);

            var enumMembersProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE_METADATA
                    , static (node, _) => node is BaseTypeDeclarationSyntax t
                        && (t is not TypeDeclarationSyntax td || td.TypeParameterList is null)
                    , ExtractEnumMembersCandidate
                )
                .Where(static t => t.IsValid);

            var typeAsMemberProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      TYPE_AS_MEMBER_ATTRIBUTE_METADATA
                    , static (node, _) => node is BaseTypeDeclarationSyntax t
                        && (t is not TypeDeclarationSyntax td || td.TypeParameterList is null)
                    , ExtractTypeAsMemberCandidate
                )
                .Where(static t => t.IsValid);

            var memberProvider = enumMembersProvider.Collect()
                .Combine(typeAsMemberProvider.Collect())
                .Select(static (t, _) => t.Left.AddRange(t.Right));

            var combined = templateProvider
                .Combine(memberProvider)
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static TemplateMemberCandidate ExtractEnumMembersCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: true);
        }

        private static TemplateMemberCandidate ExtractTypeAsMemberCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: false);
        }

        private static TemplateMemberCandidate ExtractMemberFromAttributeContext(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
            , bool isEnumMembers
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            {
                return default;
            }

            if (context.Attributes.Length < 1)
            {
                return default;
            }

            var attrib = context.Attributes[0];

            if (attrib.ConstructorArguments.Length < 2)
            {
                return default;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol templateSymbol
                || templateSymbol.IsUnmanagedType == false
                || templateSymbol.IsUnboundGenericType
                || templateSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE) == false
            )
            {
                return default;
            }

            var attribLocation = LocationInfo.From(
                attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? Location.None
            );

            return TemplateMemberCandidate.Extract(
                  typeSymbol
                , templateSymbol.ToFullName()
                , attrib
                , isEnumMembers
                , attribLocation
                , token
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , EnumTemplateCandidate templateCandidate
            , ImmutableArray<TemplateMemberCandidate> memberCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (templateCandidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var declaration = new EnumTemplateDeclaration(
                      templateCandidate
                    , memberCandidates
                    , compilation.references
                );

                var hintName = $"{GENERATOR_NAME}__{templateCandidate.fileHintName}.g.cs";
                var sourceFilePath = BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);
                var source = declaration.WriteCode();

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , templateCandidate.location.ToLocation()
                        , sourceFilePath
                        , sourceText
                        , projectPath
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                // Generator bugs are silently swallowed — do not emit diagnostics from the
                // generator. User-facing validation is handled by EnumTemplateAnalyzer.
            }
        }

        private static string BuildSourceFilePath(string assemblyName, string hintName, string projectPath)
        {
            if (projectPath is not null)
            {
                var dir = $"{projectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
        }
    }
}
