namespace EncosyTower.SourceGen.Generators.Data
{
    public static class Helpers
    {
        public const string GENERATOR_NAME = "DataGenerator";

        public const string NAMESPACE = "EncosyTower.Data";
        public const string IDATA = $"global::{NAMESPACE}.IData";
        public const string IREADONLY_DATA = $"global::{NAMESPACE}.IReadOnlyData";
        public const string IDATA_TABLE_ASSET = $"global::{NAMESPACE}.IDataTableAsset";
        public const string DATA_TABLE_ASSET = $"global::{NAMESPACE}.DataTableAsset";
        public const string DATA_PROPERTY_ATTRIBUTE = $"global::{NAMESPACE}.DataPropertyAttribute";
        public const string DATA_CONVERTER_ATTRIBUTE = $"global::{NAMESPACE}.DataConverterAttribute";
        public const string SERIALIZABLE_ATTRIBUTE = "global::System.SerializableAttribute";
        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";
        public const string DATA_MUTABLE_ATTRIBUTE = $"global::{NAMESPACE}.DataMutableAttribute";
        public const string DATA_FIELD_POLICY_ATTRIBUTE = $"global::{NAMESPACE}.DataFieldPolicyAttribute";
        public const string PROPERTY_TYPE_ATTRIBUTE = $"global::{NAMESPACE}.PropertyTypeAttribute";
        public const string DATA_WITHOUT_ID_ATTRIBUTE = $"global::{NAMESPACE}.DataWithoutIdAttribute";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public const string GENERATE_PROPERTY_BAG_ATTRIBUTE = "global::Unity.Properties.GeneratePropertyBagAttribute";
        public const string CREATE_PROPERTY_ATTRIBUTE = "global::Unity.Properties.CreatePropertyAttribute";
        public const string CREATE_PROPERTY = "global::Unity.Properties.CreateProperty";
        public const string DONT_CREATE_PROPERTY_ATTRIBUTE = "global::Unity.Properties.DontCreatePropertyAttribute";
        public const string DONT_CREATE_PROPERTY = "global::Unity.Properties.DontCreateProperty";

        public const string UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdAttribute";

        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Data.DataGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public const string LIST_TYPE_T = "global::System.Collections.Generic.List<";
        public const string DICTIONARY_TYPE_T = "global::System.Collections.Generic.Dictionary<";
        public const string HASH_SET_TYPE_T = "global::System.Collections.Generic.HashSet<";
        public const string QUEUE_TYPE_T = "global::System.Collections.Generic.Queue<";
        public const string STACK_TYPE_T = "global::System.Collections.Generic.Stack<";

        public const string IREADONLY_LIST_TYPE_T = "global::System.Collections.Generic.IReadOnlyList<";
        public const string ILIST_TYPE_T = "global::System.Collections.Generic.IList<";
        public const string ISET_TYPE_T = "global::System.Collections.Generic.ISet<";
        public const string IREADONLY_DICTIONARY_TYPE_T = "global::System.Collections.Generic.IReadOnlyDictionary<";
        public const string IDICTIONARY_TYPE_T = "global::System.Collections.Generic.IDictionary<";
        public const string READONLY_MEMORY_TYPE_T = "global::System.ReadOnlyMemory<";
        public const string READONLY_SPAN_TYPE_T = "global::System.ReadOnlySpan<";
        public const string MEMORY_TYPE_T = "global::System.Memory<";
        public const string SPAN_TYPE_T = "global::System.Span<";

        public const string GENERATED_PROPERTY_FROM_FIELD_ATTRIBUTE = $"[global::{NAMESPACE}.SourceGen.GeneratedPropertyFromField(nameof({{0}}), typeof({{1}}))]";
        public const string GENERATED_FIELD_FROM_PROPERTY_ATTRIBUTE = $"[global::{NAMESPACE}.SourceGen.GeneratedFieldFromProperty(nameof({{0}}))]";
    }
}
