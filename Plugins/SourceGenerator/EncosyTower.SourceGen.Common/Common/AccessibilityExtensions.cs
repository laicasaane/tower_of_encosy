using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class AccessibilityExtensions
    {
        public static string ToKeyword(this Accessibility self)
        {
            return self switch {
                  Accessibility.Internal => "internal"
                , Accessibility.Private => "private"
                , Accessibility.Protected => "protected"
                , Accessibility.ProtectedAndInternal => "private protected"
                , Accessibility.ProtectedOrInternal => "protected internal"
                , Accessibility.Public => "public"
                , _ => string.Empty
            };
        }
    }
}
