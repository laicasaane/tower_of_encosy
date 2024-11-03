using System;
using System.Collections.Immutable;
using System.Text;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    public class InterfaceRef
    {
        public InterfaceDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string FullName { get; }

        public bool Verbose { get; }

        public ImmutableArray<ISymbol> Members { get; private set; }

        public string FullContainingNameWithDot { get; private set; }

        public string AccessKeyword { get; private set; }

        public string StructName { get; private set; }

        public InterfaceRef(
              InterfaceDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , bool verbose
        )
        {
            this.Syntax = syntax;
            this.Symbol = symbol;
            this.Verbose = verbose;
            this.FullName = symbol.ToFullName();

            InitFullContainingName();
            InitAccessKeyword();
            InitStructName();
            InitMembers();
        }

        private void InitFullContainingName()
        {
            var sb = new StringBuilder(Symbol.ToFullName());
            var name = Symbol.Name.AsSpan();
            var startIndex = sb.Length - name.Length;

            sb.Remove(startIndex, name.Length);

            FullContainingNameWithDot = sb.ToString();
        }

        private void InitAccessKeyword()
        {
            switch (Symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    AccessKeyword = "private";
                    return;

                case Accessibility.ProtectedAndInternal:
                    AccessKeyword = "private protected";
                    return;

                case Accessibility.Protected:
                    AccessKeyword = "protected";
                    return;

                case Accessibility.Internal:
                    AccessKeyword = "internal";
                    return;

                case Accessibility.ProtectedOrInternal:
                    AccessKeyword = "protected internal";
                    return;

                default:
                    AccessKeyword = "public";
                    return;
            }
        }

        private void InitStructName()
        {
            var nameSpan = Symbol.Name.AsSpan();

            if (nameSpan.Length < 1)
            {
                StructName = "__PolymorphicStruct__";
                return;
            }

            if (nameSpan[0] == 'I'
                && nameSpan.Length > 1
                && char.IsUpper(nameSpan[1])
            )
            {
                StructName = $"{nameSpan.Slice(1).ToString()}";
                return;
            }

            StructName = $"{Symbol.Name}";
        }

        private void InitMembers()
        {
            var memberArrayBuilder = ImmutableArrayBuilder<ISymbol>.Rent();

            GetMembers(Symbol, ref memberArrayBuilder);

            foreach (var symbol in Symbol.AllInterfaces)
            {
                GetMembers(symbol, ref memberArrayBuilder);
            }

            Members = memberArrayBuilder.ToImmutable();
            memberArrayBuilder.Dispose();

            static void GetMembers(
                  INamedTypeSymbol symbol
                , ref ImmutableArrayBuilder<ISymbol> memberArrayBuilder
            )
            {
                foreach (var member in symbol.GetMembers())
                {
                    if (member is IMethodSymbol method
                        && method.MethodKind is (MethodKind.PropertyGet or MethodKind.PropertySet)
                    )
                    {
                        continue;
                    }

                    memberArrayBuilder.Add(member);
                }
            }
        }
    }
}
