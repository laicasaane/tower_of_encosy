using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Internal
{
    internal static class SymbolFormats
    {
        public static readonly SymbolDisplayFormat FullyQualified = new(
              globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included
            , typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );

        public static readonly SymbolDisplayFormat SimpleNoGlobal = new(
              globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted
            , typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
            , genericsOptions: SymbolDisplayGenericsOptions.None
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );
    }
}
