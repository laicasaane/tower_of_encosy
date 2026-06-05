using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    partial class EnumExtensionsDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.EnumExtensions.EnumExtensionsGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string ALLOCATOR_MANAGER = "UC.AllocatorManager.AllocatorHandle";
        private const string CLASS_VALUES = "Values";
        private const string CLASS_UNDERLYING_VALUES = "UnderlyingValues";
        private const string CLASS_NAMES = "Names";
        private const string CLASS_DISPLAY_NAMES = "DisplayNames";
        private const string CLASS_FIXED_NAMES = "FixedNames";
        private const string CLASS_FIXED_DISPLAY_NAMES = "FixedDisplayNames";

        private static readonly string[] s_primitiveTypes = new string[] {
            "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong"
        };

        public string GeneratedCode { get; set; } = GENERATED_CODE;

        public string ExcludeCoverage { get; set; } = EXCLUDE_COVERAGE;

        public string AggressiveInlining { get; set; } = AGGRESSIVE_INLINING;

        public string WriteCode()
        {
            var p = Printer.DefaultLarge;

            WriteCode(ref p);

            return p.Result;
        }

        public void WriteCode(ref Printer p)
        {
            if (OnlyClass == false)
            {
                WriteInterface(ref p);
                WriteExtendedStruct(ref p);

                if (HasFlags)
                {
                    WriteExtendedStruct_BitFlagEnumerator(ref p);
                }
            }

            WriteClass(ref p);
        }

        private void WriteInterface(ref Printer p)
        {
            WriteAttribute(ref p);

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine(Accessibility.GetKeyword()).Print(" partial interface I").PrintEndLine(ExtensionsName);
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(": ETEE.IEnumExtensions<")
                    .Print(StructName).Print(", ").Print(FullyQualifiedName).Print(", ").Print(UnderlyingTypeName)
                    .PrintEndLine(">");

                if (HasFlags)
                {
                    p.PrintBeginLine(", ETEE.IEnumBitField<").Print(FullyQualifiedName).PrintEndLine(">");
                }

                if (ReferenceUnityCollections)
                {
                    p.PrintLine(", ETCon.IToFixedString");
                    p.PrintLine(", ETCon.IToDisplayFixedString");
                    p.PrintBeginLine(", ETCon.IToFixedString<").Print(PrintFixedStringTypeName).PrintEndLine(">");
                    p.PrintBeginLine(", ETCon.IToDisplayFixedString<").Print(PrintFixedStringTypeName).PrintEndLine(">");
                }
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteExtendedStruct(ref Printer p)
        {
            WriteAttribute(ref p);

            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintLine("[SRIS.StructLayout(SRIS.LayoutKind.Explicit)]");
            p.PrintBeginLine(Accessibility.GetKeyword()).Print(" readonly partial struct ").Print(StructName)
                .Print(" : I").PrintEndLine(ExtensionsName);
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(", S.IEquatable<").Print(StructName).PrintEndLine(">");
                p.PrintBeginLine(", S.IComparable<").Print(StructName).PrintEndLine(">");
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
                p.PrintLine("[SRIS.FieldOffset(0)]");
                p.PrintBeginLine("private readonly ").Print(FullyQualifiedName).PrintEndLine(" _value;");
                p.PrintEndLine();

                p.PrintLine("[SRIS.FieldOffset(0)]");
                p.PrintBeginLine("private readonly ").Print(UnderlyingTypeName).PrintEndLine(" _underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public ").Print(StructName).Print("(")
                    .Print(FullyQualifiedName).PrintEndLine(" value) : this()");
                p.OpenScope();
                {
                    p.PrintLine("_value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("public ").Print(FullyQualifiedName).PrintEndLine(" Value");
                p.OpenScope();
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("get => _value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("public ").Print(UnderlyingTypeName).PrintEndLine(" UnderlyingValue");
                p.OpenScope();
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("get => _underlyingValue;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public int Length");
                p.OpenScope();
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("get => ").Print(ExtensionsName).PrintEndLine(".Length;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public bool IsDefined");
                p.OpenScope();
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("get => ").Print(ExtensionsName).PrintEndLine(".IsDefined(_value);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public ").Print(StructName).Print(" Create(")
                    .Print(FullyQualifiedName).Print(" value) => new ").Print(StructName).PrintEndLine("(value);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public ").Print(StructName)
                    .Print(" CreateFromUnderlyingValue(").Print(UnderlyingTypeName).Print(" value) => new ")
                    .Print(StructName).Print("((").Print(FullyQualifiedName).PrintEndLine(")value);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public string ToStringFast() => ToStringFast(true);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public string ToStringFast(bool emptyIfUndefined) => ")
                    .Print(ExtensionsName).PrintEndLine(".ToStringFast(_value, emptyIfUndefined);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public string ToDisplayString() => ToDisplayString(true);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public string ToDisplayString(bool emptyIfUndefined) => ")
                    .Print(ExtensionsName).PrintEndLine(".ToDisplayStringFast(_value, emptyIfUndefined);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public string ToDisplayStringFast() => ToDisplayStringFast(true);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public string ToDisplayStringFast(bool emptyIfUndefined) => ")
                    .Print(ExtensionsName).PrintEndLine(".ToDisplayStringFast(_value, emptyIfUndefined);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(string name, out ").Print(StructName)
                    .Print(" value) => ")
                    .PrintEndLine("TryParse(name, out value, false, false);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(string name, out ").Print(StructName)
                    .Print(" value, bool ignoreCase) => ")
                    .PrintEndLine("TryParse(name, out value, ignoreCase, false);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(string name, out ").Print(StructName)
                    .PrintEndLine(" value, bool ignoreCase, bool allowMatchingMetadataAttribute)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var result = ").Print(ExtensionsName)
                        .PrintEndLine(".TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);");
                    p.PrintBeginLine("value = new ").Print(StructName).PrintEndLine("(enumValue);");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> name, out ").Print(StructName)
                    .Print(" value) => ")
                    .PrintEndLine("TryParse(name, out value, false, false);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> name, out ").Print(StructName)
                    .Print(" value, bool ignoreCase) => ")
                    .PrintEndLine("TryParse(name, out value, ignoreCase, false);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> name, out ").Print(StructName)
                    .PrintEndLine(" value, bool ignoreCase, bool allowMatchingMetadataAttribute)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var result = ").Print(ExtensionsName)
                        .PrintEndLine(".TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);");
                    p.PrintBeginLine("value = new ").Print(StructName).PrintEndLine("(enumValue);");
                    p.PrintLine("return result;");
                }
                p.CloseScope();
                p.PrintEndLine();

                if (ReferenceUnityCollections)
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(PrintFixedStringTypeName).Print(" ToFixedString() => ")
                        .PrintEndLine("ToFixedString(true);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(PrintFixedStringTypeName)
                        .Print(" ToFixedString(bool emptyIfUndefined) => ")
                        .Print(ExtensionsName).PrintEndLine(".ToFixedString(_value, emptyIfUndefined);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(PrintFixedStringTypeName).Print(" ToDisplayFixedString() => ")
                        .PrintEndLine("ToDisplayFixedString(true);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(PrintFixedStringTypeName)
                        .Print(" ToDisplayFixedString(bool emptyIfUndefined) => ")
                        .Print(ExtensionsName).PrintEndLine(".ToDisplayFixedString(_value, emptyIfUndefined);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("public TFixedString ToFixedString<TFixedString>()");
                    p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                        .PrintEndLine("UC.IUTF8Bytes");
                    p.WithIncreasedIndent().PrintBeginLine("=> ETCol.EncosyFixedStringExtensions")
                        .PrintEndLine(".CastTo<TFixedString>(ToFixedString());");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("public TFixedString ToDisplayFixedString<TFixedString>()");
                    p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                        .PrintEndLine("UC.IUTF8Bytes");
                    p.WithIncreasedIndent().PrintBeginLine("=> ETCol.EncosyFixedStringExtensions")
                        .PrintEndLine(".CastTo<TFixedString>(ToDisplayFixedString());");
                    p.PrintEndLine();
                }

                if (ReferenceUnityCollections)
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("public bool TryFormat(");
                    p = p.IncreasedIndent();
                    {
                        p.PrintLine("  S.Span<char> destination");
                        p.PrintLine(", out int charsWritten");
                    }
                    p = p.DecreasedIndent();
                    p.PrintBeginLine(") => ").Print(ExtensionsName)
                        .PrintEndLine(".TryFormat(_value, destination, out charsWritten);");
                    p.PrintEndLine();
                }

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public bool TryFormat(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  S.Span<char> destination");
                    p.PrintLine(", out int charsWritten");
                    p.PrintLine(", S.ReadOnlySpan<char> format");
                    p.PrintLine(", S.IFormatProvider provider = null");
                }
                p = p.DecreasedIndent();
                p.PrintBeginLine(") => ").Print(ExtensionsName)
                    .PrintEndLine(".TryFormat(_value, destination, out charsWritten, format, provider);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool IsNameDefined(string name) => ")
                    .Print(ExtensionsName).Print(".IsNameDefined(name, default(")
                    .Print(FullyQualifiedName).PrintEndLine("));");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => ")
                    .Print(ExtensionsName).Print(".IsNameDefined(name, default(")
                    .Print(FullyQualifiedName).PrintEndLine("), allowMatchingMetadataAttribute);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public int ToIndex() => ")
                    .Print(ExtensionsName).PrintEndLine(".FindIndex(_value);");
                p.PrintEndLine();

                if (HasFlags)
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public bool Contains(").Print(FullyQualifiedName).Print(" flags) => ")
                        .Print(ExtensionsName).PrintEndLine(".Contains(_value, flags);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public bool Any(").Print(FullyQualifiedName).Print(" flags) => ")
                        .Print(ExtensionsName).PrintEndLine(".Any(_value, flags);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(FullyQualifiedName).Print(" Unset(").Print(FullyQualifiedName).Print(" flags) => ")
                        .Print(ExtensionsName).PrintEndLine(".Unset(_value, flags);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public ").Print(FullyQualifiedName).Print(" Set(").Print(FullyQualifiedName).Print(" flags) => ")
                        .Print(ExtensionsName).PrintEndLine(".Set(_value, flags);");
                    p.PrintEndLine();
                }

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public override string ToString() => ToStringFast(true);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public string ToString(string format, S.IFormatProvider formatProvider) => ")
                    .PrintEndLine("ToStringFast(true);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public override int GetHashCode() => _underlyingValue.GetHashCode();");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public int CompareTo(").Print(StructName)
                    .PrintEndLine(" other) => this._underlyingValue.CompareTo(other._underlyingValue);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public bool Equals(").Print(StructName)
                    .PrintEndLine(" other) => this._underlyingValue == other._underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public override bool Equals(object obj) => obj is ").Print(StructName)
                    .PrintEndLine(" other && this._underlyingValue == other._underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static implicit operator ")
                    .Print(StructName).Print("(").Print(FullyQualifiedName)
                    .Print(" value) => new ").Print(StructName).PrintEndLine("(value);");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static bool operator ==(")
                    .Print(StructName).Print(" left, ")
                    .Print(StructName).PrintEndLine(" right) => left._underlyingValue == right._underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static bool operator !=(")
                    .Print(StructName).Print(" left, ")
                    .Print(StructName).PrintEndLine(" right) => left._underlyingValue != right._underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static bool operator <(")
                    .Print(StructName).Print(" left, ")
                    .Print(StructName).PrintEndLine(" right) => left._underlyingValue < right._underlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static bool operator >(")
                    .Print(StructName).Print(" left, ")
                    .Print(StructName).PrintEndLine(" right) => left._underlyingValue > right._underlyingValue;");
                p.PrintEndLine();

                if (HasFlags)
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator ~(")
                        .Print(StructName).Print(" value) => new ")
                        .Print(StructName).PrintEndLine("(~value._value);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator <<(")
                        .Print(StructName).Print(" value, int bits) => new ")
                        .Print(StructName).Print("((").Print(FullyQualifiedName)
                        .PrintEndLine(")(value._underlyingValue << bits));");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator >>(")
                        .Print(StructName).Print(" value, int bits) => new ")
                        .Print(StructName).Print("((").Print(FullyQualifiedName)
                        .PrintEndLine(")(value._underlyingValue >> bits));");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator &(")
                        .Print(StructName).Print(" left, ")
                        .Print(StructName).Print(" right) => new ")
                        .Print(StructName).PrintEndLine("(left._value & right._value);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator |(")
                        .Print(StructName).Print(" left, ")
                        .Print(StructName).Print(" right) => new ")
                        .Print(StructName).PrintEndLine("(left._value | right._value);");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public static ").Print(StructName).Print(" operator ^(")
                        .Print(StructName).Print(" left, ")
                        .Print(StructName).Print(" right) => new ")
                        .Print(StructName).PrintEndLine("(left._value ^ right._value);");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteExtendedStruct_BitFlagEnumerator(ref Printer p)
        {
            p.PrintBeginLine("partial struct ").Print(StructName)
                .Print(" : SCG.IEnumerable<")
                .Print(FullyQualifiedName).PrintEndLine(">");
            p.OpenScope();
            {
                p.PrintLine(AggressiveInlining);
                p.PrintLine("public readonly Enumerator GetEnumerator()");
                p.OpenScope();
                {
                    p.PrintLine("return new Enumerator(this);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("readonly SCG.IEnumerator<")
                    .Print(FullyQualifiedName).Print("> SCG.IEnumerable<")
                    .Print(FullyQualifiedName).PrintEndLine(">.GetEnumerator()");
                p.OpenScope();
                {
                    p.PrintLine("return GetEnumerator();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("readonly SC.IEnumerator ")
                    .PrintEndLine("SC.IEnumerable.GetEnumerator()");
                p.OpenScope();
                {
                    p.PrintLine("return GetEnumerator();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("public partial struct Enumerator")
                    .Print(" : SCG.IEnumerator<")
                    .Print(FullyQualifiedName).PrintEndLine(">");
                p.OpenScope();
                {
                    p.PrintBeginLine("private readonly ").Print(StructName).PrintEndLine(" _value;");
                    p.PrintLine("private int _index;");
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintBeginLine("public Enumerator(").Print(StructName).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintLine("_value = value;");
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("public readonly ").Print(FullyQualifiedName).PrintEndLine(" Current");
                    p.OpenScope();
                    {
                        p.PrintLine(AggressiveInlining);
                        p.PrintBeginLine("get => ").Print(ExtensionsName).PrintEndLine(".Values.AsSpan()[_index];");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("readonly object SC.IEnumerator.Current");
                    p.OpenScope();
                    {
                        p.PrintLine(AggressiveInlining);
                        p.PrintLine("get => this.Current;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("public bool MoveNext()");
                    p.OpenScope();
                    {
                        p.PrintLine("var value = _value;");
                        p.PrintLine("var index = _index + 1;");
                        p.PrintBeginLine("var length = (uint)").Print(ExtensionsName).PrintEndLine(".Length;");
                        p.PrintBeginLine("var flags = ").Print(ExtensionsName).PrintEndLine(".Values.AsSpan();");
                        p.PrintEndLine();

                        p.PrintLine("while ((uint)index < length)");
                        p.OpenScope();
                        {
                            p.PrintLine("var flag = flags[index];");
                            p.PrintEndLine();

                            p.PrintLine("if (flag != 0 && value.Contains(flag))");
                            p.OpenScope();
                            {
                                p.PrintLine("_index = index;");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintLine("index++;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("public void Reset()");
                    p.OpenScope();
                    {
                        p.PrintLine("_index = -1;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine("public readonly void Dispose() { }");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClass(ref Printer p)
        {
            WriteAttribute(ref p);

            var @this = ParentIsNamespace ? "this " : "";

            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine(Accessibility.GetKeyword()).Print(" static partial class ").PrintEndLine(ExtensionsName);
            p.OpenScope();
            {
                if (NoDocumentation == false)
                {
                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// The number of members in the enum.");
                    p.PrintLine("/// This is a non-distinct count of defined names.");
                    p.PrintLine("/// </summary>");
                }

                p.PrintLine($"public const int Length = {Members.Count};");
                p.PrintEndLine();

                WriteToString(ref p, @this);

                if (OnlyNames == false)
                {
                    p.PrintLine(AggressiveInlining);
                    p.PrintLine($"public static {StructName} AsExtended({@this}{FullyQualifiedName} value)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> new {StructName}(value);");
                    p = p.DecreasedIndent();
                    p.PrintEndLine();

                    foreach (var type in s_primitiveTypes)
                    {
                        p.PrintLine(AggressiveInlining);
                        p.PrintLine($"public static {StructName} As{StructName}({@this}{type} value)");
                        p = p.IncreasedIndent();
                        p.PrintLine($"=> new {StructName}(({FullyQualifiedName})({UnderlyingTypeName})value);");
                        p = p.DecreasedIndent();
                        p.PrintEndLine();
                    }
                }

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static {UnderlyingTypeName} ToUnderlyingValue({@this}{FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> ({UnderlyingTypeName})value;");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                if (WithoutTryParse == false)
                {
                    WriteTryParse(ref p, @this);
                    WriteTryParseSpan(ref p, @this);
                }

                if (WithoutIsDefined == false)
                {
                    WriteIsDefined(ref p, @this);
                }

                if (OnlyNames == false)
                {
                    WriteTryFormat(ref p, @this);
                    WriteFindIndex(ref p, @this);
                    WriteFlags(ref p, @this);
                    WriteClassValues(ref p);
                    WriteClassUnderlyingValues(ref p);
                }

                WriteClassNames(ref p);
                WriteClassDisplayNames(ref p);
                WriteClassFixedNames(ref p);
                WriteClassFixedDisplayNames(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteAttribute(ref Printer p)
        {
            if (OnlyNames)
            {
                return;
            }

            p.PrintBeginLine("[ETEESG.GeneratedEnumExtensionsFor(typeof(")
                .Print(FullyQualifiedName).Print("), typeof(I")
                .Print(ExtensionsName).Print("), typeof(")
                .Print(ExtensionsName).Print("), typeof(")
                .Print(StructName).Print(")")
                .PrintEndLine(")]");
        }

        private void WriteClassFixedDisplayNames(ref Printer p)
        {
            if (ReferenceUnityCollections == false)
            {
                return;
            }

            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_FIXED_DISPLAY_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine($"public static {PrintFixedStringTypeName} {member.name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AggressiveInlining);
                        p.PrintLine($"get => ({PrintFixedStringTypeName}){CLASS_DISPLAY_NAMES}.{member.name};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static UC.NativeArray<").Print(PrintFixedStringTypeName)
                    .Print("> ToNativeArray(").Print(ALLOCATOR_MANAGER).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintBeginLine("=> ToNativeArray<").Print(PrintFixedStringTypeName)
                        .PrintEndLine(">(allocator);");
                p.PrintEndLine();

                p.PrintBeginLine("public static UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(")
                    .Print(ALLOCATOR_MANAGER).PrintEndLine("  allocator)");
                p.WithIncreasedIndent().PrintLine("where TFixedString : unmanaged, UC.INativeList<byte>, UC.IUTF8Bytes");
                p.OpenScope();
                {
                    p.PrintBeginLine("var names = UC.CollectionHelper.CreateNativeArray<TFixedString>(")
                        .Print(ExtensionsName).Print(".Length, allocator, ")
                        .PrintEndLine("UC.NativeArrayOptions.UninitializedMemory);");

                    var index = 0;

                    foreach (var member in Members)
                    {
                        p.PrintBeginLine("names[").Print(index).Print("] = ")
                            .Print("ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(")
                            .Print(member.name)
                            .PrintEndLine(");");

                        index++;

                        if (index >= Members.Count)
                        {
                            break;
                        }
                    }

                    p.PrintLine("return names;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static {PrintFixedStringTypeName} Get({FullyQualifiedName} value)");
                p.WithIncreasedIndent().PrintLine("=> Get(value, true);");
                p.PrintEndLine();

                p.PrintBeginLine($"public static {PrintFixedStringTypeName} Get({FullyQualifiedName} value")
                    .PrintEndLine(", bool emptyIfUndefined)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine($"_ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassFixedNames(ref Printer p)
        {
            if (ReferenceUnityCollections == false)
            {
                return;
            }

            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_FIXED_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine($"public static {PrintFixedStringTypeName} {member.name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AggressiveInlining);
                        p.PrintLine($"get => ({PrintFixedStringTypeName}){CLASS_NAMES}.{member.name};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(AggressiveInlining);
                p.PrintBeginLine("public static UC.NativeArray<").Print(PrintFixedStringTypeName)
                    .Print("> ToNativeArray(").Print(ALLOCATOR_MANAGER).PrintEndLine(" allocator)");
                p.WithIncreasedIndent().PrintBeginLine("=> ToNativeArray<").Print(PrintFixedStringTypeName)
                        .PrintEndLine(">(allocator);");
                p.PrintEndLine();

                p.PrintBeginLine("public static UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(")
                    .Print(ALLOCATOR_MANAGER).PrintEndLine("  allocator)");
                p.WithIncreasedIndent().PrintLine("where TFixedString : unmanaged, UC.INativeList<byte>, UC.IUTF8Bytes");
                p.OpenScope();
                {
                    p.PrintBeginLine("var names = UC.CollectionHelper.CreateNativeArray<TFixedString>(")
                        .Print(ExtensionsName).Print(".Length, allocator, ")
                        .PrintEndLine("UC.NativeArrayOptions.UninitializedMemory);");

                    var index = 0;

                    foreach (var member in Members)
                    {
                        p.PrintBeginLine("names[").Print(index).Print("] = ")
                            .Print("ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(")
                            .Print(member.name)
                            .PrintEndLine(");");

                        index++;

                        if (index >= Members.Count)
                        {
                            break;
                        }
                    }

                    p.PrintLine("return names;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static {PrintFixedStringTypeName} Get({FullyQualifiedName} value)");
                p.WithIncreasedIndent().PrintLine("=> Get(value, true);");
                p.PrintEndLine();

                p.PrintBeginLine($"public static {PrintFixedStringTypeName} Get({FullyQualifiedName} value")
                    .PrintEndLine(", bool emptyIfUndefined)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine($"_ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassDisplayNames(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_DISPLAY_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    if (string.IsNullOrEmpty(member.displayName) == false)
                    {
                        p.PrintLine($"public const string {member.name} = \"{member.displayName}\";");
                    }
                    else
                    {
                        p.PrintLine($"public const string {member.name} = {CLASS_NAMES}.{member.name};");
                    }

                    p.PrintEndLine();
                }

                p.PrintLine($"private static readonly string[] s_names = new string[]");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{member.name},");
                    }
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public static S.ReadOnlyMemory<string> AsMemory() => s_names;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public static S.ReadOnlySpan<string> AsSpan() => s_names;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static string Get({FullyQualifiedName} value) => Get(value, true);");
                p.PrintEndLine();

                p.PrintLine($"public static string Get({FullyQualifiedName} value, bool emptyIfUndefined)");
                p = p.IncreasedIndent();
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine("_ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassNames(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine($"public const string {member.name} = nameof({FullyQualifiedName}.{member.name});");
                    p.PrintEndLine();
                }

                p.PrintLine($"private static readonly string[] s_names = new string[]");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{member.name},");
                    }
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public static S.ReadOnlyMemory<string> AsMemory() => s_names;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine("public static S.ReadOnlySpan<string> AsSpan() => s_names;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static string Get({FullyQualifiedName} value) => Get(value, true);");
                p.PrintEndLine();

                p.PrintLine($"public static string Get({FullyQualifiedName} value, bool emptyIfUndefined)");
                p = p.IncreasedIndent();
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine("_ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassUnderlyingValues(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_UNDERLYING_VALUES);
            p.OpenScope();
            {
                p.PrintLine($"private static readonly {UnderlyingTypeName}[] s_values = new {UnderlyingTypeName}[]");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"ToUnderlyingValue({FullyQualifiedName}.{member.name}),");
                    }
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static S.ReadOnlyMemory<{UnderlyingTypeName}> AsMemory() => s_values;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static S.ReadOnlySpan<{UnderlyingTypeName}> AsSpan() => s_values;");

                if (ReferenceUnityCollections)
                {
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine($"public static UC.NativeArray<{UnderlyingTypeName}> ToNativeArray({ALLOCATOR_MANAGER} allocator)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> UC.CollectionHelper.CreateNativeArray<{UnderlyingTypeName}>(s_values, allocator);");
                    p = p.DecreasedIndent();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassValues(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(ExcludeCoverage);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_VALUES);
            p.OpenScope();
            {
                p.PrintLine($"private static readonly {FullyQualifiedName}[] s_values = new {FullyQualifiedName}[]");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name},");
                    }
                }
                p.CloseScope("};");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static S.ReadOnlyMemory<{FullyQualifiedName}> AsMemory() => s_values;");
                p.PrintEndLine();

                p.PrintLine(AggressiveInlining);
                p.PrintLine($"public static S.ReadOnlySpan<{FullyQualifiedName}> AsSpan() => s_values;");

                if (ReferenceUnityCollections)
                {
                    p.PrintEndLine();

                    p.PrintLine(AggressiveInlining);
                    p.PrintLine($"public static UC.NativeArray<{FullyQualifiedName}> ToNativeArray({ALLOCATOR_MANAGER} allocator)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> UC.CollectionHelper.CreateNativeArray<{FullyQualifiedName}>(s_values, allocator);");
                    p = p.DecreasedIndent();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse(ref Printer p, string @this)
        {
            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool TryParse({@this}string name, out {FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintLine("=> TryParse(name, out value, false, false);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <param name=\"ignoreCase\"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool TryParse({@this}string name, out {FullyQualifiedName} value, bool ignoreCase)");
            p = p.IncreasedIndent();
            p.PrintLine("=> TryParse(name, out value, ignoreCase, false);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <param name=\"ignoreCase\"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>");
                p.PrintLine("/// <param name=\"allowMatchingMetadataAttribute\">If <c>true</c>, considers the value included in metadata attributes such as");
                p.PrintLine("/// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine($"public static bool TryParse({@this}string name, out {FullyQualifiedName} value, bool ignoreCase, bool allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
                p.PrintLine("if (string.IsNullOrWhiteSpace(name))");
                p.OpenScope();
                {
                    p.PrintLine("value = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("var stringComparison = ignoreCase ? ")
                    .Print("S.StringComparison.OrdinalIgnoreCase : ")
                    .PrintEndLine("S.StringComparison.Ordinal;");
                p.PrintEndLine();

                if (IsDisplayAttributeUsed)
                {
                    p.PrintLine("if (allowMatchingMetadataAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (name)");
                        p.OpenScope();
                        {
                            foreach (var member in Members)
                            {
                                if (string.IsNullOrEmpty(member.displayName) == false)
                                {
                                    p.PrintLine($"case string s when s.Equals({CLASS_DISPLAY_NAMES}.{member.name}, stringComparison):");
                                    p.OpenScope();
                                    {
                                        p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }
                            }

                            p.PrintLine("default: break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }

                p.PrintLine("switch (name)");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"case string s when s.Equals({CLASS_NAMES}.{member.name}, stringComparison):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine($"case string s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                    p.OpenScope();
                    {
                        p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintLine("value = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParseSpan(ref Printer p, string @this)
        {
            const string SPAN = "S.ReadOnlySpan<char>";

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool TryParse({@this}{SPAN} name, out {FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintLine("=> TryParse(name, out value, false, false);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <param name=\"ignoreCase\"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool TryParse({@this}{SPAN} name, out {FullyQualifiedName} value, bool ignoreCase)");
            p = p.IncreasedIndent();
            p.PrintLine("=> TryParse(name, out value, ignoreCase, false);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Converts the string representation of the name or numeric value of");
                p.PrintLine($"/// an <see cref=\"{FullyQualifiedName}\" /> to the equivalent instance.");
                p.PrintLine("/// The return value indicates whether the conversion succeeded.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The case-sensitive string representation of the enumeration name or underlying value to convert</param>");
                p.PrintLine("/// <param name=\"value\">When this method returns, contains an object of type ");
                p.PrintLine($"/// <see cref=\"{FullyQualifiedName}\" /> whose");
                p.PrintLine("/// value is represented by <paramref name=\"value\"/> if the parse operation succeeds.");
                p.PrintLine("/// If the parse operation fails, contains the default value of the underlying type");
                p.PrintLine($"/// of <see cref=\"{FullyQualifiedName}\" />. This parameter is passed uninitialized.</param>");
                p.PrintLine("/// <param name=\"ignoreCase\"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>");
                p.PrintLine("/// <param name=\"allowMatchingMetadataAttribute\">If <c>true</c>, considers the value included in metadata attributes such as");
                p.PrintLine("/// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>");
                p.PrintLine("/// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine($"public static bool TryParse({@this}{SPAN} name, out {FullyQualifiedName} value, bool ignoreCase, bool allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
                p.PrintLine("if (name.IsEmpty)");
                p.OpenScope();
                {
                    p.PrintLine("value = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("var stringComparison = ignoreCase ? ")
                    .Print("S.StringComparison.OrdinalIgnoreCase : ")
                    .PrintEndLine("S.StringComparison.Ordinal;");
                p.PrintEndLine();

                if (IsDisplayAttributeUsed)
                {
                    p.PrintLine("if (allowMatchingMetadataAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("switch (name)");
                        p.OpenScope();
                        {
                            foreach (var member in Members)
                            {
                                if (string.IsNullOrEmpty(member.displayName) == false)
                                {
                                    p.PrintLine($"case {SPAN} s when S.MemoryExtensions.Equals(s, S.MemoryExtensions.AsSpan({CLASS_DISPLAY_NAMES}.{member.name}), stringComparison):");
                                    p.OpenScope();
                                    {
                                        p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }
                            }

                            p.PrintLine("default: break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }

                p.PrintLine("switch (name)");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"case {SPAN} s when S.MemoryExtensions.Equals(s, S.MemoryExtensions.AsSpan({CLASS_NAMES}.{member.name}), stringComparison):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine($"case {SPAN} s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                    p.OpenScope();
                    {
                        p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintLine("value = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteFlags(ref Printer p, string @this)
        {
            if (HasFlags == false)
            {
                return;
            }

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Determines whether some of the bit fields are set in the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns><c>true</c> if all of the bit fields that are set in <c>flags</c> are also set in the current instance; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool Contains({@this}{FullyQualifiedName} value, {FullyQualifiedName} flags)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> (value & flags) == flags;");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Determines whether any of the bit fields are set in the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns><c>true</c> if any of the bit fields that are set in <c>flags</c> is also set in the current instance; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool Any({@this}{FullyQualifiedName} value, {FullyQualifiedName} flags)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> (value & flags) != 0;");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Unsets one or more bit fields on the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns>A new instance without bit fields that are set in <c>flags</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {FullyQualifiedName} Unset({@this}{FullyQualifiedName} value, {FullyQualifiedName} flags)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> value & (~flags);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Sets one or more bit fields on the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns>A new instance with bit fields that are set in <c>flags</c>.</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {FullyQualifiedName} Set({@this}{FullyQualifiedName} value, {FullyQualifiedName} flags)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> value | flags;");
            p = p.DecreasedIndent();
            p.PrintEndLine();
        }

        private void WriteIsDefined(ref Printer p, string @this)
        {
            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Returns a boolean telling whether the given enum value exists in the enumeration.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to check if it's defined</param>");
                p.PrintLine("/// <returns><c>true</c> if the value exists in the enumeration, <c>false</c> otherwise</returns>");
            }

            p.PrintLine($"public static bool IsDefined({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            {
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => true,");
                    }

                    p.PrintLine("_ => false,");
                }
                p.CloseScope("};");
            }
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Returns a boolean telling whether an enum with the given name exists in the enumeration.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The name to check if it's defined</param>");
                p.PrintLine("/// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static bool IsNameDefined({@this}string name, {FullyQualifiedName} _)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> IsNameDefined(name, default({FullyQualifiedName}), allowMatchingMetadataAttribute: false);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Returns a boolean telling whether an enum with the given name exists in the enumeration,");
                p.PrintLine("/// or if a member decorated with a <c>[Display]</c> attribute");
                p.PrintLine("/// with the required name exists.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"name\">The name to check if it's defined</param>");
                p.PrintLine("/// <param name=\"allowMatchingMetadataAttribute\">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>");
                p.PrintLine("/// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated");
                p.PrintLine("/// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>");
            }

            p.PrintLine($"public static bool IsNameDefined({@this}string name, {FullyQualifiedName} _, bool allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
                if (IsDisplayAttributeUsed)
                {
                    p.PrintLine("var isDefinedInDisplayAttribute = false;");
                    p.PrintEndLine();

                    p.PrintLine("if (allowMatchingMetadataAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("isDefinedInDisplayAttribute = name switch");
                        p.OpenScope();
                        {
                            foreach (var member in Members)
                            {
                                if (string.IsNullOrEmpty(member.displayName) == false)
                                {
                                    p.PrintLine($"{CLASS_DISPLAY_NAMES}.{member.name} => true,");
                                }
                            }

                            p.PrintLine("_ => false,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (isDefinedInDisplayAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return name switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{CLASS_NAMES}.{member.name} => true,");
                    }

                    p.PrintLine("_ => false,");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteFindIndex(ref Printer p, string @this)
        {
            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Finds the index for a given enum value in the enumeration.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to find the index for</param>");
                p.PrintLine("/// <returns>The zero-based index if the enum value exists in the enumeration, otherwise -1.</returns>");
            }

            p.PrintLine($"public static int FindIndex({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            {
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    var members = Members;
                    var count = members.Count;

                    for (var i = 0; i < count; i++)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{members[i].name} => {i},");
                    }

                    p.PrintLine("_ => -1,");
                }
                p.CloseScope("};");
            }
            p = p.DecreasedIndent();
            p.PrintEndLine();
        }

        private void WriteTryFormat(ref Printer p, string @this)
        {
            if (ReferenceUnityCollections)
            {
                p.PrintLine("public static bool TryFormat(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine($"  {@this}{FullyQualifiedName} value");
                    p.PrintLine(", S.Span<char> destination");
                    p.PrintLine(", out int charsWritten");
                }
                p = p.DecreasedIndent();
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("if (IsDefined(value))");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value)")
                            .PrintEndLine(", destination, out charsWritten);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("return ETCol.EncosyFixedStringExtensions.TryFormat(")
                        .Print("ETCol.EncosyFixedStringExtensions.ToFixedString(ToUnderlyingValue(value))")
                        .PrintEndLine(", destination, out charsWritten);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            p.PrintLine("public static bool TryFormat(");
            p = p.IncreasedIndent();
            {
                p.PrintLine($"  {@this}{FullyQualifiedName} value");
                p.PrintLine(", S.Span<char> destination");
                p.PrintLine(", out int charsWritten");
                p.PrintLine(", S.ReadOnlySpan<char> format");
                p.PrintLine(", S.IFormatProvider provider = null");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("if (IsDefined(value))");
                p.OpenScope();
                {
                    if (ReferenceUnityCollections)
                    {
                        p.PrintBeginLine("return ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value)")
                            .PrintEndLine(", destination, out charsWritten);");
                    }
                    else
                    {
                        p.PrintLine("var span = S.MemoryExtensions.AsSpan(ToStringFast(value));");
                        p.PrintEndLine();

                        p.PrintLine("if (span.Length == 0 || span.TryCopyTo(destination) == false)");
                        p.OpenScope();
                        {
                            p.PrintLine("charsWritten = 0;");
                            p.PrintLine(" return false;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("charsWritten = span.Length;");
                        p.PrintLine(" return true;");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (ToUnderlyingValue(value).TryFormat(destination, out var chars, format, provider))");
                p.OpenScope();
                {
                    p.PrintLine("charsWritten = chars;");
                    p.PrintLine(" return true;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("charsWritten = 0;");
                p.PrintLine(" return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToString(ref Printer p, string @this)
        {
            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintLine("/// <returns>The string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static string ToStringFast({@this}{FullyQualifiedName} value)");
            p.WithIncreasedIndent().PrintLine("=> ToStringFast(value, true);");
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintBeginLine("/// <param name=\"emptyIfUndefined\">If <c>true</c>, returns an empty string for ")
                    .PrintEndLine("undefined values; otherwise, returns the string representation of the value</param>");
                p.PrintLine("/// <returns>The string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static string ToStringFast({@this}{FullyQualifiedName} value, bool emptyIfUndefined)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_NAMES).PrintEndLine(".Get(value, emptyIfUndefined);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// If the attribute is decorated with a <c>[Display]</c> attribute, then");
                p.PrintLine("/// uses the provided value. Otherwise uses the name of the member, equivalent to");
                p.PrintLine("/// calling <c>ToString()</c> on <paramref name=\"value\"/>.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintLine("/// <returns>The string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static string ToDisplayStringFast({@this}{FullyQualifiedName} value)");
            p.WithIncreasedIndent().PrintLine("=> ToDisplayStringFast(value, true);");
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// If the attribute is decorated with a <c>[Display]</c> attribute, then");
                p.PrintLine("/// uses the provided value. Otherwise uses the name of the member, equivalent to");
                p.PrintLine("/// calling <c>ToString()</c> on <paramref name=\"value\"/>.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintBeginLine("/// <param name=\"emptyIfUndefined\">If <c>true</c>, returns an empty string for ")
                    .PrintEndLine("undefined values; otherwise, returns the string representation of the value</param>");
                p.PrintLine("/// <returns>The string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static string ToDisplayStringFast({@this}{FullyQualifiedName} value, bool emptyIfUndefined)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_DISPLAY_NAMES).PrintEndLine(".Get(value, emptyIfUndefined);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (ReferenceUnityCollections == false)
            {
                return;
            }

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the fixed string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintLine("/// <returns>The fixed string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {PrintFixedStringTypeName} ToFixedString({@this}{FullyQualifiedName} value)");
            p.WithIncreasedIndent().PrintLine("=> ToFixedString(value, true);");
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the fixed string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintBeginLine("/// <param name=\"emptyIfUndefined\">If <c>true</c>, returns an empty string for ")
                    .PrintEndLine("undefined values; otherwise, returns the string representation of the value</param>");
                p.PrintLine("/// <returns>The fixed string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {PrintFixedStringTypeName} ToFixedString({@this}{FullyQualifiedName} value, bool emptyIfUndefined)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_FIXED_NAMES).PrintEndLine(".Get(value, emptyIfUndefined);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the fixed string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// If the attribute is decorated with a <c>[Display]</c> attribute, then");
                p.PrintLine("/// uses the provided value. Otherwise uses the name of the member, equivalent to");
                p.PrintLine("/// calling <c>ToString()</c> on <paramref name=\"value\"/>.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintLine("/// <returns>The fixed string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {PrintFixedStringTypeName} ToDisplayFixedString({@this}{FullyQualifiedName} value)");
            p.WithIncreasedIndent().PrintLine("=> ToDisplayFixedString(value, true);");
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine($"/// Returns the fixed string representation of the <see cref=\"{FullyQualifiedName}\"/> value.");
                p.PrintLine("/// If the attribute is decorated with a <c>[Display]</c> attribute, then");
                p.PrintLine("/// uses the provided value. Otherwise uses the name of the member, equivalent to");
                p.PrintLine("/// calling <c>ToString()</c> on <paramref name=\"value\"/>.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <param name=\"value\">The value to retrieve the string value for</param>");
                p.PrintBeginLine("/// <param name=\"emptyIfUndefined\">If <c>true</c>, returns an empty string for ")
                    .PrintEndLine("undefined values; otherwise, returns the string representation of the value</param>");
                p.PrintLine("/// <returns>The fixed string representation of the value</returns>");
            }

            p.PrintLine(AggressiveInlining);
            p.PrintLine($"public static {PrintFixedStringTypeName} ToDisplayFixedString({@this}{FullyQualifiedName} value, bool emptyIfUndefined)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_FIXED_DISPLAY_NAMES).PrintEndLine(".Get(value, emptyIfUndefined);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            p.PrintLine($"private static {PrintFixedStringTypeName} ToFixedString({UnderlyingTypeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"var fs = new {PrintFixedStringTypeName}();");
                p.PrintLine("UC.FixedStringMethods.Append(ref fs, value);");
                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
