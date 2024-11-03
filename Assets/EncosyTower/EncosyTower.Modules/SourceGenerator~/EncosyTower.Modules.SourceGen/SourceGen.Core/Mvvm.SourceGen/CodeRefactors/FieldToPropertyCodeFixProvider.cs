using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace EncosyTower.Modules.Mvvm.CodeRefactors
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldToPropertyCodeFixProvider)), Shared]
    internal class FieldToPropertyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(MvvmDiagnosticAnalyzer.DIAGNOSTIC_FIELD);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md
            // for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent
                .AncestorsAndSelf()
                .OfType<FieldDeclarationSyntax>()
                .First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                      title: "Replace field by property"
                    , createChangedSolution: c => MakePropertyAsync(context.Document, declaration, c)
                    , equivalenceKey: "Replace field by property"
                )
                , diagnostic
            );
        }

        private async Task<Solution> MakePropertyAsync(
              Document document
            , FieldDeclarationSyntax fieldDecl
            , CancellationToken cancellationToken
        )
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var propAttribListList = new List<List<AttributeSyntax>>();
            var propAttribCheck = new HashSet<string>();
            var fieldAttribListList = new List<List<AttributeSyntax>>();

            foreach (var fieldAttribList in fieldDecl.AttributeLists)
            {
                var attributes = fieldAttribList.Attributes;

                if (attributes.Count < 1)
                {
                    continue;
                }

                var propList = new List<AttributeSyntax>();
                var fieldList = new List<AttributeSyntax>();
                var targetKind = fieldAttribList.Target?.Identifier.Kind();

                if (targetKind is SyntaxKind.FieldKeyword)
                {
                    fieldList.AddRange(attributes);
                    fieldAttribListList.Add(fieldList);
                    continue;
                }

                if (targetKind is SyntaxKind.PropertyKeyword)
                {
                    foreach (var attrib in attributes)
                    {
                        var name = attrib.ToString();

                        if (propAttribCheck.Contains(name) == false)
                        {
                            propAttribCheck.Add(name);
                            propList.Add(attrib);
                        }
                    }

                    propAttribListList.Add(propList);
                    continue;
                }

                foreach (var attrib in attributes)
                {
                    var (_, target) = GetAttributeInfo(semanticModel, attrib);
                    var name = attrib.ToString();

                    if (target.HasFlag(AttributeTargets.Property))
                    {
                        if (propAttribCheck.Contains(name) == false)
                        {
                            propAttribCheck.Add(name);
                            propList.Add(attrib);
                        }
                    }

                    if (target.HasFlag(AttributeTargets.Field))
                    {
                        if (propAttribCheck.Contains(name) == false)
                        {
                            fieldList.Add(attrib);
                        }
                    }
                }

                if (propList.Count > 0)
                {
                    propAttribListList.Add(propList);
                }

                if (fieldList.Count > 0)
                {
                    fieldAttribListList.Add(fieldList);
                }
            }

            var variableDelc = fieldDecl.Declaration.Variables.First();
            var propName = variableDelc.Identifier.Text.ToPropertyName();

            var getAccessor = SyntaxFactory.AccessorDeclaration(
                    kind: SyntaxKind.GetAccessorDeclaration
                , attributeLists: new SyntaxList<AttributeListSyntax>()
                , modifiers: SyntaxFactory.TokenList()
                , keyword: SyntaxFactory.Token(SyntaxKind.GetKeyword)
                , semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                , expressionBody: SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"Get_{propName}"))
                )
            );

            var setAccessor = SyntaxFactory.AccessorDeclaration(
                    kind: SyntaxKind.SetAccessorDeclaration
                , attributeLists: new SyntaxList<AttributeListSyntax>()
                , modifiers: SyntaxFactory.TokenList()
                , keyword: SyntaxFactory.Token(SyntaxKind.SetKeyword)
                , semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                , expressionBody: SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName($"Set_{propName}")
                        , SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                            )
                        )
                    )
                )
            );

            var propDecl = SyntaxFactory.PropertyDeclaration(fieldDecl.Declaration.Type, propName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(getAccessor, setAccessor)
                .WithAdditionalAnnotations(Formatter.Annotation)
                .WithTrailingTrivia(fieldDecl.GetTrailingTrivia())
                ;

            var withAttribListTrivia = false;

            for (var i = 0; i < propAttribListList.Count; i++)
            {
                var list = propAttribListList[i];
                var propAttribList = SyntaxFactory.AttributeList(
                      openBracketToken: SyntaxFactory.Token(SyntaxKind.OpenBracketToken)
                    , target: null
                    , attributes: SyntaxFactory.SeparatedList(list)
                    , closeBracketToken: SyntaxFactory.Token(SyntaxKind.CloseBracketToken)
                );

                if (i == 0)
                {
                    withAttribListTrivia = true;
                    propAttribList = propAttribList.WithTriviaFrom(fieldDecl.AttributeLists[0]);
                }

                propDecl = propDecl.AddAttributeLists(propAttribList);
            }

            for (var i = 0; i < fieldAttribListList.Count; i++)
            {
                var list = fieldAttribListList[i];
                var fieldAttribList = SyntaxFactory.AttributeList(
                      openBracketToken: SyntaxFactory.Token(SyntaxKind.OpenBracketToken)
                    , target: SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.FieldKeyword))
                    , attributes: SyntaxFactory.SeparatedList(list)
                    , closeBracketToken: SyntaxFactory.Token(SyntaxKind.CloseBracketToken)
                );

                if (i == 0 && withAttribListTrivia == false)
                {
                    withAttribListTrivia = true;
                    fieldAttribList = fieldAttribList.WithTriviaFrom(fieldDecl.AttributeLists[0]);
                }

                propDecl = propDecl.AddAttributeLists(fieldAttribList);
            }

            var newRoot = root.ReplaceNode(fieldDecl, propDecl);
            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }

        private static (string, AttributeTargets) GetAttributeInfo(
              SemanticModel semanticModel
            , AttributeSyntax attribSyntax
        )
        {
            var attribSymbol = semanticModel.GetSymbolInfo(attribSyntax);

            if (attribSymbol.TryGetAttributeTypeSymbol(out INamedTypeSymbol attribTypeSymbol) == false)
            {
                return (string.Empty, 0);
            }

            var attributeUsageAttribute = attribTypeSymbol.GetAttribute("global::System.AttributeUsageAttribute");

            if (attributeUsageAttribute == null
                || attributeUsageAttribute.ConstructorArguments.Length < 1
            )
            {
                return (string.Empty, 0);
            }

            return (attribTypeSymbol.Name, (AttributeTargets)attributeUsageAttribute.ConstructorArguments[0].Value);
        }
    }
}
