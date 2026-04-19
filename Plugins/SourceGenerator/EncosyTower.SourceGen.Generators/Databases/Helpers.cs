namespace EncosyTower.SourceGen.Generators.Databases
{
    public static class Helpers
    {
        public const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        public const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        public const string SKIP_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string PR_DATABASE_ASSET = "ETDB.DatabaseAsset";

        public const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";

        public const string PR_AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        public const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_GENERATED_ASSET_NAME = $"[ETDBSG.GeneratedAssetNameConstant(typeof({{0}}), typeof({{1}}))]";
        public const string PR_STRUCT_LAYOUT_AUTO = "[SRIS.StructLayout(SRIS.LayoutKind.Auto)]";
        public const string PR_SERIALIZABLE = "[S.Serializable]";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string DATA_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataAttribute";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";
    }
}
