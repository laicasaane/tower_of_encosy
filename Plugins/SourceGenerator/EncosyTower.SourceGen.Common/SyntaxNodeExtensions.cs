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
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen
{
    public static class SyntaxNodeExt
    {
        public static int GetStableHashCode(this SyntaxTree syntaxTree)
            => SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;

        public static string GetHintName( this SyntaxTree syntaxTree, SyntaxNode node, string fileName)
            => GetHintName(syntaxTree, node.GetLineNumber(), fileName);

        public static string GetHintName(this SyntaxTree syntaxTree, int salting, string fileName)
        {
            var stableHashCode = syntaxTree.GetStableHashCode();
            var postfix = string.Empty;

            if (string.IsNullOrWhiteSpace(fileName) == false)
            {
                postfix = $"{fileName}{postfix}";
            }

            return $"{postfix}_{stableHashCode}_{salting}.g.cs";
        }

        public static bool HasModifier(this MemberDeclarationSyntax cls, SyntaxKind modifier)
            => cls.Modifiers.Any(m => m.IsKind(modifier));

        public static int GetLineNumber(this SyntaxNode node)
            => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        /// <summary>
        /// Test if a node represent an identifier by comparing string followed
        /// by a TypeArgumentListSyntax, which is returned through typeArgumentListSyntax.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="identifier"></param>
        /// <param name="typeArgumentListSyntax"></param>
        /// <returns></returns>
        public static bool IsIdentifier(
              this SyntaxNode syntaxNode
            , ReadOnlySpan<char> identifier
            , out TypeArgumentListSyntax typeArgumentListSyntax
        )
        {
            switch (syntaxNode)
            {
                case GenericNameSyntax genericNameSyntax:
                {
                    typeArgumentListSyntax = genericNameSyntax.TypeArgumentList;
                    return MemoryExtensions.Equals(
                          genericNameSyntax.Identifier.ValueText.AsSpan()
                        , identifier
                        , StringComparison.Ordinal
                    );
                }

                case SimpleNameSyntax simpleNameSyntax:
                {
                    typeArgumentListSyntax = default;
                    return MemoryExtensions.Equals(
                          simpleNameSyntax.Identifier.ValueText.AsSpan()
                        , identifier
                        , StringComparison.Ordinal
                    );
                }
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
        public static bool IsTypeNameCandidate(
              this SyntaxNode syntaxNode
            , string typeNameNamesapce
            , string typeName
            , CancellationToken token = default
        )
        {
            return IsTypeNameCandidate(syntaxNode, typeNameNamesapce.AsSpan(), typeName.AsSpan(), out _, token);
        }

        public static bool IsTypeNameCandidate(
              this SyntaxNode syntaxNode
            , string fullyQualifedTypeName
            , CancellationToken token = default
        )
        {
            return IsTypeNameCandidate(syntaxNode, fullyQualifedTypeName, out _, token);
        }

        public static bool IsTypeNameCandidate(
              this SyntaxNode syntaxNode
            , string fullyQualifedTypeName
            , out TypeArgumentListSyntax typeArgumentListSyntax
            , CancellationToken token = default
        )
        {
            var span = fullyQualifedTypeName.AsSpan();
            var iLastDot = span.LastIndexOf('.');

            if (iLastDot < 0)
            {
                return IsTypeNameCandidate(
                      syntaxNode
                    , ReadOnlySpan<char>.Empty
                    , span
                    , out typeArgumentListSyntax
                    , token
                );
            }

            return IsTypeNameCandidate(
                  syntaxNode
                , span.Slice(0, iLastDot)
                , span.Slice(iLastDot + 1)
                , out typeArgumentListSyntax
                , token
            );
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
        /// <param name="typeArgumentListSyntax">output the TypeArgumentListSyntax node if the type represented by this SyntaxNode is generic</param>
        /// <returns></returns>
        public static bool IsTypeNameCandidate(
              this SyntaxNode syntaxNode
            , ReadOnlySpan<char> typeNameNamesapce
            , ReadOnlySpan<char> typeName
            , out TypeArgumentListSyntax typeArgumentListSyntax
            , CancellationToken token = default
        )
        {
            token.ThrowIfCancellationRequested();

            switch (syntaxNode)
            {
                case QualifiedNameSyntax qualifiedNameSyntax:
                {
                    // Fast estimate right part and extract a possible TypeArgumentListSyntax to our own TypeArgumentListSyntax output
                    if (IsIdentifier(qualifiedNameSyntax.Right, typeName, out typeArgumentListSyntax) == false)
                    {
                        return false;
                    }

                    var iLastDot = typeNameNamesapce.LastIndexOf('.');

                    if (iLastDot < 0)
                    {
                        //End of qualified names
                        var typename = qualifiedNameSyntax.Left.ToString().AsSpan();

                        if (typename.StartsWith("global::".AsSpan()))
                        {
                            typename = typename.Slice(8);
                        }

                        typeArgumentListSyntax = default;

                        return MemoryExtensions.Equals(typename, typeNameNamesapce, StringComparison.Ordinal);
                    }
                    else if (qualifiedNameSyntax.Left != null)
                    {
                        // Fast estimate left part without extracting any TypeArgumentListSyntax
                        return qualifiedNameSyntax.Left.IsTypeNameCandidate(
                              typeNameNamesapce.Slice(0, iLastDot)
                            , typeNameNamesapce.Slice(iLastDot + 1)
                            , out _
                            , token
                        );
                    }
                    else
                    {
                        // Limit the test here, any remaining qualified name is assumed to be a known scope. e.g. part of a using statement or other type defined withing the same unit.
                        return true;
                    }
                }

                default:
                {
                    // Check if current node is the identifier symbolName
                    // and if the current node's scope knows of the scope name symbolNamesapce
                    return IsIdentifier(syntaxNode, typeName, out typeArgumentListSyntax);
                }
            }
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
        public static bool HasAttributeCandidate(
              this SyntaxNode syntaxNode
            , string attributeNameSpace
            , string attributeName
            , CancellationToken token = default
        )
        {
            foreach (var attribListCandidate in syntaxNode.ChildNodes())
            {
                token.ThrowIfCancellationRequested();

                if (attribListCandidate == null || attribListCandidate.IsKind(SyntaxKind.AttributeList) == false)
                {
                    continue;
                }

                foreach (var attribCandidate in attribListCandidate.ChildNodes())
                {
                    token.ThrowIfCancellationRequested();

                    if (attribCandidate is AttributeSyntax attrib
                        && attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName, token)
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
            , CancellationToken token = default
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var attribList in syntaxNode.AttributeLists)
            {
                token.ThrowIfCancellationRequested();

                foreach (var attrib in attribList.Attributes)
                {
                    token.ThrowIfCancellationRequested();

                    if (attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName, token))
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
            , CancellationToken token = default
        )
        {
            foreach (var attribList in syntaxNode.AttributeLists)
            {
                token.ThrowIfCancellationRequested();

                foreach (var attrib in attribList.Attributes)
                {
                    token.ThrowIfCancellationRequested();

                    if (attrib.Name.IsTypeNameCandidate(attributeNameSpace, attributeName, token))
                    {
                        return attrib;
                    }
                }
            }

            return null;
        }

        public static TRoot InsertNodesAfterThenRemove<TRoot>(
              this TRoot root
            , SyntaxNode nodeInList
            , IEnumerable<SyntaxNode> newNodes
            , SyntaxRemoveOptions removeOptions
        )
            where TRoot : SyntaxNode
        {
            root = root.TrackNodes(nodeInList);

            var newNodeInList = root.GetCurrentNode(nodeInList);
            root = root.InsertNodesAfter(newNodeInList, newNodes);

            var toRemove = root.GetCurrentNode(nodeInList);
            return root.RemoveNode(toRemove, removeOptions);
        }

        public static string GetDisplayNameOrDefault(
              this MemberDeclarationSyntax syntax
            , string defaultValue
            , CancellationToken token = default
        )
        {
            var displayName = defaultValue;
            Get(syntax, ref displayName, token);
            return displayName;

            static void Get(MemberDeclarationSyntax syntax, ref string displayName, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();

                foreach (var attributeList in syntax.AttributeLists)
                {
                    token.ThrowIfCancellationRequested();

                    if (attributeList is null)
                    {
                        continue;
                    }

                    foreach (var attrib in attributeList.Attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        if (attrib is null
                            || attrib.Name is not IdentifierNameSyntax identifierName
                            || attrib.ArgumentList is not AttributeArgumentListSyntax attributeArgumentList
                            || attributeArgumentList.Arguments is not { Count: > 0 } attributeArguments
                            || attributeArguments[0].Expression is not LiteralExpressionSyntax literalExpression
                            || literalExpression.Token.Value is not string displayNameValue
                        )
                        {
                            continue;
                        }

                        switch (identifierName.Identifier.Text)
                        {
                            case "Label":
                            case "Description":
                            case "Display":
                            case "DisplayName":
                                displayName = displayNameValue;
                                return;
                        }
                    }
                }
            }
        }
    }
}
