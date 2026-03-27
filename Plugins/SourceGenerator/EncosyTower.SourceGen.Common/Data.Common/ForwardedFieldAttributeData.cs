using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    /// <summary>
    /// Cache-friendly, equatable model for a forwarded attribute on a <c>[DataProperty]</c> member.
    /// Stores the attribute's fully-qualified type name alongside its precomputed syntax string,
    /// avoiding retention of <see cref="Microsoft.CodeAnalysis.ISymbol"/> or
    /// <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> references.
    /// </summary>
    public struct ForwardedFieldAttributeData : IEquatable<ForwardedFieldAttributeData>
    {
        /// <summary>
        /// The fully-qualified type name of the attribute (e.g. "global::UnityEngine.SerializeField").
        /// Used to check for special attribute handling (SerializeField, DontCreateProperty, etc.).
        /// </summary>
        public string fullTypeName;

        /// <summary>
        /// Precomputed result of <c>attribute.GetSyntax().ToFullString()</c>.
        /// Emitted verbatim into generated code.
        /// </summary>
        public string attributeSyntax;

        public readonly bool Equals(ForwardedFieldAttributeData other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(attributeSyntax, other.attributeSyntax, StringComparison.Ordinal);

        public readonly override bool Equals(object obj)
            => obj is ForwardedFieldAttributeData other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullTypeName, attributeSyntax);
    }
}
