using System;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.RuntimeTypeCache.SourceGen
{
    [Generator]
    internal class RuntimeTypeCacheGenerator : IIncrementalGenerator
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Types.Caches.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCacheGenerator);

        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.RuntimeTypeCache.SourceGen.RuntimeTypeCacheGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Modules.Types.RuntimeTypeCache";
        private const string GENERATED_RUNTIME_TYPE_CACHE = "[global::EncosyTower.Modules.Types.Caches.SourceGen.GeneratedRuntimeTypeCache]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: GetMatchedType
            ).Where(ValidateCandiate);

            var combined = typeProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });

            static bool ValidateCandiate(Candidate candidate)
            {
                return candidate.cacheAttributeType is not CacheAttributeType.None
                    && candidate.containingSyntax is not null
                    && candidate.containingType is not null
                    && candidate.typeSyntax is not null
                    && candidate.type is not null
                    ;
            }
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is MemberAccessExpressionSyntax syntax
                && syntax.Expression is IdentifierNameSyntax { Identifier.ValueText: "RuntimeTypeCache" }
                && syntax.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } member
                && IsMemberSupported(member.Identifier.ValueText)
                && GetContainingType(syntax) is not null;
        }

        private static bool IsMemberSupported(string memberName)
        {
            return memberName switch {
                "GetTypesDerivedFrom" => true,
                "GetTypesWithAttribute" => true,
                "GetFieldsWithAttribute" => true,
                "GetMethodsWithAttribute" => true,
                _ => false,
            };
        }

        private static Candidate GetMatchedType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var semanticModel = context.SemanticModel;

            var syntax = context.Node as MemberAccessExpressionSyntax;
            var identifier = syntax.Expression as IdentifierNameSyntax;
            var identifierType = semanticModel.GetTypeInfo(identifier, token).Type;

            if (identifierType.ToFullName() is not RUNTIME_TYPE_CACHE)
            {
                return default;
            }

            var member = syntax.Name as GenericNameSyntax;
            var typeArgList = member.TypeArgumentList;
            var containingSyntax = GetContainingType(syntax);

            var containingType = semanticModel.GetDeclaredSymbol(containingSyntax, token);
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            var cacheAttributeType = member.Identifier.ValueText switch {
                "GetTypesDerivedFrom" => CacheAttributeType.CacheTypesDerivedFrom,
                "GetTypesWithAttribute" => CacheAttributeType.CacheTypesWithAttribute,
                "GetFieldsWithAttribute" => CacheAttributeType.CacheFieldsWithAttribute,
                "GetMethodsWithAttribute" => CacheAttributeType.CacheMethodsWithAttribute,
                _ => CacheAttributeType.None,
            };

            var candidate = new Candidate {
                containingSyntax = containingSyntax,
                containingType = containingType,
                typeSyntax = typeArgList.Arguments[0],
                type = typeInfo.Type,
                cacheAttributeType = cacheAttributeType,
            };

            if (syntax.Parent is InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments }
                && arguments[0].Expression is LiteralExpressionSyntax literal
                && string.IsNullOrWhiteSpace(literal.Token.ValueText) == false
            )
            {
                candidate.assemblyName = literal.Token.ValueText;
            }

            return candidate;
        }

        private static TypeDeclarationSyntax GetContainingType(SyntaxNode node)
        {
            var parent = node.Parent;

            while (parent is not null && parent is not TypeDeclarationSyntax)
            {
                parent = parent.Parent;
            }

            return parent as TypeDeclarationSyntax;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , Candidate candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var containingSyntax = candidate.containingSyntax;
                var containingType = candidate.containingType;
                var syntaxTree = containingSyntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                context.OutputSource(
                      outputSourceGenFiles
                    , containingSyntax
                    , WriteCode(candidate)
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, candidate.typeSyntax, containingType.ToValidIdentifier())
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
                    , candidate.containingSyntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_RUNTIME_TYPE_CACHE_01"
                , "Runtime Type Cache Generator Error"
                , "This error indicates a bug in the Runtime Type Cache source generators. Error message: '{0}'."
                , "RuntimeTypeCache"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static string WriteCode(Candidate cdd)
        {
            var containingSyntax = cdd.containingSyntax;
            var containingType = cdd.containingType;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, cdd.containingSyntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var isStruct = containingType.TypeKind == TypeKind.Struct;
            var isRefStruct = isStruct && containingSyntax.Modifiers.Any(SyntaxKind.RefKeyword);
            var isRecord = containingSyntax.Modifiers.Any(SyntaxKind.RecordKeyword);

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(isRecord, "record ")
                    .PrintIf(isStruct, "struct ", "class ")
                    .Print(containingSyntax.Identifier.ValueText)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Provides information about the types, fields and methods to be cached.");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(GENERATED_RUNTIME_TYPE_CACHE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);

                    p.PrintBeginLine("[global::EncosyTower.Modules.Types.Caches.");

                    switch (cdd.cacheAttributeType)
                    {
                        case CacheAttributeType.CacheTypesDerivedFrom:
                            p.Print(nameof(CacheAttributeType.CacheTypesDerivedFrom));
                            break;

                        case CacheAttributeType.CacheTypesWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheTypesWithAttribute));
                            break;

                        case CacheAttributeType.CacheFieldsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheFieldsWithAttribute));
                            break;

                        case CacheAttributeType.CacheMethodsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheMethodsWithAttribute));
                            break;
                    }

                    p.Print("(typeof(").Print(cdd.type.ToFullName()).Print(")");

                    if (string.IsNullOrEmpty(cdd.assemblyName) == false)
                    {
                        p.Print(", \"").Print(cdd.assemblyName).Print("\"");
                    }

                    p.PrintEndLine(")]");
                    p.PrintBeginLine("private struct RuntimeTypeCache_")
                        .Print(cdd.typeSyntax.GetLineNumber().ToString())
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private enum CacheAttributeType
        {
            None,
            CacheTypesDerivedFrom,
            CacheTypesWithAttribute,
            CacheFieldsWithAttribute,
            CacheMethodsWithAttribute,
        }

        private struct Candidate
        {
            public TypeDeclarationSyntax containingSyntax;
            public INamedTypeSymbol containingType;
            public TypeSyntax typeSyntax;
            public ITypeSymbol type;
            public string assemblyName;
            public CacheAttributeType cacheAttributeType;
        }
    }
}
