using System;

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
        private const string VALIDATION_ATTRIBUTES = "[HideInCallstack, StackTraceHidden, " +
            "Conditional(\"UNITY_EDITOR\"), Conditional(\"DEVELOPMENT_BUILD\")]";

        public readonly string WriteCode()
        {
            var count = statDataCollection.Count;

            var enumUnderlyingType = (count + 1) switch {
                > ushort.MaxValue => "int",
                > byte.MaxValue => "ushort",
                _ => "byte",
            };

            var underlyingType = count switch {
                > ushort.MaxValue => "int",
                > byte.MaxValue => "ushort",
                _ => "byte",
            };

            var p = new Printer(0, 1024 * 512);

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

                    WriteConstructors(ref p);
                    WriteProperties(ref p);
                    WriteMethods(ref p);
                    WriteTypeEnum(ref p, enumUnderlyingType);
                    WriteTypeIdStruct(ref p, enumUnderlyingType);
                    WriteIndicesStruct(ref p);
                    WriteIndexStructs(ref p, underlyingType);
                    WriteIndexRecordStruct(ref p);
                    WriteStatIndexRecordStruct(ref p);
                    WriteStatHandleRecordStruct(ref p);
                    WriteStatIndicesStruct(ref p);
                    WriteStatHandlesStruct(ref p);
                    WriteStatDataCollection(ref p);
                    WriteOptionsStruct(ref p);
                    WriteBakerStruct(ref p);
                    WriteAccessorStruct(ref p);
                    WriteReaderStruct(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteExtensionsClass(ref p);

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    WriteHelperConstants(ref p);
                    WriteThrowMethods(ref p, underlyingType);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("static partial class ").Print(typeName).PrintEndLine("Extensions");
                p.OpenScope();
                {
                    WriteHelperConstants(ref p);
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

        private readonly void WriteConstructors(ref Printer p)
        {
            p.PrintBeginLine("static ").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfTypesLengthExceedsStatUserDataCapacity();");
            }
            p.CloseScope();
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
            p.PrintBeginLine("/// ").PrintEndLine("<summary>");
            p.PrintBeginLine("/// ").Print("Returns a <see cref=\"").Print(typeName)
                .PrintEndLine("\"/> from the value of <typeparamref name=\"T\"/>.");
            p.PrintBeginLine("/// ").PrintEndLine("</summary>");
            p.PrintBeginLine("/// ").PrintEndLine("<remarks>");
            p.PrintBeginLine("/// ").Print("The layout and size of <typeparamref name=\"T\"/> ")
                .Print("must be the same as <see cref=\"").Print(typeName).Print("\"/>, because this method uses ")
                .Print("<c>UnsafeUtility.As&lt;T, ").Print(typeName)
                .PrintEndLine("&gt;</c> under the hood.");
            p.PrintBeginLine("/// ").PrintEndLine("</remarks>");
            p.PrintBeginLine("/// ").PrintEndLine("<seealso cref=\"UnsafeUtility.As{U, T}\"/>");
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").Print(typeName).PrintEndLine(" CastFrom<T>(T value)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfCannotCastFromType<T>();");
                p.PrintEndLine();

                p.PrintBeginLine("return UnsafeUtility.As<T, ").Print(typeName).PrintEndLine(">(ref value);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("/// <inheritdoc cref=\"CastFrom{T}(T)\"/>");
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ref ").Print(typeName).PrintEndLine(" CastFrom<T>(ref T value)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfCannotCastFromType<T>();");
                p.PrintEndLine();

                p.PrintBeginLine("return ref UnsafeUtility.As<T, ").Print(typeName).PrintEndLine(">(ref value);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public static Indices GetIndicesFrom<T>(T value)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("return CastFrom<T>(value).indices;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public static ref Indices GetIndicesFrom<T>(ref T value)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("return ref CastFrom<T>(ref value).indices;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public static StatIndices GetStatIndicesFrom<T>(T value)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("return CastFrom<T>(value).GetStatIndices();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public static StatHandles GetStatHandlesFrom<T>(T value, Entity entity)");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("return CastFrom<T>(value).GetStatHandles(entity);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly StatIndices GetStatIndices()");
            p.OpenScope();
            {
                p.PrintLine("return indices.ToStatIndices();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly StatHandles GetStatHandles(Entity entity)");
            p.OpenScope();
            {
                p.PrintLine("return indices.ToStatHandles(entity);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public readonly void FindValidIndices(NativeHashMap<TypeId, Index> result)");
            p.OpenScope();
            {
                p.PrintLine("result.Clear();");
                p.PrintLine("result.IncreaseCapacityTo(LENGTH);");
                p.PrintEndLine();

                p.PrintLine("var indices = this.indices;");
                p.PrintLine("var types = Types;");
                p.PrintLine("var length = types.Length;");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < length; i++)");
                p.OpenScope();
                {
                    p.PrintLine("var type = types[i];");
                    p.PrintLine("var index = indices[type];");
                    p.PrintEndLine();

                    p.PrintLine("if (index.IsValid)");
                    p.OpenScope();
                    {
                        p.PrintLine("result.TryAdd(type, index);");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public readonly void FindValidStatIndices(NativeHashMap<TypeId, StatIndex> result)");
            p.OpenScope();
            {
                p.PrintLine("result.Clear();");
                p.PrintLine("result.IncreaseCapacityTo(LENGTH);");
                p.PrintEndLine();

                p.PrintLine("var indices = this.indices;");
                p.PrintLine("var types = Types;");
                p.PrintLine("var length = types.Length;");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < length; i++)");
                p.OpenScope();
                {
                    p.PrintLine("var type = types[i];");
                    p.PrintLine("var index = indices[type];");
                    p.PrintEndLine();

                    p.PrintLine("if (index.IsValid)");
                    p.OpenScope();
                    {
                        p.PrintLine("result.TryAdd(type, index);");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public readonly void FindValidStatHandles(Entity entity, NativeHashMap<TypeId, StatHandle> result)");
            p.OpenScope();
            {
                p.PrintLine("result.Clear();");
                p.PrintLine("result.IncreaseCapacityTo(LENGTH);");
                p.PrintEndLine();

                p.PrintLine("var indices = this.indices;");
                p.PrintLine("var types = Types;");
                p.PrintLine("var length = types.Length;");
                p.PrintEndLine();

                p.PrintLine("for (var i = 0; i < length; i++)");
                p.OpenScope();
                {
                    p.PrintLine("var type = types[i];");
                    p.PrintLine("var index = indices[type];");
                    p.PrintEndLine();

                    p.PrintLine("if (index.IsValid)");
                    p.OpenScope();
                    {
                        p.PrintLine("result.TryAdd(type, new(entity, index));");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteTypeEnum(ref Printer p, string underlyingType)
        {
            p.Print("#region TYPE ENUM").PrintEndLine();
            p.Print("#endregion ======").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum Type : ").PrintEndLine(underlyingType);
            p.OpenScope();
            {
                p.PrintLine("Undefined = 0,");

                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine(statData.typeName).Print(" = ").Print(i + 1).PrintEndLine(",");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteTypeIdStruct(ref Printer p, string underlyingType)
        {
            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct TypeId : IEquatable<TypeId>");
            p.OpenScope();
            {
                p.PrintBeginLine("public const uint OFFSET = ").Print(typeIdOffset).PrintEndLine(";");
                p.PrintEndLine();

                p.PrintLine("public readonly Type Value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public TypeId(Type value)");
                p.OpenScope();
                {
                    p.PrintLine("Value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator TypeId(Type value)");
                p.WithIncreasedIndent().PrintLine("=> new(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Type(TypeId id)");
                p.WithIncreasedIndent().PrintLine("=> id.Value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool operator ==(TypeId lhs, TypeId rhs)");
                p.WithIncreasedIndent().PrintLine("=> lhs.Equals(rhs);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool operator !=(TypeId lhs, TypeId rhs)");
                p.WithIncreasedIndent().PrintLine("=> !lhs.Equals(rhs);");
                p.PrintEndLine();

                p.PrintBeginLine("/// ").PrintEndLine("<summary>");
                p.PrintBeginLine("/// ").Print("Converts <paramref name=\"type\"/> to the corresponding valid index")
                    .Print(" of the array <see cref=\"").Print(typeName).PrintEndLine(".Types\"/>.");
                p.PrintBeginLine("/// ").PrintEndLine("</summary>");
                p.PrintBeginLine("/// ").PrintEndLine("<remarks>");
                p.PrintBeginLine("/// ").PrintEndLine("The value of <see cref=\"Type.Undefined\"/> is invalid.");
                p.PrintBeginLine("/// ").PrintEndLine("</remarks>");
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static int ToValidArrayIndex(Type type)");
                p.OpenScope();
                {
                    p.PrintLine("// Remove the value of Type.Undefined");
                    p.PrintLine("return (int)type - 1;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static uint EncodeToStatUserData(Type type)");
                p.OpenScope();
                {
                    p.PrintLine("return (uint)type + OFFSET;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static Type DecodeFromStatUserData(uint userData)");
                p.OpenScope();
                {
                    p.PrintLine("return (Type)(userData - OFFSET);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static Type DecodeFromStatUserData(in StatSystem.Stat stat)");
                p.OpenScope();
                {
                    p.PrintLine("return DecodeFromStatUserData(stat.UserData);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool ValidateType(Type type)");
                p.OpenScope();
                {
                    p.PrintLine("return (uint)type < (uint)LENGTH;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool ValidateStatUserData(uint userData)");
                p.OpenScope();
                {
                    p.PrintLine("return (uint)DecodeFromStatUserData(userData) < (uint)LENGTH;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool ValidateStatUserData(in StatSystem.Stat stat)");
                p.OpenScope();
                {
                    p.PrintLine("return ValidateStatUserData(stat.UserData);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public static bool ValidateStat<TStatData>(in StatSystem.Stat<TStatData> stat)");
                p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
                p.OpenScope();
                {
                    p.PrintLine("if (ValidateStatUserData(stat.UserData) == false) return false;");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];

                        p.PrintBeginLine("if (typeof(TStatData) == typeof(").Print(statData.typeName)
                            .PrintEndLine(")) return true;");
                    }

                    p.PrintEndLine();
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public bool Equals(TypeId other)");
                p.WithIncreasedIndent().PrintLine("=> Value == other.Value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is TypeId other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override int GetHashCode()");
                p.WithIncreasedIndent().PrintBeginLine("=> ((").Print(underlyingType)
                    .PrintEndLine(")Value).GetHashCode();");
                p.PrintEndLine();


            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteIndexStructs(ref Printer p, string fieldType)
        {
            p.Print("#region INDEX").PrintEndLine();
            p.Print("#endregion ==").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("[Serializable]");
            p.PrintLine("public partial struct Index : IEquatable<Index>");
            p.OpenScope();
            {
                p.PrintLine("public static readonly Index Null = default;");
                p.PrintEndLine();

                p.PrintLine("/// <inheritdoc cref=\"StatIndex.value\"/>");
                p.PrintBeginLine("public ").Print(fieldType).PrintEndLine(" value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Index(").Print(fieldType).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("this.value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool IsValid");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => value > Null.value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator Index(")
                    .Print(fieldType).PrintEndLine(" value)");
                p.WithIncreasedIndent().PrintLine("=> new(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(fieldType)
                    .PrintEndLine("(Index index)");
                p.WithIncreasedIndent().PrintLine("=> index.value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Index(StatIndex index)");
                p.WithIncreasedIndent().PrintBeginLine("=> new((").Print(fieldType).PrintEndLine(")index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator StatIndex(Index index)");
                p.WithIncreasedIndent().PrintLine("=> new((int)index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool Equals(Index other)");
                p.WithIncreasedIndent().PrintLine("=> value == other.value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is Index other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> value.GetHashCode();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override string ToString()");
                p.WithIncreasedIndent().PrintLine("=> value.ToString();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly FixedString32Bytes ToFixedString()");
                p.WithIncreasedIndent().PrintLine("=> value.ToFixedString();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatHandle ToStatHandle(Entity entity)");
                p.WithIncreasedIndent().PrintLine("=> new(entity, this);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region INDEX<TSTAT_DATA>").PrintEndLine();
            p.Print("#endregion ==============").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("[Serializable]");
            p.PrintLine("public partial struct Index<TStatData> : IEquatable<Index<TStatData>>");
            p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
            p.OpenScope();
            {
                p.PrintLine("public static readonly Index<TStatData> Null = default;");
                p.PrintEndLine();

                p.PrintLine("/// <inheritdoc cref=\"StatIndex.value\"/>");
                p.PrintBeginLine("public ").Print(fieldType).PrintEndLine(" value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Index(").Print(fieldType).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("this.value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool IsValid");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => value > Null.value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator Index<TStatData>(")
                    .Print(fieldType).PrintEndLine(" value)");
                p.WithIncreasedIndent().PrintLine("=> new(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(fieldType)
                    .PrintEndLine("(Index<TStatData> index)");
                p.WithIncreasedIndent().PrintLine("=> index.value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Index<TStatData>(StatIndex<TStatData> index)");
                p.WithIncreasedIndent().PrintBeginLine("=> new((").Print(fieldType).PrintEndLine(")index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Index<TStatData>(Index index)");
                p.WithIncreasedIndent().PrintLine("=> new(index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator Index(Index<TStatData> index)");
                p.WithIncreasedIndent().PrintLine("=> new(index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator StatIndex<TStatData>(Index<TStatData> index)");
                p.WithIncreasedIndent().PrintLine("=> new((int)index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static implicit operator StatIndex(Index<TStatData> index)");
                p.WithIncreasedIndent().PrintLine("=> new((int)index.value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool Equals(Index<TStatData> other)");
                p.WithIncreasedIndent().PrintLine("=> value == other.value;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is Index<TStatData> other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> value.GetHashCode();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override string ToString()");
                p.WithIncreasedIndent().PrintLine("=> value.ToString();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly FixedString32Bytes ToFixedString()");
                p.WithIncreasedIndent().PrintLine("=> value.ToFixedString();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatHandle<TStatData> ToStatHandle(Entity entity)");
                p.WithIncreasedIndent().PrintLine("=> new(entity, this);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteIndexRecordStruct(ref Printer p)
        {
            p.Print("#region INDEX RECORD").PrintEndLine();
            p.Print("#endregion =========").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct IndexRecord : IEquatable<IndexRecord>");
            p.OpenScope();
            {
                p.PrintLine("public readonly Index Index;");
                p.PrintLine("public readonly Type Type;");
                p.PrintLine("public readonly bool IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public IndexRecord(Index index, Type type, bool isValid)");
                p.OpenScope();
                {
                    p.PrintLine("Index = index;");
                    p.PrintLine("Type = type;");
                    p.PrintLine("IsValid = isValid;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public void Deconstruct(out Index index, out Type type, out bool isValid)");
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

        private readonly void WriteStatIndexRecordStruct(ref Printer p)
        {
            p.Print("#region STAT INDEX RECORD").PrintEndLine();
            p.Print("#endregion ==============").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct StatIndexRecord : IEquatable<StatIndexRecord>");
            p.OpenScope();
            {
                p.PrintLine("public readonly StatIndex Index;");
                p.PrintLine("public readonly Type Type;");
                p.PrintLine("public readonly bool IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public StatIndexRecord(StatIndex index, Type type, bool isValid)");
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
                p.PrintLine("public bool Equals(StatIndexRecord other)");
                p.WithIncreasedIndent().PrintBeginLine("=> Index == other.Index && Type == other.Type")
                    .PrintEndLine(" && IsValid == other.IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is StatIndexRecord other && Equals(other);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override int GetHashCode()");
                p.WithIncreasedIndent().PrintLine("=> HashValue.Combine(Index, Type, IsValid);");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteStatHandleRecordStruct(ref Printer p)
        {
            p.Print("#region STAT HANDLE RECORD").PrintEndLine();
            p.Print("#endregion ===============").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly partial struct StatHandleRecord : IEquatable<StatHandleRecord>");
            p.OpenScope();
            {
                p.PrintLine("public readonly StatHandle Handle;");
                p.PrintLine("public readonly Type Type;");
                p.PrintLine("public readonly bool IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public StatHandleRecord(StatHandle handle, Type type, bool isValid)");
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
                p.PrintLine("public bool Equals(StatHandleRecord other)");
                p.WithIncreasedIndent().PrintBeginLine("=> Handle == other.Handle && Type == other.Type")
                    .PrintEndLine(" && IsValid == other.IsValid;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public override bool Equals(object obj)");
                p.WithIncreasedIndent().PrintLine("=> obj is StatHandleRecord other && Equals(other);");
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
            p.Print("#region INDICES").PrintEndLine();
            p.Print("#endregion ====").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Indices : IHasLength, IAsSpan<Index>, IAsReadOnlySpan<Index>");
            p.OpenScope();
            {
                p.PrintBeginLine("public const int LENGTH = ").Print(typeName).PrintEndLine(".LENGTH;");
                p.PrintEndLine();

                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintBeginLine("public Index<").Print(statData.typeName).Print("> ")
                        .Print(statData.fieldName).PrintEndLine(";");
                }

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Indices(ReadOnlySpan<Index> values) : this()");
                p.OpenScope();
                {
                    p.PrintLine("values.CopyTo(AsSpan());");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref Index this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[index];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public ref Index this[Type type]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];");
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
                p.PrintLine("public static explicit operator Indices(Span<Index> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator Indices(ReadOnlySpan<Index> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool TryGet(Type type, out Index result)");
                p.OpenScope();
                {
                    p.PrintLine("var index = TypeId.ToValidArrayIndex(type);");
                    p.PrintEndLine();

                    p.PrintLine("if ((uint)index < (uint)LENGTH)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = AsReadOnlySpan()[index];");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly Span<Index> AsSpan()");
                p.OpenScope();
                {
                    p.PrintLine("unsafe");
                    p.OpenScope();
                    {
                        var firstFieldName = statDataCollection[0].fieldName;

                        p.PrintBeginLine("fixed (void* ptr = &").Print(firstFieldName).PrintEndLine(")");
                        p.OpenScope();
                        {
                            p.PrintLine("return new Span<Index>(ptr, LENGTH);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<Index> AsReadOnlySpan()");
                p.WithIncreasedIndent().PrintLine("=> AsSpan();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly ReadOnlySpan<Index>.Enumerator GetEnumerator()");
                p.WithIncreasedIndent().PrintLine("=> AsReadOnlySpan().GetEnumerator();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatIndices ToStatIndices()");
                p.OpenScope();
                {
                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var fieldName = statDataCollection[i].fieldName;

                            p.PrintBeginLine(fieldName).Print(" = ").Print(fieldName).PrintEndLine(",");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatHandles ToStatHandles(Entity entity)");
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
                p.PrintLine("public readonly Index<TStatData> GetIndexFor<TStatData>()");
                p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
                p.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];

                        p.PrintBeginLine("if (typeof(TStatData) == typeof(").Print(statData.typeName).PrintEndLine("))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return (Index<TStatData>)((Index)")
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

        private readonly void WriteStatIndicesStruct(ref Printer p)
        {
            p.Print("#region STAT INDICES").PrintEndLine();
            p.Print("#endregion =========").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct StatIndices : IHasLength, IAsSpan<StatIndex>, IAsReadOnlySpan<StatIndex>");
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
                p.PrintLine("public StatIndices(ReadOnlySpan<StatIndex> values) : this()");
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
                    p.PrintLine("get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];");
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
                p.PrintLine("public static explicit operator StatIndices(Span<StatIndex> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator StatIndices(ReadOnlySpan<StatIndex> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool TryGet(Type type, out StatIndex result)");
                p.OpenScope();
                {
                    p.PrintLine("var index = TypeId.ToValidArrayIndex(type);");
                    p.PrintEndLine();

                    p.PrintLine("if ((uint)index < (uint)LENGTH)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = AsReadOnlySpan()[index];");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
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
                p.PrintLine("public readonly Indices ToIndices()");
                p.OpenScope();
                {
                    p.PrintLine("return new()");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(fieldName).Print(" = (Index<")
                                .Print(statData.typeName).Print(">)")
                                .Print(fieldName).PrintEndLine(",");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatHandles ToStatHandles(Entity entity)");
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
                p.PrintLine("public readonly StatIndex<TStatData> GetStatIndexFor<TStatData>()");
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

                p.PrintLine("public readonly NativeList<StatIndexRecord> ToRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<StatIndexRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var indices = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var index = indices[i];");
                        p.PrintLine("result.AddNoResize(new StatIndexRecord(index, types[i], index.IsValid));");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<StatIndexRecord> ToValidRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<StatIndexRecord>(LENGTH, allocator);");
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
                            p.PrintLine("result.AddNoResize(new StatIndexRecord(index, types[i], true));");
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

        private readonly void WriteStatHandlesStruct(ref Printer p)
        {
            p.Print("#region STAT HANDLES").PrintEndLine();
            p.Print("#endregion =========").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct StatHandles : IHasLength, IAsSpan<StatHandle>, IAsReadOnlySpan<StatHandle>");
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
                p.PrintLine("public StatHandles(ReadOnlySpan<StatHandle> values) : this()");
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
                    p.PrintLine("get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];");
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
                p.PrintLine("public static explicit operator StatHandles(Span<StatHandle> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static explicit operator StatHandles(ReadOnlySpan<StatHandle> values)");
                p.WithIncreasedIndent().PrintLine("=> new(values);");

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool TryGet(Type type, out StatHandle result)");
                p.OpenScope();
                {
                    p.PrintLine("var index = TypeId.ToValidArrayIndex(type);");
                    p.PrintEndLine();

                    p.PrintLine("if ((uint)index < (uint)LENGTH)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = AsReadOnlySpan()[index];");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
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
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(fieldName).Print(" = (Index<")
                                .Print(statData.typeName).Print(">)")
                                .Print(fieldName).PrintEndLine(".index,");
                        }
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly StatIndices ToStatIndices()");
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
                p.PrintLine("public readonly StatHandle<TStatData> GetStatHandleFor<TStatData>()");
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

                p.PrintLine("public readonly NativeList<StatHandleRecord> ToRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<StatHandleRecord>(LENGTH, allocator);");
                    p.PrintLine("var types = Types;");
                    p.PrintLine("var handles = AsReadOnlySpan();");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < LENGTH; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var handle = handles[i];");
                        p.PrintLine("result.AddNoResize(new StatHandleRecord(handle, types[i], handle.IsValid));");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly NativeList<StatHandleRecord> ToValidRecords(AllocatorManager.AllocatorHandle allocator)");
                p.OpenScope();
                {
                    p.PrintLine("var result = new NativeList<StatHandleRecord>(LENGTH, allocator);");
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
                            p.PrintLine("result.AddNoResize(new StatHandleRecord(handle, types[i], true));");
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

                p.Print("#region STAT DATA - ").Print(typeName.ToUpper()).PrintEndLine();
                p.Print("#endregion =========").PrintRepeat('=', typeName.Length).PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly StatSystem.ValuePair ToValuePair()");
                    p.WithIncreasedIndent().PrintBeginLine("=> IsValuePair ")
                        .Print("? new StatSystem.ValuePair(BaseValue, CurrentValue) ")
                        .PrintEndLine(": new StatSystem.ValuePair(CurrentValue);");
                    p.PrintEndLine();

                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static partial class Params");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public static StatDataParams<")
                            .Print(typeName)
                            .PrintEndLine("> Create(Option<bool> produceChangeEvents = default)");
                        p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                            .Print(typeName).Print("(), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.")
                            .Print(typeName).PrintEndLine("));");
                        p.PrintEndLine();

                        var singleArgName = statData.singleValue ? "value" : "baseValue";

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public static StatDataParams<")
                            .Print(typeName).Print("> Create(")
                            .Print(statData.valueTypeName).Print(" ").Print(singleArgName)
                            .PrintEndLine(", Option<bool> produceChangeEvents = default)");
                        p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                            .Print(typeName).Print("(").Print(singleArgName).Print("), default, produceChangeEvents, ")
                            .Print("TypeId.EncodeToStatUserData(Type.").Print(typeName).PrintEndLine("));");
                        p.PrintEndLine();

                        if (statData.singleValue == false)
                        {
                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("public static StatDataParams<")
                                .Print(typeName).Print("> Create(")
                                .Print(statData.valueTypeName).Print(" baseValue, ")
                                .Print(statData.valueTypeName).Print(" currentValue, ")
                                .PrintEndLine("Option<bool> produceChangeEvents = default)");
                            p.WithIncreasedIndent().PrintBeginLine("=> new(new ")
                                .Print(typeName).Print("(baseValue, currentValue), default, produceChangeEvents, ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(typeName).PrintEndLine("));");
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteOptionsStruct(ref Printer p)
        {
            p.Print("#region OPTIONS").PrintEndLine();
            p.Print("#endregion ====").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Options");
            p.OpenScope();
            {
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public partial struct Data");
                p.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;
                        var typeName = statData.typeName;

                        p.PrintBeginLine("public Option<").Print(typeName).Print("> ")
                            .Print(fieldName).PrintEndLine(";");
                    }

                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public Data(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;
                            var typeName = statData.typeName;

                            p.PrintBeginLine(comma).Print("Option<").Print(typeName).Print("> ")
                                .Print(fieldName).PrintEndLine(" = default");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(")");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine("this.").Print(fieldName).Print(" = ")
                                .Print(fieldName).PrintEndLine(";");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public bool TrySet(Type type, in StatSystem.Stat stat)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < count; i++)
                            {
                                var statData = statDataCollection[i];
                                var fieldName = statData.fieldName;
                                var typeName = statData.typeName;

                                p.PrintBeginLine("case Type.").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("this.").Print(fieldName).Print(" = ")
                                        .Print("StatSystem.API.MakeStatData<").Print(typeName)
                                        .PrintEndLine(">(stat.ValuePair);");
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
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public partial struct ProduceChangeEvents");
                p.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("public Option<bool> ")
                            .Print(fieldName).PrintEndLine(";");
                    }

                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public ProduceChangeEvents(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(comma).Print("Option<bool> ")
                                .Print(fieldName).PrintEndLine(" = default");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(")");
                    p.OpenScope();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine("this.").Print(fieldName).Print(" = ")
                                .Print(fieldName).PrintEndLine(";");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public bool TrySet(Type type, in StatSystem.Stat stat)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (type)");
                        p.OpenScope();
                        {
                            for (var i = 0; i < count; i++)
                            {
                                var statData = statDataCollection[i];
                                var fieldName = statData.fieldName;
                                var typeName = statData.typeName;

                                p.PrintBeginLine("case Type.").Print(typeName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("this.").Print(fieldName)
                                        .PrintEndLine(" = stat.ProduceChangeEvents;");
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
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteBakerStruct(ref Printer p)
        {
            p.Print("#region BAKER").PrintEndLine();
            p.Print("#endregion ==").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Baker");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Baker<").Print(typeName)
                    .PrintEndLine("> Create(StatSystem.Baker baker)");
                p.OpenScope();
                {
                    p.PrintLine("return new() { baker = baker, statCollection = new(), };");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Baker<").Print(typeName)
                    .Print("> Bake(IBaker ibaker, Entity entity, ")
                    .PrintEndLine("StatSystem.ValuePair.Composer valuePairComposer = default)");
                p.OpenScope();
                {
                    p.PrintLine("var baker = StatSystem.API.BakeStatComponents(ibaker, entity, valuePairComposer);");
                    p.PrintLine("return new() { baker = baker, statCollection = new(), };");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Baker<T> where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("public StatSystem.Baker baker;");
                p.PrintBeginLine("public ").Print(typeName).PrintEndLine(" statCollection;");
                p.PrintEndLine();

                p.PrintLine("static Baker()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfCannotCastToType<T>();");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteResetMethod(ref p);
                WriteCreateAllStatsMethod(ref p, statDataCollection.AsReadOnlySpan());
                WriteCreateStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteCreateStatMethod(ref p, statDataCollection[i]);
                }

                WriteSetStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteSetStatMethod(ref p, statDataCollection[i]);
                }

                WriteSetOrCreateStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteSetOrCreateStatMethod(ref p, statDataCollection[i]);
                }

                WriteCreateComponentMethod(ref p, typeName);
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteResetMethod(ref Printer p)
            {
                p.PrintLine("public Baker<T> Reset(StatSystem.ValuePair.Composer valuePairComposer = default)");
                p.OpenScope();
                {
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintLine("indices.AsSpan().Fill(default);");
                    p.PrintLine("baker.Clear(valuePairComposer);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCreateAllStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Baker<T> CreateAllStats(bool produceChangeEvents = false)");
                p.OpenScope();
                {
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];

                        p.PrintBeginLine("indices.").Print(statData.fieldName)
                            .Print(" = baker.CreateStatHandle(new ").Print(statData.typeName)
                            .Print("(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.")
                            .Print(statData.typeName).PrintEndLine(")).index;");
                    }

                    p.PrintEndLine();
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCreateStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Baker<T> CreateStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var comma = i > 0 ? ", " : "  ";

                        p.PrintBeginLine(comma).Print("StatDataParams<").Print(statData.typeName).Print("> ")
                            .Print(statData.fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("if (").Print(fieldName).Print(".IsCreated && ")
                            .Print(fieldName).PrintEndLine(".StatData.HasValue)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("indices.").Print(fieldName)
                                .Print(" = baker.CreateStatHandle(")
                                .Print(fieldName).Print(".StatData.GetValueOrThrow(), ")
                                .Print(fieldName).Print(".ProduceChangeEvents.GetValueOrDefault(), ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(statData.typeName)
                                .PrintEndLine(")).index;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCreateStatMethod(ref Printer p, StatDataDefinition statData)
            {
                p.PrintBeginLine("public Baker<T> CreateStat(StatDataParams<").Print(statData.typeName)
                    .PrintEndLine("> value)");
                p.OpenScope();
                {
                    p.PrintLine("if (value.IsCreated && value.StatData.HasValue)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("statCollection.indices.").Print(statData.fieldName)
                            .Print(" = baker.CreateStatHandle(value.StatData.GetValueOrThrow(), ")
                            .Print("value.ProduceChangeEvents.GetValueOrDefault(), ")
                            .Print("TypeId.EncodeToStatUserData(Type.").Print(statData.typeName)
                            .PrintEndLine(")).index;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Baker<T> SetStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var comma = i > 0 ? ", " : "  ";

                        p.PrintBeginLine(comma).Print("StatDataParams<").Print(statData.typeName).Print("> ")
                            .Print(statData.fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var entity = baker.Entity;");
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("if (").Print(fieldName).Print(".IsCreated && ")
                            .Print(fieldName).PrintEndLine(".StatData.HasValue)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                                .Print(">(entity, indices.")
                                .Print(fieldName).PrintEndLine(");");
                            p.PrintBeginLine("var userData = TypeId.EncodeToStatUserData(Type.")
                                .Print(typeName).PrintEndLine(");");
                            p.PrintEndLine();

                            p.PrintBeginLine("ThrowCannotSetIfDoesNotExist(baker, handle, userData, \"")
                                .Print(typeName).PrintEndLine("\");");
                            p.PrintEndLine();

                            p.PrintBeginLine("baker.SetStat(handle, ")
                                .Print(fieldName).Print(".StatData.GetValueOrThrow(), ")
                                .Print(fieldName).PrintEndLine(".ProduceChangeEvents.GetValueOrDefault(), userData);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetStatMethod(ref Printer p, StatDataDefinition statData)
            {
                p.PrintBeginLine("public Baker<T> SetStat(StatDataParams<").Print(statData.typeName)
                    .PrintEndLine("> value)");
                p.OpenScope();
                {
                    p.PrintLine("if (value.IsCreated && value.StatData.HasValue)");
                    p.OpenScope();
                    {
                        var fieldName = statData.fieldName;
                        var typeName = statData.typeName;

                        p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                            .Print(">(baker.Entity, statCollection.indices.")
                            .Print(fieldName).PrintEndLine(");");
                        p.PrintBeginLine("var userData = TypeId.EncodeToStatUserData(Type.")
                            .Print(typeName).PrintEndLine(");");
                        p.PrintEndLine();

                        p.PrintBeginLine("ThrowCannotSetIfDoesNotExist(baker, handle, userData, \"")
                            .Print(typeName).PrintEndLine("\");");
                        p.PrintEndLine();

                        p.PrintBeginLine("baker.SetStat(handle, value.StatData.GetValueOrThrow(), ")
                            .PrintEndLine("value.ProduceChangeEvents.GetValueOrDefault(), userData);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetOrCreateStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Baker<T> SetOrCreateStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var comma = i > 0 ? ", " : "  ";

                        p.PrintBeginLine(comma).Print("StatDataParams<").Print(statData.typeName).Print("> ")
                            .Print(statData.fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var entity = baker.Entity;");
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("if (").Print(fieldName).Print(".IsCreated && ")
                            .Print(fieldName).PrintEndLine(".StatData.HasValue)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("ref var index = ref indices.").Print(fieldName).PrintEndLine(";");

                            p.PrintBeginLine("index = baker.SetStatOrCreateHandle(new(entity, index), ")
                                .Print(fieldName).Print(".StatData.GetValueOrThrow(), ")
                                .Print(fieldName).Print(".ProduceChangeEvents.GetValueOrDefault(), ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(statData.typeName)
                                .PrintEndLine(")).index;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetOrCreateStatMethod(ref Printer p, StatDataDefinition statData)
            {
                p.PrintBeginLine("public Baker<T> SetOrCreateStat(StatDataParams<").Print(statData.typeName)
                    .PrintEndLine("> value)");
                p.OpenScope();
                {
                    p.PrintLine("if (value.IsCreated && value.StatData.HasValue)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("ref var index = ref statCollection.indices.")
                            .Print(statData.fieldName).PrintEndLine(";");

                        p.PrintBeginLine("index = baker.SetStatOrCreateHandle(new(baker.Entity, index),")
                            .Print(" value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), ")
                            .Print("TypeId.EncodeToStatUserData(Type.").Print(statData.typeName)
                            .PrintEndLine(")).index;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCreateComponentMethod(ref Printer p, string typeName)
            {
                p.PrintBeginLine("/// ").PrintEndLine("<summary>");
                p.PrintBeginLine("/// ").Print("Returns a new baker with the type <typeparamref name=\"TComponentData\"/> ")
                    .Print("that is compatible to <see cref=\"").Print(typeName).PrintEndLine("\"/>.");
                p.PrintBeginLine("/// ").PrintEndLine("</summary>");
                p.PrintBeginLine("/// ").PrintEndLine("<remarks>");
                p.PrintBeginLine("/// ").Print("The layout and size of <typeparamref name=\"TComponentData\"/> ")
                    .Print("must be the same as <see cref=\"").Print(typeName).Print("\"/>, because this method uses ")
                    .Print("<c>UnsafeUtility.As&lt;").Print(typeName)
                    .PrintEndLine(", TComponentData&gt;</c> under the hood.");
                p.PrintBeginLine("/// ").PrintEndLine("</remarks>");
                p.PrintBeginLine("/// ").PrintEndLine("<seealso cref=\"UnsafeUtility.As{U, T}\"/>");
                p.PrintLine("public Baker<TComponentData> CreateComponentData<TComponentData>()");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfCannotCastToType<TComponentData>();");
                    p.PrintEndLine();

                    p.PrintLine("return new() { baker = baker, statCollection = statCollection, };");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteAccessorStruct(ref Printer p)
        {
            p.Print("#region ACCESSOR").PrintEndLine();
            p.Print("#endregion =====").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Accessor");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Accessor<T> Create<T>(Entity entity, T statCollection, ")
                    .PrintEndLine("StatSystem.Accessor accessor, StatSystem.WorldData worldData)");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("where T : unmanaged");
                }
                p = p.DecreasedIndent();
                p.OpenScope();
                {
                    p.PrintBeginLine("return new() { entity = entity, accessor = accessor, worldData = worldData")
                        .Print(", statCollection = ").Print(typeName).Print(".CastFrom(statCollection)")
                        .PrintEndLine(" };");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Accessor<T> where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("public StatSystem.Accessor accessor;");
                p.PrintLine("public StatSystem.WorldData worldData;");
                p.PrintLine("public Entity entity;");
                p.PrintBeginLine("public ").Print(typeName).PrintEndLine(" statCollection;");
                p.PrintEndLine();

                p.PrintLine("static Accessor()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfCannotCastFromType<T>();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => entity != Entity.Null && worldData.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly Indices Indices");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => statCollection.indices;");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteTryCreateAllStatsMethod(ref p, statDataCollection.AsReadOnlySpan());
                WriteTryCreateStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteTryCreateStatMethod(ref p, statDataCollection[i]);
                }

                for (var i = 0; i < count; i++)
                {
                    WriteTryCreateOrsSetStatMethod(ref p, statDataCollection[i]);
                }

                WriteFindValidStatsMethod(ref p);
                WriteTryGetStatForTypeMethod(ref p);

                for (var i = 0; i < count; i++)
                {
                    WriteTryGetStatMethod(ref p, statDataCollection[i]);
                }

                for (var i = 0; i < count; i++)
                {
                    WriteTryGetStatDataMethod(ref p, statDataCollection[i]);
                }

                WriteTrySetBaseValueToStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteTrySetStatBaseValueMethod(ref p, statDataCollection[i]);
                }

                WriteTrySetCurrentValueToStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteTrySetStatCurrentValueMethod(ref p, statDataCollection[i]);
                }

                WriteTrySetValuesToStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteTrySetStatValuesMethod(ref p, statDataCollection[i]);
                }

                WriteTrySetProduceChangeEventsForAllStatsMethod(ref p, statDataCollection.AsReadOnlySpan());
                WriteTrySetProduceChangeEventsForStatsMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteTrySetStatProduceChangeEventsMethod(ref p, statDataCollection[i]);
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteTryCreateAllStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Accessor<T> TryCreateAllStats(bool produceChangeEvents = false)");
                p.OpenScope();
                {
                    p.PrintLine("var entity = this.entity;");
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintLine("var handles = indices.ToStatHandles(entity);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("if (accessor.TryGetStat(handles.").Print(fieldName)
                            .Print(", out _").PrintEndLine(") == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (accessor.TryCreateStatHandle<").Print(typeName)
                                .Print(">(entity, new ").Print(typeName).Print("(), produceChangeEvents, ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(typeName)
                                .PrintEndLine("), out var statHandle))");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("indices.").Print(fieldName).PrintEndLine(" = statHandle.index;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryCreateStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Accessor<T> TryCreateStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var comma = i > 0 ? ", " : "  ";

                        p.PrintBeginLine(comma).Print("StatDataParams<").Print(statData.typeName).Print("> ")
                            .Print(statData.fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var entity = this.entity;");
                    p.PrintLine("ref var indices = ref statCollection.indices;");
                    p.PrintLine("var handles = indices.ToStatHandles(entity);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("if (").Print(fieldName)
                            .Print(".IsCreated && ").Print(fieldName)
                            .Print(".StatData.HasValue && accessor.TryGetStat(handles.").Print(fieldName)
                            .Print(", out _").PrintEndLine(") == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (accessor.TryCreateStatHandle<").Print(typeName)
                                .Print(">(entity, ").Print(fieldName).Print(".StatData.GetValueOrThrow(), ")
                                .Print(fieldName).Print(".ProduceChangeEvents.GetValueOrDefault(), ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(typeName)
                                .PrintEndLine("), out var statHandle))");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("indices.").Print(fieldName).PrintEndLine(" = statHandle.index;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryCreateStatMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintBeginLine("public Accessor<T> TryCreateStat(StatDataParams<").Print(statData.typeName)
                    .PrintEndLine("> value, out bool success)");
                p.OpenScope();
                {
                    p.PrintLine("success = false;");
                    p.PrintEndLine();

                    p.PrintLine("if (value.IsCreated && value.StatData.HasValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("var entity = this.entity;");
                        p.PrintLine("ref var indices = ref statCollection.indices;");
                        p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                            .Print(">(entity, indices.").Print(fieldName).PrintEndLine(");");
                        p.PrintEndLine();

                        p.PrintLine("if (accessor.TryGetStat(handle, out _) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (accessor.TryCreateStatHandle<").Print(typeName)
                                .Print(">(entity, value.StatData.GetValueOrThrow(), ")
                                .Print("value.ProduceChangeEvents.GetValueOrDefault(), ")
                                .Print("TypeId.EncodeToStatUserData(Type.").Print(typeName)
                                .PrintEndLine("), out var statHandle))");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("indices.").Print(fieldName).PrintEndLine(" = statHandle.index;");
                                p.PrintLine("success = true;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryCreateOrsSetStatMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintBeginLine("public Accessor<T> TryCreateOrSetStat(StatDataParams<").Print(statData.typeName)
                    .PrintEndLine("> value, out bool success)");
                p.OpenScope();
                {
                    p.PrintLine("success = false;");
                    p.PrintEndLine();

                    p.PrintLine("if (value.IsCreated && value.StatData.HasValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("var entity = this.entity;");
                        p.PrintLine("ref var indices = ref statCollection.indices;");
                        p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                            .Print(">(entity, indices.").Print(fieldName).PrintEndLine(");");
                        p.PrintBeginLine("var userData = ").Print("TypeId.EncodeToStatUserData(Type.").Print(typeName)
                            .PrintEndLine(");");
                        p.PrintEndLine();

                        p.PrintLine("if (accessor.TryGetStat(handle, out _) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (accessor.TryCreateStatHandle<").Print(typeName)
                                .Print(">(entity, value.StatData.GetValueOrThrow(), ")
                                .Print("value.ProduceChangeEvents.GetValueOrDefault(), ")
                                .PrintEndLine("userData, out var statHandle))");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("indices.").Print(fieldName).PrintEndLine(" = statHandle.index;");
                                p.PrintLine("success = true;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (success = accessor.TrySetStatData<").Print(typeName)
                                .PrintEndLine(">(handle, value.StatData.GetValueOrThrow(), ref worldData))");
                            p.OpenScope();
                            {
                                p.PrintLine("accessor.TrySetStatUserData(handle, userData);");
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintBeginLine("if (success && ")
                                .PrintEndLine("value.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))");
                            p.OpenScope();
                            {
                                p.PrintLine("accessor.TrySetStatProduceChangeEvents(handle, produceChangeEvents);");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteFindValidStatsMethod(ref Printer p)
            {
                p.PrintLine("public readonly Accessor<T> FindValidStats(NativeHashMap<TypeId, StatSystem.Stat> result)");
                p.OpenScope();
                {
                    p.PrintLine("result.Clear();");
                    p.PrintLine("result.IncreaseCapacityTo(LENGTH);");
                    p.PrintEndLine();

                    p.PrintLine("var stats = accessor.GetStats(entity);");
                    p.PrintLine("var length = stats.Length;");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = stats[i];");
                        p.PrintLine("var type = TypeId.DecodeFromStatUserData(stat.UserData);");
                        p.PrintEndLine();

                        p.PrintLine("if (TypeId.ValidateType(type))");
                        p.OpenScope();
                        {
                            p.PrintLine("result.TryAdd(type, stat);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatForTypeMethod(ref Printer p)
            {
                p.PrintBeginLine("public readonly Accessor<T> TryGetStat(Type type, out StatSystem.Stat stat, ")
                    .PrintEndLine("out StatHandle handle, out bool success)");
                p.OpenScope();
                {
                    p.PrintLine("if (Indices.TryGet(type, out var index))");
                    p.OpenScope();
                    {
                        p.PrintLine("handle = new StatHandle(entity, index);");
                        p.PrintBeginLine("success = accessor.TryGetStat(handle, out stat) && ")
                            .PrintEndLine("TypeId.DecodeFromStatUserData(stat.UserData) == type;");
                    }
                    p.CloseScope();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintLine("handle = default;");
                        p.PrintLine("stat = default;");
                        p.PrintLine("success = false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly Accessor<T> TryGetStat(out StatSystem.Stat<")
                    .Print(typeName).Print("> stat, out bool success, out StatHandle<")
                    .Print(typeName).PrintEndLine("> handle, out bool typeMatched)");
                p.OpenScope();
                {
                    p.PrintBeginLine("handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");
                    p.PrintLine("success = accessor.TryGetStat(handle, out stat);");
                    p.PrintBeginLine("typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.")
                        .Print(typeName).PrintEndLine(";");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatDataMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintBeginLine("public readonly Accessor<T> TryGetStatData(out ")
                    .Print(typeName).Print(" statData, out bool success, out StatHandle<").Print(typeName)
                    .PrintEndLine("> handle)");
                p.OpenScope();
                {
                    p.PrintBeginLine("handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");
                    p.PrintEndLine();

                    p.PrintBeginLine("if (accessor.TryGetStat(handle, out var stat) && ")
                        .Print("TypeId.DecodeFromStatUserData(stat.UserData) == Type.")
                        .Print(typeName).PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("statData = StatSystem.API.MakeStatData<")
                            .Print(typeName).PrintEndLine(">(stat.ValuePair);");
                        p.PrintLine("success = true;");
                    }
                    p.CloseScope();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintLine("statData = default;");
                        p.PrintLine("success = false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetBaseValueToStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Accessor<T> TrySetBaseValueToStats(in Options.Data options)");
                p.OpenScope();
                {
                    p.PrintLine("return TrySetBaseValueToStats(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(comma).Print("options.").Print(fieldName).PrintEndLine(".TryGetBaseValue()");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public Accessor<T> TrySetBaseValueToStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var comma = i > 0 ? ", " : "  ";
                        var statData = statDataCollection[i];
                        var typeName = statData.valueTypeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine(comma).Print("Option<").Print(typeName).Print("> ")
                            .Print(fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintLine("var results = NativeArray.Create<bool>(LENGTH, Allocator.Temp);");
                    p.PrintBeginLine("var paramsForStats = NativeArray.CreateFast")
                        .PrintEndLine("<StatValueParams<StatSystem.ValuePair>>(LENGTH, Allocator.Temp);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("paramsForStats[").Print(i).Print("] = new((")
                            .Print(fieldName).Print(".HasValue ? new ")
                            .Print(typeName).Print(" { baseValue = ").Print(fieldName)
                            .Print(".GetValueOrThrow() }.ToValuePair() : Option.None)")
                            .Print(", (StatHandle)handles.").Print(fieldName).PrintEndLine(");");
                    }

                    p.PrintEndLine();

                    p.PrintLine("accessor.TrySetBaseValueToStats(paramsForStats, results, ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetStatBaseValueMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor<T> TrySetStatBaseValue(")
                    .Print(typeName).PrintEndLine(" value, out bool success)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");

                    p.PrintLine("success = accessor.TrySetStatBaseValue(handle, value.ToValuePair(), ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetCurrentValueToStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Accessor<T> TrySetCurrentValueToStats(in Options.Data options)");
                p.OpenScope();
                {
                    p.PrintLine("return TrySetCurrentValueToStats(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(comma).Print("options.").Print(fieldName).PrintEndLine(".TryGetCurrentValue()");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public Accessor<T> TrySetCurrentValueToStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var comma = i > 0 ? ", " : "  ";
                        var statData = statDataCollection[i];
                        var typeName = statData.valueTypeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine(comma).Print("Option<").Print(typeName).Print("> ")
                            .Print(fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintLine("var results = NativeArray.Create<bool>(LENGTH, Allocator.Temp);");
                    p.PrintBeginLine("var paramsForStats = NativeArray.CreateFast")
                        .PrintEndLine("<StatValueParams<StatSystem.ValuePair>>(LENGTH, Allocator.Temp);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("paramsForStats[").Print(i).Print("] = new((")
                            .Print(fieldName).Print(".HasValue ? new ")
                            .Print(typeName).Print(" { currentValue = ").Print(fieldName)
                            .Print(".GetValueOrThrow() }.ToValuePair() : Option.None)")
                            .Print(", (StatHandle)handles.").Print(fieldName).PrintEndLine(");");
                    }

                    p.PrintEndLine();

                    p.PrintLine("accessor.TrySetCurrentValueToStats(paramsForStats, results, ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetStatCurrentValueMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor<T> TrySetStatCurrentValue(")
                    .Print(typeName).PrintEndLine(" value, out bool success)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");

                    p.PrintLine("success = accessor.TrySetStatCurrentValue(handle, value.ToValuePair(), ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetValuesToStatsMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Accessor<T> TrySetValuesToStats(in Options.Data options)");
                p.OpenScope();
                {
                    p.PrintLine("return TrySetValuesToStats(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(comma).Print("options.").Print(fieldName).PrintEndLine();
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public Accessor<T> TrySetValuesToStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var comma = i > 0 ? ", " : "  ";
                        var statData = statDataCollection[i];
                        var typeName = statData.typeName;
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine(comma).Print("Option<").Print(typeName).Print("> ")
                            .Print(fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintLine("var results = NativeArray.Create<bool>(LENGTH, Allocator.Temp);");
                    p.PrintBeginLine("var paramsForStats = NativeArray.CreateFast")
                        .PrintEndLine("<StatValueParams<StatSystem.ValuePair>>(LENGTH, Allocator.Temp);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("paramsForStats[").Print(i).Print("] = new(")
                            .Print(fieldName).Print(".TryGetValuePair(), (StatHandle)handles.")
                            .Print(fieldName).PrintEndLine(");");
                    }

                    p.PrintEndLine();

                    p.PrintLine("accessor.TrySetDataToStats(paramsForStats, results, ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetStatValuesMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor<T> TrySetStatValues(")
                    .Print(typeName).PrintEndLine(" value, out bool success)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");

                    p.PrintLine("success = accessor.TrySetStatValues(handle, value.ToValuePair(), ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetProduceChangeEventsForAllStatsMethod(
                  ref Printer p
                , ReadOnlySpan<StatDataDefinition> statDataCollection
            )
            {
                var count = statDataCollection.Length;

                p.PrintLine("public Accessor<T> TrySetProduceChangeEventsForAllStats(bool value)");
                p.OpenScope();
                {
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintLine("var results = NativeArray.Create<bool>(LENGTH, Allocator.Temp);");
                    p.PrintBeginLine("var paramsForStats = NativeArray.CreateFast")
                        .PrintEndLine("<StatValueParams<StatSystem.ValuePair>>(LENGTH, Allocator.Temp);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("paramsForStats[").Print(i)
                            .Print("] = new(Option.None, (StatHandle)handles.")
                            .Print(fieldName).PrintEndLine(", value);");
                    }

                    p.PrintEndLine();

                    p.PrintLine("accessor.TrySetDataToStats(paramsForStats, results, ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTrySetProduceChangeEventsForStatsMethod(
                  ref Printer p
                , ReadOnlySpan<StatDataDefinition> statDataCollection
            )
            {
                var count = statDataCollection.Length;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public Accessor<T> TrySetProduceChangeEventsForStats(in Options.ProduceChangeEvents options)");
                p.OpenScope();
                {
                    p.PrintLine("return TrySetProduceChangeEventsForStats(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var comma = i > 0 ? ", " : "  ";
                            var statData = statDataCollection[i];
                            var fieldName = statData.fieldName;

                            p.PrintBeginLine(comma).Print("options.").Print(fieldName).PrintEndLine();
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public Accessor<T> TrySetProduceChangeEventsForStats(");
                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var comma = i > 0 ? ", " : "  ";
                        var statData = statDataCollection[i];

                        p.PrintBeginLine(comma).Print("Option<bool> ").Print(statData.fieldName).PrintEndLine(" = default");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintLine("var results = NativeArray.Create<bool>(LENGTH, Allocator.Temp);");
                    p.PrintBeginLine("var paramsForStats = NativeArray.CreateFast")
                        .PrintEndLine("<StatValueParams<StatSystem.ValuePair>>(LENGTH, Allocator.Temp);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;

                        p.PrintBeginLine("paramsForStats[").Print(i)
                            .Print("] = new(Option.None, (StatHandle)handles.")
                            .Print(fieldName).Print(", ").Print(fieldName).PrintEndLine(");");
                    }

                    p.PrintEndLine();

                    p.PrintLine("accessor.TrySetDataToStats(paramsForStats, results, ref worldData);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }


            static void WriteTrySetStatProduceChangeEventsMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public Accessor<T> TrySetStatProduceChangeEvents(Bool<")
                    .Print(typeName).PrintEndLine("> value, out bool success)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");

                    p.PrintLine("success = accessor.TrySetStatProduceChangeEvents(handle, value);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteReaderStruct(ref Printer p)
        {
            p.Print("#region READER").PrintEndLine();
            p.Print("#endregion ===").PrintEndLine();
            p.PrintEndLine();

            var count = statDataCollection.Count;

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Reader");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static Reader<T> Create<T>(Entity entity, T statCollection, ")
                    .PrintEndLine("DynamicBuffer<StatSystem.Stat> statBuffer)");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("where T : unmanaged");
                }
                p = p.DecreasedIndent();
                p.OpenScope();
                {
                    p.PrintBeginLine("return new() { entity = entity")
                        .Print(", statBuffer = statBuffer.AsNativeArray().AsReadOnly()")
                        .Print(", statCollection = ").Print(typeName).Print(".CastFrom(statCollection)")
                        .PrintEndLine(" };");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public partial struct Reader<T> where T : unmanaged");
            p.OpenScope();
            {
                p.PrintLine("public Entity entity;");
                p.PrintLine("public NativeArray<StatSystem.Stat>.ReadOnly statBuffer;");
                p.PrintBeginLine("public ").Print(typeName).PrintEndLine(" statCollection;");
                p.PrintEndLine();

                p.PrintLine("static Reader()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfCannotCastFromType<T>();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly bool IsCreated");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => entity != Entity.Null && statBuffer.IsCreated;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly Indices Indices");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => statCollection.indices;");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteContainsTypeMethod(ref p);
                WriteContainsTStatDataMethod(ref p, statDataCollection.AsReadOnlySpan());

                for (var i = 0; i < count; i++)
                {
                    WriteContainsMethod(ref p, statDataCollection[i]);
                }

                WriteFindValidStatsMethod(ref p);
                WriteGetStatDataOptionsMethod(ref p);
                WriteGetProduceChangeEventsOptionsMethod(ref p);
                WriteTryGetStatForTypeMethod(ref p);

                for (var i = 0; i < count; i++)
                {
                    WriteTryGetStatMethod(ref p, statDataCollection[i]);
                }

                for (var i = 0; i < count; i++)
                {
                    WriteTryGetStatDataMethod(ref p, statDataCollection[i]);
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteContainsTypeMethod(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly Reader<T> Contains(Type type, out bool result)");
                p.OpenScope();
                {
                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintLine("result = Indices.ToStatHandles(entity).TryGet(type, out var handle)");
                    p.WithIncreasedIndent()
                        .PrintLine("&& StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(type), stats);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteContainsTStatDataMethod(ref Printer p, ReadOnlySpan<StatDataDefinition> statDataCollection)
            {
                var count = statDataCollection.Length;

                p.PrintLine("public readonly Reader<T> Contains<TStatData>(out bool result)");
                p.WithIncreasedIndent().PrintLine("where TStatData : unmanaged, IStatData");
                p.OpenScope();
                {
                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintLine("var handles = Indices.ToStatHandles(entity);");
                    p.PrintEndLine();

                    for (var i = 0; i < count; i++)
                    {
                        var statData = statDataCollection[i];
                        var fieldName = statData.fieldName;
                        var typeName = statData.typeName;

                        p.PrintBeginLine("if (typeof(TStatData) == typeof(").Print(statData.typeName).PrintEndLine("))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = StatSystem.API.Contains(handles.")
                                .Print(fieldName).Print(", TypeId.EncodeToStatUserData(Type.")
                                .Print(typeName).PrintEndLine("), stats);");
                            p.PrintLine("return this;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("result = false;");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteContainsMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly Reader<T> Contains(Index<").Print(typeName)
                    .PrintEndLine("> index, out bool result)");
                p.OpenScope();
                {
                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintBeginLine("var handle = new StatHandle<").Print(typeName).PrintEndLine(">(entity, index);");
                    p.PrintBeginLine("result = StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(Type.")
                        .Print(typeName).PrintEndLine("), stats);");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteFindValidStatsMethod(ref Printer p)
            {
                p.PrintLine("public readonly Reader<T> FindValidStats(NativeHashMap<TypeId, StatSystem.Stat> result)");
                p.OpenScope();
                {
                    p.PrintLine("result.Clear();");
                    p.PrintLine("result.IncreaseCapacityTo(LENGTH);");
                    p.PrintEndLine();

                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintLine("var length = stats.Length;");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = stats[i];");
                        p.PrintLine("var type = TypeId.DecodeFromStatUserData(stat.UserData);");
                        p.PrintEndLine();

                        p.PrintLine("if (TypeId.ValidateType(type))");
                        p.OpenScope();
                        {
                            p.PrintLine("result.TryAdd(type, stat);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetStatDataOptionsMethod(ref Printer p)
            {
                p.PrintLine("public readonly Reader<T> GetStatDataOptions(out Options.Data result)");
                p.OpenScope();
                {
                    p.PrintLine("result = new();");
                    p.PrintEndLine();

                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintLine("var length = stats.Length;");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = stats[i];");
                        p.PrintLine("var type = TypeId.DecodeFromStatUserData(stat.UserData);");
                        p.PrintEndLine();

                        p.PrintLine("if (TypeId.ValidateType(type))");
                        p.OpenScope();
                        {
                            p.PrintLine("result.TrySet(type, stat);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetProduceChangeEventsOptionsMethod(ref Printer p)
            {
                p.PrintLine("public readonly Reader<T> GetProduceChangeEventsOptions(out Options.ProduceChangeEvents result)");
                p.OpenScope();
                {
                    p.PrintLine("result = new();");
                    p.PrintEndLine();

                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintLine("var length = stats.Length;");
                    p.PrintEndLine();

                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        p.PrintLine("var stat = stats[i];");
                        p.PrintLine("var type = TypeId.DecodeFromStatUserData(stat.UserData);");
                        p.PrintEndLine();

                        p.PrintLine("if (TypeId.ValidateType(type))");
                        p.OpenScope();
                        {
                            p.PrintLine("result.TrySet(type, stat);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatForTypeMethod(ref Printer p)
            {
                p.PrintBeginLine("public readonly Reader<T> TryGetStat(Type type, out StatSystem.Stat stat, ")
                    .PrintEndLine("out StatHandle handle, out bool success)");
                p.OpenScope();
                {
                    p.PrintLine("if (Indices.TryGet(type, out var index))");
                    p.OpenScope();
                    {
                        p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                        p.PrintLine("handle = new StatHandle(entity, index);");
                        p.PrintBeginLine("success = StatSystem.API.TryGetStat(handle, stats, out stat) && ")
                            .PrintEndLine("TypeId.DecodeFromStatUserData(stat.UserData) == type;");
                    }
                    p.CloseScope();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintLine("handle = default;");
                        p.PrintLine("stat = default;");
                        p.PrintLine("success = false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly Reader<T> TryGetStat(out StatSystem.Stat<")
                    .Print(typeName).Print("> stat, out bool success, out StatHandle<")
                    .Print(typeName).PrintEndLine("> handle, out bool typeMatched)");
                p.OpenScope();
                {
                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintBeginLine("handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");
                    p.PrintLine("success = StatSystem.API.TryGetStat(handle, stats, out stat);");
                    p.PrintBeginLine("typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.")
                        .Print(typeName).PrintEndLine(";");
                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteTryGetStatDataMethod(ref Printer p, StatDataDefinition statData)
            {
                var typeName = statData.typeName;
                var fieldName = statData.fieldName;

                p.PrintBeginLine("public readonly Reader<T> TryGetStatData(out ")
                    .Print(typeName).Print(" statData, out bool success, out StatHandle<").Print(typeName)
                    .PrintEndLine("> handle)");
                p.OpenScope();
                {
                    p.PrintLine("var stats = statBuffer.AsReadOnlySpan();");
                    p.PrintBeginLine("handle = new StatHandle<").Print(typeName)
                        .Print(">(entity, Indices.").Print(fieldName).PrintEndLine(");");
                    p.PrintEndLine();

                    p.PrintBeginLine("if (StatSystem.API.TryGetStat(handle, stats, out var stat) && ")
                        .Print("TypeId.DecodeFromStatUserData(stat.UserData) == Type.")
                        .Print(typeName).PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("statData = StatSystem.API.MakeStatData<")
                            .Print(typeName).PrintEndLine(">(stat.ValuePair);");
                        p.PrintLine("success = true;");
                    }
                    p.CloseScope();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintLine("statData = default;");
                        p.PrintLine("success = false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return this;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteExtensionsClass(ref Printer p)
        {
            var count = statDataCollection.Count;
            var statCollectionType = typeName;

            p.Print("#region EXTENSIONS").PrintEndLine();
            p.Print("#endregion =======").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("public static partial class ").Print(typeName).PrintEndLine("Extensions");
            p.OpenScope();
            {
                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];
                    var typeName = statData.typeName;
                    var valueTypeName = statData.valueTypeName;
                    var singleValue = statData.singleValue;

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static Option<StatSystem.ValuePair> TryGetValuePair(this Option<")
                        .Print(statCollectionType).Print(".").Print(typeName).PrintEndLine("> value)");
                    p.WithIncreasedIndent().PrintBeginLine("=> value.TryGetValue(out var stat) ")
                        .PrintEndLine("? stat.ToValuePair() : Option.None;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static Option<").Print(valueTypeName).Print("> TryGetBaseValue(this Option<")
                        .Print(statCollectionType).Print(".").Print(typeName).PrintEndLine("> value)");
                    p.WithIncreasedIndent().PrintBeginLine("=> value.TryGetValue(out var stat) ")
                        .Print("? stat.").PrintIf(singleValue, "value", "baseValue").PrintEndLine(" : Option.None;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static Option<").Print(valueTypeName).Print("> TryGetCurrentValue(this Option<")
                        .Print(statCollectionType).Print(".").Print(typeName).PrintEndLine("> value)");
                    p.WithIncreasedIndent().PrintBeginLine("=> value.TryGetValue(out var stat) ")
                        .Print("? stat.").PrintIf(singleValue, "value", "currentValue").PrintEndLine(" : Option.None;");
                    p.PrintEndLine();
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static TComponentData ToComponent<TComponentData>(this ")
                    .Print(typeName).PrintEndLine(".Baker<TComponentData> baker)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("return UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref baker.statCollection);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void AddComponentToEntity<TComponentData>(this ")
                    .Print(typeName).PrintEndLine(".Baker<TComponentData> baker)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var component = ref UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref baker.statCollection);");
                    p.PrintLine("var statBaker = baker.baker;");
                    p.PrintLine("statBaker.IBaker.AddComponent(statBaker.Entity, component);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static TComponentData ToComponent<TComponentData>(this ")
                    .Print(typeName).PrintEndLine(".Accessor<TComponentData> accessor)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("return UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref accessor.statCollection);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void SetComponentToEntity<TComponentData>(this ")
                    .Print(typeName).PrintEndLine(".Accessor<TComponentData> accessor, EntityManager entityManager)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var component = ref UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref accessor.statCollection);");
                    p.PrintLine("entityManager.SetComponentData(accessor.entity, component);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void SetComponentToEntity<TComponentData>(this ")
                    .Print(typeName).PrintEndLine(".Accessor<TComponentData> accessor, EntityCommandBuffer ecb)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var component = ref UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref accessor.statCollection);");
                    p.PrintLine("ecb.SetComponent(accessor.entity, component);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static void SetComponentToEntity<TComponentData>(this ")
                    .Print(typeName).Print(".Accessor<TComponentData> accessor, ")
                    .PrintEndLine("EntityCommandBuffer.ParallelWriter ecb, int sortKey)");
                p.WithIncreasedIndent().PrintLine("where TComponentData : unmanaged, IComponentData");
                p.OpenScope();
                {
                    p.PrintBeginLine("ref var component = ref UnsafeUtility.As<")
                        .Print(typeName).PrintEndLine(", TComponentData>(ref accessor.statCollection);");
                    p.PrintLine("ecb.SetComponent(sortKey, accessor.entity, component);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteHelperConstants(ref Printer p)
        {
            p.PrintBeginLine("private const ").Print(METHOD_IMPL_OPTIONS)
                        .Print(" INLINING = ").Print(INLINING).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();
        }

        private readonly void WriteThrowMethods(ref Printer p, string indexType)
        {
            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine("private static void ThrowCannotSetIfDoesNotExist(")
                .PrintEndLine("StatSystem.Baker baker, StatHandle handle, uint userData, string statDataName)");
            p.OpenScope();
            {
                p.PrintLine("if (baker.Contains(handle) == false)");
                p.OpenScope();
                {
                    p.PrintLine("throw new InvalidOperationException(");
                    p.WithIncreasedIndent().PrintBeginLine("$\"")
                        .Print("Cannot set data for stat '{statDataName}' because the handle index is invalid. ")
                        .Print("Please consider using CreateAllStats first.")
                        .PrintEndLine("\"");
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (baker.Contains(handle, userData) == false)");
                p.OpenScope();
                {
                    p.PrintLine("throw new InvalidOperationException(");
                    p.WithIncreasedIndent().PrintBeginLine("$\"")
                        .Print("Cannot set data for stat '{statDataName}' because the stored user data ")
                        .Print("does not equal to '{TypeId.DecodeFromStatUserData(userData)}'.")
                        .Print("Please double-check the creation of the handle '{handle}'.")
                        .PrintEndLine("\"");
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine("private static void ThrowIfCannotCastToType<T>()");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintBeginLine("if (UnsafeUtility.SizeOf<").Print(typeName)
                    .PrintEndLine(">() == UnsafeUtility.SizeOf<T>()) return;");
                p.PrintEndLine();

                p.PrintLine("throw new InvalidCastException(");
                p.WithIncreasedIndent().PrintBeginLine("$\"")
                    .Print("Cannot cast '").Print(typeName).Print("' into '{typeof(T)}' because ")
                    .Print("these types are not of the same size.")
                    .PrintEndLine("\"");
                p.PrintLine(");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine("private static void ThrowIfCannotCastFromType<T>()");
            p.WithIncreasedIndent().PrintLine("where T : unmanaged");
            p.OpenScope();
            {
                p.PrintBeginLine("if (UnsafeUtility.SizeOf<").Print(typeName)
                    .PrintEndLine(">() == UnsafeUtility.SizeOf<T>()) return;");
                p.PrintEndLine();

                p.PrintLine("throw new InvalidCastException(");
                p.WithIncreasedIndent().PrintBeginLine("$\"")
                    .Print("Cannot cast '{typeof(T)}' into '").Print(typeName).Print("' because ")
                    .Print("these types are not of the same size.")
                    .PrintEndLine("\"");
                p.PrintLine(");");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintLine("private static void ThrowIfTypesLengthExceedsStatUserDataCapacity()");
            p.OpenScope();
            {
                var count = statDataCollection.Count;

                p.PrintBeginLine("if (StatSystem.Stat.USER_DATA_SIZE >= UnsafeUtility.SizeOf<")
                    .Print(indexType).PrintEndLine(">()) return;");
                p.PrintEndLine();

                p.PrintLine("throw new OverflowException(");
                p.WithIncreasedIndent().PrintBeginLine("$\"")
                    .Print("'").Print(typeName).Print("' contains ").Print(count).Print(" stat data types ")
                    .Print("thus exceeds the maximum capacity of '").Print(statSystemFullTypeName)
                    .Print(".Stat.UserData' which is ").Print("{StatSystem.Stat.USER_DATA_SIZE} bytes.")
                    .PrintEndLine("\"");
                p.PrintLine(");");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
