// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.Modules.SourceGen
{
    public static class TypeCreationHelpers
    {
        /// <summary>
        /// Line to replace with on generated source.
        /// </summary>
        public const string GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE = "// __generatedline__";

        public const string NEWLINE = "\n";

        /// <summary>
        /// Generates the SourceText from a given list of root nodes using the SyntaxTree's path.
        /// Includes the existing SyntaxTree's root using statements,
        /// and adds correct line directives.
        /// </summary>
        /// <param name="generatorName">Name to base filepath on.</param>
        /// <param name="generatorExecutionContext">Context of the generator executed.</param>
        /// <param name="originalSyntaxTree">Original SyntaxTree to base SourceText location and usings on.</param>
        /// <param name="rootNodes">Root nodes to add to compilation unit.</param>
        /// <returns>The SourceText based on nodes and SyntaxTree filepath.</returns>
        public static SourceText GenerateSourceTextForRootNodes(
              string generatorName
            , GeneratorExecutionContext generatorExecutionContext
            , SyntaxTree originalSyntaxTree
            , IEnumerable<MemberDeclarationSyntax> rootNodes
        )
        {
            // Create compilation unit
            var existingUsings = originalSyntaxTree
                .GetCompilationUnitRoot(generatorExecutionContext.CancellationToken)
                .WithoutPreprocessorTrivia().Usings;

            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(rootNodes.ToArray())
                .WithoutPreprocessorTrivia()
                .WithUsings(existingUsings)
                .NormalizeWhitespace(eol:NEWLINE);

            var generatedSourceFilePath = originalSyntaxTree
                .GetGeneratedSourceFilePath(generatorExecutionContext.Compilation.Assembly.Name, generatorName)
                .Replace('\\', '/');

            // Output as source
            var sourceTextForNewClass = compilationUnit.GetText(Encoding.UTF8)
                .WithInitialLineDirectiveToGeneratedSource(generatedSourceFilePath)
                .WithIgnoreUnassignedVariableWarning();

            // Add line directives for lines with `GeneratedLineTriviaToGeneratedSource` or #line
            var textChanges = new List<TextChange>();

            foreach (var line in sourceTextForNewClass.Lines)
            {
                var lineText = line.ToString();

                if (lineText.Contains(GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE))
                {
                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Replace(
                              GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE
                            , $"#line {line.LineNumber + 2} \"{generatedSourceFilePath}\""
                        )
                    ));
                }
                else if (lineText.Contains("#line") && lineText.TrimStart().IndexOf("#line", StringComparison.Ordinal) != 0)
                {
                    var indexOfLineDirective = lineText.IndexOf("#line", StringComparison.Ordinal);
                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Substring(0, indexOfLineDirective - 1)
                            + NEWLINE
                            + lineText.Substring(indexOfLineDirective)
                    ));
                }
            }

            return sourceTextForNewClass.WithChanges(textChanges);
        }

        /// <summary>
        /// Get root nodes of a replaced SyntaxTree.
        /// Where all the original nodes in dictionary is replaced with new nodes.
        /// </summary>
        /// <param name="syntaxTree">SyntaxTree to look through.</param>
        /// <param name="originalToReplaced">Dictionary containing keys of original nodes, and values of replacements.</param>
        /// <returns>Root nodes of replaced SyntaxTrees.</returns>
        public static List<MemberDeclarationSyntax> GetReplacedRootNodes(
              SyntaxTree syntaxTree
            , IDictionary<TypeDeclarationSyntax, TypeDeclarationSyntax> originalToReplaced
        )
        {
            var newRootNodes = new List<MemberDeclarationSyntax>();
            var allOriginalNodesAlsoInReplacedTree = originalToReplaced.Keys
                .SelectMany(node => node.AncestorsAndSelf())
                .ToImmutableHashSet();

            foreach (var childNode in syntaxTree.GetRoot().ChildNodes())
            {
                switch (childNode)
                {
                    case NamespaceDeclarationSyntax _:
                    case ClassDeclarationSyntax _:
                    case StructDeclarationSyntax _:
                    {
                        var newRootNode = ConstructReplacedTree(childNode, originalToReplaced, allOriginalNodesAlsoInReplacedTree);
                        if (newRootNode != null)
                            newRootNodes.Add(newRootNode);
                        break;
                    }
                }
            }

            return newRootNodes;
        }

        /// <summary>
        /// Constructs a replaced tree based on a root note.
        /// Uses originalToReplacedNode to replace.
        /// Filtered based on replacementNodeCandidates.
        /// </summary>
        /// <param name="currentNode">Root to replace downwards from.</param>
        /// <param name="originalToReplacedNode">Dictionary containing keys of original nodes, and values of replacements.</param>
        /// <param name="replacementNodeCandidates">A list of nodes to look through. (ie. only these nodes will be replaced.)</param>
        /// <returns>Top member of replaced tree.</returns>
        /// <exception cref="InvalidOperationException">
        /// Happens if currentNode is not a class, namespace or struct. (and is contained in replacementNodeCandidates.)
        /// </exception>
        /// <remarks> Uses Downwards Recursion. </remarks>
        static MemberDeclarationSyntax ConstructReplacedTree(SyntaxNode currentNode,
            IDictionary<TypeDeclarationSyntax, TypeDeclarationSyntax> originalToReplacedNode,
            ImmutableHashSet<SyntaxNode> replacementNodeCandidates)
        {
            // If this node shouldn't exist in replaced tree, early out
            if (!replacementNodeCandidates.Contains(currentNode))
                return null;

            // Otherwise, check for replaced children by recursing
            var replacedChildren =
                currentNode
                    .ChildNodes()
                    .Select(childNode => ConstructReplacedTree(childNode, originalToReplacedNode, replacementNodeCandidates))
                    .Where(child => child != null).ToArray();

            // Either get the replaced node for this level - or create one - and add the replaced children
            // No node found, need to create a new one to represent this node in the hierarchy
            return currentNode switch {
                NamespaceDeclarationSyntax namespaceNode =>
                    SyntaxFactory.NamespaceDeclaration(namespaceNode.Name)
                        .AddMembers(replacedChildren)
                        .WithModifiers(namespaceNode.Modifiers)
                        .WithUsings(namespaceNode.Usings),

                ClassDeclarationSyntax classNode =>
                    SyntaxFactory.ClassDeclaration(classNode.Identifier)
                        .AddMembers(replacedChildren)
                        .WithBaseList(classNode.BaseList)
                        .WithModifiers(classNode.Modifiers),

                StructDeclarationSyntax structNode =>
                    SyntaxFactory.StructDeclaration(structNode.Identifier)
                        .AddMembers(replacedChildren)
                        .WithBaseList(structNode.BaseList)
                        .WithModifiers(structNode.Modifiers),

                RecordDeclarationSyntax recordNode =>
                    SyntaxFactory.RecordDeclaration(recordNode.ClassOrStructKeyword, recordNode.Identifier)
                        .AddMembers(replacedChildren)
                        .WithBaseList(recordNode.BaseList)
                        .WithModifiers(recordNode.Modifiers),

                InterfaceDeclarationSyntax interfaceNode =>
                    SyntaxFactory.InterfaceDeclaration(interfaceNode.Identifier)
                        .AddMembers(replacedChildren)
                        .WithBaseList(interfaceNode.BaseList)
                        .WithModifiers(interfaceNode.Modifiers),

                TypeDeclarationSyntax typeNode when originalToReplacedNode.ContainsKey(typeNode) =>
                    originalToReplacedNode[typeNode]?.AddMembers(replacedChildren),

                _ => throw new InvalidOperationException(
                    $"Expecting class or namespace declaration in syntax tree for {currentNode} but found {currentNode.Kind()}"
                )
            };
        }

        public static SourceText GenerateSourceTextForRootNodes(
              string generatedSourceFilePath
            , SyntaxNode containingSyntax
            , SyntaxNode originalSyntax
            , string generatedSyntax
            , CancellationToken cancellationToken
        )
        {
            var printer = Printer.DefaultLarge;
            var result = WriteOpeningSyntax_AndReturnClosingSyntax(
                  ref printer
                , containingSyntax
                , originalSyntax
                , cancellationToken
            );

            printer.PrintLine(generatedSyntax);

            var (numClosingBracesRequired, numNotClosedIfDirectives) = result;

            if (numClosingBracesRequired > 0)
            {
                printer.PrintEndLine();
            }

            for (int i = 0; i < numClosingBracesRequired; i++)
            {
                printer = printer.DecreasedIndent();
                printer.PrintLine("}");
            }

            if (numNotClosedIfDirectives > 0)
            {
                printer.PrintEndLine();
            }

            for (int i = 0; i < numNotClosedIfDirectives; i++)
            {
                printer.PrintLine("#endif");
            }

            // Output as source
            var sourceTextForNewClass = SourceText.From(printer.Result, Encoding.UTF8)
                .WithInitialLineDirectiveToGeneratedSource(generatedSourceFilePath)
                .WithIgnoreUnassignedVariableWarning();

            // Add line directives for lines with `GeneratedLineTriviaToGeneratedSource` or #line
            var textChanges = new List<TextChange>();

            foreach (var line in sourceTextForNewClass.Lines)
            {
                var lineText = line.ToString();

                if (lineText.Contains(GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE))
                {
                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Replace(
                              GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE
                            , $"#line {line.LineNumber + 2} \"{generatedSourceFilePath}\""
                        )
                    ));

                    continue;
                }

                if (lineText.Contains("#line")
                    && lineText.TrimStart().IndexOf("#line", StringComparison.Ordinal) != 0
                )
                {
                    var indexOfLineDirective = lineText.IndexOf("#line", StringComparison.Ordinal);

                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Substring(0, indexOfLineDirective - 1)
                            + NEWLINE
                            + lineText.Substring(indexOfLineDirective)
                    ));

                    continue;
                }
            }

            return sourceTextForNewClass.WithChanges(textChanges);
        }

        private static ClosingSyntax WriteOpeningSyntax_AndReturnClosingSyntax(
              ref Printer printer
            , SyntaxNode containingTypeSyntax
            , SyntaxNode originalSyntax
            , CancellationToken cancellationToken
        )
        {
            var (openingSyntaxes, numClosingBraces) = GetOpeningSyntaxes(containingTypeSyntax);

            var directives = new Stack<SyntaxTrivia>();
            var numNotClosedDirectives = 0;

            GetDirectivesFromSelfAndParents(
                  originalSyntax as MemberDeclarationSyntax
                , directives
                , ref numNotClosedDirectives
            );

            var uniqueUsings = new HashSet<string>();
            var usings = SyntaxFactory.List<UsingDirectiveSyntax>();

            GetUsings(containingTypeSyntax?.SyntaxTree, uniqueUsings, ref usings, cancellationToken);

            foreach (var @using in usings)
            {
                GetDirectivesFromSelf(@using, directives, ref numNotClosedDirectives);
            }

            printer.PrintEndLine();

            var firstDirective = true;

            foreach (var directive in directives)
            {
                if (firstDirective && directive.IsKind(SyntaxKind.IfDirectiveTrivia) == false)
                {
                    continue;
                }

                firstDirective = false;

                printer.PrintLine(directive.ToString());
            }

            if (directives.Count > 0)
            {
                printer.PrintEndLine();
            }

            foreach (var @using in usings)
            {
                printer.PrintLine(@using.ToString());
            }

            if (usings.Count > 0)
            {
                printer.PrintEndLine();
            }

            foreach (var (openingValue, addIndentAfter) in openingSyntaxes)
            {
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

            return new(numClosingBraces, numNotClosedDirectives);

            static void GetUsings(
                  SyntaxTree syntaxTree
                , HashSet<string> uniqueUsings
                , ref SyntaxList<UsingDirectiveSyntax> usings
                , CancellationToken cancellationToken
            )
            {
                if (syntaxTree == null)
                {
                    return;
                }

                var currentUsings = syntaxTree.GetCompilationUnitRoot(cancellationToken).Usings;

                foreach (var @using in currentUsings)
                {
                    if (uniqueUsings.Add(@using.Name.ToString()))
                    {
                        usings = usings.Add(@using);
                    }
                }
            }
        }

        private static OpeningSyntaxes GetOpeningSyntaxes(SyntaxNode node)
        {
            var opening = new Stack<OpeningSyntax>();
            var numBracesToClose = 0;
            var parentSyntax = node?.Parent;

            while (parentSyntax != null)
            {
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

                    case NamespaceDeclarationSyntax namespaceSyntax:
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

        private static void GetDirectivesFromSelfAndParents(
              MemberDeclarationSyntax originalSyntax
            , Stack<SyntaxTrivia> directives
            , ref int numNotClosedDirectives
        )
        {
            GetDirectivesFromSelf(originalSyntax, directives, ref numNotClosedDirectives);

            var parentSyntax = originalSyntax?.Parent;

            while (parentSyntax != null)
            {
                switch (parentSyntax)
                {
                    case NamespaceDeclarationSyntax namespaceSyntax:
                    {
                        GetDirectives(namespaceSyntax.NamespaceKeyword, directives, ref numNotClosedDirectives);
                        GetDirectives(namespaceSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case BaseNamespaceDeclarationSyntax namespaceSyntax:
                    {
                        GetDirectives(namespaceSyntax.NamespaceKeyword, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case RecordDeclarationSyntax recordSyntax:
                    {
                        GetDirectives(recordSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        GetDirectives(recordSyntax.Keyword, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case ClassDeclarationSyntax classSyntax:
                    {
                        GetDirectives(classSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        GetDirectives(classSyntax.Keyword, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case StructDeclarationSyntax structSyntax:
                    {
                        GetDirectives(structSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        GetDirectives(structSyntax.Keyword, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case InterfaceDeclarationSyntax interfaceSyntax:
                    {
                        GetDirectives(interfaceSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        GetDirectives(interfaceSyntax.Keyword, directives, ref numNotClosedDirectives);
                        break;
                    }

                    case BaseTypeDeclarationSyntax typeSyntax:
                    {
                        GetDirectives(typeSyntax.OpenBraceToken, directives, ref numNotClosedDirectives);
                        break;
                    }
                }

                GetDirectivesFromSelf(parentSyntax as MemberDeclarationSyntax, directives, ref numNotClosedDirectives);
                parentSyntax = parentSyntax.Parent;
            }
        }

        private static void GetDirectivesFromSelf(
              UsingDirectiveSyntax node
            , Stack<SyntaxTrivia> directives
            , ref int numNotClosedDirectives
        )
        {
            if (node.ContainsDirectives == false)
            {
                return;
            }

            var tokens = node.ChildTokens();

            if (tokens == null)
            {
                return;
            }

            foreach (var token in tokens)
            {
                GetDirectives(token, directives, ref numNotClosedDirectives);
            }
        }

        private static void GetDirectivesFromSelf(
              MemberDeclarationSyntax node
            , Stack<SyntaxTrivia> directives
            , ref int numNotClosedDirectives
        )
        {
            if (node == null)
            {
                return;
            }

            var tokens = node.Modifiers;

            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                GetDirectives(tokens[i], directives, ref numNotClosedDirectives);
            }

            var attribLists = node.AttributeLists;

            for (var i = attribLists.Count - 1; i >= 0; i--)
            {
                var token = attribLists[i].OpenBracketToken;
                GetDirectives(token, directives, ref numNotClosedDirectives);
            }
        }

        private static void GetDirectives(
              SyntaxToken token
            , Stack<SyntaxTrivia> directives
            , ref int numNotClosedDirectives
        )
        {
            var list = token.LeadingTrivia;

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var trivia = list[i];

                if (trivia.IsDirective == false)
                {
                    continue;
                }

                directives.Push(trivia);

                if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
                    || trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
                )
                {
                    numNotClosedDirectives++;
                }
                else if (trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                {
                    numNotClosedDirectives--;
                }
            }
        }

        private record struct ClosingSyntax(int NumClosingBraces, int NumNotClosedDirectives);

        private record struct OpeningSyntax(string Value, bool AddIndentAfter);

        private record struct OpeningSyntaxes(Stack<OpeningSyntax> Syntaxes, int NumClosingBraces);
    }
}
