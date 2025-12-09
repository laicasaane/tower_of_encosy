namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatCollectionDefinition
    {
        private const string METHOD_IMPL_OPTIONS = "MethodImplOptions";
        private const string INLINING = $"{METHOD_IMPL_OPTIONS}.AggressiveInlining";
        private const string GENERATOR = "\"EncosyTower.SourceGen.Generators.Entities.Stats.StatCollectionGenerator\"";

        private const string AGGRESSIVE_INLINING = "[MethodImpl(INLINING)]";
        private const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    WriteConsts(ref p);

                    p.PrintLine("public Indices indices;");
                    p.PrintEndLine();

                    WriteProperties(ref p);
                    WriteMethods(ref p);
                    WriteTypeEnum(ref p);
                    WriteIndexRecordStruct(ref p);
                    WriteHandleRecordStruct(ref p);
                    WriteIndicesStruct(ref p);
                    WriteHandlesStruct(ref p);
                    WriteStatDataCollection(ref p);

                    p.Print("#region BAKING").PrintEndLine();
                    p.Print("#endregion ===").PrintEndLine();
                    p.PrintEndLine();

                    WriteBakingStruct(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    p.PrintBeginLine("private const ").Print(METHOD_IMPL_OPTIONS)
                        .Print(" INLINING = ").Print(INLINING).PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteConsts(ref Printer p)
        {
            var count = statDataCollection.Count;

            p.PrintBeginLine("public const int LENGTH = ").Print(count).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintLine("private readonly static Type[] s_types = new Type[]");
            p.OpenScope();
            {
                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine("Type.").Print(statData.typeName).PrintEndLine(",");
                }
            }
            p.CloseScope("};");
            p.PrintEndLine();
        }

        private readonly void WriteProperties(ref Printer p)
        {
            p.PrintLine("public static ReadOnlySpan<Type> Types");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("get => s_types;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteMethods(ref Printer p)
        {
        }

        private readonly void WriteTypeEnum(ref Printer p)
        {
            var count = statDataCollection.Count;
            var underlyingType = GeneratorHelpers.GetEnumUnderlyingTypeFromMemberCount(count);

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum Type : ").PrintEndLine(underlyingType);
            p.OpenScope();
            {
                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine(statData.typeName).Print(" = ").Print(i).PrintEndLine(",");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteIndexRecordStruct(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct IndexRecord : IEquatable<IndexRecord>");
            p.OpenScope();
            {
                p.PrintLine("public readonly StatIndex Index;");
                p.PrintLine("public readonly Type Type;");
                p.PrintLine("public readonly bool IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public IndexRecord(StatIndex index, Type type, bool isValid)");
                p.OpenScope();
                {
                    p.PrintLine("Index = index;");
                    p.PrintLine("Type = type;");
                    p.PrintLine("IsValid = isValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void Deconstruct(out StatIndex index, out Type type, out bool isValid)");
                p.OpenScope();
                {
                    p.PrintLine("index = Index;");
                    p.PrintLine("type = Type;");
                    p.PrintLine("isValid = IsValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool Equals(IndexRecord other)");
                p.WithIncreasedIndent().PrintBeginLine("=> Index == other.Index && Type == other.Type")
                    .PrintEndLine(" && IsValid == other.IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is IndexRecord other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> HashValue.Combine(Index, Type, IsValid);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteHandleRecordStruct(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct HandleRecord : IEquatable<HandleRecord>");
            p.OpenScope();
            {
                p.PrintLine("public readonly StatHandle Handle;");
                p.PrintLine("public readonly Type Type;");
                p.PrintLine("public readonly bool IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public HandleRecord(StatHandle handle, Type type, bool isValid)");
                p.OpenScope();
                {
                    p.PrintLine("Handle = handle;");
                    p.PrintLine("Type = type;");
                    p.PrintLine("IsValid = isValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void Deconstruct(out StatHandle handle, out Type type, out bool isValid)");
                p.OpenScope();
                {
                    p.PrintLine("handle = Handle;");
                    p.PrintLine("type = Type;");
                    p.PrintLine("isValid = IsValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool Equals(HandleRecord other)");
                p.WithIncreasedIndent().PrintBeginLine("=> Handle == other.Handle && Type == other.Type")
                    .PrintEndLine(" && IsValid == other.IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is HandleRecord other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> HashValue.Combine(Handle, Type, IsValid);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteIndicesStruct(ref Printer p)
        {
            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Indices : IHasLength, IAsSpan<StatIndex>, IAsReadOnlySpan<StatIndex>");
            p.OpenScope();
            {
                p.PrintBeginLine("public const int LENGTH = ").Print(typeName).PrintEndLine(".LENGTH;");
                p.PrintEndLine();

                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine("public StatIndex<").Print(statData.typeName).Print("> ")
                        .Print(statData.fieldName).PrintEndLine(";");
                }

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Indices(ReadOnlySpan<StatIndex> values) : this()");
                p.OpenScope();
                {
                    p.PrintLine("values.CopyTo(AsSpan());");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref StatIndex this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[index];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref StatIndex this[Type type]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[(int)type];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => LENGTH;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Indices(Span<StatIndex> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Indices(ReadOnlySpan<StatIndex> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine("public readonly Span<StatIndex> AsSpan()");
                p.OpenScope();
                {
                    p.PrintLine("unsafe");
                    p.OpenScope();
                    {
                        var firstFieldName = statDataCollection[0].fieldName;

                        p.PrintBeginLine("fixed (void* ptr = &").Print(firstFieldName).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintLine("return new Span<StatIndex>(ptr, LENGTH);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<StatIndex> AsReadOnlySpan()");
                p.WithIncreasedIndent().PrintLine("=> AsSpan();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<StatIndex>.Enumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> AsReadOnlySpan().GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly Handles ToHandles(Entity entity)");
                p.OpenScope();
                {
                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var fieldName = statDataCollection[i].fieldName;

                            p.PrintBeginLine(fieldName).Print(" = new(entity, ")
                                .Print(fieldName).PrintEndLine("),");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatIndex<TStatData> GetIndexFor<TStatData>()");
                p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
                p.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];

                        p.PrintBeginLine("if (typeof(TStatData) == typeof(").Print(statData.typeName).PrintEndLine("))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return (StatIndex<TStatData>)((StatIndex)")
                                .Print(statData.fieldName).PrintEndLine(");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<IndexRecord> ToRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<IndexRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var indices = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var index = indices[i];");
                        p.PrintLine("result.AddNoResize(new IndexRecord(index, types[i], index.IsValid));");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<IndexRecord> ToValidRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<IndexRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var indices = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var index = indices[i];");
                        p.PrintEndLine();

                        p.PrintLine("if (index.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("result.AddNoResize(new IndexRecord(index, types[i], true));");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteHandlesStruct(ref Printer p)
        {
            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Handles : IHasLength, IAsSpan<StatHandle>, IAsReadOnlySpan<StatHandle>");
            p.OpenScope();
            {
                p.PrintBeginLine("public const int LENGTH = ").Print(typeName).PrintEndLine(".LENGTH;");
                p.PrintEndLine();

                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine("public StatHandle<").Print(statData.typeName).Print("> ")
                        .Print(statData.fieldName).PrintEndLine(";");
                }

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Handles(ReadOnlySpan<StatHandle> values) : this()");
                p.OpenScope();
                {
                    p.PrintLine("values.CopyTo(AsSpan());");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref StatHandle this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[index];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref StatHandle this[Type type]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[(int)type];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => LENGTH;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Handles(Span<StatHandle> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Handles(ReadOnlySpan<StatHandle> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine("public readonly Span<StatHandle> AsSpan()");
                p.OpenScope();
                {
                    p.PrintLine("unsafe");
                    p.OpenScope();
                    {
                        var firstFieldName = statDataCollection[0].fieldName;

                        p.PrintBeginLine("fixed (void* ptr = &").Print(firstFieldName).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintLine("return new Span<StatHandle>(ptr, LENGTH);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<StatHandle> AsReadOnlySpan()");
                p.WithIncreasedIndent().PrintLine("=> AsSpan();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<StatHandle>.Enumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> AsReadOnlySpan().GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly Indices ToIndices()");
                p.OpenScope();
                {
                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var fieldName = statDataCollection[i].fieldName;

                            p.PrintBeginLine(fieldName).Print(" = ")
                                .Print(fieldName).PrintEndLine(".index,");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatHandle<TStatData> GetHandleFor<TStatData>()");
                p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
                p.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];

                        p.PrintBeginLine("if (typeof(TStatData) == typeof(").Print(statData.typeName).PrintEndLine("))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return (StatHandle<TStatData>)((StatHandle)")
                                .Print(statData.fieldName).PrintEndLine(");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<HandleRecord> ToRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<HandleRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var handles = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var handle = handles[i];");
                        p.PrintLine("result.AddNoResize(new HandleRecord(handle, types[i], handle.IsValid));");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<HandleRecord> ToValidRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<HandleRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var handles = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var handle = handles[i];");
                        p.PrintEndLine();

                        p.PrintLine("if (handle.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("result.AddNoResize(new HandleRecord(handle, types[i], true));");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteStatDataCollection(ref Printer p)
        {
            var statDataCollection = this.statDataCollection;
            var length = statDataCollection.Count;

            for (var i = 0; i < length; i++)
            {
                WriteStatCreation(ref p, statDataCollection[i]);
            }

            return;

            static void WriteStatCreation(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;

                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static StatData<")
                        .Print(typeName)
                        .PrintEndLine("> Create(bool produceChangeEvents = false)");
                    p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                        .Print(typeName).PrintEndLine("(), produceChangeEvents);");
                    p.PrintEndLine();

                    var singleArgName = statData.singleValue ? "value" : "baseValue";

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static StatData<")
                        .Print(typeName).Print("> Create(")
                        .Print(statData.valueTypeName).Print(" ").Print(singleArgName)
                        .PrintEndLine(", bool produceChangeEvents = false)");
                    p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                        .Print(typeName).Print("(").Print(singleArgName).PrintEndLine("), produceChangeEvents);");
                    p.PrintEndLine();

                    if (statData.singleValue == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public static StatData<")
                            .Print(typeName).Print("> Create(")
                            .Print(statData.valueTypeName).Print(" baseValue, ")
                            .Print(statData.valueTypeName).Print(" currentValue, ")
                            .PrintEndLine("bool produceChangeEvents = false)");
                        p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                            .Print(typeName).PrintEndLine("(baseValue, currentValue), produceChangeEvents);");
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteBakingStruct(ref Printer p)
        {
            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Baking");
            p.OpenScope();
            {
                p.PrintLine("public StatSystem.StatBaker statBaker;");
                p.PrintBeginLine("public ").Print(typeName).PrintEndLine(" statCollection;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static Baking Begin(StatSystem.StatBaker statBaker)");
                p.OpenScope();
                {
                    p.PrintLine("return new() { statBaker = statBaker, statCollection = new(), };");
                }
                p.CloseScope();
                p.PrintEndLine();

                for (var i = 0; i < count; i++)
                {
                    WriteSetStatMethod(ref p, statDataCollection[i]);
                }

                p.PrintBeginLine("/// ").PrintEndLine("<summary>");
                p.PrintBeginLine("/// ").Print("Converts <see cref=\"statCollection\"/> into the component ")
                    .PrintEndLine("<typeparamref name=\"T\"/> then adds to the entity.");
                p.PrintBeginLine("/// ").PrintEndLine("</summary>");
                p.PrintBeginLine("/// ").PrintEndLine("<remarks>");
                p.PrintBeginLine("/// ").Print("The layout and size of <typeparamref name=\"T\"/> must be the same ")
                    .Print("as <see cref=\"").Print(typeName).PrintEndLine("\"/>");
                p.PrintBeginLine("/// ").PrintEndLine("</remarks>");
                p.PrintLine("public T FinishThenAddComponent<T>()");
                p.WithIncreasedIndent().PrintLine("where T : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var component = ref UnsafeUtility.As<").Print(typeName)
                        .PrintEndLine(", T>(ref statCollection);");

                    p.PrintLine("statBaker.Baker.AddComponent(statBaker.Entity, component);");
                    p.PrintLine("return component;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteSetStatMethod(ref Printer p, StatDataDefinition statData)
            {
                p.PrintBeginLine("public Baking SetStat(StatData<").Print(statData.typeName)
                    .PrintEndLine("> value)");
                p.OpenScope();
                {
                    p.PrintLine("if (value.IsValid)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("statCollection.indices.").Print(statData.fieldName)
                            .PrintEndLine(" = statBaker.CreateStatHandle(value.Data, value.ProduceChangeEvents).index;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }
    }
}
