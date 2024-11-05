using EncosyTower.Modules.SourceGen;

namespace EncosyTower.Modules.UnionIds.SourceGen
{
    partial class UnionIdDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.UnionIds.SourceGen.UnionIdGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        private const string FIELD_OFFSET = "[global::System.Runtime.InteropServices.FieldOffset({0})]";
        private const string ODIN_PROPERTY_ORDER = "[global::Sirenix.OdinInspector.PropertyOrder({0})]";
        private const string ODIN_SHOW_IN_INSPECTOR = "[global::Sirenix.OdinInspector.ShowInInspector]";
        private const string ODIN_SHOW_IF = "[global::Sirenix.OdinInspector.ShowIf(nameof(Kind), IdKind.{0})]";
        private const string ODIN_LABEL = "[global::Sirenix.OdinInspector.LabelText(\"{0}\")]";
        private const string INSPECTOR_NAME = "[global::UnityEngine.InspectorName(\"{0}\")]";
        private const string DESCRIPTION = "[global::System.ComponentModel.Description(\"{0}\")]";
        private const string SERIALIZABLE = "[global::System.Serializable]";
        private const string NON_SERIALIZED = "[field: global::System.NonSerialized]";
        private const string SERIALIZE_FIELD = "[global::UnityEngine.SerializeField]";

        private readonly static string[] s_operators = new[] { "==", "!=", "<", "<=", ">", ">=" };
        private readonly static string[] s_comparers = new[] { "<", "<=", ">", ">=" };

