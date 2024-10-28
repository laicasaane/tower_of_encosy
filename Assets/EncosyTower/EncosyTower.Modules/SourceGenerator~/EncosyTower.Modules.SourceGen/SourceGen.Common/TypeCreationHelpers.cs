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

        static (string start, StringBuilder end) GetBaseDeclaration(SyntaxNode typeSyntax)
        {
            var builderStart = "";
            var builderEnd = new StringBuilder();
            var curliesToClose = 0;
            var parentSyntax = typeSyntax.Parent as MemberDeclarationSyntax;

            while (parentSyntax != null && (
                   parentSyntax.IsKind(SyntaxKind.RecordDeclaration)
                || parentSyntax.IsKind(SyntaxKind.RecordStructDeclaration)
                || parentSyntax.IsKind(SyntaxKind.StructDeclaration)
                || parentSyntax.IsKind(SyntaxKind.ClassDeclaration)
                || parentSyntax.IsKind(SyntaxKind.InterfaceDeclaration)
                || parentSyntax.IsKind(SyntaxKind.NamespaceDeclaration)
            ))
            {
                switch (parentSyntax)
                {
                    case RecordDeclarationSyntax parentRecordSyntax:
                    {
                        var keyword = parentRecordSyntax.ClassOrStructKeyword.ValueText; // e.g. class/struct
                        var typeName = parentRecordSyntax.Identifier.ToString() + parentRecordSyntax.TypeParameterList; // e.g. Outer/Generic<T>
                        var constraint = parentRecordSyntax.ConstraintClauses.ToString(); // e.g. where T: new()
                        builderStart = $"partial record {keyword} {typeName} {constraint} {{{NEWLINE}{builderStart}";
                        break;
                    }

                    case TypeDeclarationSyntax parentTypeSyntax:
                    {
                        var keyword = parentTypeSyntax.Keyword.ValueText; // e.g. class/struct/interface
                        var typeName = parentTypeSyntax.Identifier.ToString() + parentTypeSyntax.TypeParameterList; // e.g. Outer/Generic<T>
                        var constraint = parentTypeSyntax.ConstraintClauses.ToString(); // e.g. where T: new()
                        builderStart = $"partial {keyword} {typeName} {constraint} {{{NEWLINE}{builderStart}";
                        break;
                    }

                    case NamespaceDeclarationSyntax parentNameSpaceSyntax:
                    {
                        var usings = parentNameSpaceSyntax.Usings.ToString();

                        if (string.IsNullOrWhiteSpace(usings) == false)
                        {
                            builderStart = $"{usings}{NEWLINE}{NEWLINE}{builderStart}";
                        }

                        builderStart = $"namespace {parentNameSpaceSyntax.Name} {{{NEWLINE}{NEWLINE}{builderStart}";
                        break;
                    }
                }

                curliesToClose++;
                parentSyntax = parentSyntax.Parent as MemberDeclarationSyntax;
            }

            builderEnd.AppendLine();
            builderEnd.Append('}', curliesToClose);

            return (builderStart, builderEnd);
        }

        public static SourceText GenerateSourceTextForRootNodes(
              string generatedSourceFilePath
            , SyntaxNode originalSyntax
            , string generatedSyntax
            , CancellationToken cancellationToken
        )
        {
            var syntaxTreeSourceBuilder = new StringWriter(new StringBuilder());
            var (start, end) = GetBaseDeclaration(originalSyntax);
            var usings = originalSyntax.SyntaxTree.GetCompilationUnitRoot(cancellationToken).Usings;

            foreach (var @using in usings)
            {
                if (@using.ContainsDirectives)
                {
                    var numberOfNotClosedIfDirectives = 0;
                    foreach (var token in @using.ChildTokens())
                        foreach (var trivia in token.LeadingTrivia)
                            if (trivia.IsDirective)
                            {
                                if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia))
                                    numberOfNotClosedIfDirectives++;
                                else if (trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                                    numberOfNotClosedIfDirectives--;
                            }
                    end.Insert(0, NEWLINE + "#endif", numberOfNotClosedIfDirectives);
                }
            }

            syntaxTreeSourceBuilder.Write(usings.ToFullString());
            syntaxTreeSourceBuilder.Write(NEWLINE);
            syntaxTreeSourceBuilder.Write(start);
            syntaxTreeSourceBuilder.Write(generatedSyntax);
            syntaxTreeSourceBuilder.Write(NEWLINE);
            syntaxTreeSourceBuilder.Write(end.Replace("\r\n", NEWLINE).ToString());
            syntaxTreeSourceBuilder.Flush();

            // Output as source
            var sourceTextForNewClass = SourceText.From(syntaxTreeSourceBuilder.ToString(), Encoding.UTF8)
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
    }
}
