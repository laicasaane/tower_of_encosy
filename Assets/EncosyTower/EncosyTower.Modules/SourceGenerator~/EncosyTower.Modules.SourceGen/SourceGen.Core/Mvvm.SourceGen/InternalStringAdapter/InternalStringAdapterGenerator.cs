using System;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.InternalStringAdapterSourceGen
{
    [Generator]
    public class InternalStringAdapterGenerator : IIncrementalGenerator
    {
        private const string COMPONENT_MODEL_NS = "global::EncosyTower.Modules.Mvvm.ComponentModel";
        private const string VIEW_BINDING_NS = "EncosyTower.Modules.Mvvm.ViewBinding";
        private const string INPUT_NS = "EncosyTower.Modules.Mvvm.Input";
        private const string IOBSERVABLE_OBJECT = "global::EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject";
        private const string IADAPTER = "global::EncosyTower.Modules.Mvvm.ViewBinding.IAdapter";
        private const string ADAPTER_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.ViewBinding.AdapterAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => IsSyntaxMatch(node, token),
                transform: static (syntaxContext, token) => GetSemanticMatch(syntaxContext, token)
            ).Where(static t => t is { });

            var candidateToIgnoreProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => IsAdapterTypeSyntaxMatch(node, token),
                transform: static (syntaxContext, token) => GetTypeInAdapterDeclaration(syntaxContext, token)
            ).Where(static t => t is not null);

            var combined = candidateProvider.Collect()
                .Combine(candidateToIgnoreProvider.Collect())
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider);

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

        public static bool IsSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is FieldDeclarationSyntax field)
            {
                if (field.HasAttributeCandidate(COMPONENT_MODEL_NS, "ObservableProperty"))
                {
                    return true;
                }
            }

            if (syntaxNode is MethodDeclarationSyntax method && method.ParameterList.Parameters.Count == 1)
            {
                if (method.HasAttributeCandidate(INPUT_NS, "RelayCommand")
                    || method.HasAttributeCandidate(VIEW_BINDING_NS, "Binding")
                )
                {
                    return true;
                }
            }

            if (syntaxNode is PropertyDeclarationSyntax property
                && property.Parent is ClassDeclarationSyntax classSyntax
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0
            )
            {
                return true;
            }

            return false;
        }

        public static TypeRef GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE) == false)
            {
                return null;
            }

            var semanticModel = context.SemanticModel;

            if (context.Node is FieldDeclarationSyntax field)
            {
                return new TypeRef {
                    Syntax = field.Declaration.Type,
                    Symbol = semanticModel.GetTypeInfo(field.Declaration.Type, token).Type,
                };
            }

            if (context.Node is MethodDeclarationSyntax method)
            {
                var typeSyntax = method.ParameterList.Parameters[0].Type;

                return new TypeRef {
                    Syntax = typeSyntax,
                    Symbol = semanticModel.GetTypeInfo(typeSyntax, token).Type,
                };
            }

            if (context.Node is PropertyDeclarationSyntax property
                && property.Parent is ClassDeclarationSyntax classSyntax
                && classSyntax.DoesSemanticMatch(IOBSERVABLE_OBJECT, context.SemanticModel, token)
                && classSyntax.AnyFieldHasNotifyPropertyChangedForAttribute(property)
            )
            {
                return new TypeRef {
                    Syntax = property.Type,
                    Symbol = semanticModel.GetTypeInfo(property.Type, token).Type,
                };
            }

            return null;
        }

        public static bool IsAdapterTypeSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is TypeDeclarationSyntax typeDeclareSyntax
                && typeDeclareSyntax.Kind() is (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                && typeDeclareSyntax.BaseList != null
                && typeDeclareSyntax.BaseList.Types.Count > 0
                && typeDeclareSyntax.AttributeLists.Count > 0;
        }

        public static INamedTypeSymbol GetTypeInAdapterDeclaration(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE) == false
                || context.Node is not TypeDeclarationSyntax typeDeclareSyntax
                || typeDeclareSyntax.Kind() is not (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                || typeDeclareSyntax.BaseList == null
                || typeDeclareSyntax.BaseList.Types.Count < 1
                || typeDeclareSyntax.AttributeLists.Count < 1
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var declaredSymbol = semanticModel.GetDeclaredSymbol(typeDeclareSyntax, token);

            foreach (var baseType in typeDeclareSyntax.BaseList.Types)
            {
                var baseTypeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                if (baseTypeInfo.Type is INamedTypeSymbol interfaceSymbol
                    && interfaceSymbol.ToFullName() == IADAPTER
                    && TryGetMatchTypeFromAttribute(declaredSymbol, out var type)
                )
                {
                    return type;
                }

                if ((ImplementsInterface(baseTypeInfo.Type.Interfaces)
                    || ImplementsInterface(baseTypeInfo.Type.AllInterfaces))
                    && TryGetMatchTypeFromAttribute(declaredSymbol, out type)
                )
                {
                    return type;
                }
            }

            return null;

            static bool ImplementsInterface(ImmutableArray<INamedTypeSymbol> interfaces)
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.ToFullName() == IADAPTER)
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool TryGetMatchTypeFromAttribute(
                  INamedTypeSymbol declaredSymbol
                , out INamedTypeSymbol sourceType
            )
            {
                if (declaredSymbol == null)
                {
                    sourceType = default;
                    return false;
                }

                var attribute = declaredSymbol.GetAttribute(ADAPTER_ATTRIBUTE);

                if (attribute == null
                    || attribute.ConstructorArguments.Length != 2
                    || attribute.ConstructorArguments[1].Value is not INamedTypeSymbol toArg
                    || toArg.ToFullName() != "string"
                    || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol fromArg
                )
                {
                    sourceType = default;
                    return false;
                }

                sourceType = fromArg;
                return sourceType != null;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ImmutableArray<TypeRef> candidates
            , ImmutableArray<INamedTypeSymbol> candidatesToIgnore
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var declaration = new InternalStringAdapterDeclaration(candidates, candidatesToIgnore);

                declaration.GenerateAdapters(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_INTERNAL_STRING_ADAPTER_01"
                , "Internal String Adapter Generator Error"
                , "This error indicates a bug in the Internal String Adapter source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Mvvm.ViewBinding.IAdapter"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