        public string WriteCode()
        {
            var typeName = Syntax.Identifier.Text;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine(STRUCT_LAYOUT);
            p.PrintBeginLine("partial struct ").Print(typeName).Print(" ")
                .Print(": global::System.IEquatable<").Print(typeName).Print(">")
                .Print(", global::System.IComparable<").Print(typeName).Print(">")
                .PrintEndLine();

            p.OpenScope();
            {
                WriteFields(ref p);
                WriteConstructors(ref p, typeName);
                WriteConstructorIdKindString(ref p, typeName);
                WriteConstructorStringString(ref p, typeName);
                WriteConstructorIdKindSpan(ref p, typeName);
                WriteConstructorSpanSpan(ref p, typeName);
                WritePartialTryParseMethods(ref p);
                WriteTryParseIdKindString(ref p, typeName);
                WriteTryParseStringString(ref p, typeName);
                WriteTryParseIdKindSpan(ref p, typeName);
                WriteTryParseSpanSpan(ref p, typeName);
                WriteCommonMethods(ref p, typeName);
                WriteToString(ref p);
                WriteToDisplayString(ref p);
                WriteToFixedString(ref p);
                WriteToDisplayFixedString(ref p);
                WriteGetIdStringFast(ref p);
                WriteGetIdDisplayStringFast(ref p);
                WriteGetIdFixedString(ref p);
                WriteGetIdDisplayFixedString(ref p);
                WritePartialAppendMethods(ref p);
                WriteIdKindEnum(ref p);
                WriteSerializable(ref p, typeName);
                WriteIdEnumExtensions(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            KindExtensionsRef.WriteCode(ref p);

            WriteEnumeration(ref p);

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteFields(ref Printer p)
        {
            p.PrintLine(FIELD_OFFSET, 0).PrintLine(GENERATED_CODE);
            p.PrintBeginLine("private readonly ").Print(RawTypeName).PrintEndLine(" _raw;");
            p.PrintEndLine();

            {
                var order = 1;

                p.PrintLine(FIELD_OFFSET, 0);

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                    p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                }

                p.PrintBeginLine("public readonly ").Print(IdRawTypeName).Print(" Id").PrintEndLine(";");
                p.PrintEndLine();

                if (KindRefs.Count > 0)
                {
                    var label = DisplayNameForId;
                    var hasLabel = string.IsNullOrEmpty(label) == false;

                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;
                        var fullName = kind.fullName;

                        p.PrintLine(FIELD_OFFSET, 0);
                        p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                        p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                        p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                        p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                        p.PrintLineIf(this.References.odin, ODIN_SHOW_IF, (object)kindName).PrintLine(GENERATED_CODE);
                        p.PrintBeginLine("public readonly ").Print(fullName).Print(" Id_").Print(kindName).PrintEndLine(";");
                        p.PrintEndLine();

                        order += 1;
                    }
                }
            }

            {
                var label = DisplayNameForKind;
                var hasLabel = string.IsNullOrEmpty(label) == false;
                var canShowKind = KindEnumIsEmpty == false && References.odin;

                p.PrintLine(FIELD_OFFSET, KindFieldOffset);
                p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                p.PrintLineIf(hasLabel && canShowKind, ODIN_LABEL, (object)label);
                p.PrintLineIf(canShowKind, ODIN_SHOW_IN_INSPECTOR);
                p.PrintLineIf(canShowKind, ODIN_PROPERTY_ORDER, 0).PrintLine(GENERATED_CODE);
                p.PrintLine("public readonly IdKind Kind;");
                p.PrintEndLine();
            }
        }

        private void WriteConstructors(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("private ").Print(typeName).Print("(").Print(RawTypeName).PrintEndLine(" value) : this()");
            p.OpenScope();
            {
                p.PrintLine("_raw = value;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName).Print("(IdKind kind, ").Print(IdRawTypeName).PrintEndLine(" id) : this()");
            p.OpenScope();
            {
                p.PrintBeginLine("Id").PrintEndLine(" = id;");
                p.PrintLine("Kind = kind;");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var kind in KindRefs)
            {
                var kindName = kind.name;
                var fullName = kind.fullName;

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ").Print(typeName).Print("(").Print(fullName).PrintEndLine(" id) : this()");
                p.OpenScope();
                {
                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = id;");
                    p.PrintBeginLine("Kind = IdKind.").Print(kindName).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            foreach (var kind in KindRefs)
            {
                var fullName = kind.fullName;

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static implicit operator ").Print(typeName)
                    .Print("(").Print(fullName).PrintEndLine(" id)");
                p.OpenScope();
                {
                    p.PrintLine("return new(id);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteConstructorIdKindString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName)
                .Print("(IdKind kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(kind, global::System.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorStringString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName)
                .Print("(string kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(global::System.MemoryExtensions.AsSpan(kind), global::System.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorIdKindSpan(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName).PrintEndLine("(IdKind kind, global::System.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(IdRawTypeName).PrintEndLine(".TryParse(id, out var idValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("idValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("Id = idValue;");
                }
                else
                {
                    p.PrintLine("switch (kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;
                            var fullName = kind.fullName;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine(kindName)
                                        .PrintEndLine("Extensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }
                                else
                                {
                                    if (kind.hasTryParseSpan)
                                    {
                                        p.PrintBeginLine(fullName)
                                            .PrintEndLine(".TryParse(id, out var idValue);");
                                    }
                                    else
                                    {
                                        p.PrintLine("var idResult = false;");
                                        p.PrintBeginLine("var idValue = default(").Print(fullName).PrintEndLine(");");
                                        p.PrintEndLine();

                                        p.PrintBeginLine("TryParse_").Print(kindName).PrintEndLine("(id, ref idResult, ref idValue);");
                                        p.PrintEndLine();
                                    }

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("Kind = kind;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorSpanSpan(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName).PrintEndLine("(global::System.ReadOnlySpan<char> kind, global::System.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(KindRawTypeName).PrintEndLine(".TryParse(kind, out var kindValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("if (").Print(IdRawTypeName).PrintEndLine(".TryParse(id, out var idValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("idValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("Id = idValue;");
                    p.PrintLine("Kind = (IdKind)kindValue;");
                }
                else
                {
                    p.PrintBeginLine("if (").Print(KindExtensionsRef.ExtensionsName)
                        .PrintEndLine(".TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("switch (kindValue)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;
                            var fullName = kind.fullName;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine(kindName)
                                        .PrintEndLine("Extensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }
                                else
                                {
                                    if (kind.hasTryParseSpan)
                                    {
                                        p.PrintBeginLine(fullName)
                                            .PrintEndLine(".TryParse(id, out var idValue);");
                                    }
                                    else
                                    {
                                        p.PrintLine("var idResult = false;");
                                        p.PrintBeginLine("var idValue = default(").Print(fullName).PrintEndLine(");");
                                        p.PrintEndLine();

                                        p.PrintBeginLine("TryParse_").Print(kindName).PrintEndLine("(id, ref idResult, ref idValue);");
                                        p.PrintEndLine();
                                    }

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("Kind = kindValue;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WritePartialTryParseMethods(ref Printer p)
        {
            foreach (var kind in KindRefs)
            {
                if (kind.isEnum)
                {
                    continue;
                }

                if (kind.hasTryParseSpan == false)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("static partial void TryParse_").Print(kind.name)
                        .Print("(global::System.ReadOnlySpan<char> str, ref bool result, ref ").Print(kind.fullName).PrintEndLine(" value);");
                    p.PrintEndLine();
                }
            }
        }

        private void WriteTryParseIdKindString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public bool TryParse(IdKind kind, string id, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("return TryParse(kind, global::System.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParseStringString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public bool TryParse(string kind, string id, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("return TryParse(global::System.MemoryExtensions.AsSpan(kind), global::System.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParseIdKindSpan(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public bool TryParse(IdKind kind, global::System.ReadOnlySpan<char> id, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(IdRawTypeName).PrintEndLine(".TryParse(id, out var idValue))");
                    p.OpenScope();
                    {
                        p.PrintLine("result = new(kind, idValue);");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                else
                {
                    p.PrintLine("switch (kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;
                            var fullName = kind.fullName;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("if (").Print(kindName)
                                        .PrintEndLine("Extensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }
                                else
                                {
                                    if (kind.hasTryParseSpan)
                                    {
                                        p.PrintBeginLine("var idResult = ").Print(fullName)
                                            .PrintEndLine(".TryParse(id, out var idValue);");
                                    }
                                    else
                                    {
                                        p.PrintLine("var idResult = false;");
                                        p.PrintBeginLine("var idValue = default(").Print(fullName).PrintEndLine(");");
                                        p.PrintEndLine();

                                        p.PrintBeginLine("TryParse_").Print(kindName).PrintEndLine("(id, ref idResult, ref idValue);");
                                        p.PrintEndLine();
                                    }

                                    p.PrintLine("if (idResult)");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }

                                p.PrintEndLine();
                                p.PrintLine("goto FAILED;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("FAILED:");
                p.PrintLine("result = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParseSpanSpan(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public bool TryParse(global::System.ReadOnlySpan<char> kind, global::System.ReadOnlySpan<char> id, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(KindRawTypeName).PrintEndLine(".TryParse(kind, out var kindValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("if (").Print(IdRawTypeName).PrintEndLine(".TryParse(id, out var idValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("result = new((IdKind)kindValue, idValue);");
                    p.PrintLine("return true;");
                }
                else
                {
                    p.PrintBeginLine("if (").Print(KindExtensionsRef.ExtensionsName)
                        .PrintEndLine(".TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("result = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("switch (kindValue)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;
                            var fullName = kind.fullName;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("if (").Print(kindName)
                                        .PrintEndLine("Extensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }
                                else
                                {
                                    if (kind.hasTryParseSpan)
                                    {
                                        p.PrintBeginLine("var idResult = ").Print(fullName)
                                            .PrintEndLine(".TryParse(id, out var idValue);");
                                    }
                                    else
                                    {
                                        p.PrintLine("var idResult = false;");
                                        p.PrintBeginLine("var idValue = default(").Print(fullName).PrintEndLine(");");
                                        p.PrintEndLine();

                                        p.PrintBeginLine("TryParse_").Print(kindName).PrintEndLine("(id, ref idResult, ref idValue);");
                                        p.PrintEndLine();
                                    }

                                    p.PrintEndLine();

                                    p.PrintLine("if (idResult)");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }

                                p.PrintEndLine();
                                p.PrintLine("goto FAILED;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("FAILED:");
                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteCommonMethods(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly bool Equals(").Print(typeName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return _raw == other._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(typeName).PrintEndLine(" other && _raw == other._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly int CompareTo(").Print(typeName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return _raw.CompareTo(other._raw);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("return _raw.GetHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static explicit operator ").Print(RawTypeName)
                .Print("(").Print(typeName).PrintEndLine(" value)");
            p.OpenScope();
            {
                p.PrintLine("return value._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static implicit operator ").Print(typeName)
                .Print("(").Print(RawTypeName).PrintEndLine(" value)");
            p.OpenScope();
            {
                p.PrintLine("return new(value);");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var op in s_operators)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool operator ").Print(op)
                    .Print("(").Print(typeName).Print(" left, ").Print(typeName).PrintEndLine(" right)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return left._raw ").Print(op).PrintEndLine(" right._raw;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteToString(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly override string ToString()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLine("return Id.ToString();");
                }
                else
                {
                    p.PrintLine("return Kind switch");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            if (kind.isEnum)
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToStringFast()}-{")
                                    .Print(kindName).Print("Extensions.ToStringFast(Id_").Print(kindName)
                                    .PrintEndLine(")}\",");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToStringFast()}-{Id_").Print(kindName)
                                    .PrintEndLine("}\",");
                            }
                        }

                        p.PrintLine("_ => $\"{Kind.ToStringFast()}-{Id}\",");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToDisplayString(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly string ToDisplayString()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLine("return Id.ToString();");
                }
                else
                {
                    p.PrintLine("return Kind switch");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            if (kind.isEnum)
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToDisplayStringFast()}-{")
                                    .Print(kindName).Print("Extensions.ToDisplayStringFast(Id_").Print(kindName)
                                    .PrintEndLine(")}\",");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToDisplayStringFast()}-{Id_").Print(kindName)
                                    .PrintEndLine("}\",");
                            }
                        }

                        p.PrintLine("_ => $\"{Kind.ToDisplayStringFast()}-{Id}\",");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToFixedString(ref Printer p)
        {
            if (References.unityCollections == false
                || string.IsNullOrEmpty(FixedStringType)
            )
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                }
                else
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Kind.ToFixedString());");
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, '-');");
                    p.PrintEndLine();

                    p.PrintLine("switch (Kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, ")
                                        .Print(kindName).Print("Extensions").Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.hasToFixedString)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName).Print("(ref fs, Id_").Print(kindName).PrintEndLine(");");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                            p.PrintLine("break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToDisplayFixedString(ref Printer p)
        {
            if (References.unityCollections == false
                || string.IsNullOrEmpty(FixedStringType)
            )
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                }
                else
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Kind.ToDisplayFixedString());");
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, '-');");
                    p.PrintEndLine();

                    p.PrintLine("switch (Kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, ")
                                        .Print(kindName).Print("Extensions").Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.hasToFixedString)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName).Print("(ref fs, Id_").Print(kindName).PrintEndLine(");");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                            p.PrintLine("break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdStringFast(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly string GetIdStringFast()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLine("return Id.ToString();");
                }
                else
                {
                    p.PrintLine("return Kind switch");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            if (kind.isEnum)
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => ")
                                    .Print(kindName).Print("Extensions.ToStringFast(Id_").Print(kindName)
                                    .PrintEndLine("),");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => Id_").Print(kindName).Print(".ToString()")
                                    .PrintEndLine(",");
                            }
                        }

                        p.PrintLine("_ => Id.ToString(),");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdDisplayStringFast(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly string GetIdDisplayStringFast()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLine("return Id.ToString();");
                }
                else
                {
                    p.PrintLine("return Kind switch");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            if (kind.isEnum)
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => ")
                                    .Print(kindName).Print("Extensions.ToDisplayStringFast(Id_").Print(kindName)
                                    .PrintEndLine("),");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => Id_").Print(kindName).Print(".ToString()")
                                    .PrintEndLine(",");
                            }
                        }

                        p.PrintLine("_ => Id.ToString(),");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdFixedString(ref Printer p)
        {
            if (References.unityCollections == false
                || string.IsNullOrEmpty(FixedStringType)
            )
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                }
                else
                {
                    p.PrintEndLine();

                    p.PrintLine("switch (Kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, ")
                                        .Print(kindName).Print("Extensions").Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.hasToFixedString)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName).Print("(ref fs, Id_").Print(kindName).PrintEndLine(");");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                            p.PrintLine("break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdDisplayFixedString(ref Printer p)
        {
            if (References.unityCollections == false
                || string.IsNullOrEmpty(FixedStringType)
            )
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                }
                else
                {
                    p.PrintEndLine();

                    p.PrintLine("switch (Kind)");
                    p.OpenScope();
                    {
                        foreach (var kind in KindRefs)
                        {
                            var kindName = kind.name;

                            p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                            p.OpenScope();
                            {
                                if (kind.isEnum)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, ")
                                        .Print(kindName).Print("Extensions").Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.hasToFixedString)
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName).Print("(ref fs, Id_").Print(kindName).PrintEndLine(");");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);");
                            p.PrintLine("break;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("return fs;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WritePartialAppendMethods(ref Printer p)
        {
            if (References.unityCollections == false)
            {
                return;
            }

            foreach (var kind in KindRefs)
            {
                if (kind.isEnum || kind.hasToFixedString)
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("static partial void Append_").Print(kind.name)
                    .Print("(ref ").Print(FixedStringType).Print(" fs, ").Print(kind.fullName).PrintEndLine(" value);");
                p.PrintEndLine();
            }
        }

        private void WriteIdKindEnum(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum IdKind : ").PrintEndLine(KindRawTypeName);
            p.OpenScope();
            {
                uint order = 0;

                if (KindEnumIsEmpty == false && KindRefs.Count < 1)
                {
                    p.PrintLine("None = 0,");
                    p.PrintEndLine();
                    order += 1;
                }

                foreach (var kind in KindRefs)
                {
                    var kindName = kind.name;
                    var fullName = kind.fullName;
                    var kindOrder = PreserveIdKindOrder ? kind.order : order;
                    var hasLabel = string.IsNullOrEmpty(kind.displayName) == false;

                    p.PrintLine($"/// <see cref=\"{fullName}\"/>");
                    p.PrintLineIf(hasLabel, DESCRIPTION, (object)kind.displayName);
                    p.PrintLineIf(hasLabel && this.References.unity, INSPECTOR_NAME, (object)kind.displayName);
                    p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)kind.displayName);
                    p.PrintBeginLine(kindName).Print(" = ").PrintEndLine($"{kindOrder},");
                    p.PrintEndLine();

                    order += 1;
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteSerializable(ref Printer p, string typeName)
        {
            p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct Serializable ")
                .Print(": global::EncosyTower.Modules.ITryConvert<").Print(typeName).Print(">")
                .Print(", global::System.IEquatable<Serializable>")
                .Print(", global::System.IComparable<Serializable>")
                .PrintEndLine();
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLine(NON_SERIALIZED);
                }
                else
                {
                    var label = DisplayNameForKind;
                    var hasLabel = string.IsNullOrEmpty(label) == false;

                    p.PrintLineIf(References.unity, SERIALIZE_FIELD);
                    p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                    p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                    p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, 0);
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine("public IdKind Kind;");
                p.PrintEndLine();

                if (KindRefs.Count < 1)
                {
                    var label = DisplayNameForId;
                    var hasLabel = string.IsNullOrEmpty(label) == false;

                    p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                    p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                }

                p.PrintLineIf(References.unity, SERIALIZE_FIELD);
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public {IdRawTypeName} Id;");
                p.PrintEndLine();

                if (KindRefs.Count > 0)
                {
                    var order = 1;

                    var label = DisplayNameForId;
                    var hasLabel = string.IsNullOrEmpty(label) == false;

                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;
                        var fullName = kind.fullName;

                        p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                        p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                        p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                        p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                        p.PrintLineIf(this.References.odin, ODIN_SHOW_IF, (object)kindName);
                        p.PrintLine(GENERATED_CODE);
                        p.PrintLine(NON_SERIALIZED);
                        p.PrintBeginLine("public ").Print(fullName).Print(" Id_").PrintEndLine(kindName);
                        p.OpenScope();
                        {
                            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                            p.PrintBeginLine("get => (").Print(fullName).PrintEndLine(")Id;");
                            p.PrintEndLine();

                            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                            p.PrintBeginLine("set => Id = (").Print(IdRawTypeName).PrintEndLine(")value;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        order += 1;
                    }
                }

                p.PrintBeginLine("public Serializable").Print("(IdKind kind, ").Print(IdRawTypeName).PrintEndLine(" id) : this()");
                p.OpenScope();
                {
                    p.PrintLine("Kind = kind;");
                    p.PrintLine("Id = id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                if (KindRefs.Count > 0)
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;
                        var fullName = kind.fullName;

                        p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public Serializable").Print("(").Print(fullName).PrintEndLine(" id) : this()");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("Kind = IdKind.").Print(kindName).PrintEndLine(";");
                            p.PrintBeginLine("Id = (").Print(IdRawTypeName).PrintEndLine(")id;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }

                WriteConstructorIdKindString(ref p, "Serializable");
                WriteConstructorStringString(ref p, "Serializable");
                WriteConstructorIdKindSpan(ref p, "Serializable");
                WriteConstructorSpanSpan(ref p, "Serializable");

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool TryConvert(out ").Print(typeName).PrintEndLine(" result)");
                p.OpenScope();
                {
                    if (KindRefs.Count < 1)
                    {
                        p.PrintLine("result = new(Id);");
                        p.PrintLine("return true;");
                    }
                    else
                    {
                        p.PrintLine("switch (Kind)");
                        p.OpenScope();
                        {
                            foreach (var kind in KindRefs)
                            {
                                var kindName = kind.name;

                                p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                                p.OpenScope();
                                {
                                    p.PrintBeginLine("result = new(Id_").Print(kindName).PrintEndLine(");");
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
                            p.PrintEndLine();
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly bool Equals(Serializable other)");
                p.OpenScope();
                {
                    p.PrintLine("return Kind == other.Kind && Id == other.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.OpenScope();
                {
                    p.PrintLine("return obj is Serializable other && Kind == other.Kind && Id == other.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly int CompareTo(Serializable other)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ((").Print(RawTypeName).Print(")this).CompareTo((")
                        .Print(RawTypeName).PrintEndLine(")other);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly override int GetHashCode()");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ((").Print(RawTypeName).PrintEndLine(")this).GetHashCode();");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteToString(ref p);
                WriteToDisplayString(ref p);
                WriteToFixedString(ref p);
                WriteToDisplayFixedString(ref p);
                WriteGetIdStringFast(ref p);
                WriteGetIdDisplayStringFast(ref p);
                WriteGetIdFixedString(ref p);
                WriteGetIdDisplayFixedString(ref p);

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static explicit operator ").Print(RawTypeName).PrintEndLine("(Serializable value)");
                p.OpenScope();
                {
                    p.PrintLine("return new Union { id = value.Id, kind = value.Kind }.raw;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static bool operator ==(Serializable left, Serializable right)");
                p.OpenScope();
                {
                    p.PrintLine("return left.Kind == right.Kind && left.Id == right.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static bool operator !=(Serializable left, Serializable right)");
                p.OpenScope();
                {
                    p.PrintLine("return left.Kind != right.Kind || left.Id != right.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var op in s_comparers)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static bool operator ").Print(op)
                        .PrintEndLine("(Serializable left, Serializable right)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ((").Print(RawTypeName).Print(")left) ")
                            .Print(op)
                            .Print(" ((").Print(RawTypeName).PrintEndLine(")right);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(STRUCT_LAYOUT).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("private struct Union");
                p.OpenScope();
                {
                    p.PrintLine(FIELD_OFFSET, 0).PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public ").Print(RawTypeName).PrintEndLine(" raw;");
                    p.PrintEndLine();

                    p.PrintLine(FIELD_OFFSET, 0);
                    p.PrintBeginLine("public ").Print(IdRawTypeName).Print(" id").PrintEndLine(";");
                    p.PrintEndLine();

                    p.PrintLine(FIELD_OFFSET, KindFieldOffset);
                    p.PrintLine("public IdKind kind;");
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteIdEnumExtensions(ref Printer p)
        {
            foreach (var extensions in IdEnumExtensionsRefs)
            {
                extensions.WriteCode(ref p);
            }
        }

        /// <summary>
        /// Total member count of following enums
        /// <list type="bullet">
        /// <item><see cref=""/></item>
        /// <item>B</item>
        /// </list>
        /// </summary>
        /// <param name="p"></param>
        private void WriteEnumeration(ref Printer p)
        {
            var typeName = Syntax.Identifier.Text;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine(KindExtensionsRef.Accessibility.GetKeyword()).Print(" static partial class ")
                .Print(typeName)
                .PrintEndLine("Enumeration");
            p.OpenScope();
            {
                var length = 0;

                p.PrintLine("/// <summary>");
                p.PrintLine("/// Total member count of the following enums");
                p.PrintLine("/// <list type=\"bullet\">");

                for (var i = 0; i < idEnumsCount; i++)
                {
                    var idEnum = idEnums[i];
                    var count = idEnum.Members.Count;
                    length += count;

                    p.PrintBeginLine("/// <item><see cref=\"")
                        .Print(idEnum.FullyQualifiedName)
                        .Print("\"/> (")
                        .Print(count.ToString())
                        .PrintEndLine(")</item>");
                }

                p.PrintLine("/// </list>");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public const int Length = ").Print(length.ToString()).PrintEndLine(";");
                p.PrintEndLine();

                WriteEnumerationCopyTo(ref p);
                WriteEnumerationTryCopyTo(ref p);
                WriteEnumerationAddTo(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEnumerationCopyTo(ref Printer p)
        {
            var typeName = Syntax.Identifier.Text;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static void CopyTo(global::System.Span<")
                .Print(typeName)
                .PrintEndLine("> dest)");
            p.OpenScope();
            {
                p.PrintBeginLine("if (dest.Length < Length) ")
                    .PrintEndLine("throw new global::System.ArgumentOutOfRangeException(nameof(dest));");
                p.PrintEndLine();

                var index = 0;

                for (var i = 0; i < idEnumsCount; i++)
                {
                    var idEnum = idEnums[i];
                    var members = idEnum.Members;
                    var membersCount = members.Count;

                    for (var k = 0; k < membersCount; k++)
                    {
                        var member = members[k];

                        p.PrintBeginLine("dest[").Print(index.ToString()).Print("] = ")
                            .Print(idEnum.FullyQualifiedName).Print(".").Print(member.name)
                            .PrintEndLine(";");

                        index += 1;
                    }

                    if (i < idEnumsCount - 1)
                    {
                        p.PrintEndLine();
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEnumerationTryCopyTo(ref Printer p)
        {
            var typeName = Syntax.Identifier.Text;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static bool TryCopyTo(global::System.Span<")
                .Print(typeName)
                .PrintEndLine("> dest)");
            p.OpenScope();
            {
                p.PrintLine("if (dest.Length < Length) return false;");
                p.PrintEndLine();

                var index = 0;

                for (var i = 0; i < idEnumsCount; i++)
                {
                    var idEnum = idEnums[i];
                    var members = idEnum.Members;
                    var membersCount = members.Count;

                    for (var k = 0; k < membersCount; k++)
                    {
                        var member = members[k];

                        p.PrintBeginLine("dest[").Print(index.ToString()).Print("] = ")
                            .Print(idEnum.FullyQualifiedName).Print(".").Print(member.name)
                            .PrintEndLine(";");

                        index += 1;
                    }

                    if (i < idEnumsCount - 1)
                    {
                        p.PrintEndLine();
                    }
                }

                p.PrintEndLine();
                p.PrintLine("return true;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteEnumerationAddTo(ref Printer p)
        {
            var typeName = Syntax.Identifier.Text;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static void AddTo<TCollection>([global::System.Diagnostics.CodeAnalysis.NotNull] ")
                .PrintEndLine("TCollection dest)");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("where TCollection : global::System.Collections.Generic.ICollection<")
                    .Print(typeName).PrintEndLine(">");
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
                for (var i = 0; i < idEnumsCount; i++)
                {
                    var idEnum = idEnums[i];
                    var members = idEnum.Members;
                    var membersCount = members.Count;

                    for (var k = 0; k < membersCount; k++)
                    {
                        var member = members[k];

                        p.PrintBeginLine("dest.Add(")
                            .Print(idEnum.FullyQualifiedName).Print(".").Print(member.name)
                            .PrintEndLine(");");
                    }

                    if (i < idEnumsCount - 1)
                    {
                        p.PrintEndLine();
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
