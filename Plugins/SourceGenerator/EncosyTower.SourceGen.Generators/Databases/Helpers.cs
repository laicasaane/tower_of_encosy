using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public static class Helpers
    {
        public const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        public const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        public const string SKIP_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string DATABASE_ASSET = "DatabaseAsset";

        public const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";

        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string GENERATED_ASSET_NAME = $"[GeneratedAssetNameConstant(typeof({{0}}), typeof({{1}}))]";
        public const string STRUCT_LAYOUT_AUTO = "[StructLayout(LayoutKind.Auto)]";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string DATA_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataAttribute";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";

        public static bool IsIData(ITypeSymbol type)
            => type.HasAttribute(DATA_ATTRIBUTE) || type.InheritsFromInterface(IDATA);
    }
}
