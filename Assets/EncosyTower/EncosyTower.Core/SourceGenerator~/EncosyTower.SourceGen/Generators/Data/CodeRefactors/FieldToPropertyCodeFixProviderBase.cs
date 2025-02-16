using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace EncosyTower.SourceGen.Generators.Data.CodeRefactors
{
    internal abstract class FieldToPropertyCodeFixProviderBase : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DataDiagnosticAnalyzer.DIAGNOSTIC_FIELD);

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
                .FirstOrDefault();

            if (declaration?.Declaration is null)
            {
                return;
            }

            var variables = declaration.Declaration.Variables;
            var count = variables.Count;

            if (count < 1)
            {
                return;
            }

            var sb = new StringBuilder();
            var lastIndex = count - 1;

            for (var i = 0; i < count; i++)
            {
                sb.Append('\'');
                sb.Append(variables[i].Identifier.Text);
                sb.Append('\'');

                if (i < lastIndex)
                {
                    sb.Append(", ");
                }
            }

            var title = MakeTitle(count, sb);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                      title: title
                    , createChangedSolution: c => MakePropertyAsync(context.Document, declaration, c)
                    , equivalenceKey: title
                )
                , diagnostic
            );
        }

        protected abstract string MakeTitle(int fieldVariableCount, StringBuilder fieldVariableBuilder);

        protected abstract PropertyDeclarationSyntax MakePropertyBody(
              string propName
            , FieldDeclarationSyntax fieldDecl
            , PropertyDeclarationSyntax propDecl
        );

        private async Task<Solution> MakePropertyAsync(
              Document document
            , FieldDeclarationSyntax fieldDecl
            , CancellationToken cancellationToken
        )
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var fieldAttribListList = new List<List<AttributeSyntax>>();
            var propAttribListList = new List<List<AttributeSyntax>>();
            var fieldAttribCheck = new HashSet<string>();
            var propAttribCheck = new HashSet<string>();

            PrepareAttributeListLists(
                  fieldDecl
                , semanticModel
                , fieldAttribListList
                , propAttribListList
                , fieldAttribCheck
                , propAttribCheck
            );

            AddDataPropertyAttribute(fieldDecl, propAttribListList, propAttribCheck);

            var propertyType = ExtractTypeFromPropertyTypeAttribute(fieldAttribListList);
            var variables = fieldDecl.Declaration.Variables;
            var count = variables.Count;
            var newNodes = new List<SyntaxNode>();

            for (var i = 0; i < count; i++)
            {
                var variableDecl = variables[i];
                MakeProperty(fieldDecl, fieldAttribListList, propAttribListList, propertyType, variableDecl, newNodes);
            }

            root = root.InsertNodesAfterThenRemove(fieldDecl, newNodes, SyntaxRemoveOptions.KeepNoTrivia);
            return document.WithSyntaxRoot(root).Project.Solution;
        }

        private void MakeProperty(
              FieldDeclarationSyntax fieldDecl
            , List<List<AttributeSyntax>> fieldAttribListList
            , List<List<AttributeSyntax>> propAttribListList
            , TypeSyntax propertyType
            , VariableDeclaratorSyntax variableDecl
            , List<SyntaxNode> newNodes
        )
        {
            var propName = variableDecl.Identifier.Text.ToPropertyName();
            var propDecl = MakePropertyBody(
                  propName
                , fieldDecl
                , SyntaxFactory.PropertyDeclaration(propertyType ?? fieldDecl.Declaration.Type, propName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    .WithTrailingTrivia(fieldDecl.GetTrailingTrivia())
            );

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

            newNodes.Add(propDecl);
        }

        private static void PrepareAttributeListLists(
              FieldDeclarationSyntax fieldDecl
            , SemanticModel semanticModel
            , List<List<AttributeSyntax>> fieldAttribListList
            , List<List<AttributeSyntax>> propAttribListList
            , HashSet<string> fieldAttribCheck
            , HashSet<string> propAttribCheck
        )
        {
            foreach (var attribList in fieldDecl.AttributeLists)
            {
                var attributes = attribList.Attributes;

                if (attributes.Count < 1)
                {
                    continue;
                }

                var propList = new List<AttributeSyntax>();
                var fieldList = new List<AttributeSyntax>();
                var targetKind = attribList.Target?.Identifier.Kind();

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

                if (targetKind is SyntaxKind.FieldKeyword)
                {
                    foreach (var attrib in attributes)
                    {
                        var (_, _) = GetAttributeInfo(semanticModel, attrib);
                        var name = attrib.ToString();

                        if (name.StartsWith("SerializeField"))
                        {
                            continue;
                        }

                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            fieldAttribCheck.Add(name);
                            fieldList.Add(attrib);
                        }
                    }

                    fieldAttribListList.Add(fieldList);
                    continue;
                }

                foreach (var attrib in attributes)
                {
                    var (_, target) = GetAttributeInfo(semanticModel, attrib);
                    var name = attrib.ToString();

                    if (name.StartsWith("SerializeField"))
                    {
                        continue;
                    }

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
                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            fieldAttribCheck.Add(name);
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
                if (attribTypeSymbol.Name == "SerializeField")
                {
                    return (attribTypeSymbol.Name, AttributeTargets.Field);
                }

                return (string.Empty, 0);
            }

            return (attribTypeSymbol.Name, (AttributeTargets)attributeUsageAttribute.ConstructorArguments[0].Value);
        }

        private static TypeSyntax ExtractTypeFromPropertyTypeAttribute(List<List<AttributeSyntax>> attribListList)
        {
            TypeSyntax result = null;

            for (var iList = attribListList.Count - 1; iList >= 0; iList--)
            {
                var attributes = attribListList[iList];

                for (var iAttrib = attributes.Count - 1; iAttrib >= 0; iAttrib--)
                {
                    var attrib = attributes[iAttrib];
                    var name = attrib.ToString();

                    if (name.StartsWith("PropertyType") == false)
                    {
                        continue;
                    }

                    attributes.RemoveAt(iAttrib);

                    if (attrib.ArgumentList is null)
                    {
                        continue;
                    }

                    var args = attrib.ArgumentList.Arguments;

                    if (args.Count < 1 || args[0].Expression is not TypeOfExpressionSyntax typeOfSyntax)
                    {
                        continue;
                    }

                    result = typeOfSyntax.Type;
                    break;
                }

                if (attributes.Count < 1)
                {
                    attribListList.RemoveAt(iList);
                    continue;
                }

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        private static void AddDataPropertyAttribute(
              FieldDeclarationSyntax fieldDecl
            , List<List<AttributeSyntax>> propAttribListList
            , HashSet<string> propAttribCheck
        )
        {
            if (propAttribCheck.Any(static x => x.StartsWith("DataProperty")))
            {
                return;
            }

            var typeArg = SyntaxFactory.SeparatedList<AttributeArgumentSyntax>().Add(
                SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(fieldDecl.Declaration.Type))
            );

            propAttribListList.Add(new List<AttributeSyntax> {
                SyntaxFactory.Attribute(
                      SyntaxFactory.IdentifierName("DataProperty")
                    , SyntaxFactory.AttributeArgumentList(
                          openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken)
                        , arguments: typeArg
                        , closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                    )
                )
            });
        }
    }
}
