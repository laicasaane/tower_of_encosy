using System;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.EnumTemplates.SourceGen
{
    [Generator]
    internal class EnumTemplateGenerator : IIncrementalGenerator
    {
        public const string ENUM_TEMPLATE_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.EnumTemplateAttribute";
        public const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.EnumMembersForTemplateAttribute";
        public const string TYPE_MEMBER_FOR_ENUM_TEMPLATE_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.TypeNameMemberForEnumTemplateAttribute";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(EnumTemplateGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var idProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchTemplate
                , transform: GetTemplateCandidate
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var kindProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchKind
                , transform: GetKindCandidate
            ).Where(static t => t.typeSymbol is { } && t.templateSymbol is { });

            var combined = idProvider
                .Combine(kindProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
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

        private static bool IsSyntaxMatchTemplate(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is StructDeclarationSyntax structSyntax
                && structSyntax.HasAttributeCandidate("EncosyTower.Modules.EnumExtensions", "EnumTemplate");
        }

        private static bool IsSyntaxMatchKind(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            switch (syntaxNode)
            {
                case not (EnumDeclarationSyntax or TypeDeclarationSyntax):
                case TypeDeclarationSyntax { TypeParameterList: not null }:
                    return false;
            }

            var baseTypeSyntax = (BaseTypeDeclarationSyntax)syntaxNode;
            var attribSyntax = baseTypeSyntax.GetAttribute("EncosyTower.Modules.EnumExtensions", "EnumMembersForTemplate");
            attribSyntax ??= baseTypeSyntax.GetAttribute("EncosyTower.Modules.EnumExtensions", "TypeNameMemberForEnumTemplate");

            return attribSyntax?.ArgumentList is { Arguments: { Count: > 1 } args }
                && args[0].Expression is TypeOfExpressionSyntax
                && args[1].Expression is LiteralExpressionSyntax
                ;
        }

        private static TemplateCandidate GetTemplateCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null
                || symbol.IsUnmanagedType == false
                || symbol.IsUnboundGenericType
                || symbol.GetAttribute(ENUM_TEMPLATE_ATTRIBUTE) is null
            )
            {
                return default;
            }

            return new TemplateCandidate {
                syntax = syntax,
                symbol = symbol,
            };
        }

        private static KindCandidate GetKindCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not BaseTypeDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (typeSymbol == null)
            {
                return default;
            }

            var attrib = typeSymbol.GetAttribute(ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE);
            var isEnum = attrib is not null;
            attrib ??= typeSymbol.GetAttribute(TYPE_MEMBER_FOR_ENUM_TEMPLATE_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length < 2)
            {
                return default;
            }

            var args = attrib.ConstructorArguments;
            var typeArg = args[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol templateSymbol
                || templateSymbol.IsUnmanagedType == false
                || templateSymbol.IsUnboundGenericType
                || templateSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE) == false
            )
            {
                return default;
            }

            var candidate = new KindCandidate {
                typeSymbol = typeSymbol,
                templateSymbol = templateSymbol,
                attributeData = attrib,
                enumMembers = isEnum,
            };

            if (args.Length > 1)
            {
                var arg = args[1];

                if (arg.Kind == TypedConstantKind.Primitive
                    && arg.Value is ulong value
                )
                {
                    candidate.order = value;
                }
            }

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , TemplateCandidate templateCandidate
            , ImmutableArray<KindCandidate> kindCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (templateCandidate.syntax == null || templateCandidate.symbol == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntax = templateCandidate.syntax;
                var symbol = templateCandidate.symbol;
                var syntaxTree = syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                var declaration = new EnumTemplateDeclaration(
                      context
                    , templateCandidate
                    , kindCandidates
                    , compilationCandidate.references
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , declaration.WriteCode()
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, symbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
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
                    , templateCandidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ENUM_TEMPLATE_01"
                , "Enum Template Generator Error"
                , "This error indicates a bug in the Enum Template source generators. Error message: '{0}'."
                , "EncosyTower.Modules.EnumTemplateAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
