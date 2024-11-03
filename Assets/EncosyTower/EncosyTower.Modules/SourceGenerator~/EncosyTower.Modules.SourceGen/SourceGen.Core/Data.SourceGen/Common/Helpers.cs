namespace EncosyTower.Modules.Data.SourceGen
{
    public static class Helpers
    {
        public const string DATA_GENERATOR_NAME = "DataGenerator";
        public const string DATA_TABLE_ASSET_GENERATOR_NAME = "DataTableAssetGenerator";

        public const string IDATA = "global::EncosyTower.Modules.Data.IData";
        public const string IDATA_TABLE_ASSET = "global::EncosyTower.Modules.Data.IDataTableAsset";
        public const string DATA_TABLE_ASSET = "global::EncosyTower.Modules.Data.DataTableAsset";
        public const string DATA_PROPERTY_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataPropertyAttribute";
        public const string DATA_CONVERTER_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataConverterAttribute";
        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";
        public const string JSON_INCLUDE_ATTRIBUTE = "global::System.Text.Json.Serialization.JsonIncludeAttribute";
        public const string JSON_PROPERTY_ATTRIBUTE = "global::Newtonsoft.Json.JsonPropertyAttribute";
        public const string DATA_MUTABLE_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataMutableAttribute";
        public const string DATA_FIELD_POLICY_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataFieldPolicyAttribute";
        public const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Data.SkipSourceGenForAssemblyAttribute";

        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Data.DataGenerator\", \"1.8.2\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string LIST_TYPE_T = "global::System.Collections.Generic.List<";
        public const string DICTIONARY_TYPE_T = "global::System.Collections.Generic.Dictionary<";
        public const string HASH_SET_TYPE_T = "global::System.Collections.Generic.HashSet<";
        public const string QUEUE_TYPE_T = "global::System.Collections.Generic.Queue<";
        public const string STACK_TYPE_T = "global::System.Collections.Generic.Stack<";

        public const string IREADONLY_LIST_TYPE_T = "global::System.Collections.Generic.IReadOnlyList<";
        public const string IREADONLY_DICTIONARY_TYPE_T = "global::System.Collections.Generic.IReadOnlyDictionary<";
        public const string READONLY_MEMORY_TYPE_T = "global::System.ReadOnlyMemory<";
        public const string READONLY_SPAN_TYPE_T = "global::System.ReadOnlySpan<";
        public const string MEMORY_TYPE_T = "global::System.Memory<";
        public const string SPAN_TYPE_T = "global::System.Span<";

        public const string GENERATED_PROPERTY_FROM_FIELD_ATTRIBUTE = "[global::EncosyTower.Modules.Data.SourceGen.GeneratedPropertyFromField(nameof({0}), typeof({1}))]";
        public const string GENERATED_FIELD_FROM_PROPERTY_ATTRIBUTE = "[global::EncosyTower.Modules.Data.SourceGen.GeneratedFieldFromProperty(nameof({0}))]";
    }
}
