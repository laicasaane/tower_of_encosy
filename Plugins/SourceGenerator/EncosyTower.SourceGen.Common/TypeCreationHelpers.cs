// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    public static class TypeCreationHelpers
    {
        public const string NEWLINE = "\n";

        public static SourceText GenerateSourceText(
              string generatedSourceFilePath
            , string openingSource
            , string bodySource
            , string closingSource
            , Printer? overridePrinter = default
        )
        {
            // DO NOT worry about #if directives
            // Because source generators run after preprocessors,
            // every disabled code will be removed from the compilation context.
            // So there might be no generated code to worry about.

            var printer = overridePrinter ?? Printer.DefaultLarge;

            printer.PrintLine(openingSource);
            printer.PrintLine(bodySource);
            printer.PrintLine(closingSource);

            // Output as source
            return SourceText.From(printer.Result, Encoding.UTF8)
                .WithIgnoreUnassignedVariableWarning()
                .WithInitialLineDirectiveToGeneratedSource(generatedSourceFilePath)
                ;
        }

        public static void GenerateOpeningAndClosingSource(
              SyntaxNode containingSyntax
            , CancellationToken token
            , out string openingSource
            , out string closingSource
            , Printer? overridePrinter = default
            , PrinterAction printAdditionalUsings = default
        )
        {
            var printer = overridePrinter ?? Printer.DefaultLarge;

            var result = WriteOpeningSyntax_AndReturnClosingSyntax(
                  ref printer
                , containingSyntax
                , token
                , printAdditionalUsings
            );

            openingSource = printer.Result;

            printer.ClearAndIndent(printer.IndentDepth);

            var numClosingBraces = result.NumClosingBraces;

            if (numClosingBraces > 0)
            {
                printer.PrintEndLine();
            }

            for (int i = 0; i < numClosingBraces; i++)
            {
                token.ThrowIfCancellationRequested();

                printer = printer.DecreasedIndent();
                printer.PrintLine("}");
            }

            closingSource = printer.Result;
        }

        private static ClosingSyntax WriteOpeningSyntax_AndReturnClosingSyntax(
              ref Printer printer
            , SyntaxNode containingTypeSyntax
            , CancellationToken token
            , PrinterAction printAdditionUsings
        )
        {
            token.ThrowIfCancellationRequested();

            var (openingSyntaxes, numClosingBraces) = GetOpeningSyntaxes(containingTypeSyntax, token);

            var uniqueUsings = new HashSet<string>();
            var usings = SyntaxFactory.List<UsingDirectiveSyntax>();

            GetUsings(containingTypeSyntax?.SyntaxTree, uniqueUsings, ref usings, token);

            printer.PrintEndLine();

            foreach (var @using in usings)
            {
                token.ThrowIfCancellationRequested();
                printer.PrintLine(@using.ToString());
            }

            printAdditionUsings?.Invoke(ref printer);

            if (usings.Count > 0)
            {
                printer.PrintEndLine();
            }

            foreach (var (openingValue, addIndentAfter) in openingSyntaxes)
            {
                token.ThrowIfCancellationRequested();

                printer.PrintLine(openingValue);

                if (addIndentAfter)
                {
                    printer = printer.IncreasedIndent();
                }
            }

            if (openingSyntaxes.Count > 0)
            {
                printer.PrintEndLine();
            }

            return new(numClosingBraces);

            static void GetUsings(
                  SyntaxTree syntaxTree
                , HashSet<string> uniqueUsings
                , ref SyntaxList<UsingDirectiveSyntax> usings
                , CancellationToken token
            )
            {
                if (syntaxTree == null)
                {
                    return;
                }

                var currentUsings = syntaxTree.GetCompilationUnitRoot(token).Usings;

                foreach (var @using in currentUsings)
                {
                    token.ThrowIfCancellationRequested();

                    if (uniqueUsings.Add(@using.Name.ToString()))
                    {
                        usings = usings.Add(@using);
                    }
                }
            }
        }

        private static OpeningSyntaxes GetOpeningSyntaxes(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var opening = new Stack<OpeningSyntax>();
            var numBracesToClose = 0;
            var parentSyntax = node?.Parent;

            while (parentSyntax != null)
            {
                token.ThrowIfCancellationRequested();

                switch (parentSyntax)
                {
                    case RecordDeclarationSyntax recordSyntax:
                    {
                        // e.g. class/struct
                        var keyword = recordSyntax.ClassOrStructKeyword.ValueText;

                        // e.g. Outer/Generic<T>
                        var typeName = recordSyntax.Identifier.ToString() + recordSyntax.TypeParameterList;

                        // e.g. where T: new()
                        var constraint = recordSyntax.ConstraintClauses.ToString();

                        opening.Push(new("{", AddIndentAfter: true));
                        opening.Push(new($"partial record {keyword} {typeName} {constraint}", AddIndentAfter: false));
                        numBracesToClose++;
                        break;
                    }

                    case TypeDeclarationSyntax typeSyntax:
                    {
                        // e.g. class/struct
                        var keyword = typeSyntax.Keyword.ValueText;

                        // e.g. Outer/Generic<T>
                        var typeName = typeSyntax.Identifier.ToString() + typeSyntax.TypeParameterList;

                        // e.g. where T: new()
                        var constraint = typeSyntax.ConstraintClauses.ToString();

                        opening.Push(new("{", AddIndentAfter: true));
                        opening.Push(new($"partial {keyword} {typeName} {constraint}", AddIndentAfter: false));
                        numBracesToClose++;
                        break;
                    }

                    case BaseNamespaceDeclarationSyntax namespaceSyntax:
                    {
                        foreach (var usingDir in namespaceSyntax.Usings)
                        {
                            opening.Push(new($"{usingDir}", AddIndentAfter: false));
                        }

                        opening.Push(new("{", AddIndentAfter: true));
                        opening.Push(new($"namespace {namespaceSyntax.Name}", AddIndentAfter: false));
                        numBracesToClose++;
                        break;
                    }
                }

                parentSyntax = parentSyntax.Parent;
            }

            return new(opening, numBracesToClose);
        }

        private record struct ClosingSyntax(int NumClosingBraces);

        private record struct OpeningSyntax(string Value, bool AddIndentAfter);

        private record struct OpeningSyntaxes(Stack<OpeningSyntax> Syntaxes, int NumClosingBraces);
    }
}
