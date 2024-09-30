using Microsoft.CodeAnalysis;

namespace Module.Core.SourceGen
{
    public static class AccessibilityExtensions
    {
        public static string ToKeyword(this Accessibility self)
        {
            return self switch {
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                _ => "public",
            };
        }
    }
}
