using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Module.Core.Mvvm.MonoBindingContextSourceGen
{
    internal partial class MonoBindingContextDeclaration
    {
        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public MonoBindingContextDeclaration(
              ClassDeclarationSyntax candidate
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            Syntax = candidate;
            Symbol = semanticModel.GetDeclaredSymbol(candidate, token);
        }
    }
}
