using System.Composition;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Data.SourceGen
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldToPropertyGetInitCodeFixProvider)), Shared]
    internal sealed class FieldToPropertyGetInitCodeFixProvider : FieldToPropertyCodeFixProviderBase
    {
        protected override string MakeTitle(int fieldVariableCount, StringBuilder fieldVariableBuilder)
        {
            return fieldVariableCount > 1
                ? $"Replace {fieldVariableBuilder} with properties (getter and init-setter)"
                : $"Replace {fieldVariableBuilder} with property (getter and init-setter)";
        }

        protected override PropertyDeclarationSyntax MakePropertyBody(
              string propName
            , FieldDeclarationSyntax fieldDecl
            , PropertyDeclarationSyntax propDecl
        )
        {
            var getDecl = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                      arrowToken: SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                    , SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"Get_{propName}"))
                ))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            var initDecl = SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                      arrowToken: SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                    , SyntaxFactory.InvocationExpression(
                          SyntaxFactory.IdentifierName($"Set_{propName}")
                        , SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                        ))
                    )
                ))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            return propDecl.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { getDecl, initDecl })))
                .WithTrailingTrivia(fieldDecl.GetTrailingTrivia());
        }
    }
}
