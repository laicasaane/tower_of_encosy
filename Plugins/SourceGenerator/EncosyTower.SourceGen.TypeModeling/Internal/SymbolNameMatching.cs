using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Internal
{
    internal static class SymbolNameMatching
    {
        internal static bool HasFullName(ITypeSymbol symbol, string fullyQualifiedName)
        {
            if (symbol is null || fullyQualifiedName is null)
            {
                return false;
            }

            var span = fullyQualifiedName.AsSpan();
            var indexOfAngle = span.IndexOf('<');

            if (indexOfAngle >= 0)
            {
                return string.Equals(symbol.ToDisplayString(SymbolFormats.FullyQualified), fullyQualifiedName);
            }

            if (span.StartsWith("global::".AsSpan(), StringComparison.Ordinal))
            {
                span = span.Slice(8);
            }

            return MatchesQualifiedSpan(symbol, span);
        }

        internal static bool MatchesQualifiedSpan(ITypeSymbol symbol, ReadOnlySpan<char> remaining)
        {
            if (symbol is null)
            {
                return false;
            }

            // Find the last '.' that is not inside angle brackets so we can split
            // containing-context from type-name even when the name has generic args.
            int lastDot = -1;
            int depth = 0;

            for (int i = remaining.Length - 1; i >= 0; i--)
            {
                char c = remaining[i];

                if (c == '>')
                {
                    depth++;
                }
                else if (c == '<')
                {
                    depth--;
                }
                else if (c == '.' && depth == 0)
                {
                    lastDot = i;
                    break;
                }
            }

            var typePart = lastDot >= 0 ? remaining.Slice(lastDot + 1) : remaining;
            var prefixPart = lastDot >= 0 ? remaining.Slice(0, lastDot) : default;

            // Strip generic args from the type-name portion for the name equality check.
            int angleIdx = typePart.IndexOf('<');
            var typeName = angleIdx >= 0 ? typePart.Slice(0, angleIdx) : typePart;

            if (typeName.Equals(symbol.Name.AsSpan(), StringComparison.Ordinal) == false)
            {
                return false;
            }

            if (prefixPart.IsEmpty)
            {
                // There must be no containing type, and the namespace must be global.
                return symbol.ContainingType == null && symbol.ContainingNamespace?.IsGlobalNamespace == true;
            }

            return symbol.ContainingType != null
                ? MatchesQualifiedSpan(symbol.ContainingType, prefixPart)
                : MatchesNamespaceSpan(symbol.ContainingNamespace, prefixPart);
        }

        internal static bool MatchesNamespaceSpan(INamespaceSymbol ns, ReadOnlySpan<char> remaining)
        {
            if (ns == null || ns.IsGlobalNamespace)
            {
                return remaining.IsEmpty;
            }

            if (remaining.IsEmpty)
            {
                return false;
            }

            int lastDot = remaining.LastIndexOf('.');
            var nsPart = lastDot >= 0 ? remaining.Slice(lastDot + 1) : remaining;
            var prefixPart = lastDot >= 0 ? remaining.Slice(0, lastDot) : default;

            if (nsPart.Equals(ns.Name.AsSpan(), StringComparison.Ordinal) == false)
            {
                return false;
            }

            if (prefixPart.IsEmpty)
            {
                return ns.ContainingNamespace?.IsGlobalNamespace == true;
            }

            return MatchesNamespaceSpan(ns.ContainingNamespace, prefixPart);
        }
    }
}