using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Formatting;

namespace EncosyTower.SourceGen.Generators.Data.CodeRefactors
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToFieldCodeFixProvider)), Shared]
    internal class PropertyToFieldCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DataDiagnosticAnalyzer.DIAGNOSTIC_PROPERTY);

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
                .FirstOrDefault();

            if (declaration?.Identifier.Text is null)
            {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                      title: $"Replace '{declaration.Identifier.Text}' with field"
                    , createChangedSolution: c => MakePropertyAsync(context.Document, declaration, c)
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
            var fieldAttribListList = new List<List<AttributeSyntax>>();
            var propAttribListList = new List<List<AttributeSyntax>>();
            var fieldAttribCheck = new HashSet<string>();
            var propAttribCheck = new HashSet<string>();

            TypeSyntax fieldTypeSyntax = null;

            PrepareAttributeListLists(
                propertyDecl
                , semanticModel
                , fieldAttribListList
                , propAttribListList
                , fieldAttribCheck
                , propAttribCheck
                , cancellationToken
                , ref fieldTypeSyntax
            );

            AddPropertyTypeAttribute(propertyDecl, fieldAttribListList, fieldAttribCheck, fieldTypeSyntax);

            var fieldName = propertyDecl.Identifier.Text.ToFieldName();
            var varDecl = SyntaxFactory.VariableDeclaration(fieldTypeSyntax ?? propertyDecl.Type);
            varDecl = varDecl.WithVariables(
                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(fieldName))
            );

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

        private static void PrepareAttributeListLists(
              PropertyDeclarationSyntax propertyDecl
            , SemanticModel semanticModel
            , List<List<AttributeSyntax>> fieldAttribListList
            , List<List<AttributeSyntax>> propAttribListList
            , HashSet<string> fieldAttribCheck
            , HashSet<string> propAttribCheck
            , CancellationToken cancellationToken
            , ref TypeSyntax fieldTypeSyntax
        )
        {
            foreach (var attribList in propertyDecl.AttributeLists)
            {
                var attributes = attribList.Attributes;

                if (attributes.Count < 1)
                {
                    continue;
                }

                var propList = new List<AttributeSyntax>();
                var fieldList = new List<AttributeSyntax>();
                var targetKind = attribList.Target?.Identifier.Kind();

                if (targetKind is SyntaxKind.FieldKeyword)
                {
                    foreach (var attrib in attributes)
                    {
                        var name = attrib.ToString();

                        if (fieldAttribCheck.Contains(name) == false)
                        {
                            fieldAttribCheck.Add(name);
                            fieldList.Add(attrib);
                        }
                    }

                    fieldAttribListList.Add(fieldList);
                    continue;
                }

                if (targetKind is SyntaxKind.PropertyKeyword)
                {
                    foreach (var attrib in attributes)
                    {
                        var name = attrib.ToString();

                        if (name.StartsWith("DataProperty"))
                        {
                            if (attrib.ArgumentList is { Arguments.Count: > 0 } argList
                                && argList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExp
                            )
                            {
                                fieldTypeSyntax = typeOfExp.Type;
                            }
                            continue;
                        }

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

                    if (name.StartsWith("DataProperty"))
                    {
                        if (attrib.ArgumentList is { Arguments.Count: > 0 } argList
                            && argList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExp
                        )
                        {
                            fieldTypeSyntax = typeOfExp.Type;
                        }
                        continue;
                    }

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
                        if (propAttribCheck.Contains(name) == false)
                        {
                            propAttribCheck.Add(name);
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

            if (fieldAttribCheck.Contains("SerializeField") == false
                && fieldAttribCheck.Contains("JsonProperty") == false
                && fieldAttribCheck.Contains("JsonInclude") == false
            )
            {
                var referenceUnityEngine = false;
                var referenceNewtonsoft = false;
                var referenceSystemTextJson = false;
                var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDecl, cancellationToken);

                foreach (var assembly in propertySymbol.ContainingModule.ReferencedAssemblySymbols)
                {
                    var assemblyName = assembly.ToDisplayString();

                    if (assemblyName.StartsWith("UnityEngine,"))
                    {
                        referenceUnityEngine = true;
                        continue;
                    }

                    if (assemblyName.StartsWith("Newtonsoft.Json,"))
                    {
                        referenceNewtonsoft = true;
                        continue;
                    }

                    if (assemblyName.StartsWith("System.Text.Json,"))
                    {
                        referenceSystemTextJson = true;
                        continue;
                    }
                }

                if (referenceUnityEngine)
                {
                    fieldAttribListList.Add(new List<AttributeSyntax> {
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("SerializeField"))
                    });
                }
                else if (referenceNewtonsoft)
                {
                    fieldAttribListList.Add(new List<AttributeSyntax> {
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("JsonProperty"))
                    });
                }
                else if (referenceSystemTextJson)
                {
                    fieldAttribListList.Add(new List<AttributeSyntax> {
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("JsonInclude"))
                    });
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

        private static void AddPropertyTypeAttribute(
              PropertyDeclarationSyntax propertyDecl
            , List<List<AttributeSyntax>> fieldAttribListList
            , HashSet<string> fieldAttribCheck
            , TypeSyntax fieldTypeSyntax
        )
        {
            if (fieldAttribCheck.Any(static x => x.StartsWith("PropertyType"))
                || fieldTypeSyntax == null
                || propertyDecl.Type.IsEquivalentTo(fieldTypeSyntax)
            )
            {
                return;
            }

            var typeArg = SyntaxFactory.SeparatedList<AttributeArgumentSyntax>().Add(
                SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(propertyDecl.Type))
            );

            fieldAttribListList.Add(new List<AttributeSyntax> {
                SyntaxFactory.Attribute(
                      SyntaxFactory.IdentifierName("PropertyType")
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
