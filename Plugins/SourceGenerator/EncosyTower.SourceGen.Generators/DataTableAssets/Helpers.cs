namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    public static class Helpers
    {
        public const string GENERATOR_NAME = "DataTableAssetGenerator";

        public const string NAMESPACE = "EncosyTower.Databases";
        public const string IDATA = $"global::{NAMESPACE}.IData";
        public const string DATA_TABLE_ASSET = $"global::{NAMESPACE}.DataTableAsset";
        public const string DATABASE_ATTRIBUTE = $"global::{NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{NAMESPACE}.TableAttribute";
        public const string HORIZONTAL_LIST_ATTRIBUTE = $"global::{NAMESPACE}.HorizontalAttribute";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.DataTableAssets.DataTableAssetGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
    }
}
