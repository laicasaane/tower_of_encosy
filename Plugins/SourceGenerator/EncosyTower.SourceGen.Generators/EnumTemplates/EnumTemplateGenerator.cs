using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    [Generator]
    internal sealed class EnumTemplateGenerator : IIncrementalGenerator
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

        private static readonly DiagnosticDescriptor s_errorDescriptor = new(
              id: "SG_ENUM_TEMPLATE_UNKNOWN_0001"
            , title: "Enum Template Generator Error"
            , messageFormat: "This error indicates a bug in the Enum Template source generators. Error message: '{0}'."
            , category: "EncosyTower.EnumExtensions"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: ""
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var templateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_TEMPLATE_ATTRIBUTE_METADATA
                    , static (node, _) => node is StructDeclarationSyntax
                    , EnumTemplateSpec.Extract
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

        private static TemplateMemberSpec ExtractEnumMembersCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: true);
        }

        private static TemplateMemberSpec ExtractTypeAsMemberCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: false);
        }

        private static TemplateMemberSpec ExtractMemberFromAttributeContext(
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
                || templateSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            var attribLocation = LocationInfo.From(
                attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? Location.None
            );

            return TemplateMemberSpec.Extract(
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
            , CompilationInfo compilation
            , EnumTemplateSpec templateCandidate
            , ImmutableArray<TemplateMemberSpec> memberCandidates
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
                var declaration = new EnumTemplateDeclaration(
                      templateCandidate
                    , memberCandidates
                    , compilation.references
                );

                var assemblyName = compilation.assemblyName;
                var hintName = $"{templateCandidate.fileHintName}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , templateCandidate.openingSource
                    , declaration.WriteCode()
                    , templateCandidate.closingSource
                    , hintName
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
                    , templateCandidate.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }
    }
}
