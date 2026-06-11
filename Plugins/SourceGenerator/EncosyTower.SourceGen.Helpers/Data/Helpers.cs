namespace EncosyTower.SourceGen.Helpers.Data
{
    public static class Helpers
    {
        public const string GENERATOR_NAME = "DataGenerator";

        public const string NAMESPACE = "EncosyTower.Data";
        public const string DATA_ATTRIBUTE_METADATA = $"{NAMESPACE}.DataAttribute";
        public const string DATA_ATTRIBUTE = $"global::{NAMESPACE}.DataAttribute";
        public const string IDATA = $"global::{NAMESPACE}.IData";
        public const string DATA_PROPERTY_ATTRIBUTE = $"global::{NAMESPACE}.DataPropertyAttribute";
        public const string DATA_AUTHORING_CONVERTER_ATTRIBUTE = $"global::{NAMESPACE}.Authoring.DataAuthoringConverterAttribute";
        public const string DATA_MANUAL_AUTHORING_ATTRIBUTE = $"global::{NAMESPACE}.Authoring.DataManualAuthoringAttribute";
        public const string DATA_COMPARER_ATTRIBUTE = $"global::{NAMESPACE}.DataComparerAttribute";
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

        public const string LIST_FAST_TYPE_T = "global::EncosyTower.Collections.ListFast<";
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

        public const string MEMORY_EXTENSIONS = "global::System.MemoryExtensions";
        public const string ARRAY_EXTENSIONS = "global::EncosyTower.Collections.Extensions.EncosyArrayExtensions";
        public const string LIST_EXTENSIONS = "global::EncosyTower.Collections.Extensions.EncosyListExtensions";
        public const string DICTIONARY_EXTENSIONS = "global::EncosyTower.Collections.Extensions.EncosyDictionaryExtensions";
        public const string LIST_FAST_EXTENSIONS_UNSAFE = "global::EncosyTower.Collections.Extensions.Unsafe.ListFastExtensionsUnsafe";
        public const string HASH_SET_API = "global::EncosyTower.Collections.Extensions.HashSetAPI";
        public const string REFERENCE_EXTENSIONS = "global::EncosyTower.SystemExtensions.EncosyReferenceExtensions";

        public const string PR_DONT_CREATE_PROPERTY = "UP.DontCreatePropertyAttribute";

        public const string PR_AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Data.DataGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";

        public const string PR_MEMORY_EXTENSIONS = "S.MemoryExtensions";
        public const string PR_ARRAY_EXTENSIONS = "ETCE.EncosyArrayExtensions";
        public const string PR_LIST_EXTENSIONS = "ETCE.EncosyListExtensions";
        public const string PR_HASH_SET_API = "ETCE.HashSetAPI";
        public const string PR_DICTIONARY_EXTENSIONS = "ETCE.EncosyDictionaryExtensions";

        public const string PR_GENERATED_PROPERTY_FROM_FIELD = $"[ETDSG.GeneratedPropertyFromField(nameof({{0}}), typeof({{1}}))]";
        public const string PR_GENERATED_FIELD_FROM_PROPERTY = $"[ETDSG.GeneratedFieldFromProperty(nameof({{0}}))]";
    }
}
