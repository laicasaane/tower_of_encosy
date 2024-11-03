using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EncosyTower.Modules.SourceGen
{
    public static class SyntaxNodeExt
    {
        public static IEnumerable<MemberDeclarationSyntax> GetContainingTypesAndNamespacesFromMostToLeastNested(
            this SyntaxNode syntaxNode
        )
        {
            SyntaxNode current = syntaxNode;

            while (current.Parent != null && (
                current.Parent is NamespaceDeclarationSyntax
                || current.Parent is ClassDeclarationSyntax
                || current.Parent is StructDeclarationSyntax
            ))
            {
                yield return current.Parent as MemberDeclarationSyntax;
                current = current.Parent;
            }
        }

        public static bool IsReadOnly(this ParameterSyntax parameter)
            => parameter.Modifiers.Any(mod => mod.IsKind(SyntaxKind.InKeyword));

        public static bool IsReadOnly(this IParameterSymbol parameter)
            => parameter.RefKind == RefKind.In;

        public static IEnumerable<NamespaceDeclarationSyntax> GetNamespacesFromMostToLeastNested(this SyntaxNode syntaxNode)
        {
            SyntaxNode current = syntaxNode;

            while (current.Parent != null && current.Parent is NamespaceDeclarationSyntax nds)
            {
                yield return nds;
                current = current.Parent;
            }
        }

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, SyntaxNode node)
            => GetGeneratedSourceFileName(syntaxTree, generatorName, node.GetLocation().GetLineSpan().StartLinePosition.Line);

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, int salting = 0)
        {
            var (isSuccess, fileName) = TryGetFileNameWithoutExtension(syntaxTree);
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;

            var postfix = generatorName.Length > 0 ? $"__{generatorName}" : string.Empty;

            if (isSuccess)
                fileName = $"{fileName}{postfix}_{stableHashCode}{salting}.g.cs";
            else
                fileName = Path.Combine($"{Path.GetRandomFileName()}{postfix}", ".g.cs");

            return fileName;
        }

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, SyntaxNode node, string typeName)
            => GetGeneratedSourceFileName(syntaxTree, generatorName, node.GetLocation().GetLineSpan().StartLinePosition.Line, typeName);

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, int salting, string typeName)
        {
            var (isSuccess, fileName) = TryGetFileNameWithoutExtension(syntaxTree);
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;

            var postfix = generatorName.Length > 0 ? $"__{generatorName}" : string.Empty;

            if (string.IsNullOrWhiteSpace(typeName) == false)
            {
                postfix = $"__{typeName}{postfix}";
            }

            if (isSuccess)
                fileName = $"{fileName}{postfix}_{stableHashCode}{salting}.g.cs";
            else
                fileName = Path.Combine($"{Path.GetRandomFileName()}{postfix}", ".g.cs");

            return fileName;
        }

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, string fileName, SyntaxNode node)
            => GetGeneratedSourceFileName(syntaxTree, generatorName, fileName, node.GetLocation().GetLineSpan().StartLinePosition.Line);

        public static string GetGeneratedSourceFileName(this SyntaxTree syntaxTree, string generatorName, string fileName, int salting = 0)
        {
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;
            var postfix = generatorName.Length > 0 ? $"__{generatorName}" : string.Empty;

            return $"{fileName}{postfix}_{stableHashCode}{salting}.g.cs";
        }

        public static string GetGeneratedSourceFilePath(this SyntaxTree syntaxTree, string assemblyName, string generatorName)
        {
            var fileName = GetGeneratedSourceFileName(syntaxTree, generatorName);
            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var saveToDirectory = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(saveToDirectory);
                return saveToDirectory + fileName;
            }
            return $"Temp/GeneratedCode/{assemblyName}";
        }

        public static (bool IsSuccess, string FileName) TryGetFileNameWithoutExtension(this SyntaxTree syntaxTree)
        {
            var fileName = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
            return (IsSuccess: true, fileName);
        }

        private class PreprocessorTriviaRemover : CSharpSyntaxRewriter
        {
            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                return trivia.Kind() switch {
                    SyntaxKind.DisabledTextTrivia
                 or SyntaxKind.PreprocessingMessageTrivia
                 or SyntaxKind.IfDirectiveTrivia
                 or SyntaxKind.ElifDirectiveTrivia
                 or SyntaxKind.ElseDirectiveTrivia
                 or SyntaxKind.EndIfDirectiveTrivia
                 or SyntaxKind.RegionDirectiveTrivia
                 or SyntaxKind.EndRegionDirectiveTrivia
                 or SyntaxKind.DefineDirectiveTrivia
                 or SyntaxKind.UndefDirectiveTrivia
                 or SyntaxKind.ErrorDirectiveTrivia
                 or SyntaxKind.WarningDirectiveTrivia
                 or SyntaxKind.PragmaWarningDirectiveTrivia
                 or SyntaxKind.PragmaChecksumDirectiveTrivia
                 or SyntaxKind.ReferenceDirectiveTrivia
                 or SyntaxKind.BadDirectiveTrivia
                     => default,

                    _ => trivia,
                };
            }
        }

        public static T WithoutPreprocessorTrivia<T>(this T node)
            where T : SyntaxNode
        {
            var preprocessorTriviaRemover = new PreprocessorTriviaRemover();
            return (T)preprocessorTriviaRemover.Visit(node);
        }

        // Lambda body can exist as block, statement or just expression (create BodyBlock in the last two cases)
        public static BlockSyntax ToBlockSyntax(this ParenthesizedLambdaExpressionSyntax node)
        {
            if (node.Block != null)
                return node.Block;

            if (node.Body is StatementSyntax lambdaBodyStatement)
                return Block(lambdaBodyStatement);

            if (node.Body is ExpressionSyntax lambdaBodyExpression)
                return Block(SyntaxFactory.ExpressionStatement(lambdaBodyExpression));

            throw new InvalidOperationException($"Invalid lambda body: {node.Body}");
        }

        public static bool ContainsDynamicCode(this InvocationExpressionSyntax invoke)
        {
            var argumentList = invoke.DescendantNodes().OfType<ArgumentListSyntax>().LastOrDefault();
            return argumentList?.DescendantNodes().OfType<ConditionalExpressionSyntax>().FirstOrDefault() != null;
        }

        public static bool HasModifier(this ClassDeclarationSyntax cls, SyntaxKind modifier)
            => cls.Modifiers.Any(m => m.IsKind(modifier));

        public static T AncestorOfKind<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor is T t)
                    return t;
            }

            throw new InvalidOperationException($"No Ancestor {nameof(T)} found.");
        }

        public static T AncestorOfKindOrDefault<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor is T t)
                    return t;
            }

            return null;
        }

        public static SyntaxNode NodeAfter(this SyntaxNode node, Func<SyntaxNodeOrToken, bool> predicate)
        {
            var nodeFound = false;
            var descendents = node.DescendantNodesAndTokens().ToArray();

            for (var i = 0; i < descendents.Count(); ++i)
            {
                if (nodeFound && descendents[i].IsNode)
                    return descendents[i].AsNode();

                if (predicate(descendents[i]))
                    nodeFound = true;
            }

            return null;
        }

        public static SyntaxNode WithLineTrivia(this SyntaxNode node, string originalFilePath, int originalLineNumber, int offsetLineNumber = 1)
        {
            if (string.IsNullOrEmpty(originalFilePath))
                return node;

            var lineTrivia = Comment($"#line {originalLineNumber + offsetLineNumber} \"{originalFilePath}\"");
            return node.WithLeadingTrivia(lineTrivia, CarriageReturnLineFeed);
        }

        public static int GetLineNumber(this SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line;

        public static SyntaxNode WithHiddenLineTrivia(this SyntaxNode node)
            => node.WithLeadingTrivia(Comment($"#line hidden"), CarriageReturnLineFeed);

        // Walk direct ancestors that are MemberAccessExpressionSyntax and InvocationExpressionSyntax and collect invocations
        // This collects things like Entities.WithAll().WithNone().Run() without getting additional ancestor invocations.
        public static Dictionary<string, List<InvocationExpressionSyntax>> GetMethodInvocations(this SyntaxNode node)
        {
            var result = new Dictionary<string, List<InvocationExpressionSyntax>>();
            var parent = node.Parent;

            while (parent is MemberAccessExpressionSyntax memberAccessExpression)
            {
                parent = parent.Parent;

                if (parent is InvocationExpressionSyntax invocationExpression)
                {
                    var memberName = memberAccessExpression.Name.Identifier.ValueText;
                    result.Add(memberName, invocationExpression);
                    parent = parent.Parent;
                }
                else if (parent is not MemberAccessExpressionSyntax)
                {
                    break;
                }
            }

            return result;
        }

        public static string GetModifierString(this ParameterSyntax parameter)
        {
            if (parameter.Modifiers.Any(mod => mod.IsKind(SyntaxKind.InKeyword)))
                return "in";

            if (parameter.Modifiers.Any(mod => mod.IsKind(SyntaxKind.RefKeyword)))
                return "ref";

            return "";
        }

        /// <summary>
        /// Reduce to the subset of all the SyntaxNode of a matching SyntaxKind
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static IEnumerable<SyntaxNode> OfKind(this IEnumerable<SyntaxNode> syntaxNode, SyntaxKind kind) => syntaxNode.Where(x => x.IsKind(kind));

        /// <summary>
        /// Reduce to the subset of all the SyntaxToken of a matching SyntaxKind
        /// </summary>
        /// <param name="syntaxToken"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static IEnumerable<SyntaxToken> OfKind(this IEnumerable<SyntaxToken> syntaxToken, SyntaxKind kind) => syntaxToken.Where(x => x.IsKind(kind));

        /// <summary>
        /// Retrieve the first child that is of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="syntaxNode"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool TryGetFirstChildByType<T>(this SyntaxNode syntaxNode, out T child) => (child = syntaxNode.ChildNodes().OfType<T>().FirstOrDefault()) != null;
        /// <summary>
        /// Return if a SyntaxNode has a child token of a given SyntaxKind
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool HasTokenOfKind(this SyntaxNode syntaxNode, SyntaxKind kind) => syntaxNode.ChildTokens().OfKind(kind).Any();

        /// <summary>
        /// Try to retrieve the first child of this SyntaxNode that is of a given SyntaxKind
        /// </summary>
        /// <param name="syntaxNode">This SyntaxNode</param>
        /// <param name="kind">The SyntaxKind to look for</param>
        /// <param name="child">The first child of matching syntax kind if found</param>
        /// <returns>true if found. false if not.</returns>
        public static bool TryGetFirstChildByKind(this SyntaxNode syntaxNode, SyntaxKind kind, out SyntaxNode child) => (child = syntaxNode.ChildNodes().OfKind(kind).FirstOrDefault()) != null;

        /// <summary>
        /// Test if a node represent an identifier by comparing string followed
        /// by a TypeArgumentListSyntax, which is returned through typeArgumentListSyntax.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="identifier"></param>
        /// <param name="typeArgumentListSyntax"></param>
        /// <returns></returns>
        public static bool IsIdentifier(this SyntaxNode syntaxNode, string identifier, out TypeArgumentListSyntax typeArgumentListSyntax)
        {
            switch (syntaxNode)
            {
                case GenericNameSyntax genericNameSyntax:
                    typeArgumentListSyntax = genericNameSyntax.TypeArgumentList;
                    return genericNameSyntax.Identifier.ValueText == identifier;
                case SimpleNameSyntax simpleNameSyntax:
                    typeArgumentListSyntax = default;
                    return simpleNameSyntax.Identifier.ValueText == identifier;
            }
            typeArgumentListSyntax = default;
            return false;
        }

        /// <summary>
        /// Figures out as fast as possible if the syntax node does not represent a type name.
        /// Use for early-out tests within the OnVisitSyntaxNode calls.
        /// Use the SemanticModel from GeneratorExecutionContext.Compilation.GetSemanticModel() to get an accurate result during the Execute call.
        ///
        /// Returns false if the node is found to *not* be equal using fast early-out tests.
        /// Returns true if type name is likely equal.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="typeNameNamesapce">The host namepsace of the type name. e.g. "Unity.Entities"</param>
        /// <param name="typeName">The unqualified type name of the generic type. e.g. "Entity" </param>
        /// <returns></returns>
        public static bool IsTypeNameCandidate(this SyntaxNode syntaxNode, string typeNameNamesapce, string typeName)
            => IsTypeNameCandidate(syntaxNode, typeNameNamesapce, typeName, out _);

        /// <summary>
        /// Figures out as fast as possible if the syntax node does not represent a type name.
        /// Use for early-out tests within the OnVisitSyntaxNode calls.
        /// Use the SemanticModel from GeneratorExecutionContext.Compilation.GetSemanticModel() to get an accurate result during the Execute call.
        ///
        /// Returns false if the node is found to *not* be equal using fast early-out tests.
        /// Returns true if type name is likely equal.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="typeNameNamesapce">The host namepsace of the type name. e.g. "Unity.Entities"</param>
        /// <param name="typeName">The unqualified type name of the generic type. e.g. "Entity" </param>
        /// <param name="typeArgumentListSyntax">output the TypeArgumentListSyntax node if the type represented by this SyntaxNode is generic</param>
        /// <returns></returns>
        public static bool IsTypeNameCandidate(
              this SyntaxNode syntaxNode
            , string typeNameNamesapce
            , string typeName
            , out TypeArgumentListSyntax typeArgumentListSyntax
        )
        {
            switch (syntaxNode)
            {
                case QualifiedNameSyntax qualifiedNameSyntax:
                    // Fast estimate right part and extract a possible TypeArgumentListSyntax to our own TypeArgumentListSyntax output
                    if (!IsIdentifier(qualifiedNameSyntax.Right, typeName, out typeArgumentListSyntax))
                    {
                        return false;
                    }
                    var iLastDot = typeNameNamesapce.LastIndexOf('.');
                    if (iLastDot < 0)
                    {
                        //End of qualified names
                        var typename = qualifiedNameSyntax.Left.ToString();

                        if (typename.StartsWith("global::"))
                            typename = typename.Substring(8);

                        typeArgumentListSyntax = default;
                        return typename == typeNameNamesapce;
                    }
                    else if (qualifiedNameSyntax.Left != null)
                    {
                        // Fast estimate left part without extracting any TypeArgumentListSyntax
                        return qualifiedNameSyntax.Left.IsTypeNameCandidate(
                              typeNameNamesapce.Substring(0, iLastDot)
                            , typeNameNamesapce.Substring(iLastDot + 1)
                        );
                    }
                    else
                    {
                        // Limit the test here, any remaining qualified name is assumed to be a known scope. e.g. part of a using statement or other type defined withing the same unit.
                        return true;
                    }

                default:
                    // Check if current node is the identifier symbolName
                    // and if the current node's scope knows of the scope name symbolNamesapce
                    return IsIdentifier(syntaxNode, typeName, out typeArgumentListSyntax);
            }
        }

        /// <summary>
        /// Figures out as fast as possible if the syntax node does not represent a type name.
        /// Use for early-out tests within the OnVisitSyntaxNode calls.
        /// Use the SemanticModel from GeneratorExecutionContext.Compilation.GetSemanticModel() to get an accurate result during the Execute call.
        ///
        /// Returns false if the node is found to *not* be equal using fast early-out tests.
        /// Returns true if type name is likely equal and extract the first generic type parameter into genericParam0.
        /// </summary>
        /// <param name="syntaxNode">Node to test type name against</param>
        /// <param name="typeNameNamesapce">The host namepsace of the type name. e.g. "Unity.Entities"</param>
        /// <param name="typeName">The unqualified type name of the generic type. e.g. "ComponentDataRef" </param>
        /// <param name="genericParam0">TypeSyntax of the first generic type name represented by the SyntaxNode</param>
        /// <returns></returns>
        public static bool IsTypeNameGenericCandidate(
              this SyntaxNode syntaxNode
            , string typeNameNamesapce
            , string typeName
            , out TypeSyntax genericParam0
        )
        {
            if (IsTypeNameCandidate(syntaxNode, typeNameNamesapce, typeName, out var typeArgumentList))
            {
                if (typeArgumentList != null
                    && TryGetFirstChildByType<TypeSyntax>(typeArgumentList, out var typeSyntax)
                )
                {
                    genericParam0 = typeSyntax;
                    return true;
                }
            }

            genericParam0 = default;
            return false;
        }

        /// <summary>
        /// Perform a string compare between the resolved symbol full type name represented by the SyntaxNode and a given string
        /// </summary>
        /// <param name="syntaxNode">Node that represent a type name</param>
        /// <param name="model"></param>
        /// <param name="symbolTypeName">Full type name to compare to</param>
        /// <returns></returns>
        public static bool IsSymbol(this SyntaxNode syntaxNode, SemanticModel model, string symbolTypeName)
        {

            var symbol = model.GetSymbolInfo(syntaxNode).Symbol;
            if (symbol == null) return false;
            var symbolTypeName2 = symbol.GetSymbolType().ToFullName();
            return symbolTypeName2 == symbolTypeName;
        }

        /// <summary>
        /// Figures out as fast as possible if the node has an attribute that may be equal the to string provided.
        /// Use for early-out tests within the OnVisitSyntaxNode calls.
        /// Use the SemanticModel from GeneratorExecutionContext.Compilation.GetSemanticModel() to get an accurate result during the Execute call.
        ///
        /// Returns false if no attribute is likely equal using fast early-out tests.
        /// Returns true if an attribute is likely equal.
        /// </summary>
        /// <param name="syntaxNode">Node to test type name against</param>
        /// <param name="attributeNameSpace">The host namepsace of the attribute type name. e.g. "Unity.Entities"</param>
        /// <param name="attributeName">The unqualified attribute name. e.g. "UpdateBefore" </param>
        /// <returns></returns>
        public static bool HasAttributeCandidate(this SyntaxNode syntaxNode, string attributeNameSpace, string attributeName)
        {
            foreach (var attribListCandidate in syntaxNode.ChildNodes())
            {
                if (attribListCandidate == null || attribListCandidate.IsKind(SyntaxKind.AttributeList) == false)
                {
                    continue;
                }

                foreach (var attribCandidate in attribListCandidate.ChildNodes())
                {
                    if (attribCandidate is AttributeSyntax attrib
                        && attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasAttribute(
              this MemberDeclarationSyntax syntaxNode
            , string attributeNameSpace
            , string attributeName
        )
        {
            foreach (var attribList in syntaxNode.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    if (attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static AttributeSyntax GetAttribute(
              this MemberDeclarationSyntax syntaxNode
            , string attributeNameSpace
            , string attributeName
        )
        {
            foreach (var attribList in syntaxNode.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    if (attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName))
                    {
                        return attrib;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the full type name of the node's enclosing namespace.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <returns>e.g. "Unity.Entities"</returns>
        public static string GetParentNamespace(this SyntaxNode syntaxNode)
            => string.Join(".", syntaxNode.GetNamespacesFromMostToLeastNested().Reverse().Select(n => n.Name.ToString()));

        public static bool TryGetGenericParam1TypeName(this SyntaxNode syntaxNode, SemanticModel semanticModel, out string typename)
        {
            typename = null;
            var genericParam1 = syntaxNode.DescendantNodes()
                .OfType<TypeArgumentListSyntax>()
                .FirstOrDefault()?.DescendantNodes()
                .OfType<TypeSyntax>()
                .FirstOrDefault();

            if (genericParam1 == null)
                return false;

            typename = semanticModel.GetSymbolInfo(genericParam1).Symbol?.GetSymbolType().ToFullName();
            return typename != null;
        }

        public static ISymbol GetGenericParam1Symbol(this SyntaxNode syntaxNode, SemanticModel semanticModel, out string typename)
        {
            typename = null;

            var genericParam1 = syntaxNode.DescendantNodes()
                .OfType<TypeArgumentListSyntax>()
                .FirstOrDefault()?.DescendantNodes()
                .OfType<TypeSyntax>()
                .FirstOrDefault();

            if (genericParam1 == null)
                return null;

            var symbol = semanticModel.GetSymbolInfo(genericParam1).Symbol;
            typename = symbol?.GetSymbolType().ToFullName();
            return symbol;
        }
    }
}
