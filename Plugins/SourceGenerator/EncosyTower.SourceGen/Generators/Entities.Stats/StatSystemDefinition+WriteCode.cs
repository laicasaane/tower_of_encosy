using System;
using System.Collections.Generic;
using System.Linq;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatSystemDefinition
    {
        private const string METHOD_IMPL_OPTIONS = "MethodImplOptions";
        private const string INLINING = $"{METHOD_IMPL_OPTIONS}.AggressiveInlining";
        private const string GENERATOR = "\"EncosyTower.SourceGen.Generators.Entities.Stats.StatSystemGenerator\"";

        private const string AGGRESSIVE_INLINING = "[MethodImpl(INLINING)]";
        private const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        private const string SERIALIZABLE = "[Serializable]";
        private const string SERIALIZED_FIELD = "[SerializeField]";
        private const string BURST_COMPILE = "[BurstCompile]";
        private const string STRUCT_LAYOUT_EXPLICIT = $"[StructLayout(LayoutKind.Explicit)]";
        private const string FIELD_OFFSET_0 = "[FieldOffset(0)]";
        private const string FIELD_OFFSET_X = "[FieldOffset({0})]";
        private const string VALIDATION_ATTRIBUTES = "[HideInCallstack, StackTraceHidden, " +
            "Conditional(\"UNITY_EDITOR\"), Conditional(\"DEVELOPMENT_BUILD\")]";

        private const string IEQUATABLE = "IEquatable";
        private const string HASH_VALUE = "HashValue";

        private const string IJOB = $"IJob";
        private const string JOB_HANDLE = $"JobHandle";

        private const string IBUFFER_ELEMENT_DATA = $"IBufferElementData";
        private const string IBAKER = $"IBaker";
        private const string ENTITY = $"Entity";
        private const string ENTITY_MANAGER = $"EntityManager";
        private const string ECB = $"EntityCommandBuffer";
        private const string ECB_WRITER = $"EntityCommandBuffer.ParallelWriter";
        private const string STAT_BUFFER = $"DynamicBuffer<Stat>";
        private const string LOOKUP_STAT = $"BufferLookup<Stat>";
        private const string LOOKUP_OBSERVER = $"BufferLookup<StatObserver>";
        private const string LOOKUP_MODIFIER = $"BufferLookup<StatModifier>";
        private const string SYSTEM_STATE = $"SystemState";
        private const string LOOKUP_STAT_XML = $"BufferLookup{{TStat}}";
        private const string STAT_BUFFER_XML = $"DynamicBuffer{{TStat}}";
        private const string LOOKUP_OBSERVER_XML = $"BufferLookup{{TStatObserver}}";
        private const string LOOKUP_MODIFIER_XML = $"BufferLookup{{TStatModifier}}";

        private const string RO_SPAN_OBSERVER = "ReadOnlySpan<StatObserver>";
        private const string RO_SPAN_MODIFIER = "ReadOnlySpan<StatModifier>";
        private const string RO_SPAN_STAT = "ReadOnlySpan<Stat>";
        private const string RO_SPAN_OBSERVER_XML = "ReadOnlySpan{TStatObserver}";
        private const string RO_SPAN_MODIFIER_XML = "ReadOnlySpan{TStatModifier}";
        private const string RO_SPAN_STAT_XML = "ReadOnlySpan{TStat}";

        private const string STAT_SINGLE = $"StatSingle";
        private const string MODIFIER_RANGE = $"ModifierRange";
        private const string OBSERVER_RANGE = $"ObserverRange";
        private const string ISTAT_DATA = $"IStatData";
        private const string ISTAT_VALUE_PAIR = $"IStatValuePair";
        private const string ISTAT_VALUE_PAIR_COMPOSER = $"IStatValuePairComposer<ValuePair>";
        private const string ISTAT = $"IStat<ValuePair>";
        private const string ISTAT_MODIFIER_STACK = $"IStatModifierStack<ValuePair, Stat>";
        private const string ISTAT_MODIFIER = $"IStatModifier<ValuePair, Stat, StatModifier.Stack>";
        private const string ISTAT_OBSERVER = $"IStatObserver";
        private const string STAT_VARIANT_TYPE = $"StatVariantType";
        private const string STAT_VARIANT = $"StatVariant";
        private const string STAT_HANDLE = $"StatHandle";
        private const string STAT_HANDLE_T = $"StatHandle<TStatData>";
        private const string STAT_MODIFIER_HANDLE = $"StatModifierHandle";
        private const string STAT_READER = $"StatReader<ValuePair, Stat>";
        private const string STAT_VARIANT_TYPE_EXCEPTION = $"StatVariantTypeException";
        private const string STAT_CHANGE_EVENT = $"StatChangeEvent<ValuePair>";
        private const string MODIFIER_TRIGGER_EVENT = $"ModifierTriggerEvent<{TYPES_4}>";
        private const string STAT_MODIFIER_RECORD = $"StatModifierRecord<{TYPES_4}>";
        private const string STAT_READER_XML = $"StatReader{{TValuePair, TStat}}";
        private const string STAT_HANDLE_T_XML = $"StatHandle{{TStatData}}";
        private const string STAT_MODIFIER_RECORD_XML = $"StatModifierRecord{{{TYPES_4_T}}}";
        private const string STAT_CHANGE_EVENT_XML = $"StatChangeEvent{{TValuePair}}";
        private const string MODIFIER_TRIGGER_EVENT_XML = $"ModifierTriggerEvent{{{TYPES_4_T}}}";

        private const string TYPES_6 = "ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer";
        private const string TYPES_5 = "ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver";
        private const string TYPES_4 = "ValuePair, Stat, StatModifier, StatModifier.Stack";
        private const string TYPES_6_T = "TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer";
        private const string TYPES_5_T = "TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver";
        private const string TYPES_4_T = "TValuePair, TStat, TStatModifier, TStatModifierStack";
        private const string TYPES_4_T_COMPOSER = "TValuePair, TStat, TStatData, TValuePairComposer";
        private const string TYPES_3_T_OBSERVER = "TValuePair, TStat, TStatObserver";
        private const string TYPES_3_T_DATA = "TValuePair, TStat, TStatData";
        private const string TYPES_2_T = "TValuePair, TStat";

        private const string STAT_API = $"StatAPI";
        private const string STAT_WORLD_DATA = $"StatWorldData<{TYPES_5}>";
        private const string STAT_ACCESSOR = $"StatAccessor<{TYPES_6}>";
        private const string STAT_ACCESSOR_READONLY = $"StatAccessor<{TYPES_6}>.ReadOnly";
        private const string STAT_BAKER = $"StatBaker<{TYPES_6}>";
        private const string DEFERRED_LIST_JOB = $"DeferredUpdateStatListJob<{TYPES_6}>";
        private const string DEFERRED_QUEUE_JOB = $"DeferredUpdateStatQueueJob<{TYPES_6}>";
        private const string DEFERRED_STREAM_JOB = $"DeferredUpdateStatStreamJob<{TYPES_6}>";
        private const string DEFERRED_UNSAFE_BLOCK_LIST_JOB = $"DeferredUpdateStatUnsafeBlockListJob<{TYPES_6}>";
        private const string STAT_WORLD_DATA_XML = $"StatWorldData{{{TYPES_5_T}}}";
        private const string STAT_ACCESSOR_XML = $"StatAccessor{{{TYPES_6_T}}}";
        private const string STAT_ACCESSOR_READONLY_XML = $"StatAccessor{{{TYPES_6_T}}}.ReadOnly";
        private const string STAT_BAKER_XML = $"StatBaker{{{TYPES_6_T}}}";

        private const string UNSAFE_UTILITY = $"UnsafeUtility";
        private const string ALLOCATOR_HANDLE = $"AllocatorManager.AllocatorHandle";
        private const string NATIVE_LIST_STAT_HANDLE = $"NativeList<{STAT_HANDLE}>";
        private const string NATIVE_QUEUE_STAT_HANDLE = $"NativeQueue<{STAT_HANDLE}>";
        private const string NATIVE_STREAM_READER = $"NativeStream.Reader";
        private const string NATIVE_LIST_OBSERVER = $"NativeList<StatObserver>";
        private const string NATIVE_LIST_OBSERVER_XML = $"NativeList{{TStatObserver}}";
        private const string NATIVE_SET_ENTITY = $"NativeHashSet<{ENTITY}>";
        private const string NATIVE_LIST_MODIFIER_HANDLE = $"NativeList<{STAT_MODIFIER_HANDLE}>";
        private const string NATIVE_ARRAY_MODIFIER_EVENT = $"NativeArray<ModifierTriggerEvent>";
        private const string NATIVE_LIST_MODIFIER_EVENT = $"NativeList<ModifierTriggerEvent>";
        private const string NATIVE_LIST_MODIFIER_EVENT_GENERIC = $"NativeList<{MODIFIER_TRIGGER_EVENT}>";
        private const string NATIVE_ARRAY_STAT_CHANGE_EVENT = $"NativeArray<{STAT_CHANGE_EVENT}>";
        private const string NATIVE_LIST_STAT_CHANGE_EVENT = $"NativeList<{STAT_CHANGE_EVENT}>";
        private const string NATIVE_LIST_MODIFIER_RECORD = $"NativeList<StatModifierRecord>";
        private const string NATIVE_LIST_MODIFIER_RECORD_GENERIC = $"NativeList<{STAT_MODIFIER_RECORD}>";
        private const string NATIVE_LIST_MODIFIER_RECORD_GENERIC_XML = $"NativeList{{{STAT_MODIFIER_RECORD_XML}}}";
        private const string NATIVE_LIST_STAT_HANDLE_XML = $"NativeList{{{STAT_HANDLE}}}";
        private const string NATIVE_SET_ENTITY_XML = $"NativeHashSet{{{ENTITY}}}";
        private const string NATIVE_LIST_MODIFIER_HANDLE_XML = $"NativeList{{{STAT_MODIFIER_HANDLE}}}";
        private const string NATIVE_LIST_MODIFIER_RECORD_XML = $"NativeList{{{STAT_MODIFIER_RECORD_XML}}}";
        private const string NATIVE_LIST_MODIFIER_EVENT_XML = $"NativeList{{{MODIFIER_TRIGGER_EVENT_XML}}}";
        private const string NATIVE_LIST_STAT_CHANGE_EVENT_XML = $"NativeList{{{STAT_CHANGE_EVENT_XML}}}";

        private const string UNSAFE_BLOCK_LIST_HANDLE = $"global::Latios.Unsafe.UnsafeParallelBlockList<{STAT_HANDLE}>";

        private delegate void WriteAction(ref Printer p);

        public readonly string WriteCode(References references)
        {
            FilterTypes(out var singleTypes, out var pairTypes, out var incompatTypes);

            var p = new Printer(0, 1024 * 512);
            var keyword = syntaxKeyword;
            var statSystemName = typeName;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("partial ").Print(keyword).Print(" ").PrintEndLine(statSystemName);
                p.OpenScope();
                {
                    WriteStat(ref p);
                    WriteStatObserver(ref p);
                    WriteStatModifier(ref p);
                    WriteWrappers(ref p);
                    WriteJobs(ref p, references);
                    WriteValuePair(ref p);
                    WriteIsCompatibleMethod(ref p, typeName, singleTypes, pairTypes);
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

                    WriteStatImpl(ref p);
                    WriteAPIImpl(ref p);
                    WriteReaderImpl(ref p);
                    WriteAccessorImpl(ref p);
                    WriteAccessorReadOnlyImpl(ref p);
                    WriteBakerImpl(ref p);
                    WriteWorldDataImpl(ref p);
                    WriteOtherImpl(ref p, references);
                    WriteValuePairImpl(ref p, typeName, maxDataSize, singleTypes, pairTypes, incompatTypes);
                    WriteStatDataStore(ref p, singleTypes, pairTypes);
                    WritePairTypes(ref p, pairTypes);
                    WriteThrowMethods(ref p, typeName);
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void FilterTypes(
              out List<TypeRecord> singleTypes
            , out List<TypeRecord> pairTypes
            , out List<TypeRecord> incompatTypes
        )
        {
            var types = StatGeneratorAPI.Types.AsSpan();
            var typeNames = StatGeneratorAPI.TypeNames.AsSpan();
            var sizes = StatGeneratorAPI.Sizes.AsSpan();
            var maxSize = maxDataSize;
            var halfSize = maxSize / 2;
            singleTypes = new List<TypeRecord>(types.Length);
            pairTypes = new List<TypeRecord>(types.Length);
            incompatTypes = new List<TypeRecord>(types.Length);

            for (var i = 1; i < types.Length; i++)
            {
                var type = types[i];
                var typeName = typeNames[i];
                var size = sizes[i];
                var record = new TypeRecord("", type, typeName, size, false);

                if (size > maxSize)
                {
                    incompatTypes.Add(record);
                    continue;
                }

                singleTypes.Add(record);

                if (size <= halfSize)
                {
                    pairTypes.Add(record);
                }
            }
        }

        private static void WriteStat(ref Printer p)
        {
            p.PrintLine("public partial struct Stat<TStatData> { }");
            p.PrintEndLine();

            p.PrintBeginLine("public partial struct Stat ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(SERIALIZED_FIELD);
                p.PrintBeginLine("private ").Print(MODIFIER_RANGE).PrintEndLine(" _modifierRange;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintBeginLine("private ").Print(OBSERVER_RANGE).PrintEndLine(" _observerRange;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("private StatDataStore _valueData;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintBeginLine("private ").Print(STAT_VARIANT_TYPE).PrintEndLine(" _valueType;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("private bool _valueIsPair;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("private bool _produceChangeEvents;");
                p.PrintEndLine();

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

                p.PrintLine("public ValuePair ValuePair");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("readonly get => StatValueUnion.Convert(_valueData, _valueIsPair, _valueType);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set => StatValueUnion.Convert(value, ref _valueData, ref _valueIsPair, ref _valueType);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> ValuePair.GetBaseValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> ValuePair.GetCurrentValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
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

        private static void WriteStatImpl(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Stat");
            p.OpenScope();
            {
                p.PrintLine(STRUCT_LAYOUT_EXPLICIT);
                p.PrintLine("struct StatValueUnion");
                p.OpenScope();
                {
                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintLine("private struct ExposedValuePair");
                    p.OpenScope();
                    {
                        p.PrintLine("public StatDataStore data;");
                        p.PrintEndLine();

                        p.PrintBeginLine("public ").Print(STAT_VARIANT_TYPE).PrintEndLine(" type;");
                        p.PrintEndLine();

                        p.PrintLine("public bool isPair;");
                        p.PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine(FIELD_OFFSET_0).PrintEndLine(GENERATED_CODE);
                    p.PrintLine("private ValuePair _valuePair;");
                    p.PrintEndLine();

                    p.PrintBeginLine(FIELD_OFFSET_0).PrintEndLine(GENERATED_CODE);
                    p.PrintLine("private ExposedValuePair _exposed;");
                    p.PrintEndLine();

                    p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static ValuePair Convert(StatDataStore data, bool isPair, ")
                        .Print(STAT_VARIANT_TYPE).PrintEndLine(" type)");
                    p.OpenScope();
                    {
                        p.PrintLine("return new StatValueUnion");
                        p.OpenScope();
                        {
                            p.PrintLine("_exposed = new ExposedValuePair");
                            p.OpenScope();
                            {
                                p.PrintLine("data = data,");
                                p.PrintLine("isPair = isPair,");
                                p.PrintLine("type = type,");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope("}._valuePair;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static void Convert(ValuePair valuePair, ")
                        .Print("ref StatDataStore data, ref bool isPair, ref ")
                        .Print(STAT_VARIANT_TYPE).PrintEndLine(" type)");
                    p.OpenScope();
                    {
                        p.PrintLine("var exposed = new StatValueUnion { _valuePair = valuePair }._exposed;");
                        p.PrintLine("data = exposed.data;");
                        p.PrintLine("isPair = exposed.isPair;");
                        p.PrintLine("type = exposed.type;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Stat<TStatData>");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.OpenScope();
            {
                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("private Stat _value;");
                p.PrintEndLine();

                p.PrintLine("static Stat()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCompatible(default(TStatData));");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Stat(in Stat value)");
                p.OpenScope();
                {
                    p.PrintLine("_value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

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

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Stat<TStatData>(in Stat value)");
                p.WithIncreasedIndent().PrintLine("=> new(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Stat(in Stat<TStatData> value)");
                p.WithIncreasedIndent().PrintLine("=> value._value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> _value.GetBaseValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent().PrintLine("=> _value.GetCurrentValueOrDefault(defaultValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
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

                p.PrintLine(AGGRESSIVE_INLINING);
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
            p.PrintBeginLine("public partial struct StatObserver ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT_OBSERVER)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(SERIALIZED_FIELD);
                p.PrintBeginLine("private ").Print(STAT_HANDLE).PrintEndLine(" _observerHandle;");
                p.PrintEndLine();

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
            p.PrintBeginLine("public partial struct StatModifier ")
                .Print(": ").Print(IBUFFER_ELEMENT_DATA).Print(", ").Print(ISTAT_MODIFIER)
                .PrintEndLine();
            p.OpenScope();
            {
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

                p.PrintLine(AGGRESSIVE_INLINING);
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial struct Stack ")
                    .Print(": ").Print(ISTAT_MODIFIER_STACK)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
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

                    p.PrintLine(AGGRESSIVE_INLINING);
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
            WriteModifierAndHandleRecord(ref p, "ModifierTriggerEvent");
            WriteModifierAndHandleRecord(ref p, "StatModifierRecord");

            p.PrintLine("public static partial class API { }");
            p.PrintEndLine();

            WriteAPI(ref p, "Reader", STAT_READER, "reader", WriteReaderConstructor);
            WriteAPI(ref p, "Accessor", STAT_ACCESSOR, "accessor", WriteAccessorConstructor);

            p.PrintLine("partial struct Accessor");
            p.OpenScope();
            {
                WriteAPI(ref p, "ReadOnly", STAT_ACCESSOR_READONLY, "accessor", WriteAccessorReadOnlyConstructor);
            }
            p.CloseScope();
            p.PrintEndLine();

            WriteAPI(ref p, "StatBaker", STAT_BAKER, "baker", WriteBakerConstructor);
            WriteAPI(ref p, "WorldData", STAT_WORLD_DATA, "worldData", WriteWorldDataConstructor);

            return;

            static void WriteAPI(
                  ref Printer p
                , string wrapperName
                , string fieldTypeName
                , string fieldName
                , WriteAction constructorWriter
            )
            {
                p.PrintBeginLine("public partial struct ").PrintEndLine(wrapperName);
                p.OpenScope();
                {
                    p.PrintBeginLine("public ").Print(fieldTypeName).Print(" ").Print(fieldName).PrintEndLine(";");
                    p.PrintEndLine();

                    constructorWriter(ref p);

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("private ").Print(wrapperName).Print("(in ")
                        .Print(fieldTypeName).Print(" ").Print(fieldName).PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").Print(fieldName).Print(" = ").Print(fieldName).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static implicit operator ").Print(wrapperName).Print("(in ")
                        .Print(fieldTypeName).Print(" ").Print(fieldName).PrintEndLine(")");
                    p.WithIncreasedIndent()
                        .PrintBeginLine("=> new(").Print(fieldName).PrintEndLine(");");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static implicit operator ").Print(fieldTypeName).Print("(in ")
                        .Print(wrapperName).Print(" ").Print(fieldName).PrintEndLine(")");
                    p.WithIncreasedIndent()
                        .PrintBeginLine("=> ").Print(fieldName).Print(".").Print(fieldName).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteReaderConstructor(ref Printer p)
            {
            }

            static void WriteAccessorConstructor(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor(ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(" state) : this(ref state, default) { }");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor(ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(" state, ValuePair.Composer valuePairComposer)");
                p.OpenScope();
                {
                    p.PrintLine("this.accessor = new(ref state, valuePairComposer);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteAccessorReadOnlyConstructor(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ReadOnly(ref ").Print(SYSTEM_STATE).PrintEndLine(" state)");
                p.OpenScope();
                {
                    p.PrintLine("this.accessor = new(ref state);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteBakerConstructor(ref Printer p)
            {
            }

            static void WriteWorldDataConstructor(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public WorldData(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.OpenScope();
                {
                    p.PrintLine("this.worldData = new(allocator);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public WorldData(int initialCapacity, ").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.OpenScope();
                {
                    p.PrintLine("this.worldData = new(initialCapacity, allocator);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteModifierAndHandleRecord(ref Printer p, string name)
            {
                p.PrintBeginLine("public partial struct ").PrintEndLine(name);
                p.OpenScope();
                {
                    p.PrintBeginLine("public ").Print(STAT_MODIFIER_HANDLE).PrintEndLine(" handle;");
                    p.PrintEndLine();

                    p.PrintLine("public StatModifier modifier;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").Print(name)
                        .Print("(").Print(STAT_MODIFIER_HANDLE).PrintEndLine(" handle, StatModifier modifier)");
                    p.OpenScope();
                    {
                        p.PrintLine("this.handle = handle;");
                        p.PrintLine("this.modifier = modifier;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
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
                p.PrintLine(BURST_COMPILE);
                p.PrintBeginLine("public partial struct ").Print(name).Print(" : ").PrintEndLine(IJOB);
                p.OpenScope();
                {
                    p.PrintBeginLine("private ").Print(typeName).PrintEndLine(" _jobData;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public ").Print(name)
                        .Print("(in Accessor accessor, in WorldData worldData, ")
                        .Print(collectionName).PrintEndLine(" statsToUpdate)");
                    p.OpenScope();
                    {
                        p.PrintLine("_jobData = new()");
                        p.OpenScope();
                        {
                            p.PrintLine("statAccessor = accessor.accessor,");
                            p.PrintLine("statWorldData = worldData.worldData,");
                            p.PrintLine("statsToUpdate = statsToUpdate,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public void Execute()");
                    p.WithIncreasedIndent().PrintLine("=> _jobData.Execute();");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteValuePair(ref Printer p)
        {
            p.PrintLine(SERIALIZABLE);
            p.PrintBeginLine("public partial struct ValuePair ")
                .Print(": ").Print(ISTAT_VALUE_PAIR)
                .Print(", ").Print(IEQUATABLE).Print("<ValuePair>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("internal StatDataStore _data;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintBeginLine("internal ").Print(STAT_VARIANT_TYPE).PrintEndLine(" _type;");
                p.PrintEndLine();

                p.PrintLine(SERIALIZED_FIELD);
                p.PrintLine("internal bool _isPair;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ValuePair(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("_type = value.Type;");
                    p.PrintLine("_data = StatDataStore.Store(_isPair = false, value, value);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ValuePair(in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("_type = baseValue.Type;");
                    p.PrintLine("_data = StatDataStore.Store(_isPair = true, baseValue, currentValue);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("private ValuePair(bool isPair, in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.OpenScope();
                {
                    p.PrintLine("_type = baseValue.Type;");
                    p.PrintLine("_data = StatDataStore.Store(_isPair = isPair, baseValue, currentValue);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT_TYPE).PrintEndLine(" Type");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _type;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool IsPair");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _isPair;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is ValuePair other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool Equals(ValuePair other)");
                p.WithIncreasedIndent().PrintLine("=> StatDataStore.Equals(this, other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> StatDataStore.GetHashCode(this);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetBaseValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent()
                    .PrintLine("=> StatDataStore.TryGetBaseValue(this, out var value) ? value : defaultValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(STAT_VARIANT).Print(" GetCurrentValueOrDefault(in ")
                    .Print(STAT_VARIANT).PrintEndLine(" defaultValue = default)");
                p.WithIncreasedIndent()
                    .PrintLine("=> StatDataStore.TryGetCurrentValue(this, out var value) ? value : defaultValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetBaseValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> StatDataStore.TrySetBaseValue(ref this, value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetCurrentValue(in ").Print(STAT_VARIANT).PrintEndLine(" value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> StatDataStore.TrySetCurrentValue(ref this, value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetValues(in ").Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
                p.WithIncreasedIndent()
                    .PrintLine("=> StatDataStore.TrySetValues(ref this, baseValue, currentValue);");
                p.PrintEndLine();

                WriteComposer(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteComposer(ref Printer p)
            {
                p.PrintLine(SERIALIZABLE);
                p.PrintBeginLine("public partial struct Composer ")
                    .Print(": ").Print(ISTAT_VALUE_PAIR_COMPOSER)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
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
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial class API");
            p.OpenScope();
            {
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{").Print(TYPES_5_T).Print("}(")
                    .Print(ENTITY).Print(", ").Print(ENTITY_MANAGER)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ENTITY_MANAGER).PrintEndLine(" entityManager)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, entityManager);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{").Print(TYPES_5_T).Print("}(")
                    .Print(ENTITY).Print(", ").Print(ECB)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ECB).PrintEndLine(" ecb)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, ecb);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".AddStatComponents{").Print(TYPES_5_T).Print("}(")
                    .Print(ENTITY).Print(", ").Print(ECB_WRITER).Print(", int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void AddStatComponents(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(ECB_WRITER).PrintEndLine(" ecb, int sortKey)");
                p.WithIncreasedIndent().PrintBeginLine("=> ").Print(STAT_API).Print(".AddStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver")
                        .PrintEndLine(">(entity, ecb, sortKey);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".BakeStatComponents{").Print(TYPES_6_T).Print("}(")
                    .Print(IBAKER).Print(", ").Print(ENTITY).Print(", out ").Print(STAT_BAKER_XML)
                    .Print(", TValuePairComposer")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static StatBaker BakeStatComponents(")
                    .Print(IBAKER).Print(" baker, ")
                    .Print(ENTITY).PrintEndLine(" entity, ValuePair.Composer valuePairComposer = default)");
                p.OpenScope();
                {
                    p.PrintBeginLine(STAT_API).Print(".BakeStatComponents<")
                        .Print("ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer")
                        .PrintEndLine(">(baker, entity, out var statBaker, valuePairComposer);");
                    p.PrintLine("return statBaker;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".CreateStatHandle{").Print(TYPES_4_T_COMPOSER).Print("}(")
                    .Print(ENTITY).Print(", TStatData, bool, ref ").Print(STAT_BUFFER_XML).Print(", TValuePairComposer")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ").Print(STAT_HANDLE_T).Print(" CreateStatHandle<TStatData>(")
                    .Print(ENTITY).Print(" entity, TStatData statData, bool produceChangeEvents, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer, ValuePair.Composer valuePairComposer = default)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".CreateStatHandle<ValuePair, Stat, TStatData, ValuePair.Composer>")
                    .PrintEndLine("(entity, statData, produceChangeEvents, ref statBuffer, valuePairComposer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".CreateStatHandle{").Print(TYPES_4_T_COMPOSER).Print("}(")
                    .Print(ENTITY).Print(", TValuePair, bool, ref ")
                    .Print(STAT_BUFFER_XML).Print(", out TStatData, TValuePairComposer")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ").Print(STAT_HANDLE_T).Print(" CreateStatHandle<TStatData>(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, bool produceChangeEvents, ref ")
                    .Print(STAT_BUFFER).Print(" statBuffer, out TStatData statData, ")
                    .PrintEndLine("ValuePair.Composer valuePairComposer = default)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .Print(".CreateStatHandle<ValuePair, Stat, TStatData, ValuePair.Composer>")
                    .PrintEndLine("(entity, valuePair, produceChangeEvents, ref statBuffer, out statData, valuePairComposer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".CreateStatHandle{").Print(TYPES_2_T).Print("}(")
                    .Print(ENTITY).Print(", TValuePair, bool, ref ").Print(STAT_BUFFER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ").Print(STAT_HANDLE).Print(" CreateStatHandle(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, bool produceChangeEvents, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".CreateStatHandle<ValuePair, Stat>(entity, valuePair, produceChangeEvents, ref statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".EntityHasAnyOtherDependantStatEntities{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void EntityHasAnyOtherDependantStatEntities(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).PrintEndLine(" lookupObservers)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, lookupObservers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".EntityHasAnyOtherDependantStatEntities{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void EntityHasAnyOtherDependantStatEntities(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).PrintEndLine(" observerBufferOnEntity)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, observerBufferOnEntity);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatEntitiesOfEntity{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER_XML).Print(", ").Print(NATIVE_SET_ENTITY_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void GetOtherDependantStatEntitiesOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_SET_ENTITY).PrintEndLine(" dependentEntities)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, lookupObservers, dependentEntities);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatEntitiesOfEntity{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER_XML).Print(", ").Print(NATIVE_SET_ENTITY_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void GetOtherDependantStatEntitiesOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).Print(" observerBufferOnEntity, ")
                    .Print(NATIVE_SET_ENTITY).PrintEndLine(" dependentEntities)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentEntities);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatsOfEntity{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER_XML).Print(", ").Print(NATIVE_LIST_STAT_HANDLE_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void GetOtherDependantStatsOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" dependentStats)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatsOfEntity<StatObserver>(entity, lookupObservers, dependentStats);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetOtherDependantStatsOfEntity{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(RO_SPAN_OBSERVER_XML).Print(", ").Print(NATIVE_LIST_STAT_HANDLE_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void GetOtherDependantStatsOfEntity(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(RO_SPAN_OBSERVER).Print(" observerBufferOnEntity, ")
                    .Print(NATIVE_LIST_STAT_HANDLE).PrintEndLine(" dependentStats)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetOtherDependantStatsOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentStats);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatData{").Print(TYPES_3_T_DATA).Print("}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(RO_SPAN_STAT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static TStatData GetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(RO_SPAN_STAT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Stat<TStatData> GetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStat<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Stat GetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStat<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatValue{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ValuePair GetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".GetStatValue<ValuePair, Stat>(statHandle, statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".GetStatEntitiesThatEntityDependsOn{").Print(TYPES_4_T).Print("}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_MODIFIER_XML).Print(", ")
                    .Print(NATIVE_SET_ENTITY_XML).Print(", ").Print(NATIVE_LIST_STAT_HANDLE_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".GetStatEntitiesThatEntityDependsOn{").Print(TYPES_4_T).Print("}(")
                    .Print(RO_SPAN_MODIFIER_XML).Print(", ").Print(NATIVE_SET_ENTITY_XML).Print(", ")
                    .Print(NATIVE_LIST_STAT_HANDLE_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".TryGetAllObservers{TStatObserver}(")
                    .Print(ENTITY).Print(", ").Print(LOOKUP_OBSERVER_XML).Print(", ").Print(NATIVE_LIST_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetAllObservers(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(LOOKUP_OBSERVER).Print(" lookupObservers, ")
                    .Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetAllObservers<StatObserver>(entity, lookupObservers, observers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetModifierCount{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool TryGetModifierCount(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out int modifierCount)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetModifierCount<ValuePair, Stat>(statHandle, lookupStats, out modifierCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetModifiersOfStat{").Print(TYPES_4_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", ")
                    .Print(LOOKUP_MODIFIER_XML).Print(", ").Print(NATIVE_LIST_MODIFIER_RECORD_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".TryGetObserverCount{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetObserverCount(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out int observerCount)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetObserverCount<ValuePair, Stat>(statHandle, lookupStats, out observerCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetObserversOfStat{").Print(TYPES_3_T_OBSERVER).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", ")
                    .Print(LOOKUP_OBSERVER_XML).Print(", ").Print(NATIVE_LIST_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".TryGetStatData{").Print(TYPES_3_T_DATA).Print("}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(LOOKUP_STAT_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatData<ValuePair, Stat, TStatData>(statHandle, lookupStats, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatData{").Print(TYPES_3_T_DATA).Print("}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".TryGetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
                    .Print(".TryGetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, lookupStats, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStat{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(RO_SPAN_STAT).PrintEndLine(" statBuffer, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStat<ValuePair, Stat>(statHandle, statBuffer, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatValue{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(LOOKUP_STAT_XML).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ")
                    .Print(LOOKUP_STAT).PrintEndLine(" lookupStats, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> ").Print(STAT_API)
                    .PrintEndLine(".TryGetStatValue<ValuePair, Stat>(statHandle, lookupStats, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_API)
                    .Print(".TryGetStatValue{").Print(TYPES_2_T).Print("}(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
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
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Reader");
            p.OpenScope();
            {
                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.reader.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool UseLookup");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.reader.UseLookup;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_READER_XML)
                    .Print(".TryGetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent().PrintLine("=> this.reader.TryGetStatData<TStatData>(statHandle, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_READER_XML)
                    .Print(".TryGetStat{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.reader.TryGetStat(statHandle, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_READER_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out Stat stat)");
                p.WithIncreasedIndent().PrintLine("=> this.reader.TryGetStat(statHandle, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_READER_XML)
                    .Print(".TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out ValuePair valuePair)");
                p.WithIncreasedIndent().PrintLine("=> this.reader.TryGetStatValue(statHandle, out valuePair);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteAccessorImpl(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct Accessor");
            p.OpenScope();
            {
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(", TStatModifier, out ").Print(STAT_MODIFIER_HANDLE)
                    .Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, StatModifier statModifier, out ")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" statModifierHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryAddStatModifier(affectedStatHandle, statModifier, out statModifierHandle, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryAddStatModifiersBatch(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_MODIFIER_XML).Print(", ")
                    .Print(NATIVE_LIST_MODIFIER_HANDLE_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryAddStatModifiersBatch(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, ")
                    .Print(RO_SPAN_MODIFIER).Print(" modifiers, ")
                    .Print(NATIVE_LIST_MODIFIER_HANDLE).PrintEndLine(" statModifierHandles, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryAddStatModifiersBatch(affectedStatHandle, modifiers, statModifierHandles, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryCreateStatHandle{TStatData}(")
                    .Print(ENTITY).Print(", TValuePair, bool, out ").Print(STAT_HANDLE_T_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryCreateStatHandle<TStatData>(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, ")
                    .Print("bool produceChangeEvents, out ").Print(STAT_HANDLE_T)
                    .PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> this.accessor.TryCreateStatHandle<TStatData>(entity, valuePair, produceChangeEvents, ")
                    .PrintEndLine("out statHandle, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryCreateStatHandle{TStatData}(")
                    .Print(ENTITY).Print(", TValuePair, bool, out ").Print(STAT_HANDLE_T_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryCreateStatHandle<TStatData>(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, ")
                    .Print("bool produceChangeEvents, out ").Print(STAT_HANDLE_T)
                    .PrintEndLine(" statHandle)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintBeginLine("=> this.accessor.TryCreateStatHandle<TStatData>(entity, valuePair, produceChangeEvents, ")
                    .PrintEndLine("out statHandle);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryCreateStatHandle{TStatData}(")
                    .Print(ENTITY).Print(", TStatData, bool, out ").Print(STAT_HANDLE_T_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryCreateStatHandle<TStatData>(")
                    .Print(ENTITY).Print(" entity, TStatData statData, bool produceChangeEvents, out ")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryCreateStatHandle<TStatData>(entity, statData, produceChangeEvents, out statHandle);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryCreateStatHandle(")
                    .Print(ENTITY).Print(", TValuePair, bool, out ").Print(STAT_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryCreateStatHandle(")
                    .Print(ENTITY).Print(" entity, ValuePair valuePair, bool produceChangeEvents, out ")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryCreateStatHandle(entity, valuePair, produceChangeEvents, out statHandle);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetAllObservers(")
                    .Print(ENTITY).Print(", ").Print(NATIVE_LIST_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetAllObservers(")
                    .Print(ENTITY).Print(" entity, ").Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetAllObservers(entity, observers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetObserverCount(")
                    .Print(STAT_HANDLE).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetObserverCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int observerCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetObserverCount(statHandle, out observerCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatData<TStatData>(statHandle, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatData<TStatData>(statHandle, statBuffer, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.accessor.TryGetStat(statHandle, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.accessor.TryGetStat(statHandle, statBuffer, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStat(statHandle, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStat(statHandle, statBuffer, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatValue(statHandle, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatValue(statHandle, statBuffer, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).Print(", out TStatModifier")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" modifierHandle, out StatModifier statModifier)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatModifier(modifierHandle, out statModifier);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetModifierCount(")
                    .Print(STAT_HANDLE).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetModifierCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int modifierCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetModifierCount(statHandle, out modifierCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryGetModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(NATIVE_LIST_MODIFIER_RECORD)
                    .PrintEndLine(" modifiers)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var modifiersTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_RECORD).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC)
                        .PrintEndLine(">(ref modifiers);");

                    p.PrintLine("return this.accessor.TryGetModifiersOfStat(statHandle, modifiersTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryRemoveModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryRemoveModifiersOfStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryRemoveModifiersOfStat(statHandle, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryRemoveStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryRemoveStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" modifierHandle, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryRemoveStatModifier(modifierHandle, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, ref ")
                    .Print(STAT_BUFFER_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ValuePair value, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatBaseValue(statHandle, value, ref statBuffer, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatBaseValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, in ValuePair value, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatBaseValue(statHandle, value, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, ref ")
                    .Print(STAT_BUFFER_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ValuePair value, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatCurrentValue(statHandle, value, ref statBuffer, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatCurrentValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, in ValuePair value, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatCurrentValue(statHandle, value, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", TStatData, ref ")
                    .Print(STAT_BUFFER_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, TStatData statData, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatData<TStatData>(statHandle, statData, ref statBuffer, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", TStatData, ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, TStatData statData, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatData<TStatData>(statHandle, statData, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, in ").Print(STAT_VARIANT)
                    .Print(", ref ").Print(STAT_BUFFER_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ValuePair value, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatValues(statHandle, value, ref statBuffer, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(", in TValuePair, in ")
                    .Print(STAT_VARIANT).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatValues(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, in ValuePair value, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatValues(statHandle, value, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(", in ").Print(STAT_VARIANT).Print(", in ").Print(STAT_VARIANT)
                    .Print(", ref ").Print(STAT_BUFFER_XML).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).Print(" currentValue, ref ").Print(STAT_BUFFER)
                    .PrintEndLine(" statBuffer, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatValues(statHandle, baseValue, currentValue, ref statBuffer, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(", in ").Print(STAT_VARIANT).Print(", in ")
                    .Print(STAT_VARIANT).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatValues(")
                    .Print(STAT_HANDLE).Print(" statHandle, in ")
                    .Print(STAT_VARIANT).Print(" baseValue, in ")
                    .Print(STAT_VARIANT).Print(" currentValue, ref WorldData worldData)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatValues(statHandle, baseValue, currentValue, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatProduceChangeEvents(")
                    .Print(STAT_HANDLE).Print(", bool")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatProduceChangeEvents(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, bool value)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatProduceChangeEvents(statHandle, value);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TrySetStatProduceChangeEvents(")
                    .Print(STAT_HANDLE).Print(", bool, ref ").Print(STAT_BUFFER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TrySetStatProduceChangeEvents(")
                    .Print(STAT_HANDLE).Print(" statHandle, bool value, ref ")
                    .Print(STAT_BUFFER).PrintEndLine(" statBuffer)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TrySetStatProduceChangeEvents(statHandle, value, ref statBuffer);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryUpdateAllStats(")
                    .Print(ENTITY).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void TryUpdateAllStats(")
                    .Print(ENTITY).PrintEndLine(" entity, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintLine("=> this.accessor.TryUpdateAllStats(entity, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".TryUpdateStat(")
                    .Print(STAT_HANDLE).Print(", ref ").Print(STAT_WORLD_DATA_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void TryUpdateStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, ref WorldData worldData)");
                p.WithIncreasedIndent().PrintLine("=> this.accessor.TryUpdateStat(statHandle, ref worldData.worldData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_XML)
                    .Print(".Update(")
                    .Print("ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void Update(ref ").Print(SYSTEM_STATE).PrintEndLine(" state)");
                p.WithIncreasedIndent().PrintLine("=> this.accessor.Update(ref state);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteAccessorReadOnlyImpl(ref Printer p)
        {
            p.PrintLine("partial struct Accessor");
            p.OpenScope();
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct ReadOnly");
            p.OpenScope();
            {
                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetAllObservers(")
                    .Print(ENTITY).Print(", ").Print(NATIVE_LIST_OBSERVER_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetAllObservers(")
                    .Print(ENTITY).Print(" entity, ").Print(NATIVE_LIST_OBSERVER).PrintEndLine(" observers)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetAllObservers(entity, observers);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetObserverCount(")
                    .Print(STAT_HANDLE).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetObserverCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int observerCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetObserverCount(statHandle, out observerCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatData<TStatData>(statHandle, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStatData{TStatData}(")
                    .Print(STAT_HANDLE_T_XML).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatData<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatData<TStatData>(statHandle, statBuffer, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).PrintEndLine(" statHandle, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.accessor.TryGetStat(statHandle, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat<TStatData>(")
                    .Print(STAT_HANDLE_T).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    p.PrintLine("var result = this.accessor.TryGetStat(statHandle, statBuffer, out var statUntyped);");
                    p.PrintLine("stat = statUntyped;");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStat(statHandle, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TStat")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out Stat stat)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStat(statHandle, statBuffer, out stat);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatValue(statHandle, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(", ").Print(RO_SPAN_STAT_XML).Print(", out TValuePair")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatValue(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(RO_SPAN_STAT)
                    .PrintEndLine(" statBuffer, out ValuePair valuePair)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatValue(statHandle, statBuffer, out valuePair);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).Print(", out TStatModifier")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetStatModifier(")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" modifierHandle, out StatModifier statModifier)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetStatModifier(modifierHandle, out statModifier);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetModifierCount(")
                    .Print(STAT_HANDLE).Print(", out int")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetModifierCount(")
                    .Print(STAT_HANDLE).PrintEndLine(" statHandle, out int modifierCount)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.accessor.TryGetModifierCount(statHandle, out modifierCount);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".TryGetModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(", ").Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetModifiersOfStat(")
                    .Print(STAT_HANDLE).Print(" statHandle, ").Print(NATIVE_LIST_MODIFIER_RECORD)
                    .PrintEndLine(" modifiers)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var modifiersTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_RECORD).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_RECORD_GENERIC)
                        .PrintEndLine(">(ref modifiers);");

                    p.PrintLine("return this.accessor.TryGetModifiersOfStat(statHandle, modifiersTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_ACCESSOR_READONLY_XML)
                    .Print(".Update(")
                    .Print("ref ").Print(SYSTEM_STATE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void Update(ref ").Print(SYSTEM_STATE).PrintEndLine(" state)");
                p.WithIncreasedIndent().PrintLine("=> this.accessor.Update(ref state);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteBakerImpl(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct StatBaker");
            p.OpenScope();
            {
                p.PrintLine("public readonly IBaker Baker");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.baker.Baker;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly Entity Entity");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.baker.Entity;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_BAKER_XML)
                    .Print(".CreateStatHandle{TStatData}(")
                    .Print("TStatData, bool")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(STAT_HANDLE_T).Print(" CreateStatHandle<TStatData>(")
                    .PrintEndLine("TStatData statData, bool produceChangeEvents)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.baker.CreateStatHandle<TStatData>(statData, produceChangeEvents);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_BAKER_XML)
                    .Print(".CreateStatHandle{TStatData}(")
                    .Print("TValuePair, bool")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(STAT_HANDLE_T).Print(" CreateStatHandle<TStatData>(")
                    .PrintEndLine("ValuePair valuePair, bool produceChangeEvents)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.baker.CreateStatHandle<TStatData>(valuePair, produceChangeEvents);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_BAKER_XML)
                    .Print(".CreateStatHandle{TStatData}(")
                    .Print("TValuePair, bool, out TStatData")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(STAT_HANDLE_T).Print(" CreateStatHandle<TStatData>(")
                    .PrintEndLine("ValuePair valuePair, bool produceChangeEvents, out TStatData statData)");
                p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
                p.WithIncreasedIndent()
                    .PrintLine("=> this.baker.CreateStatHandle<TStatData>(valuePair, produceChangeEvents, out statData);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_BAKER_XML)
                    .Print(".CreateStatHandle(")
                    .Print("TValuePair, bool")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(STAT_HANDLE)
                    .PrintEndLine(" CreateStatHandle(ValuePair valuePair, bool produceChangeEvents)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.baker.CreateStatHandle(valuePair, produceChangeEvents);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_BAKER_XML)
                    .Print(".TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(", TStatModifier, out ").Print(STAT_MODIFIER_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryAddStatModifier(")
                    .Print(STAT_HANDLE).Print(" affectedStatHandle, ")
                    .Print("StatModifier modifier, out ")
                    .Print(STAT_MODIFIER_HANDLE).PrintEndLine(" statModifierHandle)");
                p.WithIncreasedIndent()
                    .PrintLine("=> this.baker.TryAddStatModifier(affectedStatHandle, modifier, out statModifierHandle);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteWorldDataImpl(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct WorldData");
            p.WithIncreasedIndent()
                .PrintLine(": global::System.IDisposable")
                .PrintLine(", global::Unity.Collections.INativeDisposable");
            p.OpenScope();
            {
                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => this.worldData.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".AddModifierTriggerEvent(")
                    .Print("in ").Print(MODIFIER_TRIGGER_EVENT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void AddModifierTriggerEvent(in ModifierTriggerEvent evt)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.AddModifierTriggerEvent(new(evt.handle, evt.modifier));");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".AddStatChangeEvent(")
                    .Print("in ").Print(STAT_CHANGE_EVENT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void AddStatChangeEvent(in ").Print(STAT_CHANGE_EVENT).PrintEndLine(" evt)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.AddStatChangeEvent(evt);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".ClearModifierTriggerEvents(")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void ClearModifierTriggerEvents()");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.ClearModifierTriggerEvents();");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".ClearStatChangeEvents(")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void ClearStatChangeEvents()");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.ClearStatChangeEvents();");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".Clear(")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void Clear()");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.Clear();");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".Dispose(")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void Dispose()");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.Dispose();");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".Dispose(")
                    .Print(JOB_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(JOB_HANDLE).Print(" Dispose(")
                    .Print(JOB_HANDLE).PrintEndLine(" inputDeps)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.Dispose(inputDeps);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".GetModifierTriggerEvents(")
                    .Print(ALLOCATOR_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(NATIVE_ARRAY_MODIFIER_EVENT)
                    .Print(" GetModifierTriggerEvents(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintBeginLine("=> this.worldData.GetModifierTriggerEvents(allocator)")
                    .PrintEndLine(".Reinterpret<ModifierTriggerEvent>();");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".GetModifierTriggerEvents(")
                    .Print(NATIVE_LIST_MODIFIER_EVENT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly void GetModifierTriggerEvents(")
                    .Print(NATIVE_LIST_MODIFIER_EVENT).PrintEndLine(" result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var resultTemp = ref ").Print(UNSAFE_UTILITY).Print(".As<")
                        .Print(NATIVE_LIST_MODIFIER_EVENT).Print(", ")
                        .Print(NATIVE_LIST_MODIFIER_EVENT_GENERIC)
                        .PrintEndLine(">(ref result);");
                    p.PrintLine("this.worldData.GetModifierTriggerEvents(resultTemp);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".GetStatChangeEvents(")
                    .Print(ALLOCATOR_HANDLE)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(NATIVE_ARRAY_STAT_CHANGE_EVENT)
                    .Print(" GetStatChangeEvents(").Print(ALLOCATOR_HANDLE).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.GetStatChangeEvents(allocator);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".GetStatChangeEvents(")
                    .Print(NATIVE_LIST_STAT_CHANGE_EVENT_XML)
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly void GetStatChangeEvents(")
                    .Print(NATIVE_LIST_STAT_CHANGE_EVENT).PrintEndLine(" result)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.GetStatChangeEvents(result);");
                p.PrintEndLine();

                p.PrintBeginLine("/// <inheritdoc cref=\"").Print(STAT_WORLD_DATA_XML)
                    .Print(".SetStatModifierStack(")
                    .Print("in TStatModifierStack")
                    .PrintEndLine(")\"/>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void SetStatModifierStack(in StatModifier.Stack stack)");
                p.WithIncreasedIndent().PrintLine("=> this.worldData.SetStatModifierStack(stack);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteOtherImpl(ref Printer p, References references)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct StatObserver { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct StatModifier { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct ModifierTriggerEvent { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct StatModifierRecord { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct DeferredUpdateStatListJob { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct DeferredUpdateStatQueueJob { }");
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct DeferredUpdateStatStreamJob { }");
            p.PrintEndLine();

            if (references.latiosCore)
            {
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("partial struct DeferredUpdateStatUnsafeBlockListJob { }");
                p.PrintEndLine();
            }
        }

        private static void WriteValuePairImpl(
              ref Printer p
            , string systemTypeName
            , int maxDataSize
            , List<TypeRecord> singleTypes
            , List<TypeRecord> pairTypes
            , List<TypeRecord> incompatTypes
        )
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("partial struct ValuePair");
            p.OpenScope();
            {
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("partial struct Composer { }");
                p.PrintEndLine();

                for (var i = 0; i < singleTypes.Count; i++)
                {
                    var (ns, type, _, size, customNs) = singleTypes[i];

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .Print(STAT_SINGLE).Print("<")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine("> value)");
                    p.WithIncreasedIndent().PrintLine("=> new(value.Value);");
                    p.PrintEndLine();
                }

                for (var i = 0; i < pairTypes.Count; i++)
                {
                    var (ns, type, _, size, customNs) = pairTypes[i];

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> new(value, value);");
                    p.PrintEndLine();

                    size += size;

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .Print("(")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .Print(", ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .Print(")")
                        .PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> new(value.Item1, value.Item2);");
                    p.PrintEndLine();
                }

                var halfDataSize = maxDataSize / 2;

                for (var i = 0; i < singleTypes.Count; i++)
                {
                    var (ns, type, _, size, customNs) = singleTypes[i];

                    if (size <= halfDataSize)
                    {
                        continue;
                    }

                    p.PrintBeginLine("[global::System.Obsolete(\"'")
                        .Print(systemTypeName).Print("' can only store up to ").Print(halfDataSize)
                        .Print(" bytes for a pair value, while '")
                        .Print(type).Print("' is ").Print(size)
                        .Print(" bytes thus cannot be stored as a pair value.")
                        .PrintEndLine("\", true)]");
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> default;");
                    p.PrintEndLine();
                }

                for (var i = 0; i < incompatTypes.Count; i++)
                {
                    var (ns, type, _, size, customNs) = incompatTypes[i];

                    p.PrintBeginLine("[global::System.Obsolete(\"'")
                        .Print(systemTypeName).Print("' can only store up to ")
                        .Print(maxDataSize).Print(" bytes for a single value, while '")
                        .Print(type).Print("' is ").Print(size).Print(" bytes thus incompatible.")
                        .PrintEndLine("\", true)]");
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .Print(STAT_SINGLE).Print("<")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine("> value)");
                    p.WithIncreasedIndent().PrintLine("=> default;");
                    p.PrintEndLine();
                }

                for (var i = 0; i < incompatTypes.Count; i++)
                {
                    var (ns, type, _, size, customNs) = incompatTypes[i];

                    p.PrintBeginLine("[global::System.Obsolete(\"'")
                        .Print(systemTypeName).Print("' can only store up to ")
                        .Print(halfDataSize).Print(" bytes for a pair value, while '")
                        .Print(type).Print("' is ").Print(size).Print(" bytes thus incompatible.")
                        .PrintEndLine("\", true)]");
                    p.PrintBeginLine("public static implicit operator ValuePair(")
                        .PrintIf(size > 8, "in ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type)
                        .PrintEndLine(" value)");
                    p.WithIncreasedIndent().PrintLine("=> default;");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteStatDataStore(ref Printer p, List<TypeRecord> singleTypes, List<TypeRecord> pairTypes)
        {
            p.PrintLine(SERIALIZABLE).PrintLine(STRUCT_LAYOUT_EXPLICIT);
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("internal partial struct StatDataStore");
            p.OpenScope();
            {
                for (var i = 0; i < singleTypes.Count; i++)
                {
                    var (ns, type, typeName, _, customNs) = singleTypes[i];

                    p.PrintBeginLine(SERIALIZED_FIELD).PrintEndLine(FIELD_OFFSET_0);
                    p.PrintBeginLine("private ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).Print(" _").Print(typeName).PrintEndLine(";");
                    p.PrintEndLine();
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static StatDataStore Store(bool isPair, in ")
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

                    p.PrintBeginLine("static StatDataStore StoreSingle(in ")
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
                                    .Print(" => new StatDataStore { _")
                                    .Print(typeName).Print(" = currentValue.")
                                    .Print(typeName).PrintEndLine(" },");
                            }

                            p.PrintLine("_ => default,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("static StatDataStore StorePair(in ")
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
                                    .Print(", currentValue.").Print(typeName).PrintEndLine(").data,");
                            }

                            p.PrintLine("_ => default,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    p.PrintBeginLine("result = pair._data._").Print(typeName)
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
                                        .PrintEndLine("(pair._data).baseValue;");
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    p.PrintBeginLine("result = pair._data._").Print(typeName).PrintEndLine(";");
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
                                        .PrintEndLine("(pair._data).currentValue;");
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    p.PrintBeginLine("pair._data._").Print(typeName)
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
                                    p.PrintBeginLine("pair._data = new Pair").Print(typeName)
                                        .Print("(pair._data) { baseValue = value.").Print(typeName)
                                        .PrintEndLine(" }.data;");
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    p.PrintBeginLine("pair._data._").Print(typeName)
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
                                    p.PrintBeginLine("pair._data = new Pair").Print(typeName)
                                        .Print("(pair._data) { currentValue = value.").Print(typeName)
                                        .PrintEndLine(" }.data;");
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    p.PrintBeginLine("pair._data._").Print(typeName)
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
                                    p.PrintBeginLine("pair._data = new Pair").Print(typeName)
                                        .Print("(pair._data) { baseValue = baseValue.").Print(typeName)
                                        .Print(", currentValue = currentValue.").Print(typeName)
                                        .PrintEndLine(" }.data;");
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

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    .Print(" => ").Print(HASH_VALUE)
                                    .Print(".Combine(pair._type, pair._data._").Print(typeName)
                                    .PrintEndLine("),");
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
                                    .Print(typeName).PrintEndLine("(pair._data)),");
                            }

                            p.PrintLine("_ => 0,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
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
                                    .Print(" => pairA._data._").Print(typeName)
                                    .Print(".Equals(pairB._data._").Print(typeName)
                                    .PrintEndLine("),");
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
                                    .Print(" => new Pair").Print(typeName).Print("(pairA._data).Equals(new Pair")
                                    .Print(typeName).PrintEndLine("(pairB._data)),");
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
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("struct Pair").Print(typeName)
                    .Print(" : ").Print(IEQUATABLE).Print("<Pair").Print(typeName).PrintEndLine(">");
                p.OpenScope();
                {
                    p.PrintLine(FIELD_OFFSET_0);
                    p.PrintLine("public StatDataStore data;");
                    p.PrintEndLine();

                    p.PrintLine(FIELD_OFFSET_0);
                    p.PrintBeginLine("public ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).PrintEndLine(" baseValue;");
                    p.PrintEndLine();

                    p.PrintLine(string.Format(FIELD_OFFSET_X, size));
                    p.PrintBeginLine("public ")
                        .PrintIf(customNs, ns)
                        .PrintIf(customNs, ".")
                        .Print(type).PrintEndLine(" currentValue;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public Pair").Print(typeName).PrintEndLine("(in StatDataStore data) : this()");
                    p.OpenScope();
                    {
                        p.PrintLine("this.data = data;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
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

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override int GetHashCode()");
                    p.WithIncreasedIndent().PrintBeginLine("=> ")
                        .Print(HASH_VALUE).PrintEndLine(".Combine(baseValue, currentValue);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override bool Equals(object obj)");
                    p.WithIncreasedIndent().PrintBeginLine("=> obj is Pair")
                        .Print(typeName).PrintEndLine(" other && Equals(other);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
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
            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static bool IsCompatible<TStatData>(TStatData value)");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.ValueType, value.IsValuePair);");
            p.PrintEndLine();

            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static bool IsCompatible(in ValuePair value)");
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.Type, value.IsPair);");
            p.PrintEndLine();

            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatible(in ")
                .Print(STAT_VARIANT).PrintEndLine(" value, bool isPair)");
            p.WithIncreasedIndent().PrintLine("=> IsCompatible(value.Type, isPair);");
            p.PrintEndLine();

            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool IsCompatible(")
                .Print(STAT_VARIANT_TYPE).PrintEndLine(" type, bool isPair)");
            p.WithIncreasedIndent().PrintLine("=> isPair ? IsCompatiblePair(type) : IsCompatibleSingle(type);");
            p.PrintEndLine();

            WriteXmlDoc(ref p, containerType, singleTypes);
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
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
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
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
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private static void ThrowIfMismatchedTypes(in ")
                .Print(STAT_VARIANT).Print(" baseValue, in ")
                .Print(STAT_VARIANT).PrintEndLine(" currentValue)");
            p.OpenScope();
            {
                p.PrintLine("if (baseValue.Type == currentValue.Type) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VARIANT_TYPE_EXCEPTION)
                    .Print("($\"Base and current values are of different types, ")
                    .PrintEndLine("respectively '{baseValue.Type.ToStringFast()}' and '{currentValue.Type.ToStringFast()}'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("private static void ThrowIfNotCompatible<TStatData>(TStatData value)");
            p.WithIncreasedIndent().PrintBeginLine("where TStatData : unmanaged, ").PrintEndLine(ISTAT_DATA);
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VARIANT_TYPE_EXCEPTION)
                    .Print("($\"Stat data 'typeof(TStatData)' contains values of type '{value.ValueType.ToStringFast()}'")
                    .Print(" which is not compatible to the stat system 'typeof(")
                    .Print(containerType)
                    .PrintEndLine(")'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("private static void ThrowIfNotCompatible(in ValuePair value)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VARIANT_TYPE_EXCEPTION)
                    .Print("($\"Value of type '{value.Type.ToStringFast()}' is not compatible to the stat system 'typeof(")
                    .Print(containerType)
                    .PrintEndLine(")'.\");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private static void ThrowIfNotCompatible(in ")
                .Print(STAT_VARIANT).PrintEndLine(" value, bool isPair)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCompatible(value, isPair)) return;");
                p.PrintEndLine();

                p.PrintBeginLine("throw new ").Print(STAT_VARIANT_TYPE_EXCEPTION)
                    .Print("($\"Value of type '{value.Type.ToStringFast()}' is not compatible to the stat system 'typeof(")
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
