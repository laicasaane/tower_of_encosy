using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.CodeRefactors.Entities
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ISystemCodeFixProvider)), Shared]
    internal sealed class ISystemCodeFixProvider : CodeFixProvider
    {
        private const string ACTION_COMPLETE = "Implement missing ISystem members";
        private const string ACTION_ONCREATE = "Implement ISystem.OnCreate";
        private const string ACTION_ONUPDATE = "Implement ISystem.OnUpdate";
        private const string ACTION_ONDESTROY = "Implement ISystem.OnDestroy";

        private const string ISYSTEM_FQN = "global::Unity.Entities.ISystem";
        private const string SYSTEMSTATE_FQN = "global::Unity.Entities.SystemState";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ISystemDiagnosticAnalyzer.DIAGNOSTIC_ISYSTEM);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var typeDecl = root.FindToken(diagnosticSpan.Start).Parent
                ?.AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault();

            if (typeDecl is null)
            {
                return;
            }

            var semanticModel = await context.Document
                .GetSemanticModelAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var typeSymbolCandidate = semanticModel.GetDeclaredSymbol(typeDecl, context.CancellationToken);

            if (typeSymbolCandidate is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            var state = ComputeContractState(typeDecl, typeSymbol);

            RegisterAction(context, typeDecl, state, ACTION_COMPLETE, MemberFlags.All, true);
            RegisterAction(context, typeDecl, state, ACTION_ONCREATE, MemberFlags.OnCreate, false);
            RegisterAction(context, typeDecl, state, ACTION_ONUPDATE, MemberFlags.OnUpdate, false);
            RegisterAction(context, typeDecl, state, ACTION_ONDESTROY, MemberFlags.OnDestroy, false);
        }

        private static ISystemContractState ComputeContractState(
              TypeDeclarationSyntax typeDecl
            , INamedTypeSymbol typeSymbol
        )
        {
            MemberFlags existingMembers = default;

            if (HasCompatibleLifecycleMember(typeSymbol, "OnCreate"))
            {
                existingMembers |= MemberFlags.OnCreate;
            }

            if (HasCompatibleLifecycleMember(typeSymbol, "OnUpdate"))
            {
                existingMembers |= MemberFlags.OnUpdate;
            }

            if (HasCompatibleLifecycleMember(typeSymbol, "OnDestroy"))
            {
                existingMembers |= MemberFlags.OnDestroy;
            }

            return new ISystemContractState {
                hasPartial = typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword),
                hasISystem = HasISystemConformance(typeSymbol),
                existingMembers = existingMembers,
            };
        }

        private static bool HasISystemConformance(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ImplementsInterface(ISYSTEM_FQN, true);
        }

        private static bool HasCompatibleLifecycleMember(INamedTypeSymbol typeSymbol, string methodName)
        {
            var members = typeSymbol.GetMembers(methodName);

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.ReturnsVoid == false
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                var param = method.Parameters[0];

                if (param.RefKind == RefKind.Ref && param.Type.IsType(SYSTEMSTATE_FQN))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RegisterAction(
              CodeFixContext context
            , TypeDeclarationSyntax typeDecl
            , ISystemContractState state
            , string actionTitle
            , MemberFlags missingMembers
            , bool isComplete
        )
        {
            if (isComplete)
            {
                if (state.hasPartial && state.hasISystem && state.existingMembers.HasFlag(missingMembers))
                {
                    return;
                }
            }
            else if (state.existingMembers.HasFlag(missingMembers))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                      title: actionTitle
                    , createChangedDocument: token => ApplyEditsAsync(
                          context.Document
                        , typeDecl
                        , state
                        , missingMembers & ~state.existingMembers
                        , token
                    )
                )
                , context.Diagnostics
            );
        }

        private static async Task<Document> ApplyEditsAsync(
              Document document
            , TypeDeclarationSyntax typeDecl
            , ISystemContractState state
            , MemberFlags implementMembers
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
            var newTypeDecl = typeDecl;

            if (state.hasPartial == false)
            {
                newTypeDecl = AddPartialModifier(newTypeDecl);
            }

            if (state.hasISystem == false)
            {
                newTypeDecl = AddISystemInterface(newTypeDecl);
            }

            if (implementMembers.HasFlag(MemberFlags.OnCreate))
            {
                newTypeDecl = AddLifecycleMember(newTypeDecl, "OnCreate");
            }

            if (implementMembers.HasFlag(MemberFlags.OnUpdate))
            {
                newTypeDecl = AddLifecycleMember(newTypeDecl, "OnUpdate");
            }

            if (implementMembers.HasFlag(MemberFlags.OnDestroy))
            {
                newTypeDecl = AddLifecycleMember(newTypeDecl, "OnDestroy");
            }

            var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);
            return document.WithSyntaxRoot(newRoot);
        }

        private static TypeDeclarationSyntax AddPartialModifier(TypeDeclarationSyntax typeDecl)
        {
            var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space);

            var newModifiers = typeDecl.Modifiers.Insert(typeDecl.Modifiers.Count, partialToken);
            return typeDecl.WithModifiers(newModifiers);
        }

        private static TypeDeclarationSyntax AddISystemInterface(TypeDeclarationSyntax typeDecl)
        {
            var isystemType = SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName("ISystem")
            );

            if (typeDecl.BaseList is { } baseList)
            {
                return typeDecl.WithBaseList(baseList.AddTypes(isystemType));
            }

            var colonToken = SyntaxFactory.Token(SyntaxKind.ColonToken)
                .WithTrailingTrivia(SyntaxFactory.Space);

            var newList = SyntaxFactory.BaseList(colonToken, SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(isystemType));
            return typeDecl.WithBaseList(newList);
        }

        private static TypeDeclarationSyntax AddLifecycleMember(TypeDeclarationSyntax typeDecl, string methodName)
        {
            var method = SyntaxFactory.MethodDeclaration(
                  returnType: SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword).WithTrailingTrivia(SyntaxFactory.Space))
                , identifier: methodName
            )
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
            ))
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("state"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)))
                    .WithType(SyntaxFactory.ParseTypeName("SystemState"))
            )))
            .WithBody(SyntaxFactory.Block())
            .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            var members = typeDecl.Members.Add(method);
            return typeDecl.WithMembers(members);
        }
    }
}
