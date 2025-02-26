namespace EncosyTower.SourceGen.Generators.Databases
{
    public static class Helpers
    {
        public const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        public const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        public const string SKIP_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string IDATABASE = $"global::{DATABASES_NAMESPACE}.IDatabase";
        public const string DATABASE_ASSET = $"global::{DATABASES_NAMESPACE}.DatabaseAsset";

        public const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";

        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string GENERATED_ASSET_NAME = $"[global::{DATABASES_NAMESPACE}.SourceGen.GeneratedAssetNameConstant]";
        public const string STRUCT_LAYOUT_AUTO = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";

        public const string DATABASES_AUTHORING_NAMESPACE = $"{DATABASES_NAMESPACE}.Authoring";
        public const string HORIZONTAL_LIST_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.HorizontalAttribute";
    }
}
