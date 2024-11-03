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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToFieldCodeFixProvider)), Shared]
    internal class PropertyToFieldCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(MvvmDiagnosticAnalyzer.DIAGNOSTIC_PROPERTY);

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
                .OfType<PropertyDeclarationSyntax>()
                .First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                      title: "Replace property by field"
                    , createChangedSolution: c => MakePropertyAsync(context.Document, declaration, c)
                    , equivalenceKey: "Replace property by field"
                )
                , diagnostic
            );
        }

        private async Task<Solution> MakePropertyAsync(
              Document document
            , PropertyDeclarationSyntax propertyDecl
            , CancellationToken cancellationToken
        )
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var propAttribListList = new List<List<AttributeSyntax>>();
            var fieldAttribListList = new List<List<AttributeSyntax>>();
            var fieldAttribCheck = new HashSet<string>();

            foreach (var fieldAttribList in propertyDecl.AttributeLists)
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

                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            fieldAttribCheck.Add(name);
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

                    if (target.HasFlag(AttributeTargets.Field))
                    {
                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            fieldAttribCheck.Add(name);
                            fieldList.Add(attrib);
                        }
                    }

                    if (target.HasFlag(AttributeTargets.Property))
                    {
                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            propList.Add(attrib);
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

            var fieldName = propertyDecl.Identifier.Text.ToFieldName();
            var varDecl = SyntaxFactory.VariableDeclaration(propertyDecl.Type);
            varDecl = varDecl.WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(fieldName)));

            var fieldDecl = SyntaxFactory.FieldDeclaration(varDecl)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithAdditionalAnnotations(Formatter.Annotation)
                .WithTrailingTrivia(propertyDecl.GetTrailingTrivia())
                ;

            var withAttribListTrivia = false;

            for (var i = 0; i < fieldAttribListList.Count; i++)
            {
                var list = fieldAttribListList[i];
                var fieldAttribList = SyntaxFactory.AttributeList(
                      openBracketToken: SyntaxFactory.Token(SyntaxKind.OpenBracketToken)
                    , target: null
                    , attributes: SyntaxFactory.SeparatedList(list)
                    , closeBracketToken: SyntaxFactory.Token(SyntaxKind.CloseBracketToken)
                );

                if (i == 0)
                {
                    withAttribListTrivia = true;
                    fieldAttribList = fieldAttribList.WithTriviaFrom(propertyDecl.AttributeLists[0]);
                }

                fieldDecl = fieldDecl.AddAttributeLists(fieldAttribList);
            }

            for (var i = 0; i < propAttribListList.Count; i++)
            {
                var list = propAttribListList[i];
                var propAttribList = SyntaxFactory.AttributeList(
                      openBracketToken: SyntaxFactory.Token(SyntaxKind.OpenBracketToken)
                    , target: SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.PropertyKeyword))
                    , attributes: SyntaxFactory.SeparatedList(list)
                    , closeBracketToken: SyntaxFactory.Token(SyntaxKind.CloseBracketToken)
                );

                if (i == 0 && withAttribListTrivia == false)
                {
                    withAttribListTrivia = true;
                    propAttribList = propAttribList.WithTriviaFrom(propertyDecl.AttributeLists[0]);
                }

                fieldDecl = fieldDecl.AddAttributeLists(propAttribList);
            }

            var newRoot = root.ReplaceNode(propertyDecl, fieldDecl);
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
