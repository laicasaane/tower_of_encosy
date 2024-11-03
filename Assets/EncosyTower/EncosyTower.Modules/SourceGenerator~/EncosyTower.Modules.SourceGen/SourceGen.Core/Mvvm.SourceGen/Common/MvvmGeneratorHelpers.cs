using System.Collections.Immutable;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm
{
    public static class MvvmGeneratorHelpers
    {
        public const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.SkipSourceGenForAssemblyAttribute";

        private const string COMPONENT_MODEL_NS = "global::EncosyTower.Modules.Mvvm.ComponentModel";

        public static bool IsClassSyntaxMatchByAttribute(
              SyntaxNode syntaxNode
            , CancellationToken token
            , SyntaxKind syntaxKind
            , string attributeNamespace
            , string attributeName
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.BaseList.Types.Count < 1
            )
            {
                return false;
            }

            var members = classSyntax.Members;

            foreach (var member in members)
            {
                if (member.Kind() == syntaxKind
                    && member.HasAttributeCandidate(attributeNamespace, attributeName)
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static ClassDeclarationSyntax GetClassSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
            , string interfaceName
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.DoesSemanticMatch(interfaceName, context.SemanticModel, token) == false
            )
            {
                return null;
            }

            return classSyntax;
        }

        public static bool DoesSemanticMatch(
              this ClassDeclarationSyntax classSyntax
            , string interfaceName
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            if (classSyntax.BaseList != null)
            {
                foreach (var baseType in classSyntax.BaseList.Types)
                {
                    var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                    if (typeInfo.Type.ToFullName() == interfaceName)
                    {
                        return true;
                    }

                    if (typeInfo.Type.ImplementsInterface(interfaceName))
                    {
                        return true;
                    }

                    if (IsMatch(typeInfo.Type.Interfaces, interfaceName)
                        || IsMatch(typeInfo.Type.AllInterfaces, interfaceName)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;

            static bool IsMatch(ImmutableArray<INamedTypeSymbol> interfaces, string interfaceName)
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.ToFullName() == interfaceName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool AnyFieldHasNotifyPropertyChangedForAttribute(
              this ClassDeclarationSyntax classSyntax
            , PropertyDeclarationSyntax property
        )
        {
            foreach (var member in classSyntax.Members)
            {
                if (member is not FieldDeclarationSyntax fds)
                {
                    continue;
                }

                var attrib = fds.GetAttribute(COMPONENT_MODEL_NS, "NotifyPropertyChangedFor");

                if (attrib != null
                    && attrib.ArgumentList is { } argumentList
                    && argumentList.Arguments.Count == 1
                    && argumentList.Arguments[0].Expression is InvocationExpressionSyntax invocation
                    && invocation.ArgumentList is { } invocationArgumentList
                    && invocationArgumentList.Arguments.Count == 1
                    && invocationArgumentList.Arguments[0].Expression is IdentifierNameSyntax identifierName
                    && identifierName.Identifier.ValueText == property.Identifier.ValueText
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}