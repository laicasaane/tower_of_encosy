using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
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

        public const string DEFINE_DATABASE_AUTHORING = "#define DATABASE_AUTHORING";
        public const string DEFINE_BAKING_SHEET = "#define BAKING_SHEET";

        public const string DEFINE_NO_DATABASE_AUTHORING = "#define __NO_DATABASE_AUTHORING__";
        public const string DEFINE_NO_BAKING_SHEET = "#define __NO_BAKING_SHEET__";

        public const string DIRECTIVE = "#if DATABASE_AUTHORING && BAKING_SHEET";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";
        public const string DATA_ATTRIBUTE = $"{DATA_NAMESPACE}.DataAttribute";
        public const string DATA_PROPERTY_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataPropertyAttribute";
        public const string DATA_CONVERTER_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataConverterAttribute";

        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";

        public const string LIST_FAST_TYPE_T = "global::EncosyTower.Collections.ListFast<";
        public const string LIST_TYPE_T = "global::System.Collections.Generic.List<";
        public const string DICTIONARY_TYPE_T = "global::System.Collections.Generic.Dictionary<";
        public const string HASH_SET_TYPE_T = "global::System.Collections.Generic.HashSet<";
        public const string QUEUE_TYPE_T = "global::System.Collections.Generic.Queue<";
        public const string STACK_TYPE_T = "global::System.Collections.Generic.Stack<";
        public const string VERTICAL_LIST_TYPE = "global::Cathei.BakingSheet.VerticalList<";

        public const string IREADONLY_LIST_TYPE_T = "global::System.Collections.Generic.IReadOnlyList<";
        public const string ILIST_TYPE_T = "global::System.Collections.Generic.IList<";
        public const string ISET_TYPE_T = "global::System.Collections.Generic.ISet<";
        public const string IREADONLY_DICTIONARY_TYPE_T = "global::System.Collections.Generic.IReadOnlyDictionary<";
        public const string IDICTIONARY_TYPE_T = "global::System.Collections.Generic.IDictionary<";
        public const string READONLY_MEMORY_TYPE_T = "global::System.ReadOnlyMemory<";
        public const string READONLY_SPAN_TYPE_T = "global::System.ReadOnlySpan<";
        public const string MEMORY_TYPE_T = "global::System.Memory<";
        public const string SPAN_TYPE_T = "global::System.Span<";

        public const string GENERATED_PROPERTY_FROM_FIELD = $"global::{DATA_NAMESPACE}.SourceGen.GeneratedPropertyFromFieldAttribute";
        public const string GENERATED_FIELD_FROM_PROPERTY = $"global::{DATA_NAMESPACE}.SourceGen.GeneratedFieldFromPropertyAttribute";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string SERIALIZABLE = "[Serializable]";
        public const string STRUCT_LAYOUT_AUTO = "[StructLayout(LayoutKind.Auto)]";

        public const string DATABASES_AUTHORING_NAMESPACE = $"{DATABASES_NAMESPACE}.Authoring";
        public const string ICONTAINS = $"EDASourceGen.IContains";
        public const string AUTHOR_DATABASE_ATTRIBUTE = $"EDAuthoring.AuthorDatabaseAttribute";
        public const string HORIZONTAL_LIST_ATTRIBUTE = $"EDAuthoring.HorizontalAttribute";
        public const string DATA_SHEET_CONTAINER_BASE = $"EDAuthoring.DataSheetContainerBase";
        public const string TABLE_NAMING = $"[EDASourceGen.TableNaming(\"{{0}}\", ENaming.NamingStrategy.{{1}})]";
        public const string GENERATED_SHEET_CONTAINER = $"[EDASourceGen.GeneratedSheetContainer]";
        public const string GENERATED_SHEET_ATTRIBUTE = $"[EDASourceGen.GeneratedSheet(typeof({{0}}), typeof({{1}}), typeof({{2}}), \"{{3}}\")]";
        public const string GENERATED_SHEET_ROW = $"[EDASourceGen.GeneratedSheetRow(typeof({{0}}), typeof({{1}}))]";
        public const string GENERATED_DATA_ROW = $"[EDASourceGen.GeneratedDataRow(typeof({{0}}))]";

        public static bool IsIData(ITypeSymbol type)
            => type.HasAttribute(DATA_ATTRIBUTE) || type.InheritsFromInterface(IDATA);
    }
}
