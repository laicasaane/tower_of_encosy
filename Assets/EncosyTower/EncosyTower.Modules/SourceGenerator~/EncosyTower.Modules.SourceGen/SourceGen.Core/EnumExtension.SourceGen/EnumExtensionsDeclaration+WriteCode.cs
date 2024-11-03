using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.EnumExtensions.SourceGen
{
    partial class EnumExtensionsDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.EnumExtensions.SourceGen.EnumExtensionsGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string UNITY_COLLECTIONS_ALLOCATOR = "global::Unity.Collections.AllocatorManager.AllocatorHandle";
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

        public string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p = p.IncreasedIndent();
            {
                p.PrintEndLine();
                WriteCode(ref p);
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        public void WriteCode(ref Printer p)
        {
            if (OnlyClass == false)
            {
                WriteInterface(ref p);
                WriteStruct(ref p);
            }

            WriteClass(ref p);
        }

        private void WriteInterface(ref Printer p)
        {
            WriteAttribute(ref p);

            p.PrintLine(GeneratedCode);
            p.PrintBeginLine(Accessibility.GetKeyword()).Print(" partial interface I").PrintEndLine(ExtensionsName);
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(": global::EncosyTower.Modules.EnumExtensions.IEnumExtensions<")
                    .Print(FullyQualifiedName).Print(", ").Print(UnderlyingTypeName)
                    .PrintEndLine(">");

                if (ReferenceUnityCollections)
                {
                    p.PrintBeginLine(", global::EncosyTower.Modules.IToFixedString<").Print(FixedStringTypeFullyQualifiedName).PrintEndLine(">");
                    p.PrintBeginLine(", global::EncosyTower.Modules.IToDisplayFixedString<").Print(FixedStringTypeFullyQualifiedName).PrintEndLine(">");
                }
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteStruct(ref Printer p)
        {
            WriteAttribute(ref p);

            p.PrintLine(GeneratedCode);
            p.PrintLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");
            p.PrintBeginLine(Accessibility.GetKeyword()).Print(" readonly partial struct ").Print(ExtensionsWrapperName)
                .Print(" : I").PrintEndLine(ExtensionsName);
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(", global::System.IEquatable<").Print(ExtensionsWrapperName).PrintEndLine(">");
                p.PrintBeginLine(", global::System.IComparable<").Print(ExtensionsWrapperName).PrintEndLine(">");
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
                p.PrintLine(GeneratedCode);
                p.PrintLine("[global::System.Runtime.InteropServices.FieldOffset(0)]");
                p.PrintBeginLine("public readonly ").Print(FullyQualifiedName).PrintEndLine(" Value;");
                p.PrintEndLine();

                p.PrintLine(GeneratedCode);
                p.PrintLine("[global::System.Runtime.InteropServices.FieldOffset(0)]");
                p.PrintBeginLine("public readonly ").Print(UnderlyingTypeName).PrintEndLine(" UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(ExtensionsWrapperName).Print("(")
                    .Print(FullyQualifiedName).PrintEndLine(" value) : this()");
                p.OpenScope();
                {
                    p.PrintLine("this.Value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("get => ").Print(ExtensionsName).PrintEndLine(".Length;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public string ToStringFast() => ")
                    .Print(ExtensionsName).PrintEndLine(".ToStringFast(this.Value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public string ToDisplayStringFast() => ")
                    .Print(ExtensionsName).PrintEndLine(".ToDisplayStringFast(this.Value);");
                p.PrintEndLine();

                if (ReferenceUnityCollections)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(FixedStringTypeFullyQualifiedName).Print(" ToFixedString() => ")
                        .Print(ExtensionsName).PrintEndLine(".ToFixedString(this.Value);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public ").Print(FixedStringTypeFullyQualifiedName).Print(" ToDisplayFixedString() => ")
                        .Print(ExtensionsName).PrintEndLine(".ToDisplayFixedString(this.Value);");
                    p.PrintEndLine();
                }

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(UnderlyingTypeName).Print(" ToUnderlyingValue() => ")
                    .Print(ExtensionsName).PrintEndLine(".ToUnderlyingValue(this.Value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public bool TryFormat(");
                p = p.IncreasedIndent();
                {
                    p.PrintLine("  global::System.Span<char> destination");
                    p.PrintLine(", out int charsWritten");
                    p.PrintLine(", global::System.ReadOnlySpan<char> format = default");
                    p.PrintLine(", global::System.IFormatProvider provider = null");
                }
                p = p.DecreasedIndent();
                p.PrintBeginLine(") => ").Print(ExtensionsName)
                    .PrintEndLine(".TryFormat(this.Value, destination, out charsWritten, format, provider);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool IsDefined() => ")
                    .Print(ExtensionsName).PrintEndLine(".IsDefined(this.Value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool IsDefinedIn(string name) => ")
                    .Print(ExtensionsName).Print(".IsDefinedIn(name, default(")
                    .Print(FullyQualifiedName).PrintEndLine("));");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool IsDefinedIn(string name, bool allowMatchingMetadataAttribute) => ")
                    .Print(ExtensionsName).Print(".IsDefinedIn(name, default(")
                    .Print(FullyQualifiedName).PrintEndLine("), allowMatchingMetadataAttribute);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public int FindIndex() => ")
                    .Print(ExtensionsName).PrintEndLine(".FindIndex(this.Value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public override string ToString() => ")
                    .Print(ExtensionsName).PrintEndLine(".ToStringFast(this.Value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public override int GetHashCode() => this.UnderlyingValue.GetHashCode();");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public int CompareTo(").Print(ExtensionsWrapperName)
                    .PrintEndLine(" other) => this.UnderlyingValue.CompareTo(other.UnderlyingValue);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public bool Equals(").Print(ExtensionsWrapperName)
                    .PrintEndLine(" other) => this.UnderlyingValue == other.UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public override bool Equals(object obj) => obj is ").Print(ExtensionsWrapperName)
                    .PrintEndLine(" other && this.UnderlyingValue == other.UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static implicit operator ")
                    .Print(ExtensionsWrapperName).Print("(").Print(FullyQualifiedName)
                    .Print(" value) => new ").Print(ExtensionsWrapperName).PrintEndLine("(value);");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool operator ==(")
                    .Print(ExtensionsWrapperName).Print(" left, ")
                    .Print(ExtensionsWrapperName).PrintEndLine(" right) => left.UnderlyingValue == right.UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool operator !=(")
                    .Print(ExtensionsWrapperName).Print(" left, ")
                    .Print(ExtensionsWrapperName).PrintEndLine(" right) => left.UnderlyingValue != right.UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool operator <(")
                    .Print(ExtensionsWrapperName).Print(" left, ")
                    .Print(ExtensionsWrapperName).PrintEndLine(" right) => left.UnderlyingValue < right.UnderlyingValue;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool operator >(")
                    .Print(ExtensionsWrapperName).Print(" left, ")
                    .Print(ExtensionsWrapperName).PrintEndLine(" right) => left.UnderlyingValue > right.UnderlyingValue;");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClass(ref Printer p)
        {
            WriteAttribute(ref p);

            var @this = ParentIsNamespace ? "this " : "";

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GeneratedCode);
                p.PrintLine($"public const int Length = {Members.Count};");
                p.PrintEndLine();

                WriteToString(ref p, @this);

                if (OnlyNames == false)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static {ExtensionsWrapperName} AsExtensionsWrapper({@this}{FullyQualifiedName} value)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> new {ExtensionsWrapperName}(value);");
                    p = p.DecreasedIndent();
                    p.PrintEndLine();

                    foreach (var type in s_primitiveTypes)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintLine($"public static {ExtensionsWrapperName} As{ExtensionsWrapperName}({@this}{type} value)");
                        p = p.IncreasedIndent();
                        p.PrintLine($"=> new {ExtensionsWrapperName}(({FullyQualifiedName})({UnderlyingTypeName})value);");
                        p = p.DecreasedIndent();
                        p.PrintEndLine();
                    }
                }

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static {UnderlyingTypeName} ToUnderlyingValue({@this}{FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> ({UnderlyingTypeName})value;");
                p = p.DecreasedIndent();
                p.PrintEndLine();

                WriteTryParse(ref p, @this);
                WriteTryParseSpan(ref p, @this);

                if (OnlyNames == false)
                {
                    WriteTryFormat(ref p, @this);
                    WriteIsDefined(ref p, @this);
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

            p.PrintBeginLine("[global::EncosyTower.Modules.EnumExtensions.SourceGen.GeneratedEnumExtensionsFor(typeof(")
                .Print(FullyQualifiedName).Print("), typeof(I")
                .Print(ExtensionsName).Print("), typeof(")
                .Print(ExtensionsName).Print("), typeof(")
                .Print(ExtensionsWrapperName).Print(")")
                .PrintEndLine(")]");
        }

        private void WriteClassFixedDisplayNames(ref Printer p)
        {
            if (ReferenceUnityCollections == false)
            {
                return;
            }

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_FIXED_DISPLAY_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} {member.name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => ({FixedStringTypeFullyQualifiedName}){CLASS_DISPLAY_NAMES}.{member.name};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (OnlyNames == false)
                {
                    p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static global::Unity.Collections.NativeArray<{FixedStringTypeFullyQualifiedName}>.ReadOnly AsNativeArray({UNITY_COLLECTIONS_ALLOCATOR} allocator)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var names = global::Unity.Collections.CollectionHelper.CreateNativeArray<{FixedStringTypeFullyQualifiedName}>({ExtensionsName}.Length, allocator, global::Unity.Collections.NativeArrayOptions.UninitializedMemory);");

                        var index = 0;

                        foreach (var member in Members)
                        {
                            p.PrintLine($"names[{index}] = {member.name};");
                            index++;

                            if (index >= Members.Count)
                            {
                                break;
                            }
                        }

                        p.PrintLine("return names.AsReadOnly();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} Get({FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine($"_ => ToFixedString(ToUnderlyingValue(value)),");
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_FIXED_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} {member.name}");
                    p.OpenScope();
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"get => ({FixedStringTypeFullyQualifiedName}){CLASS_NAMES}.{member.name};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (OnlyNames == false)
                {
                    p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static global::Unity.Collections.NativeArray<{FixedStringTypeFullyQualifiedName}>.ReadOnly AsNativeArray({UNITY_COLLECTIONS_ALLOCATOR} allocator)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var names = global::Unity.Collections.CollectionHelper.CreateNativeArray<{FixedStringTypeFullyQualifiedName}>({ExtensionsName}.Length, allocator, global::Unity.Collections.NativeArrayOptions.UninitializedMemory);");

                        var index = 0;

                        foreach (var member in Members)
                        {
                            p.PrintLine($"names[{index}] = {member.name};");
                            index++;

                            if (index >= Members.Count)
                            {
                                break;
                            }
                        }

                        p.PrintLine("return names.AsReadOnly();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} Get({FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine($"=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine($"_ => ToFixedString(ToUnderlyingValue(value)),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassDisplayNames(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_DISPLAY_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine(GeneratedCode);

                    if (member.displayName is not null)
                    {
                        p.PrintLine($"public const string {member.name} = \"{member.displayName}\";");
                    }
                    else
                    {
                        p.PrintLine($"public const string {member.name} = {CLASS_NAMES}.{member.name};");
                    }

                    p.PrintEndLine();
                }

                if (OnlyNames == false)
                {
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

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static global::System.ReadOnlyMemory<string> AsMemory() => s_names;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static global::System.ReadOnlySpan<string> AsSpan() => s_names;");
                    p.PrintEndLine();
                }

                p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static string Get({FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine("_ => ToUnderlyingValue(value).ToString(),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassNames(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static partial class ").PrintEndLine(CLASS_NAMES);
            p.OpenScope();
            {
                foreach (var member in Members)
                {
                    p.PrintLine(GeneratedCode);
                    p.PrintLine($"public const string {member.name} = nameof({FullyQualifiedName}.{member.name});");
                    p.PrintEndLine();
                }

                if (OnlyNames == false)
                {
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

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static global::System.ReadOnlyMemory<string> AsMemory() => s_names;");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static global::System.ReadOnlySpan<string> AsSpan() => s_names;");
                    p.PrintEndLine();
                }

                p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static string Get({FullyQualifiedName} value)");
                p = p.IncreasedIndent();
                p.PrintLine("=> value switch");
                p.OpenScope();
                {
                    foreach (var member in Members)
                    {
                        p.PrintLine($"{FullyQualifiedName}.{member.name} => {member.name},");
                    }

                    p.PrintLine("_ => ToUnderlyingValue(value).ToString(),");
                }
                p.CloseScope("};");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassUnderlyingValues(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static global::System.ReadOnlyMemory<{UnderlyingTypeName}> AsMemory() => s_values;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static global::System.ReadOnlySpan<{UnderlyingTypeName}> AsSpan() => s_values;");

                if (ReferenceUnityCollections)
                {
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static global::Unity.Collections.NativeArray<{UnderlyingTypeName}>.ReadOnly AsNativeArray({UNITY_COLLECTIONS_ALLOCATOR} allocator)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> global::Unity.Collections.CollectionHelper.CreateNativeArray<{UnderlyingTypeName}>(s_values, allocator).AsReadOnly();");
                    p = p.DecreasedIndent();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteClassValues(ref Printer p)
        {
            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static global::System.ReadOnlyMemory<{FullyQualifiedName}> AsMemory() => s_values;");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public static global::System.ReadOnlySpan<{FullyQualifiedName}> AsSpan() => s_values;");

                if (ReferenceUnityCollections)
                {
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public static global::Unity.Collections.NativeArray<{FullyQualifiedName}>.ReadOnly AsNativeArray({UNITY_COLLECTIONS_ALLOCATOR} allocator)");
                    p = p.IncreasedIndent();
                    p.PrintLine($"=> global::Unity.Collections.CollectionHelper.CreateNativeArray<{FullyQualifiedName}>(s_values, allocator).AsReadOnly();");
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

                if (IsDisplayAttributeUsed)
                {
                    p.PrintLine("if (allowMatchingMetadataAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (ignoreCase)");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (name)");
                            p.OpenScope();
                            {
                                foreach (var member in Members)
                                {
                                    if (member.displayName is not null)
                                    {
                                        p.PrintLine($"case string s when s.Equals({CLASS_DISPLAY_NAMES}.{member.name}, global::System.StringComparison.OrdinalIgnoreCase):");
                                        p.OpenScope();
                                        {
                                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                            p.PrintLine("return true;");
                                        }
                                        p.CloseScope();
                                    }
                                }

                                p.PrintLine("default: break;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (name)");
                            p.OpenScope();
                            {
                                foreach (var member in Members)
                                {
                                    if (member.displayName is not null)
                                    {
                                        p.PrintLine($"case {CLASS_DISPLAY_NAMES}.{member.name}:");
                                        p.OpenScope();
                                        {
                                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                            p.PrintLine("return true;");
                                        }
                                        p.CloseScope();
                                    }
                                }

                                p.PrintLine("default: break;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }

                p.PrintLine("if (ignoreCase)");
                p.OpenScope();
                {
                    p.PrintLine("switch (name)");
                    p.OpenScope();
                    {
                        foreach (var member in Members)
                        {
                            p.PrintLine($"case string s when s.Equals({CLASS_NAMES}.{member.name}, global::System.StringComparison.OrdinalIgnoreCase):");
                            p.OpenScope();
                            {
                                p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                        }

                        p.PrintLine($"case string s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();

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
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintLine("switch (name)");
                    p.OpenScope();
                    {
                        foreach (var member in Members)
                        {
                            p.PrintLine($"case {CLASS_NAMES}.{member.name}:");
                            p.OpenScope();
                            {
                                p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                        }

                        p.PrintLine($"case string s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();

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
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParseSpan(ref Printer p, string @this)
        {
            const string SPAN = "global::System.ReadOnlySpan<char>";

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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

                if (IsDisplayAttributeUsed)
                {
                    p.PrintLine("if (allowMatchingMetadataAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (ignoreCase)");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (name)");
                            p.OpenScope();
                            {
                                foreach (var member in Members)
                                {
                                    if (member.displayName is not null)
                                    {
                                        p.PrintLine($"case {SPAN} s when global::System.MemoryExtensions.Equals(s, global::System.MemoryExtensions.AsSpan({CLASS_DISPLAY_NAMES}.{member.name}), global::System.StringComparison.OrdinalIgnoreCase):");
                                        p.OpenScope();
                                        {
                                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                            p.PrintLine("return true;");
                                        }
                                        p.CloseScope();
                                    }
                                }

                                p.PrintLine("default: break;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (name)");
                            p.OpenScope();
                            {
                                foreach (var member in Members)
                                {
                                    if (member.displayName is not null)
                                    {
                                        p.PrintLine($"case {SPAN} s when global::System.MemoryExtensions.Equals(s, global::System.MemoryExtensions.AsSpan({CLASS_DISPLAY_NAMES}.{member.name}), global::System.StringComparison.Ordinal):");
                                        p.OpenScope();
                                        {
                                            p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                            p.PrintLine("return true;");
                                        }
                                        p.CloseScope();
                                    }
                                }

                                p.PrintLine("default: break;");
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }

                p.PrintLine("if (ignoreCase)");
                p.OpenScope();
                {
                    p.PrintLine("switch (name)");
                    p.OpenScope();
                    {
                        foreach (var member in Members)
                        {
                            p.PrintLine($"case {SPAN} s when global::System.MemoryExtensions.Equals(s, global::System.MemoryExtensions.AsSpan({CLASS_NAMES}.{member.name}), global::System.StringComparison.OrdinalIgnoreCase):");
                            p.OpenScope();
                            {
                                p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                        }

                        p.PrintLine($"case {SPAN} s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();

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
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintLine("switch (name)");
                    p.OpenScope();
                    {
                        foreach (var member in Members)
                        {
                            p.PrintLine($"case {SPAN} s when global::System.MemoryExtensions.Equals(s, global::System.MemoryExtensions.AsSpan({CLASS_NAMES}.{member.name}), global::System.StringComparison.Ordinal):");
                            p.OpenScope();
                            {
                                p.PrintLine($"value = {FullyQualifiedName}.{member.name};");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                        }

                        p.PrintLine($"case {SPAN} s when {UnderlyingTypeName}.TryParse(name, out var underlyingValue):");
                        p.OpenScope();
                        {
                            p.PrintLine($"value = ({FullyQualifiedName})underlyingValue;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();

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
                p.PrintLine("/// <returns><c>true</c> if all of the bit fields that are set in <c>flag</c> are also set in the current instance; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static bool Contains({@this}{FullyQualifiedName} value, {FullyQualifiedName} flag)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> (value & flag) == flag;");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Determines whether any of the bit fields are set in the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns><c>true</c> if any of the bit fields that are set in <c>flag</c> is also set in the current instance; otherwise, <c>false</c>.</returns>");
            }

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static bool Any({@this}{FullyQualifiedName} value, {FullyQualifiedName} flag)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> (value & flag) != 0;");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            if (NoDocumentation == false)
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Unsets one or more bit fields on the current instance.");
                p.PrintLine("/// </summary>");
                p.PrintLine("/// <returns>A new instance without bit fields that are set in <c>flags</c>.</returns>");
            }

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static {FullyQualifiedName} Unset({@this}{FullyQualifiedName} value, {FullyQualifiedName} flag)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> value & (~flag);");
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static bool IsDefinedIn({@this}string name, {FullyQualifiedName} _)");
            p = p.IncreasedIndent();
            p.PrintLine($"=> IsDefinedIn(name, default({FullyQualifiedName}), allowMatchingMetadataAttribute: false);");
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static bool IsDefinedIn({@this}string name, {FullyQualifiedName} _, bool allowMatchingMetadataAttribute)");
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
                                if (member.displayName is not null)
                                {
                                    p.PrintLine($"{CLASS_DISPLAY_NAMES}.{member.name} => true,");
                                }
                            }

                            p.PrintLine("_ => false,");
                        }
                        p.CloseScope("};");
                    }
                    p.CloseScope();

                    p.PrintLine("if (isDefinedInDisplayAttribute)");
                    p.OpenScope();
                    {
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
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

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
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
            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static bool TryFormat(");
            p = p.IncreasedIndent();
            {
                p.PrintLine($"  {@this}{FullyQualifiedName} value");
                p.PrintLine(", global::System.Span<char> destination");
                p.PrintLine(", out int charsWritten");
                p.PrintLine(", global::System.ReadOnlySpan<char> format = default");
                p.PrintLine(", global::System.IFormatProvider provider = null");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("if (IsDefined(value))");
                p.OpenScope();
                {
                    p.PrintLine("var span = global::System.MemoryExtensions.AsSpan(ToStringFast(value));");

                    p.PrintLine("if (span.Length == 0 || span.TryCopyTo(destination) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("charsWritten = 0;");
                        p.PrintLine(" return false;");
                    }
                    p.CloseScope();

                    p.PrintLine("charsWritten = span.Length;");
                    p.PrintLine(" return true;");
                }
                p.CloseScope();

                p.PrintLine("if (ToUnderlyingValue(value).TryFormat(destination, out var chars, format, provider))");
                p.OpenScope();
                {
                    p.PrintLine("charsWritten = chars;");
                    p.PrintLine(" return true;");
                }
                p.CloseScope();

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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static string ToStringFast({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_NAMES).PrintEndLine(".Get(value);");
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static string ToDisplayStringFast({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_DISPLAY_NAMES).PrintEndLine(".Get(value);");
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} ToFixedString({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_FIXED_NAMES).PrintEndLine(".Get(value);");
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

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static {FixedStringTypeFullyQualifiedName} ToDisplayFixedString({@this}{FullyQualifiedName} value)");
            p = p.IncreasedIndent();
            p.PrintBeginLine("=> ").Print(CLASS_FIXED_DISPLAY_NAMES).PrintEndLine(".Get(value);");
            p = p.DecreasedIndent();
            p.PrintEndLine();

            p.PrintLine(GeneratedCode).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private static {FixedStringTypeFullyQualifiedName} ToFixedString({UnderlyingTypeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"var fs = new {FixedStringTypeFullyQualifiedName}();");
                p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, value);");
                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}