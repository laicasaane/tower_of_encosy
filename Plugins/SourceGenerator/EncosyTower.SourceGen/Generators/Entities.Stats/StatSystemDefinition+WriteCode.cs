using System;
using System.Collections.Generic;
using System.Linq;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatSystemDefinition
    {
        private const string METHOD_IMPL_OPTIONS = "global::System.Runtime.CompilerServices.MethodImplOptions";
        private const string INLINING = $"{METHOD_IMPL_OPTIONS}.AggressiveInlining";
        private const string GENERATOR = "\"EncosyTower.SourceGen.Generators.Entities.Stats.StatSystemGenerator\"";

        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(INLINING)]";
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string SERIALIZABLE = "[global::System.Serializable]";
        private const string SERIALIZED_FIELD = "[global::UnityEngine.SerializeField]";
        private const string BURST_COMPILE = "[global::Unity.Burst.BurstCompile]";
        private const string STRUCT_LAYOUT_EXPLICIT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        private const string FIELD_OFFSET_0 = "[global::System.Runtime.InteropServices.FieldOffset(0)]";
        private const string FIELD_OFFSET_X = "[global::System.Runtime.InteropServices.FieldOffset({0})]";
        private const string VALIDATION_ATTRIBUTES = "[global::UnityEngine.HideInCallstack, " +
            "global::System.Diagnostics.StackTraceHidden, " +
            "global::System.Diagnostics.Conditional(\"UNITY_EDITOR\"), " +
            "global::System.Diagnostics.Conditional(\"DEVELOPMENT_BUILD\")]";

        private const string IEQUATABLE = "global::System.IEquatable";
        private const string HASH_VALUE = "global::EncosyTower.Common.HashValue";
        private const string BYTE_BOOL = "global::EncosyTower.Common.ByteBool";

        private const string JOB_NS = "global::Unity.Jobs";
        private const string IJOB = $"{JOB_NS}.IJob";
        private const string JOB_HANDLE = $"{JOB_NS}.JobHandle";

        private const string ENTITIES_NS = "global::Unity.Entities";
        private const string IBUFFER_ELEMENT_DATA = $"{ENTITIES_NS}.IBufferElementData";
        private const string IBAKER = $"{ENTITIES_NS}.IBaker";
        private const string ENTITY = $"{ENTITIES_NS}.Entity";
        private const string ENTITY_MANAGER = $"{ENTITIES_NS}.EntityManager";
        private const string ECB = $"{ENTITIES_NS}.EntityCommandBuffer";
        private const string ECB_WRITER = $"{ENTITIES_NS}.EntityCommandBuffer.ParallelWriter";
        private const string STAT_BUFFER = $"{ENTITIES_NS}.DynamicBuffer<Stat>";
        private const string LOOKUP_STAT = $"{ENTITIES_NS}.BufferLookup<Stat>";
        private const string LOOKUP_OBSERVER = $"{ENTITIES_NS}.BufferLookup<StatObserver>";
        private const string LOOKUP_MODIFIER = $"{ENTITIES_NS}.BufferLookup<StatModifier>";
        private const string SYSTEM_STATE = $"{ENTITIES_NS}.SystemState";

        private const string RO_SPAN_OBSERVER = "global::System.ReadOnlySpan<StatObserver>";
        private const string RO_SPAN_MODIFIER = "global::System.ReadOnlySpan<StatModifier>";
        private const string RO_SPAN_STAT = "global::System.ReadOnlySpan<Stat>";

        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string MODIFIER_RANGE = $"global::{NAMESPACE}.ModifierRange";
        private const string OBSERVER_RANGE = $"global::{NAMESPACE}.ObserverRange";
        private const string ISTAT_DATA = $"global::{NAMESPACE}.IStatData";
        private const string ISTAT_VALUE_PAIR = $"global::{NAMESPACE}.IStatValuePair";
        private const string ISTAT_VALUE_PAIR_COMPOSER = $"global::{NAMESPACE}.IStatValuePairComposer<ValuePair>";
        private const string ISTAT = $"global::{NAMESPACE}.IStat<ValuePair>";
        private const string ISTAT_MODIFIER_STACK = $"global::{NAMESPACE}.IStatModifierStack<ValuePair, Stat>";
        private const string ISTAT_MODIFIER = $"global::{NAMESPACE}.IStatModifier<ValuePair, Stat, StatModifier.Stack>";
        private const string ISTAT_OBSERVER = $"global::{NAMESPACE}.IStatObserver";
        private const string STAT_VARIANT_TYPE = $"global::{NAMESPACE}.StatVariantType";
        private const string STAT_VARIANT = $"global::{NAMESPACE}.StatVariant";
        private const string STAT_HANDLE = $"global::{NAMESPACE}.StatHandle";
        private const string STAT_HANDLE_T = $"global::{NAMESPACE}.StatHandle<TStatData>";
        private const string STAT_MODIFIER_HANDLE = $"global::{NAMESPACE}.StatModifierHandle";
        private const string STAT_READER = $"global::{NAMESPACE}.StatReader<ValuePair, Stat>";
        private const string STAT_VALUE_TYPE_EXCEPTION = $"global::{NAMESPACE}.StatValueTypeException";
        private const string STAT_CHANGE_EVENT = $"global::{NAMESPACE}.StatChangeEvent<ValuePair>";
        private const string MODIFIER_TRIGGER_EVENT = $"global::{NAMESPACE}.ModifierTriggerEvent<ValuePair, Stat, StatModifier, StatModifier.Stack>";
        private const string STAT_MODIFIER_RECORD = $"global::{NAMESPACE}.StatModifierRecord<ValuePair, Stat, StatModifier, StatModifier.Stack>";

        private const string TYPES_6 = "ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer";
        private const string TYPES_5 = "ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver";

        private const string STAT_API = $"global::{NAMESPACE}.StatAPI";
        private const string STAT_WORLD_DATA = $"global::{NAMESPACE}.StatWorldData<{TYPES_5}>";
        private const string STAT_ACCESSOR = $"global::{NAMESPACE}.StatAccessor<{TYPES_6}>";
        private const string STAT_BAKER = $"global::{NAMESPACE}.StatBaker<{TYPES_6}>";
        private const string DEFERRED_LIST_JOB = $"global::{NAMESPACE}.DeferredUpdateStatListJob<{TYPES_6}>";
        private const string DEFERRED_QUEUE_JOB = $"global::{NAMESPACE}.DeferredUpdateStatQueueJob<{TYPES_6}>";
        private const string DEFERRED_STREAM_JOB = $"global::{NAMESPACE}.DeferredUpdateStatStreamJob<{TYPES_6}>";
        private const string DEFERRED_UNSAFE_BLOCK_LIST_JOB = $"global::{NAMESPACE}.DeferredUpdateStatUnsafeBlockListJob<{TYPES_6}>";

        private const string COLLECTIONS_NS = "global::Unity.Collections";
        private const string UNSAFE_UTILITY = $"{COLLECTIONS_NS}.LowLevel.Unsafe.UnsafeUtility";
        private const string ALLOCATOR_HANDLE = $"{COLLECTIONS_NS}.AllocatorManager.AllocatorHandle";
        private const string NATIVE_LIST_STAT_HANDLE = $"{COLLECTIONS_NS}.NativeList<{STAT_HANDLE}>";
        private const string NATIVE_QUEUE_STAT_HANDLE = $"{COLLECTIONS_NS}.NativeQueue<{STAT_HANDLE}>";
        private const string NATIVE_STREAM_READER = $"{COLLECTIONS_NS}.NativeStream.Reader";
        private const string NATIVE_LIST_OBSERVER = $"{COLLECTIONS_NS}.NativeList<StatObserver>";
        private const string NATIVE_SET_ENTITY = $"{COLLECTIONS_NS}.NativeHashSet<{ENTITY}>";
        private const string NATIVE_LIST_MODIFIER_HANDLE = $"{COLLECTIONS_NS}.NativeList<{STAT_MODIFIER_HANDLE}>";
        private const string NATIVE_ARRAY_MODIFIER_EVENT = $"{COLLECTIONS_NS}.NativeArray<ModifierTriggerEvent>";
        private const string NATIVE_LIST_MODIFIER_EVENT = $"{COLLECTIONS_NS}.NativeList<ModifierTriggerEvent>";
        private const string NATIVE_LIST_MODIFIER_EVENT_GENERIC = $"{COLLECTIONS_NS}.NativeList<{MODIFIER_TRIGGER_EVENT}>";
        private const string NATIVE_ARRAY_STAT_CHANGE_EVENT = $"{COLLECTIONS_NS}.NativeArray<{STAT_CHANGE_EVENT}>";
        private const string NATIVE_LIST_STAT_CHANGE_EVENT = $"{COLLECTIONS_NS}.NativeList<{STAT_CHANGE_EVENT}>";
        private const string NATIVE_LIST_MODIFIER_RECORD = $"{COLLECTIONS_NS}.NativeList<StatModifierRecord>";
        private const string NATIVE_LIST_MODIFIER_RECORD_GENERIC = $"{COLLECTIONS_NS}.NativeList<{STAT_MODIFIER_RECORD}>";

        private const string UNSAFE_BLOCK_LIST_HANDLE = $"global::Latios.Unsafe.UnsafeParallelBlockList<{STAT_HANDLE}>";

        private delegate void WriteAction(ref Printer p);

        public readonly string WriteCode(References references)
        {
            FilterTypes(out var singleTypes, out var pairTypes);

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            var keyword = syntax.Keyword.ValueText;
            var statSystemName = syntax.Identifier.Text;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("partial ").Print(keyword).Print(" ").PrintEndLine(statSystemName);
                p.OpenScope();
                {
                    WriteIsCompatibleMethod(ref p, typeName, singleTypes, pairTypes);
                    WriteStat(ref p);
                    WriteStatObserver(ref p);
                    WriteStatModifier(ref p);
                    WriteWrappers(ref p);
                    WriteJobs(ref p, references);
                    WriteValuePair(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial ").Print(keyword).Print(" ").PrintEndLine(statSystemName);
                p.OpenScope();
                {
                    p.PrintBeginLine("private const ").Print(METHOD_IMPL_OPTIONS)
                        .Print(" INLINING = ").Print(INLINING).PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
                    p.PrintEndLine();

                    WriteStatWithStatDataImp(ref p);
                    WriteAPIImpl(ref p);
                    WriteReaderImpl(ref p);
                    WriteAccessorImpl(ref p);
                    WriteBakerImpl(ref p);
                    WriteWorldDataImpl(ref p);
                    WriteValuePairImpl(ref p, singleTypes, pairTypes);
                    WriteThrowMethods(ref p, typeName);
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void FilterTypes(out List<TypeRecord> singleTypes, out List<TypeRecord> pairTypes)
        {
            var types = StatGeneratorAPI.Types.AsSpan();
            var typeNames = StatGeneratorAPI.TypeNames.AsSpan();
            var namespaces = StatGeneratorAPI.Namespaces.AsSpan();
            var sizes = StatGeneratorAPI.Sizes.AsSpan();
            var maxSize = maxDataSize;
            var halfSize = maxSize / 2;
            singleTypes = new List<TypeRecord>(types.Length);
            pairTypes = new List<TypeRecord>(types.Length);

            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var typeName = typeNames[i];
                var ns = namespaces[i];
                var customNs = string.IsNullOrEmpty(ns) == false;
                var size = sizes[i];

                if (size > maxSize)
                {
                    continue;
                }

                var record = new TypeRecord(ns, type, typeName, size, customNs);

                singleTypes.Add(record);

                if (size <= halfSize)
                {
                    pairTypes.Add(record);
                }
            }
        }

        private static void WriteStat(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct Stat ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(STAT_HANDLE).PrintEndLine(" _handle;");
                p.PrintEndLine();

                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(MODIFIER_RANGE).PrintEndLine(" _modifierRange;");
                p.PrintEndLine();

                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(OBSERVER_RANGE).PrintEndLine(" _observerRange;");
                p.PrintEndLine();

                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(BYTE_BOOL).PrintEndLine(" _produceChangeEvents;");
                p.PrintEndLine();

                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintLine("private ValuePair _valuePair;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(STAT_HANDLE).PrintEndLine(" Handle");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _handle;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _handle = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(MODIFIER_RANGE).PrintEndLine(" ModifierRange");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _modifierRange;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _modifierRange = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(OBSERVER_RANGE).PrintEndLine(" ObserverRange");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _observerRange;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _observerRange = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public bool ProduceChangeEvents");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _produceChangeEvents;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _produceChangeEvents = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public ValuePair ValuePair");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _valuePair;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _valuePair = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> ValuePair.GetBaseValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> ValuePair.GetCurrentValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetBaseValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("var valuePair = ValuePair;");
                    p.PrintEndLine();

                    p.PrintLine("if (valuePair.TrySetBaseValue(value) == false) return false;");
                    p.PrintEndLine();

                    p.PrintLine("ValuePair = valuePair;");
                    p.PrintLine("return true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetCurrentValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("var valuePair = ValuePair;");
                    p.PrintEndLine();

                    p.PrintLine("if (valuePair.TrySetCurrentValue(value) == false) return false;");
                    p.PrintEndLine();

                    p.PrintLine("ValuePair = valuePair;");
                    p.PrintLine("return true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetValues(in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("var valuePair = ValuePair;");
                    p.PrintEndLine();

                    p.PrintLine("if (valuePair.TrySetValues(baseValue, currentValue) == false) return false;");
                    p.PrintEndLine();

                    p.PrintLine("ValuePair = valuePair;");
                    p.PrintLine("return true;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteStatWithStatDataImp(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Stat<TStatData>");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.OpenScope();
            {
                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintLine("private Stat _value;");
                p.PrintEndLine();

                p.PrintLine("static Stat()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCompatible(default(TStatData));");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public Stat(in Stat value)");
                p.OpenScope();
                {
                    p.PrintLine("_value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(STAT_HANDLE).PrintEndLine(" Handle");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _value.Handle;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _value.Handle = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(MODIFIER_RANGE).PrintEndLine(" ModifierRange");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _value.ModifierRange;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _value.ModifierRange = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(OBSERVER_RANGE).PrintEndLine(" ObserverRange");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _value.ObserverRange;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _value.ObserverRange = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public bool ProduceChangeEvents");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _value.ProduceChangeEvents;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _value.ProduceChangeEvents = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public TStatData Data");
                p.OpenScope();
                {
                    p.PrintLine("readonly get");
                    p.OpenScope();
                    {
                        p.PrintLine("var valuePair = _value.ValuePair;");
                        p.PrintLine("var result = new TStatData { BaseValue = valuePair.GetBaseValueOrDefault() };");
                        p.PrintEndLine();

                        p.PrintLine("if (result.IsValuePair)");
                        p.OpenScope();
                        {
                            p.PrintLine("result.CurrentValue = valuePair.GetCurrentValueOrDefault();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("set");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = _value;");
                        p.PrintEndLine();

                        p.PrintLine("if (value.IsValuePair)");
                        p.OpenScope();
                        {
                            p.PrintLine("stat.TrySetValues(value.BaseValue, value.CurrentValue);");
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintLine("stat.TrySetBaseValue(value.BaseValue);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("_value = stat;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public ValuePair ValuePair");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _value.ValuePair;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = _value;");
                        p.PrintLine("stat.ValuePair = value;");
                        p.PrintLine("_value = stat;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static implicit operator Stat<TStatData>(in Stat value)");
                p.WithIncreasedIndent().PrintLine("=> new(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static implicit operator Stat(in Stat<TStatData> value)");
                p.WithIncreasedIndent().PrintLine("=> value._value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> _value.GetBaseValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> _value.GetCurrentValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetBaseValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("var stat = _value;");
                    p.PrintLine("var result = stat.TrySetBaseValue(value);");
                    p.PrintLine("if (result) _value = stat;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetCurrentValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("var stat = _value;");
                    p.PrintLine("var result = stat.TrySetCurrentValue(value);");
                    p.PrintLine("if (result) _value = stat;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetValues(in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("var stat = _value;");
                    p.PrintLine("var result = stat.TrySetValues(baseValue, currentValue);");
                    p.PrintLine("if (result) _value = stat;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteStatObserver(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct StatObserver ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT_OBSERVER)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(STAT_HANDLE).PrintEndLine(" _observerHandle;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(STAT_HANDLE).PrintEndLine(" ObserverHandle");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => _observerHandle;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => _observerHandle = value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteStatModifier(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct StatModifier ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT_MODIFIER)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public uint Id");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get");
                    p.OpenScope();
                    {
                        p.PrintLine("uint result = default;");
                        p.PrintLine("GetIdInternal(ref result);");
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set");
                    p.OpenScope();
                    {
                        p.PrintLine("SetIdInternal(value);");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("readonly partial void GetIdInternal(ref uint id);");
                p.PrintEndLine();

                p.PrintLine("partial void SetIdInternal(uint value);");

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly void AddObservedStatsToList(")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" observedStatHandles)");
                p.OpenScope();
                {
                    p.PrintLine("AddObservedStatsToListInternal(observedStatHandles);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("readonly partial void AddObservedStatsToListInternal(")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" observedStatHandles);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void Apply(").Print(STAT_READER)
                    .PrintEndLine(" reader, ref StatModifier.Stack stack, out bool shouldProduceModifierTriggerEvent)");
                p.OpenScope();
                {
                    p.PrintLine("shouldProduceModifierTriggerEvent = default;");
                    p.PrintLine("ApplyInternal(reader, ref stack, ref shouldProduceModifierTriggerEvent);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("partial void ApplyInternal(Reader reader, ")
                    .PrintEndLine("ref StatModifier.Stack stack, ref bool shouldProduceModifierTriggerEvent);");
                p.PrintEndLine();

                WriteStack(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteStack(ref Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial struct Stack ")
                    .Print(": ").Print(ISTAT_MODIFIER_STACK)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void Apply(in ").Print(STAT_VARIANT).Print(" baseValue, ref ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("ApplyInternal(baseValue, ref currentValue);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("partial void ApplyInternal(in ").Print(STAT_VARIANT).Print(" baseValue, ref ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public void Reset(in Stat stat)");
                    p.OpenScope();
                    {
                        p.PrintLine("ResetInternal(stat);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("partial void ResetInternal(in Stat stat);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteWrappers(ref Printer p)
        {
            p.PrintLine("public partial struct Stat<TStatData> { }");
            p.PrintEndLine();

            WriteModifierAndHandleRecord(ref p, "ModifierTriggerEvent");
            WriteModifierAndHandleRecord(ref p, "StatModifierRecord");

            p.PrintLine("public static partial class API { }");
            p.PrintEndLine();

            WriteAPI(ref p, "Reader", STAT_READER, WriteReaderConstructor);
            WriteAPI(ref p, "Accessor", STAT_ACCESSOR, WriteAccessorConstructor);
            WriteAPI(ref p, "Baker", STAT_BAKER, WriteBakerConstructor);
            WriteAPI(ref p, "WorldData", STAT_WORLD_DATA, WriteWorldDataConstructor);

            return;

            static void WriteAPI(ref Printer p, string name, string typeName, WriteAction constructorWriter)
            {
                p.PrintBeginLine("public partial struct ").PrintEndLine(name);
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public ").Print(typeName).PrintEndLine(" value;");
                    p.PrintEndLine();

                    constructorWriter(ref p);

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("private ").Print(name).Print("(in ").Print(typeName).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("this.value = value;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static implicit operator ").Print(name).Print("(in ")
                        .Print(typeName).PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> new(value);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static implicit operator ").Print(typeName).Print("(in ")
                        .Print(name).PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> value.value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteReaderConstructor(ref Printer p)
            {
            }

            static void WriteAccessorConstructor(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public Accessor(ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(" state) : this(ref state, default) { }");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public Accessor(ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(" state, ValuePair.Composer composer)");
                p.OpenScope();
                {
                    p.PrintLine("this.value = new(ref state, composer);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteBakerConstructor(ref Printer p)
            {
            }

            static void WriteWorldDataConstructor(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public WorldData(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.OpenScope();
                {
                    p.PrintLine("this.value = new(allocator);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteModifierAndHandleRecord(ref Printer p, string name)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public struct ").PrintEndLine(name);
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public ").Print(STAT_MODIFIER_HANDLE).PrintEndLine(" handle;");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE);
                    p.PrintLine("public StatModifier modifier;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(name)
                        .Print("(").Print(STAT_MODIFIER_HANDLE).PrintEndLine(" handle, StatModifier modifier)");
                    p.OpenScope();
                    {
                        p.PrintLine("this.handle = handle;");
                        p.PrintLine("this.modifier = modifier;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                    
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public readonly void Deconstruct(out ")
                        .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" handle, out StatModifier modifier)");
                    p.OpenScope();
                    {
                        p.PrintLine("handle = this.handle;");
                        p.PrintLine("modifier = this.modifier;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteJobs(ref Printer p, References references)
        {
            Write(ref p, "DeferredUpdateStatListJob", DEFERRED_LIST_JOB, NATIVE_LIST_STAT_HANDLE);
            Write(ref p, "DeferredUpdateStatQueueJob", DEFERRED_QUEUE_JOB, NATIVE_QUEUE_STAT_HANDLE);
            Write(ref p, "DeferredUpdateStatStreamJob", DEFERRED_STREAM_JOB, NATIVE_STREAM_READER);

            if (references.latiosCore)
            {
                Write(ref p, "DeferredUpdateStatUnsafeBlockListJob", DEFERRED_UNSAFE_BLOCK_LIST_JOB, UNSAFE_BLOCK_LIST_HANDLE);
            }

            return;

            static void Write(ref Printer p, string name, string typeName, string collectionName)
            {
                p.PrintLine(BURST_COMPILE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public struct ").Print(name).Print(" : ").PrintEndLine(IJOB);
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("private ").Print(typeName).PrintEndLine(" _jobData;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(name)
                        .Print("(in Accessor accessor, in WorldData worldData, ")
                        .Print(collectionName).PrintEndLine(" statsToUpdate)");
                    p.OpenScope();
                    {
                        p.PrintLine("_jobData = new()");
                        p.OpenScope();
                        {
                            p.PrintLine("statAccessor = accessor.value,");
                            p.PrintLine("statWorldData = worldData.value,");
                            p.PrintLine("statsToUpdate = statsToUpdate,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public void Execute()");
                    p.WithIncreasedIndent().PrintLine("=> _jobData.Execute();");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteValuePair(ref Printer p)
        {
            p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct ValuePair ")
                .Print(": ").Print(ISTAT_VALUE_PAIR)
                .Print(", ").Print(IEQUATABLE).Print("<ValuePair>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(SERIALIZED_FIELD).PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(STAT_VARIANT_TYPE).PrintEndLine(" _type;");
                p.PrintEndLine();
                
                p.PrintLine(SERIALIZED_FIELD).PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private ").Print(BYTE_BOOL).PrintEndLine(" _isPair;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD).PrintLine(GENERATED_CODE);
                p.PrintLine("private DataStorage _storage;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ValuePair(bool isPair, in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("_storage = DataStorage.Store(isPair, baseValue, currentValue);");
                    p.PrintLine("_type = baseValue.Type;");
                    p.PrintLine("_isPair = isPair;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT_TYPE).PrintEndLine(" Type");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _type;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool IsPair");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _isPair;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is ValuePair other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool Equals(ValuePair other)");
                p.WithIncreasedIndent().PrintLine("=> DataStorage.Equals(this, other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> DataStorage.GetHashCode(this);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent()
                    .PrintLine("=> DataStorage.TryGetBaseValue(this, out var baseValue) ? baseValue : defaultValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent()
                    .PrintLine("=> DataStorage.TryGetCurrentValue(this, out var baseValue) ? baseValue : defaultValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetBaseValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> DataStorage.TrySetBaseValue(ref this, value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetCurrentValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> DataStorage.TrySetCurrentValue(ref this, value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetValues(in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.WithIncreasedIndent()
                    .PrintLine("=> DataStorage.TrySetValues(ref this, baseValue, currentValue);");
                p.PrintEndLine();

                WriteComposer(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteComposer(ref Printer p)
            {
                p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial struct Composer ")
                    .Print(": ").Print(ISTAT_VALUE_PAIR_COMPOSER)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ValuePair Compose(bool isPair, in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("var result = new ValuePair(isPair, baseValue, currentValue);");
                        p.PrintLine("OnCompose(isPair, baseValue, currentValue, ref result);");
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("partial void OnCompose(bool isPair, in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue, ref ValuePair result);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteAPIImpl(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial class API");
            p.OpenScope();
            {
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(ENTITY_MANAGER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ENTITY_MANAGER).PrintEndLine(" entityManager)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, entityManager);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(ECB)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ECB).PrintEndLine(" ecb)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, ecb);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(ECB_WRITER).Print(", int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ECB_WRITER).PrintEndLine(" ecb, int sortKey)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, ecb, sortKey);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".BakeStatComponents{ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer}(")
                    .Print(IBAKER).Print(", ").Print(ENTITY)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static Baker BakeStatComponents(")
                    .Print(IBAKER).Print(" baker, ")
                    .Print(ENTITY).PrintEndLine(" entity)");
                p.OpenScope();
                {
                    p.PrintBeginLine(STAT_API).Print(".BakeStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer")
                        .PrintEndLine(">(baker, entity, out var statBaker);");
                    p.PrintLine("return statBaker;");
                }
                p.CloseScope();
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".CreateStat{ValuePair, Stat, TStatData, ValuePair.Composer}(")
                    .Print(ENTITY).Print(", TStatData, bool, ref ").Print(STAT_BUFFER).Print(", ValuePair.Composer")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ").Print(STAT_HANDLE_T).Print(" CreateStat<TStatData>(")
                    .Print(ENTITY).Print(" entity, TStatData statData, bool produceChangeEvents, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer, ValuePair.Composer composer)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".CreateStat<ValuePair, Stat, TStatData, ValuePair.Composer>")
                    .PrintEndLine("(entity, statData, produceChangeEvents, ref statBuffer, composer);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".CreateStat{ValuePair, Stat}(")
                    .Print(ENTITY).Print(", ValuePair, bool, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ").Print(STAT_HANDLE).Print(" CreateStat(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, bool produceChangeEvents, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".CreateStat<ValuePair, Stat>(entity, valuePair, produceChangeEvents, ref statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".EntityHasAnyOtherDependantStatEntities{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void EntityHasAnyOtherDependantStatEntities(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).PrintEndLine(" lookupObservers)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, lookupObservers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".EntityHasAnyOtherDependantStatEntities{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void EntityHasAnyOtherDependantStatEntities(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).PrintEndLine(" observerBufferOnEntity)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, observerBufferOnEntity);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatEntitiesOfEntity{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER).Print(", ").Print(NATIVE_SET_ENTITY)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetOtherDependantStatEntitiesOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_SET_ENTITY).PrintEndLine(" dependentEntities)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, lookupObservers, dependentEntities);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatEntitiesOfEntity{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER).Print(", ").Print(NATIVE_SET_ENTITY)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetOtherDependantStatEntitiesOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).Print(" observerBufferOnEntity, ")
                    .Print(NATIVE_SET_ENTITY).PrintEndLine(" dependentEntities)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentEntities);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatsOfEntity{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER).Print(", ").Print(NATIVE_LIST_STAT_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetOtherDependantStatsOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" dependentStats)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatsOfEntity<StatObserver>(entity, lookupObservers, dependentStats);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatsOfEntity{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER).Print(", ").Print(NATIVE_LIST_STAT_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetOtherDependantStatsOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).Print(" observerBufferOnEntity, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" dependentStats)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatsOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentStats);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatData{ValuePair, Stat, TStatData}(")
                    .Print(STAT_HANDLE_T).Print(", ").Print(RO_SPAN_STAT)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static TStatData GetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE_T).Print(", ").Print(RO_SPAN_STAT)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static Stat<TStatData> GetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStat<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static Stat GetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStat<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatValue{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ValuePair GetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStatValue<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatEntitiesThatEntityDependsOn{ValuePair, Stat, StatModifier, StatModifier.Stack}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER).Print(", ")
                    .Print(NATIVE_SET_ENTITY).Print(", ").Print(NATIVE_LIST_STAT_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetStatEntitiesThatEntityDependsOn(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_MODIFIER).Print(" lookupModifiers, ")
                    .Print(NATIVE_SET_ENTITY).Print(" dependsOnEntities, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" observerStatHandles)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".GetStatEntitiesThatEntityDependsOn<ValuePair, Stat, StatModifier, StatModifier.Stack>")
                    .PrintEndLine("(entity, lookupModifiers, dependsOnEntities, observerStatHandles);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatEntitiesThatEntityDependsOn{ValuePair, Stat, StatModifier, StatModifier.Stack}(")
                    .Print(RO_SPAN_MODIFIER).Print(", ").Print(NATIVE_SET_ENTITY).Print(", ")
                    .Print(NATIVE_LIST_STAT_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void GetStatEntitiesThatEntityDependsOn(")
                    .Print(RO_SPAN_MODIFIER).Print(" modifierBufferOnEntity, ")
                    .Print(NATIVE_SET_ENTITY).Print(" dependsOnEntities, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" observerStatHandles)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".GetStatEntitiesThatEntityDependsOn<ValuePair, Stat, StatModifier, StatModifier.Stack>")
                    .PrintEndLine("(modifierBufferOnEntity, dependsOnEntities, observerStatHandles);");
                p.PrintEndLine();
                
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetAllObservers{StatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER).Print(", ").Print(NATIVE_LIST_OBSERVER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetAllObservers(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetAllObservers<StatObserver>(entity, lookupObservers, observers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetModifiersCount{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetModifiersCount(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out int modifiersCount)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetModifiersCount<ValuePair, Stat>(statHandle, lookupStats, out modifiersCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetModifiersOfStat{ValuePair, Stat, StatModifier, StatModifier.Stack}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", ")
                    .Print(LOOKUP_MODIFIER).Print(", ").Print(NATIVE_LIST_MODIFIER_RECORD)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).Print(" lookupStats, ")
                    .Print(LOOKUP_MODIFIER).Print(" lookupModifiers, ")
                    .Print(NATIVE_LIST_MODIFIER_RECORD).PrintEndLine(" modifiers)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var modifiersTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_RECORD).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC)
                        .PrintEndLine(">(ref modifiers);");

                    p.PrintBeginLine("return ").Print(STAT_API)
                        .Print(".TryGetModifiersOfStat<ValuePair, Stat, StatModifier, StatModifier.Stack>(")
                        .PrintEndLine("statHandle, lookupStats, lookupModifiers, modifiersTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetObserversCount{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetObserversCount(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out int observersCount)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetObserversCount<ValuePair, Stat>(statHandle, lookupStats, out observersCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetObserversOfStat{ValuePair, Stat, StatObserver}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", ").Print(NATIVE_LIST_OBSERVER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetObserversOfStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).Print(" lookupStats, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".TryGetObserversOfStat<ValuePair, Stat, StatObserver>(")
                    .PrintEndLine("statHandle, lookupStats, lookupObservers, observers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatData{ValuePair, Stat, TStatData}(")
                    .Print(STAT_HANDLE_T).Print(", ").Print(LOOKUP_STAT).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatData<ValuePair, Stat, TStatData>(statHandle, lookupStats, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatData{ValuePair, Stat, TStatData}(")
                    .Print(STAT_HANDLE_T).Print(", ").Print(RO_SPAN_STAT).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", out Stat{TStatData}")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintBeginLine("var result = ").Print(STAT_API)
                        .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, lookupStats, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT).Print(", out Stat{TStatData}")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintBeginLine("var result = ").Print(STAT_API)
                        .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, statBuffer, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", out Stat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, lookupStats, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT).Print(", out Stat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, statBuffer, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatValue{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT).Print(", out ValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatValue<ValuePair, Stat>(statHandle, lookupStats, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatValue{ValuePair, Stat}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT).Print(", out ValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatValue<ValuePair, Stat>(statHandle, statBuffer, out valuePair);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteReaderImpl(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Reader");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => value.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool UseLookup");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => value.UseLookup;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent().PrintLine("=> value.TryGetStatData<TStatData>(statHandle, out statData);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = value.TryGetStat(statHandle, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out Stat stat)");
                p.WithIncreasedIndent().PrintLine("=> value.TryGetStat(statHandle, out stat);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out ValuePair valuePair)");
                p.WithIncreasedIndent().PrintLine("=> value.TryGetStatValue(statHandle, out valuePair);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteAccessorImpl(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Accessor");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, StatModifier statModifier, out ")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" statModifierHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryAddStatModifier(affectedStatHandle, statModifier, out statModifierHandle, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryAddStatModifiersBatch(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, ")
                    .Print(RO_SPAN_MODIFIER).Print(" modifiers, ")
                    .Print(NATIVE_LIST_MODIFIER_HANDLE).PrintEndLine(" statModifierHandles, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryAddStatModifiersBatch(affectedStatHandle, modifiers, statModifierHandles, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryCreateStat<TStatData>(")
                    .Print(ENTITY).Print(" entity, TStatData statData, bool produceChangeEvents, out ")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryCreateStat<TStatData>(entity, statData, produceChangeEvents, out statHandle);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryCreateStat(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, bool produceChangeEvents, out ")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryCreateStat(entity, valuePair, produceChangeEvents, out statHandle);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetAllObservers(")
                    .Print(ENTITY).Print(" entity, ").Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetAllObservers(entity, observers);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetObserversCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int observersCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetObserversCount(statHandle, out observersCount);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatData<TStatData>(statHandle, out statData);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatData<TStatData>(statHandle, statBuffer, out statData);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.value.TryGetStat(statHandle, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.value.TryGetStat(statHandle, statBuffer, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStat(statHandle, out stat);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStat(statHandle, statBuffer, out stat);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatValue(statHandle, out valuePair);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatValue(statHandle, statBuffer, out valuePair);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" modifierHandle, out StatModifier statModifier)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatModifier(modifierHandle, out statModifier);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatModifiersCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int modifiersCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryGetStatModifiersCount(statHandle, out modifiersCount);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryGetStatModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(NATIVE_LIST_MODIFIER_RECORD)
                    .PrintEndLine(" modifiers)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var modifiersTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_RECORD).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC)
                        .PrintEndLine(">(ref modifiers);");

                    p.PrintLine("return this.value.TryGetStatModifiersOfStat(statHandle, modifiersTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryRemoveAllStatModifiersOfStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryRemoveAllStatModifiersOfStat(statHandle, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryRemoveStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" modifierHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryRemoveStatModifier(modifierHandle, ref worldData.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ").Print(STAT_VARIANT).Print(" baseValue, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatBaseValue(statHandle, baseValue, ref statBuffer, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ").Print(STAT_VARIANT).Print(" baseValue, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatBaseValue(statHandle, baseValue, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ").Print(STAT_VARIANT).Print(" currentValue, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatCurrentValue(statHandle, currentValue, ref statBuffer, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ")
                    .Print(STAT_VARIANT).Print(" currentValue, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatCurrentValue(statHandle, currentValue, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatValues<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, TStatData statData, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatValues<TStatData>(statHandle, statData, ref statBuffer, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatValues<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, TStatData statData, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatValues<TStatData>(statHandle, statData, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatValues(").Print(STAT_HANDLE).Print(" statHandle, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).Print(" currentValue, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatValues(statHandle, baseValue, currentValue, ref statBuffer, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatValues(").Print(STAT_HANDLE).Print(" statHandle, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).Print(" currentValue, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatValues(statHandle, baseValue, currentValue, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatProduceChangeEvents(").Print(STAT_HANDLE)
                    .PrintEndLine(" statHandle, bool value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatProduceChangeEvents(statHandle, value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TrySetStatProduceChangeEvents(").Print(STAT_HANDLE)
                    .Print(" statHandle, bool value, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TrySetStatProduceChangeEvents(statHandle, value, ref statBuffer);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void TryUpdateAllStats(").Print(ENTITY)
                    .PrintEndLine(" entity, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintLine("=> this.value.TryUpdateAllStats(entity, ref worldData.value);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void TryUpdateStat(").Print(STAT_HANDLE)
                    .PrintEndLine(" statHandle, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintLine("=> this.value.TryUpdateStat(statHandle, ref worldData.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void Update(ref ").Print(SYSTEM_STATE).PrintEndLine(" state)");
                p.WithIncreasedIndent().PrintLine("=> this.value.Update(ref state);");
                p.PrintEndLine();

            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteBakerImpl(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Baker");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(STAT_HANDLE_T).Print(" CreateStat<TStatData>(")
                    .Print("TStatData statData, bool produceChangeEvents, ")
                    .PrintEndLine("ValuePair.Composer composer = default)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.CreateStat<TStatData>(statData, produceChangeEvents, composer);");
                p.PrintEndLine();
                
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(STAT_HANDLE)
                    .PrintEndLine(" CreateStat(ValuePair valuePair, bool produceChangeEvents)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.CreateStat(valuePair, produceChangeEvents);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, ")
                    .Print("StatModifier modifier, out ")
                    .Print(STAT_MODIFIER_HANDLE).Print(" statModifierHandle, ")
                    .PrintEndLine("ValuePair.Composer valuePairComposer = default)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.value.TryAddStatModifier(affectedStatHandle, modifier, out statModifierHandle, valuePairComposer);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteWorldDataImpl(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct WorldData");
            p.WithIncreasedIndent()
                .PrintLine(": global::System.IDisposable")
                .PrintLine(", global::Unity.Collections.INativeDisposable");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.value.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void AddModifierTriggerEvent(in ModifierTriggerEvent evt)");
                p.WithIncreasedIndent().PrintLine("=> this.value.AddModifierTriggerEvent(new(evt.handle, evt.modifier));");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void AddStatChangeEvent(in ").Print(STAT_CHANGE_EVENT).PrintEndLine(" evt)");
                p.WithIncreasedIndent().PrintLine("=> this.value.AddStatChangeEvent(evt);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void ClearModifierTriggerEvents()");
                p.WithIncreasedIndent().PrintLine("=> this.value.ClearModifierTriggerEvents();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void ClearStatChangeEvents()");
                p.WithIncreasedIndent().PrintLine("=> this.value.ClearStatChangeEvents();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void Dispose()");
                p.WithIncreasedIndent().PrintLine("=> this.value.Dispose();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(JOB_HANDLE).Print(" Dispose(")
                    .Print(JOB_HANDLE).PrintEndLine(" inputDeps)");
                p.WithIncreasedIndent().PrintLine("=> this.value.Dispose(inputDeps);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(NATIVE_ARRAY_MODIFIER_EVENT)
                    .Print(" GetModifierTriggerEvents(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintBeginLine("=> this.value.GetModifierTriggerEvents(allocator)")
                    .PrintEndLine(".Reinterpret<ModifierTriggerEvent>();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly void GetModifierTriggerEvents(")
                    .Print(NATIVE_LIST_MODIFIER_EVENT).PrintEndLine(" result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var resultTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_EVENT).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_EVENT_GENERIC)
                        .PrintEndLine(">(ref result);");
                    p.PrintLine("this.value.GetModifierTriggerEvents(resultTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly ").Print(NATIVE_ARRAY_STAT_CHANGE_EVENT)
                    .Print(" GetStatChangeEvents(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintLine("=> this.value.GetStatChangeEvents(allocator);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly void GetStatChangeEvents(")
                    .Print(NATIVE_LIST_STAT_CHANGE_EVENT).PrintEndLine(" result)");
                p.WithIncreasedIndent().PrintLine("=> this.value.GetStatChangeEvents(result);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public void SetStatModifiersStack(in StatModifier.Stack stack)");
                p.WithIncreasedIndent().PrintLine("=> this.value.SetStatModifiersStack(stack);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteValuePairImpl(ref Printer p, List<TypeRecord> types, List<TypeRecord> pairTypes)
        {
            p.PrintLine("partial struct ValuePair");
            p.OpenScope();
            {
                WriteDataStorage(ref p, types, pairTypes);
                WritePairTypes(ref p, pairTypes);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteDataStorage(ref Printer p, List<TypeRecord> singleTypes, List<TypeRecord> pairTypes)
        {
            p.PrintLine(SERIALIZABLE).PrintLine(STRUCT_LAYOUT_EXPLICIT);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("private struct DataStorage");
            p.OpenScope();
            {
                for (var i = 0; i < singleTypes.Count; i++)
                {
                    var (ns, type, typeName, _, customNs) = singleTypes[i];

                    p.PrintBeginLine(FIELD_OFFSET_0).PrintEndLine(GENERATED_CODE);
                    p.PrintBeginLine("public ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).Print(" ").Print(typeName).PrintEndLine(";");
                    p.PrintEndLine();
                }

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static DataStorage Store(bool isPair, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfMismatchedTypes(baseValue, currentValue);");
                    p.PrintLine("ThrowIfNotCompatible(baseValue, isPair);");
                    p.PrintEndLine();

                    p.PrintBeginLine("return isPair")
                        .Print(" ? StorePair(baseValue, currentValue)")
                        .Print(" : StoreSingle(baseValue, currentValue)").PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("static DataStorage StoreSingle(in ")
                        .Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("return baseValue.Type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => new DataStorage { ")
                                    .Print(typeName).Print(" = currentValue.")
                                    .Print(typeName).PrintEndLine(" },");
                            }

                            p.PrintLine("_ => default,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static DataStorage StorePair(in ")
                        .Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("return baseValue.Type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => new Pair").Print(typeName).Print("(baseValue.").Print(typeName)
                                    .Print(", currentValue.").Print(typeName).PrintEndLine(").storage,");
                            }

                            p.PrintLine("_ => default,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetBaseValue(in ValuePair pair, out ")
                    .Print(STAT_VARIANT).PrintEndLine(" result)");
                p.OpenScope();
                {
                    p.PrintLine("return pair._isPair ? TryGetPair(pair, out result) : TryGetSingle(pair, out result);");
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TryGetSingle(in ValuePair pair, out ")
                        .Print(STAT_VARIANT).PrintEndLine(" result)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("result = pair._storage.").Print(typeName)
                                        .PrintEndLine(";");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("result = default;");
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TryGetPair(in ValuePair pair, out ")
                        .Print(STAT_VARIANT).PrintEndLine(" result)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("result = new Pair").Print(typeName)
                                        .PrintEndLine("(pair._storage).baseValue;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("result = default;");
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetCurrentValue(in ValuePair pair, out ")
                    .Print(STAT_VARIANT).PrintEndLine(" result)");
                p.OpenScope();
                {
                    p.PrintLine("return pair._isPair ? TryGetPair(pair, out result) : TryGetSingle(pair, out result);");
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TryGetSingle(in ValuePair pair, out ")
                        .Print(STAT_VARIANT).PrintEndLine(" result)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("result = pair._storage.").Print(typeName).PrintEndLine(";");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("result = default;");
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                    
                    p.PrintBeginLine("static bool TryGetPair(in ValuePair pair, out ")
                        .Print(STAT_VARIANT).PrintEndLine(" result)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("result = new Pair").Print(typeName)
                                        .PrintEndLine("(pair._storage).currentValue;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("result = default;");
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TrySetBaseValue(ref ValuePair pair, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("if (pair._type != value.Type) return false;");
                    p.PrintEndLine();
                    p.PrintBeginLine("return pair._isPair ")
                        .Print("? TrySetPair(ref pair, value) ")
                        .Print(": TrySetSingle(ref pair, value)").PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetSingle(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage.").Print(typeName)
                                        .Print(" = value.").Print(typeName)
                                        .PrintEndLine(";");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetPair(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage = new Pair").Print(typeName)
                                        .Print("(pair._storage) { baseValue = value.").Print(typeName)
                                        .PrintEndLine(" }.storage;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TrySetCurrentValue(ref ValuePair pair, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("if (pair._type != value.Type) return false;");
                    p.PrintEndLine();

                    p.PrintBeginLine("return pair._isPair ")
                        .Print("? TrySetPair(ref pair, value) ")
                        .Print(": TrySetSingle(ref pair, value)").PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetSingle(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage.").Print(typeName)
                                        .Print(" = value.").Print(typeName)
                                        .PrintEndLine(";");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetPair(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage = new Pair").Print(typeName)
                                        .Print("(pair._storage) { currentValue = value.").Print(typeName)
                                        .PrintEndLine(" }.storage;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TrySetValues(ref ValuePair pair, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfMismatchedTypes(baseValue, currentValue);");
                    p.PrintEndLine();
                    
                    p.PrintLine("if (pair._type != baseValue.Type) return false;");
                    p.PrintEndLine();

                    p.PrintBeginLine("return pair._isPair ")
                        .Print("? TrySetPair(ref pair, baseValue, currentValue) ")
                        .Print(": TrySetSingle(ref pair, baseValue, currentValue)").PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetSingle(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage.").Print(typeName)
                                        .Print(" = currentValue.").Print(typeName)
                                        .PrintEndLine(";");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static bool TrySetPair(ref ValuePair pair, in ")
                        .Print(STAT_VARIANT).Print(" baseValue, in ")
                        .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (pair._type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine("case ").Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("pair._storage = new Pair").Print(typeName)
                                        .Print("(pair._storage) { baseValue = baseValue.").Print(typeName)
                                        .Print(", currentValue = currentValue.").Print(typeName)
                                        .PrintEndLine(" }.storage;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }

                            p.PrintLine("default:");
                            p.OpenScope();
                            {
                                p.PrintLine("return false;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static int GetHashCode(in ValuePair pair)");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCompatible(pair);");
                    p.PrintLine("return pair._isPair ? GetPair(pair) : GetSingle(pair);");
                    p.PrintEndLine();

                    p.PrintLine("static int GetSingle(in ValuePair pair)");
                    p.OpenScope();
                    {
                        p.PrintLine("return pair._type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => ").Print(HASH_VALUE).Print(".Combine(pair._type, pair._storage.")
                                    .Print(typeName).PrintEndLine("),");
                            }

                            p.PrintLine("_ => 0,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("static int GetPair(in ValuePair pair)");
                    p.OpenScope();
                    {
                        p.PrintLine("return pair._type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => ").Print(HASH_VALUE).Print(".Combine(pair._type, new Pair")
                                    .Print(typeName).PrintEndLine("(pair._storage)),");
                            }

                            p.PrintLine("_ => 0,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static bool Equals(in ValuePair pairA, in ValuePair pairB)");
                p.OpenScope();
                {
                    p.PrintLine("if (pairA._type != pairB._type || pairA._isPair != pairB._isPair) return false;");
                    p.PrintEndLine();

                    p.PrintLine("return pairA._isPair ? EqualsPair(pairA, pairB) : EqualsSingle(pairA, pairB);");
                    p.PrintEndLine();

                    p.PrintLine("static bool EqualsSingle(in ValuePair pairA, in ValuePair pairB)");
                    p.OpenScope();
                    {
                        p.PrintLine("return pairA._type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < singleTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = singleTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => pairA._storage.").Print(typeName).Print(".Equals(pairB._storage.")
                                    .Print(typeName).PrintEndLine("),");
                            }

                            p.PrintLine("_ => false,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("static bool EqualsPair(in ValuePair pairA, in ValuePair pairB)");
                    p.OpenScope();
                    {
                        p.PrintLine("return pairA._type switch");
                        p.OpenScope();
                        {
                            for (var i = 0; i < pairTypes.Count; i++)
                            {
                                var (_, _, typeName, _, _) = pairTypes[i];

                                p.PrintBeginLine(STAT_VARIANT_TYPE).Print(".").Print(typeName)
                                    .Print(" => new Pair").Print(typeName).Print("(pairA._storage).Equals(new Pair")
                                    .Print(typeName).PrintEndLine("(pairB._storage)),");
                            }

                            p.PrintLine("_ => false,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WritePairTypes(ref Printer p, List<TypeRecord> pairTypes)
        {
            foreach (var (ns, type, typeName, size, customNs) in pairTypes)
            {
                p.PrintLine(STRUCT_LAYOUT_EXPLICIT);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("private struct Pair").Print(typeName)
                    .Print(" : ").Print(IEQUATABLE).Print("<Pair").Print(typeName).PrintEndLine(">");
                p.OpenScope();
                {
                    p.PrintBeginLine(FIELD_OFFSET_0).PrintEndLine(GENERATED_CODE);
                    p.PrintLine("public DataStorage storage;");
                    p.PrintEndLine();

                    p.PrintBeginLine(FIELD_OFFSET_0).PrintEndLine(GENERATED_CODE);
                    p.PrintBeginLine("public ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).PrintEndLine(" baseValue;");
                    p.PrintEndLine();

                    p.PrintBeginLine(string.Format(FIELD_OFFSET_X, size)).PrintEndLine(GENERATED_CODE);
                    p.PrintBeginLine("public ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).PrintEndLine(" currentValue;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public Pair").Print(typeName).PrintEndLine("(in DataStorage storage) : this()");
                    p.OpenScope();
                    {
                        p.PrintLine("this.storage = storage;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public Pair").Print(typeName).Print("(")
                        .PrintIf(size > 8, "in ")
                        .PrintIf(customNs, ns).PrintIf(customNs, ".").Print(type).Print(" baseValue, ")
                        .PrintIf(size > 8, "in ")
                        .PrintIf(customNs, ns).PrintIf(customNs, ".").Print(type).Print(" currentValue")
                        .PrintEndLine(") : this()");
                    p.OpenScope();
                    {
                        p.PrintLine("this.baseValue = baseValue;");
                        p.PrintLine("this.currentValue = currentValue;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public readonly override int GetHashCode()");
                    p.WithIncreasedIndent().PrintBeginLine("=> ")
                        .Print(HASH_VALUE).PrintEndLine(".Combine(baseValue, currentValue);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public readonly override bool Equals(object obj)");
                    p.WithIncreasedIndent().PrintBeginLine("=> obj is Pair")
                        .Print(typeName).PrintEndLine(" other && Equals(other);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public readonly bool Equals(Pair").Print(typeName).PrintEndLine(" other)");
                    p.WithIncreasedIndent()
                        .PrintLine("=> baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteIsCompatibleMethod(
              ref Printer p
            , string containerType
            , List<TypeRecord> singleTypes
            , List<TypeRecord> pairTypes
        )
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static bool IsCompatible<TStatData>(TStatData value)");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.ValueType, value.IsValuePair);");
            p.PrintEndLine();
            
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static bool IsCompatible(in ValuePair value)");
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.Type, value.IsPair);");
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatible(in ")
                .Print(STAT_VARIANT).PrintEndLine(" value, bool isPair)");
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.Type, isPair);");
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatible(")
                .Print(STAT_VARIANT_TYPE).PrintEndLine(" type, bool isPair)");
            p.WithIncreasedIndent().PrintLine("=> isPair ? IsCompatiblePair(type) : IsCompatibleSingle(type);");
            p.PrintEndLine();

            WriteXmlDoc(ref p, containerType, singleTypes);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatibleSingle(")
                .Print(STAT_VARIANT_TYPE).PrintEndLine(" type)");
            p.OpenScope();
            {
                p.PrintLine("return type switch");
                p.OpenScope();
                {
                    for (var i = 0; i < singleTypes.Count; i++)
                    {
                        var (_, _, typeName, _, _) = singleTypes[i];
                        var or = i > 0 ? "or " : "   ";

                        p.PrintBeginLine(or).Print(STAT_VARIANT_TYPE).Print(".").PrintEndLine(typeName);
                    }

                    p.PrintLine("   => true,");
                    p.PrintLine(" _ => false,");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();

            WriteXmlDoc(ref p, containerType, pairTypes);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatiblePair(")
                .Print(STAT_VARIANT_TYPE).PrintEndLine(" type)");
            p.OpenScope();
            {
                p.PrintLine("return type switch");
                p.OpenScope();
                {
                    for (var i = 0; i < pairTypes.Count; i++)
                    {
                        var (_, _, typeName, _, _) = pairTypes[i];
                        var or = i > 0 ? "or " : "   ";

                        p.PrintBeginLine(or).Print(STAT_VARIANT_TYPE).Print(".").PrintEndLine(typeName);
                    }

                    p.PrintLine("   => true,");
                    p.PrintLine(" _ => false,");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteXmlDoc(ref Printer p, string containerType, List<TypeRecord> pairTypes)
            {
                p.PrintBeginLine("/// ").PrintEndLine("<summary>");
                p.PrintBeginLine("/// ").Print("These types are compatible to the stat system <see cref=\"")
                    .Print(containerType).PrintEndLine("\"/>:");
                p.PrintBeginLine("/// ").PrintEndLine("<list type=\"bullet\">");

                var maxLength = pairTypes.Select(static x => x.TypeName.Length).Max();

                for (var i = 0; i < pairTypes.Count; i++)
                {
                    var (ns, type, typeName, _, customNs) = pairTypes[i];
                    var padding = maxLength - typeName.Length;

                    p.PrintBeginLine("/// ").Print("<item><see cref=\"")
                        .Print(STAT_VARIANT_TYPE).Print(".").Print(typeName).Print("\"/>")
                        .PrintRepeat(' ', padding)
                        .Print(" => <see cref=\"")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine("\"/></item>");
                }

                p.PrintBeginLine("/// ").PrintEndLine("</list>");
                p.PrintBeginLine("/// ").PrintEndLine("</summary>");
            }
        }

        private static void WriteThrowMethods(ref Printer p, string containerType)
        {
            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private static void ThrowIfMismatchedTypes(in ")
                .Print(STAT_VARIANT).Print(" baseValue, in ")
                .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
            p.OpenScope();
            {
                p.PrintLine("if (baseValue.Type == currentValue.Type) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VALUE_TYPE_EXCEPTION)
                    .Print("($\"Base and current values are of different types, ")
                    .PrintEndLine("respectively '{baseValue.Type}' and '{currentValue.Type}'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("private static void ThrowIfNotCompatible<TStatData>(TStatData value)");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VALUE_TYPE_EXCEPTION)
                    .Print("($\"Stat data 'typeof(TStatData)' contains values of type '{value.ValueType}'")
                    .Print(" which is not compatible to the stat system 'typeof(")
                    .Print(containerType)
                    .PrintEndLine(")'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("private static void ThrowIfNotCompatible(in ValuePair value)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VALUE_TYPE_EXCEPTION)
                    .Print("($\"Value of type '{value.Type}' is not compatible to the stat system 'typeof(")
                    .Print(containerType)
                    .PrintEndLine(")'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private static void ThrowIfNotCompatible(in ")
                .Print(STAT_VARIANT).PrintEndLine(" value, bool isPair)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value, isPair)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VALUE_TYPE_EXCEPTION)
                    .Print("($\"Value of type '{value.Type}' is not compatible to the stat system 'typeof(")
                    .Print(containerType)
                    .PrintEndLine(")'.\");"); ;
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly record struct TypeRecord(
              string Ns
            , string Type
            , string TypeName
            , int Size
            , bool CustomNs
        );
    }
}
