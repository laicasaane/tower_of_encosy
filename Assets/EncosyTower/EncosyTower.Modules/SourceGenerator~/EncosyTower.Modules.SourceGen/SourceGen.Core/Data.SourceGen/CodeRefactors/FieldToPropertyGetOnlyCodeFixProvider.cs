using System.Composition;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Data.SourceGen
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldToPropertyGetOnlyCodeFixProvider)), Shared]
    internal sealed class FieldToPropertyGetOnlyCodeFixProvider : FieldToPropertyCodeFixProviderBase
    {
        protected override string MakeTitle(int fieldVariableCount, StringBuilder fieldVariableBuilder)
        {
            return fieldVariableCount > 1
                ? $"Replace {fieldVariableBuilder} with properties (only getter)"
                : $"Replace {fieldVariableBuilder} with property (only getter)";
        }

        protected override PropertyDeclarationSyntax MakePropertyBody(
              string propName
            , FieldDeclarationSyntax fieldDecl
            , PropertyDeclarationSyntax propDecl
        )
        {
            var arrowExpression = SyntaxFactory.ArrowExpressionClause(
                  arrowToken: SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                , SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"Get_{propName}"))
            );

            return propDecl.WithExpressionBody(arrowExpression)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
