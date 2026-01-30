namespace EncosyTower.SourceGen.Generators.UnionIds
{
    partial class UnionIdDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.UnionIds.UnionIdGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT_SIZE = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = {0})]";
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
        private readonly static string[] s_comparerOps = new[] { "<", "<=", ">", ">=" };
        private readonly static (string, string)[] s_equalityOps = new[] { ("==", ""), ("!=", "!") };

        public string WriteCode()
        {
            var typeName = Syntax.Identifier.Text;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine(string.Format(STRUCT_LAYOUT_SIZE, TypeSize));
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("[global::System.ComponentModel.TypeConverter(typeof(TypeConverter))]");
            p.PrintBeginLine("partial struct ").Print(typeName).Print(" ")
                .Print(": global::System.IEquatable<").Print(typeName).Print(">")
                .Print(", global::System.IComparable<").Print(typeName).Print(">")
                .PrintEndLine();
            p = p.IncreasedIndent();
            {
                p.PrintLine(", global::EncosyTower.Conversion.IToDisplayString");

                if (References.unityCollections && string.IsNullOrEmpty(FixedStringType) == false)
                {
                    p.PrintBeginLine(", global::EncosyTower.Conversion.IToFixedString<").Print(FixedStringType).PrintEndLine(">");
                    p.PrintBeginLine(", global::EncosyTower.Conversion.IToDisplayFixedString<").Print(FixedStringType).PrintEndLine(">");
                }

                p.PrintBeginLine(", global::EncosyTower.Conversion.ITryParse<").Print(typeName).PrintEndLine(">");
                p.PrintBeginLine(", global::EncosyTower.Conversion.ITryParseSpan<").Print(typeName).PrintEndLine(">");

                foreach (var kind in KindRefs)
                {
                    if (kind.equality.Strategy != EqualityStrategy.Default)
                    {
                        p.PrintBeginLine(", global::System.IEquatable<").Print(kind.fullName).PrintEndLine(">");
                    }
                }
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
                WriteFields(ref p);
                WriteConstructors(ref p, typeName);
                WriteConstructor_IdKind_IdString(ref p, typeName);
                WriteConstructor_IdKindString_IdString(ref p, typeName);
                WriteConstructor_IdKindString_IdUnsigned(ref p, typeName);
                WriteConstructor_IdKindString_IdSigned(ref p, typeName);
                WriteConstructor_IdKind_IdSpan(ref p, typeName, false);
                WriteConstructor_IdKindSpan_IdSpan(ref p, typeName, false);
                WriteConstructor_IdKindSpan_IdUnsigned(ref p, typeName);
                WriteConstructor_IdKindSpan_IdSigned(ref p, typeName);
                WritePartialTryParseMethods(ref p);
                WriteTryParse_String(ref p, typeName);
                WriteTryParse_Span(ref p, typeName);
                WriteTryParse_IdKind_IdString(ref p, typeName);
                WriteTryParse_IdKindString_IdString(ref p, typeName);
                WriteTryParse_IdKindString_IdUnsigned(ref p, typeName);
                WriteTryParse_IdKindString_IdSigned(ref p, typeName);
                WriteTryParse_IdKind_IdSpan(ref p, typeName);
                WriteTryParse_IdKindSpan_IdSpan(ref p, typeName);
                WriteTryParse_IdKindSpan_IdUnsigned(ref p, typeName);
                WriteTryParse_IdKindSpan_IdSigned(ref p, typeName);
                WriteCommonMethods(ref p, typeName);
                WriteToString(ref p, false);
                WriteToDisplayString(ref p, false);
                WriteToFixedString(ref p, false);
                WriteToDisplayFixedString(ref p, false);
                WriteGetIdStringFast(ref p, false);
                WriteGetIdDisplayStringFast(ref p, false);
                WriteGetIdFixedString(ref p, false);
                WriteGetIdDisplayFixedString(ref p, false);
                WriteTryGetIListNames(ref p);
                WriteTryGetIListDisplayNames(ref p);
                WriteTryGetNames(ref p);
                WriteTryGetDisplayNames(ref p);
                WriteTryGetFixedNames(ref p);
                WriteTryGetFixedDisplayNames(ref p);
                WritePartialAppendMethods(ref p);
                WriteIdKindEnum(ref p);
                WriteTypeConverter(ref p, typeName);
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
            p.PrintBeginLine("public const char SEPARATOR = '").Print(Separator).PrintEndLine("';");
            p.PrintEndLine();

            p.PrintLine(FIELD_OFFSET, 0);
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

                p.PrintBeginLine("public readonly ").Print(IdRawUnsignedTypeName).Print(" IdUnsigned").PrintEndLine(";");
                p.PrintEndLine();

                p.PrintLine(FIELD_OFFSET, 0);

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                    p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                }

                p.PrintBeginLine("public readonly ").Print(IdRawSignedTypeName).Print(" IdSigned").PrintEndLine(";");
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
                        p.PrintLineIf(this.References.odin, ODIN_SHOW_IF, (object)kindName);
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
                p.PrintLineIf(canShowKind, ODIN_PROPERTY_ORDER, 0);
                p.PrintLine("public readonly IdKind Kind;");
                p.PrintEndLine();
            }
        }

        private void WriteConstructors(ref Printer p, string typeName)
        {
            p.PrintBeginLine("private ").Print(typeName).Print("(").Print(RawTypeName).PrintEndLine(" value) : this()");
            p.OpenScope();
            {
                p.PrintLine("_raw = value;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(typeName).Print("(IdKind kind, ").Print(IdRawUnsignedTypeName).PrintEndLine(" id) : this()");
            p.OpenScope();
            {
                p.PrintBeginLine("IdUnsigned").PrintEndLine(" = id;");
                p.PrintLine("Kind = kind;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(typeName).Print("(IdKind kind, ").Print(IdRawSignedTypeName).PrintEndLine(" id) : this()");
            p.OpenScope();
            {
                p.PrintBeginLine("IdSigned").PrintEndLine(" = id;");
                p.PrintLine("Kind = kind;");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var kind in KindRefs)
            {
                var kindName = kind.name;
                var fullName = kind.fullName;

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

                p.PrintLine(AGGRESSIVE_INLINING);
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

        private void WriteConstructor_IdKind_IdString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName)
                .Print("(IdKind kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(kind, global::System.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindString_IdString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName)
                .Print("(string kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(global::System.MemoryExtensions.AsSpan(kind), global::System.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindString_IdLong(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName)
                .Print("(string kind, long id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(global::System.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindString_IdUnsigned(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName).Print("(string kind, ")
                .Print(IdRawUnsignedTypeName).Print(" id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(global::System.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindString_IdSigned(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName).Print("(string kind, ")
                .Print(IdRawSignedTypeName).Print(" id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true)")
                .PrintEndLine(" : this(global::System.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKind_IdSpan(ref Printer p, string typeName, bool isSerializableStruct)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(IdKind kind")
                .Print(", global::System.ReadOnlySpan<char> id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true")
                .PrintEndLine(") : this()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    if (isSerializableStruct)
                    {
                        p.PrintBeginLine("if (long.TryParse(id, out var idValue))");
                        p.OpenScope();
                        {
                            p.PrintLine("Id = idValue;");
                        }
                        p.CloseScope();
                    }
                    else
                    {
                        p.PrintBeginLine("if (").Print(IdRawUnsignedTypeName).PrintEndLine(".TryParse(id, out var idUnsigned))");
                        p.OpenScope();
                        {
                            p.PrintLine("IdUnsigned = idUnsigned;");
                        }
                        p.CloseScope();
                        p.PrintBeginLine("else if (").Print(IdRawSignedTypeName).PrintEndLine(".TryParse(id, out var idSigned))");
                        p.OpenScope();
                        {
                            p.PrintLine("IdSigned = idSigned;");
                        }
                        p.CloseScope();
                    }

                    p.PrintEndLine();
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
                                    p.PrintBeginLine(kind.enumExtensionsName)
                                        .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }
                                else
                                {
                                    var tryParse = kind.tryParseSpan;

                                    if (tryParse.DoesExist)
                                    {
                                        if (tryParse.ParamCount == 2)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                        else
                                        {
                                            p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                                .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                    }
                                    else
                                    {
                                        p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                            .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
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

        private void WriteConstructor_IdKindSpan_IdSpan(ref Printer p, string typeName, bool isSerializableStruct)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(global::System.ReadOnlySpan<char> kind")
                .Print(", global::System.ReadOnlySpan<char> id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true")
                .PrintEndLine(") : this()");
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

                    if (isSerializableStruct)
                    {
                        p.PrintBeginLine("if (long.TryParse(id, out var idValue))");
                        p.OpenScope();
                        {
                            p.PrintLine("Id = idValue;");
                        }
                        p.CloseScope();
                    }
                    else
                    {
                        p.PrintBeginLine("if (").Print(IdRawUnsignedTypeName).PrintEndLine(".TryParse(id, out var idUnsigned))");
                        p.OpenScope();
                        {
                            p.PrintLine("IdUnsigned = idUnsigned;");
                        }
                        p.CloseScope();
                        p.PrintBeginLine("else if (").Print(IdRawSignedTypeName).PrintEndLine(".TryParse(id, out var idSigned))");
                        p.OpenScope();
                        {
                            p.PrintLine("IdSigned = idSigned;");
                        }
                        p.CloseScope();
                    }

                    p.PrintEndLine();
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
                                    p.PrintBeginLine(kind.enumExtensionsName)
                                        .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");

                                    p.PrintBeginLine("Id_").Print(kindName).PrintEndLine(" = idValue;");
                                }
                                else
                                {
                                    var tryParse = kind.tryParseSpan;

                                    if (tryParse.DoesExist)
                                    {
                                        if (tryParse.ParamCount == 2)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                        else
                                        {
                                            p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                                .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                    }
                                    else
                                    {
                                        p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                            .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
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

        private void WriteConstructor_IdKindSpan_IdLong(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(global::System.ReadOnlySpan<char> kind")
                .Print(", long id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true")
                .PrintEndLine(") : this()");
            p.OpenScope();
            {
                p.PrintLine("Id = id;").PrintEndLine();

                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(KindRawTypeName).PrintEndLine(".TryParse(kind, out var kindValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

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

                    p.PrintLine("Kind = kindValue;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindSpan_IdUnsigned(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(global::System.ReadOnlySpan<char> kind, ")
                .Print(IdRawUnsignedTypeName).Print(" id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true")
                .PrintEndLine(") : this()");
            p.OpenScope();
            {
                p.PrintLine("IdUnsigned = id;").PrintEndLine();

                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(KindRawTypeName).PrintEndLine(".TryParse(kind, out var kindValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

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

                    p.PrintLine("Kind = kindValue;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKindSpan_IdSigned(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(global::System.ReadOnlySpan<char> kind, ")
                .Print(IdRawSignedTypeName).Print(" id")
                .Print(", bool ignoreCase = true")
                .Print(", bool allowMatchingMetadataAttribute = true")
                .PrintEndLine(") : this()");
            p.OpenScope();
            {
                p.PrintLine("IdSigned = id;").PrintEndLine();

                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(KindRawTypeName).PrintEndLine(".TryParse(kind, out var kindValue) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindValue = default;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

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

                var tryParse = kind.tryParseSpan;

                if (tryParse.DoesExist && tryParse.ParamCount is 2 or 4)
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("private static partial bool TryParse_").Print(kind.name)
                    .Print("(global::System.ReadOnlySpan<char> str, out ").Print(kind.fullName)
                    .PrintEndLine(" value, bool ignoreCase, bool allowMatchingMetadataAttribute);");
                p.PrintEndLine();
            }
        }

        private void WriteTryParse_String(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public bool TryParse(string str, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("return TryParse(global::System.MemoryExtensions.AsSpan(str), out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_Span(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(global::System.ReadOnlySpan<char> str, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("if (str.IsEmpty)");
                p.OpenScope();
                {
                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("var ranges = global::EncosyTower.Common.SpanAPI.Split(str, SEPARATOR, 2);");
                p.PrintEndLine();

                p.PrintLine("global::System.Range? kindRange = default;");
                p.PrintLine("global::System.Range? idRange = default;");
                p.PrintEndLine();

                p.PrintLine("foreach (var range in ranges)");
                p.OpenScope();
                {
                    p.PrintLine("if (kindRange.HasValue == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("kindRange = range;");
                        p.PrintLine("continue;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (idRange.HasValue == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("idRange = range;");
                        p.PrintLine("continue;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (kindRange.HasValue && idRange.HasValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("break;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (kindRange.HasValue == false || idRange.HasValue == false)");
                p.OpenScope();
                {
                    p.PrintLine("result = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("var kindSpan = str[kindRange.Value];");
                p.PrintLine("var idSpan = str[idRange.Value];");
                p.PrintEndLine();

                p.PrintLine("return TryParse(kindSpan, idSpan, out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_IdKind_IdString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
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

        private void WriteTryParse_IdKindString_IdString(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
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

        private void WriteTryParse_IdKindString_IdUnsigned(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public bool TryParse(string kind, ")
                .Print(IdRawUnsignedTypeName).Print(" id, out ").Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("return TryParse(global::System.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_IdKindString_IdSigned(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public bool TryParse(string kind, ")
                .Print(IdRawSignedTypeName).Print(" id, out ").Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                p.PrintLine("return TryParse(global::System.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_IdKind_IdSpan(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(IdKind kind, global::System.ReadOnlySpan<char> id, out ")
                .Print(typeName)
                .PrintEndLine(" result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintBeginLine("if (").Print(IdRawUnsignedTypeName).PrintEndLine(".TryParse(id, out var idValue))");
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
                                    p.PrintBeginLine("if (").Print(kind.enumExtensionsName)
                                        .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }
                                else
                                {
                                    var tryParse = kind.tryParseSpan;

                                    if (tryParse.DoesExist)
                                    {
                                        if (tryParse.ParamCount == 2)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                        else
                                        {
                                            p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                                .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                    }
                                    else
                                    {
                                        p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                            .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
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

        private void WriteTryParse_IdKindSpan_IdSpan(ref Printer p, string typeName)
        {
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

                    p.PrintBeginLine("if (").Print(IdRawUnsignedTypeName).PrintEndLine(".TryParse(id, out var idValue) == false)");
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
                                    p.PrintBeginLine("if (").Print(kind.enumExtensionsName)
                                        .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("result = new(idValue);");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                }
                                else
                                {
                                    var tryParse = kind.tryParseSpan;

                                    if (tryParse.DoesExist)
                                    {
                                        if (tryParse.ParamCount == 2)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, fullName, $"default({fullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                        else
                                        {
                                            p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                                .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
                                        }
                                    }
                                    else
                                    {
                                        p.PrintBeginLine("var idResult = TryParse_").Print(kindName)
                                            .PrintEndLine("(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);");
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

        private void WriteTryParse_IdKindSpan_IdUnsigned(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(global::System.ReadOnlySpan<char> kind, ")
                .Print(IdRawUnsignedTypeName).Print(" id, out ").Print(typeName)
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

                    p.PrintLine("result = new((IdKind)kindValue, id);");
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

                    p.PrintLine("result = new(kindValue, id);");
                    p.PrintLine("return true;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_IdKindSpan_IdSigned(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(global::System.ReadOnlySpan<char> kind, ")
                .Print(IdRawSignedTypeName).Print(" id, out ").Print(typeName)
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

                    p.PrintLine("result = new((IdKind)kindValue, id);");
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

                    p.PrintLine("result = new(kindValue, id);");
                    p.PrintLine("return true;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteCommonMethods(ref Printer p, string typeName)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public readonly bool Equals(").Print(typeName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return _raw == other._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(typeName).PrintEndLine(" other && _raw == other._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public readonly int CompareTo(").Print(typeName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return _raw.CompareTo(other._raw);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("return _raw.GetHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static explicit operator ").Print(RawTypeName)
                .Print("(").Print(typeName).PrintEndLine(" value)");
            p.OpenScope();
            {
                p.PrintLine("return value._raw;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
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
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static bool operator ").Print(op)
                    .Print("(").Print(typeName).Print(" left, ").Print(typeName).PrintEndLine(" right)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return left._raw ").Print(op).PrintEndLine(" right._raw;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            foreach (var kind in KindRefs)
            {
                var kindName = kind.name;
                var fullName = kind.fullName;
                var fullNameFromNullable = kind.fullNameFromNullable;
                var equality = kind.equality;

                if (equality.Strategy == EqualityStrategy.Default)
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool Equals(").Print(fullName).PrintEndLine(" other)");
                p.OpenScope();
                {
                    if (equality.Strategy == EqualityStrategy.Operator)
                    {
                        if (equality.IsNullable)
                        {
                            p.PrintBeginLine("return (Kind == IdKind.").Print(kindName).Print(")")
                                .Print(" && (Id_").Print(kindName).Print(".HasValue == other.HasValue)")
                                .Print(" && other.HasValue ")
                                .Print(" && (Id_").Print(kindName).PrintEndLine(".Value == other.Value);");
                        }
                        else
                        {
                            p.PrintBeginLine("return Kind == IdKind.").Print(kindName)
                                .Print(" && Id_").Print(kindName).PrintEndLine(" == other;");
                        }
                    }
                    else if (equality.Strategy == EqualityStrategy.Equals)
                    {
                        if (equality.IsNullable)
                        {
                            p.PrintBeginLine("return (Kind == IdKind.").Print(kindName).Print(")")
                                .Print(" && (Id_").Print(kindName).Print(".HasValue == other.HasValue)")
                                .Print(" && other.HasValue ");

                            if (equality.IsStatic)
                            {
                                p.Print(" && ").Print(fullNameFromNullable)
                                    .Print(".Equals(Id_").Print(kindName).PrintEndLine(".Value, other.Value);");
                            }
                            else
                            {
                                p.Print(" && Id_").Print(kindName).PrintEndLine(".Value.Equals(other.Value);");
                            }
                        }
                        else
                        {
                            p.PrintBeginLine("return Kind == IdKind.").Print(kindName);

                            if (equality.IsStatic)
                            {
                                p.Print(" && ").Print(fullName)
                                    .Print(".Equals(Id_").Print(kindName).PrintEndLine(", other);");
                            }
                            else
                            {
                                p.Print(" && Id_").Print(kindName).PrintEndLine(".Equals(other);");
                            }
                        }
                    }
                    else if (equality.IsNullable)
                    {
                        p.PrintBeginLine("return (Kind == IdKind.").Print(kindName).Print(")")
                            .Print(" && (Id_").Print(kindName).Print(".HasValue == other.HasValue)")
                            .Print(" && other.HasValue ")
                            .Print(" && global::System.Collections.Generic.EqualityComparer<").Print(fullNameFromNullable)
                            .Print(">.Default.Equals(Id_").Print(kindName).PrintEndLine(".Value, other.Value);");
                    }
                    else
                    {
                        p.PrintBeginLine("return Kind == IdKind.").Print(kindName)
                            .Print(" && global::System.Collections.Generic.EqualityComparer<").Print(fullName)
                            .Print(">.Default.Equals(Id_").Print(kindName).PrintEndLine(", other);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var (op1, op2) in s_equalityOps)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(typeName).Print(" left, ").Print(fullName).PrintEndLine(" right)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ").Print(op2).PrintEndLine("left.Equals(right);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(fullName).Print(" left, ").Print(typeName).PrintEndLine(" right)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ").Print(op2).PrintEndLine("right.Equals(left);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private void WriteToString(ref Printer p, bool isSerializableStruct)
        {
            p.PrintLine("public readonly override string ToString()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(isSerializableStruct, "return Id.ToString();", "return IdUnsigned.ToString();");
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
                                    .Print("{Kind.ToStringFast()}{SEPARATOR}{");

                                if (kind.nestedEnumExtensions)
                                {
                                    p.Print(kind.enumExtensionsName).Print(".ToStringFast(");
                                }
                                else
                                {
                                    p.Print("ToString_").Print(kind.name).Print("(");
                                }

                                p.Print("Id_").Print(kindName).PrintEndLine(")}\",");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToStringFast()}{SEPARATOR}{Id_").Print(kindName)
                                    .PrintEndLine("}\",");
                            }
                        }

                        var idField = isSerializableStruct ? "Id" : "IdUnsigned";

                        p.PrintBeginLine("_ => $\"{Kind.ToStringFast()}{SEPARATOR}{")
                            .Print(idField).PrintEndLine("}\",");
                    }
                    p.CloseScope("};");
                }

                p.PrintEndLine();

                foreach (var kind in KindRefs)
                {
                    if (kind.isEnum == false || kind.nestedEnumExtensions)
                    {
                        continue;
                    }

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("static string ToString_").Print(kind.name).Print("(")
                        .Print(kind.fullName).PrintEndLine(" value)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ").Print(kind.enumExtensionsName)
                            .PrintEndLine(".ToStringFast(value);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToDisplayString(ref Printer p, bool isSerializableStruct)
        {
            p.PrintLine("public readonly string ToDisplayString()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(isSerializableStruct, "return Id.ToString();", "return IdUnsigned.ToString();");
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
                                    .Print("{Kind.ToDisplayStringFast()}{SEPARATOR}{");

                                if (kind.nestedEnumExtensions)
                                {
                                    p.Print(kind.enumExtensionsName).Print(".ToDisplayStringFast(");
                                }
                                else
                                {
                                    p.Print("ToString_").Print(kind.name).Print("(");
                                }

                                p.Print("Id_").Print(kindName).PrintEndLine(")}\",");
                            }
                            else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayString))
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToDisplayStringFast()}{SEPARATOR}{Id_").Print(kindName)
                                    .PrintEndLine(".ToDisplayString()}\",");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => $\"")
                                    .Print("{Kind.ToDisplayStringFast()}{SEPARATOR}{Id_").Print(kindName)
                                    .PrintEndLine("}\",");
                            }
                        }

                        var idField = isSerializableStruct ? "Id" : "IdUnsigned";

                        p.PrintBeginLine("_ => $\"{Kind.ToDisplayStringFast()}{SEPARATOR}{")
                            .Print(idField).PrintEndLine("}\",");
                    }
                    p.CloseScope("};");

                    p.PrintEndLine();

                    foreach (var kind in KindRefs)
                    {
                        if (kind.isEnum == false || kind.nestedEnumExtensions)
                        {
                            continue;
                        }

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("static string ToString_").Print(kind.name).Print("(")
                            .Print(kind.fullName).PrintEndLine(" value)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return ").Print(kind.enumExtensionsName)
                                .PrintEndLine(".ToDisplayStringFast(value);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteToFixedString(ref Printer p, bool isSerializableStruct)
        {
            if (References.unityCollections == false || string.IsNullOrEmpty(FixedStringType))
            {
                return;
            }

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
                }
                else
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Kind.ToFixedString());");
                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, '")
                        .Print(Separator).PrintEndLine("');");
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
                                        .Print(kind.enumExtensionsName).Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToFixedString))
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName)
                                        .Print("(ref fs, Id_").Print(kindName).PrintEndLine(", false);");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLineIf(
                                  isSerializableStruct
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                            );

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

        private void WriteToDisplayFixedString(ref Printer p, bool isSerializableStruct)
        {
            if (References.unityCollections == false || string.IsNullOrEmpty(FixedStringType))
            {
                return;
            }

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
                }
                else
                {
                    p.PrintLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Kind.ToDisplayFixedString());");
                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, '")
                        .Print(Separator).PrintEndLine("');");
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
                                        .Print(kind.enumExtensionsName).Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString))
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToDisplayFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName)
                                        .Print("(ref fs, Id_").Print(kindName).PrintEndLine(", true);");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLineIf(
                                  isSerializableStruct
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                            );

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

        private void WriteGetIdStringFast(ref Printer p, bool isSerializableStruct)
        {
            p.PrintLine("public readonly string GetIdStringFast()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(isSerializableStruct, "return Id.ToString();", "return IdUnsigned.ToString();");
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
                                    .Print(kind.enumExtensionsName).Print(".ToStringFast(Id_").Print(kindName)
                                    .PrintEndLine("),");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => Id_").Print(kindName).Print(".ToString()")
                                    .PrintEndLine(",");
                            }
                        }

                        p.PrintLineIf(isSerializableStruct, "_ => Id.ToString(),", "_ => IdUnsigned.ToString(),");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdDisplayStringFast(ref Printer p, bool isSerializableStruct)
        {
            p.PrintLine("public readonly string GetIdDisplayStringFast()");
            p.OpenScope();
            {
                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(isSerializableStruct, "return Id.ToString();", "return IdUnsigned.ToString();");
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
                                    .Print(kind.enumExtensionsName).Print(".ToDisplayStringFast(Id_").Print(kindName)
                                    .PrintEndLine("),");
                            }
                            else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayString))
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => Id_").Print(kindName).Print(".ToDisplayString()")
                                    .PrintEndLine(",");
                            }
                            else
                            {
                                p.PrintBeginLine("IdKind.").Print(kindName)
                                    .Print(" => Id_").Print(kindName).Print(".ToString()")
                                    .PrintEndLine(",");
                            }
                        }

                        p.PrintLineIf(isSerializableStruct, "_ => Id.ToString(),", "_ => IdUnsigned.ToString(),");
                    }
                    p.CloseScope("};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteGetIdFixedString(ref Printer p, bool isSerializableStruct)
        {
            if (References.unityCollections == false || string.IsNullOrEmpty(FixedStringType))
            {
                return;
            }

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
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
                                        .Print(kind.enumExtensionsName).Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToFixedString))
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName)
                                        .Print("(ref fs, Id_").Print(kindName).PrintEndLine(", false);");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLineIf(
                                  isSerializableStruct
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                            );

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

        private void WriteGetIdDisplayFixedString(ref Printer p, bool isSerializableStruct)
        {
            if (References.unityCollections == false || string.IsNullOrEmpty(FixedStringType))
            {
                return;
            }

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                        , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
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
                                        .Print(kind.enumExtensionsName).Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine("));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString))
                                {
                                    p.PrintBeginLine("global::Unity.Collections.FixedStringMethods.Append(ref fs, Id_")
                                        .Print(kindName).PrintEndLine(".ToDisplayFixedString());");
                                }
                                else
                                {
                                    p.PrintBeginLine("Append_").Print(kindName)
                                        .Print("(ref fs, Id_").Print(kindName).PrintEndLine(", true);");
                                }

                                p.PrintLine("break;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLineIf(
                                  isSerializableStruct
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, Id);"
                                , "global::Unity.Collections.FixedStringMethods.Append(ref fs, IdUnsigned);"
                            );

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

        private void WriteTryGetIListNames(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetNames(")
                .PrintEndLine("IdKind kind, global::System.Collections.Generic.IList<string> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintLine("if (result is global::EncosyTower.Collections.FasterList<string> fasterList)");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("fasterList.AddRange(").Print(kind.enumExtensionsName)
                                    .PrintEndLine(".Names.AsSpan());");
                            }
                            p.CloseScope();
                            p.PrintLine("else");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("foreach (var name in ").Print(kind.enumExtensionsName)
                                    .PrintEndLine(".Names.AsSpan())");
                                p.OpenScope();
                                {
                                    p.PrintLine("result.Add(name);");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
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
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetIListDisplayNames(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetDisplayNames(")
                .PrintEndLine("IdKind kind, global::System.Collections.Generic.IList<string> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintLine("if (result is global::EncosyTower.Collections.FasterList<string> fasterList)");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("fasterList.AddRange(").Print(kind.enumExtensionsName)
                                    .PrintEndLine(".DisplayNames.AsSpan());");
                            }
                            p.CloseScope();
                            p.PrintLine("else");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("foreach (var name in ").Print(kind.enumExtensionsName)
                                    .PrintEndLine(".DisplayNames.AsSpan())");
                                p.OpenScope();
                                {
                                    p.PrintLine("result.Add(name);");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintLine("result = global::System.Array.Empty<string>();");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetNames(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetNames(")
                .PrintEndLine("IdKind kind, out global::System.ReadOnlyMemory<string> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintBeginLine("result = ").Print(kind.enumExtensionsName)
                                .PrintEndLine(".Names.AsMemory();");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintLine("result = global::System.Array.Empty<string>();");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetDisplayNames(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetDisplayNames(")
                .PrintEndLine("IdKind kind, out global::System.ReadOnlyMemory<string> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintBeginLine("result = ").Print(kind.enumExtensionsName)
                                .PrintEndLine(".DisplayNames.AsMemory();");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintLine("result = global::System.Array.Empty<string>();");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetFixedNames(ref Printer p)
        {
            if (References.unityCollections == false)
            {
                return;
            }

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetFixedNames(")
                .Print("IdKind kind, global::Unity.Collections.AllocatorManager.AllocatorHandle allocator, ")
                .Print("out global::Unity.Collections.NativeArray<").Print(FixedStringType)
                .PrintEndLine("> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintBeginLine(kind.enumExtensionsName)
                                .PrintEndLine(".FixedNames.Get(allocator, out result);");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("result = global::Unity.Collections.CollectionHelper.CreateNativeArray<")
                            .Print(FixedStringType)
                            .PrintEndLine(">(0, allocator, global::Unity.Collections.NativeArrayOptions.UninitializedMemory);");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetFixedDisplayNames(ref Printer p)
        {
            if (References.unityCollections == false)
            {
                return;
            }

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static bool TryGetFixedDisplayNames(")
                .Print("IdKind kind, global::Unity.Collections.AllocatorManager.AllocatorHandle allocator, ")
                .Print("out global::Unity.Collections.NativeArray<").Print(FixedStringType)
                .PrintEndLine("> result)");
            p.OpenScope();
            {
                p.PrintLine("switch (kind)");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintBeginLine("case IdKind.").Print(kindName).PrintEndLine(":");
                        p.OpenScope();

                        if (kind.isEnum)
                        {
                            p.PrintBeginLine(kind.enumExtensionsName)
                                .PrintEndLine(".FixedDisplayNames.Get(allocator, out result);");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("goto default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine("default:");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("result = global::Unity.Collections.CollectionHelper.CreateNativeArray<")
                            .Print(FixedStringType)
                            .PrintEndLine(">(0, allocator, global::Unity.Collections.NativeArrayOptions.UninitializedMemory);");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
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
                if (kind.isEnum)
                {
                    continue;
                }

                if (kind.toStringMethods.HasFlag(ToStringMethods.ToFixedString)
                    && kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString)
                )
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("private static partial void Append_").Print(kind.name)
                    .Print("(ref ").Print(FixedStringType).Print(" fs, ")
                    .Print(kind.fullName).PrintEndLine(" value, bool isDisplay);");
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

        private void WriteTypeConverter(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public sealed class TypeConverter : ")
                .Print("global::EncosyTower.Serialization.ParsableStructConverter<")
                .Print(typeName).PrintEndLine(">");
            p.OpenScope();
            {
                var ignoreCase = ConverterSettings.HasFlag(ParsableStructConverterSettings.IgnoreCase);
                var allowMatching = ConverterSettings.HasFlag(ParsableStructConverterSettings.AllowMatchingMetadataAttribute);

                p.PrintBeginLine("public override bool IgnoreCase => ")
                    .Print(ignoreCase).PrintEndLine(";");
                p.PrintEndLine();

                p.PrintBeginLine("public override bool AllowMatchingMetadataAttribute => ")
                    .Print(allowMatching).PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteSerializable(ref Printer p, string typeName)
        {
            p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct Serializable ")
                .Print(": global::EncosyTower.Conversion.ITryConvert<").Print(typeName).Print(">")
                .PrintEndLine();

            p.PrintLine(", global::System.IEquatable<Serializable>");
            p.PrintLine(", global::System.IComparable<Serializable>");
            p.PrintLine(", global::EncosyTower.Conversion.IToDisplayString");

            if (References.unityCollections && string.IsNullOrEmpty(FixedStringType) == false)
            {
                p.PrintBeginLine(", global::EncosyTower.Conversion.IToFixedString<").Print(FixedStringType).PrintEndLine(">");
                p.PrintBeginLine(", global::EncosyTower.Conversion.IToDisplayFixedString<").Print(FixedStringType).PrintEndLine(">");
            }

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
                p.PrintLine("public long Id;");
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
                        p.PrintLine(NON_SERIALIZED);
                        p.PrintBeginLine("public ").Print(fullName).Print(" Id_").PrintEndLine(kindName);
                        p.OpenScope();
                        {
                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("get => new ").Print(typeName).Print("(Kind, (")
                                .PrintIf(kind.signed, IdRawSignedTypeName, IdRawUnsignedTypeName)
                                .Print(")Id).Id_")
                                .Print(kindName).PrintEndLine(";");
                            p.PrintEndLine();

                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("set => Id = (long)new ").Print(typeName).Print("(value).")
                                .PrintIf(kind.signed, "IdSigned", "IdUnsigned")
                                .PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        order += 1;
                    }
                }

                p.PrintBeginLine("public Serializable").PrintEndLine("(IdKind kind, long id) : this()");
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

                        p.PrintBeginLine("public Serializable").Print("(").Print(fullName).PrintEndLine(" id) : this()");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("Kind = IdKind.").Print(kindName).PrintEndLine(";");
                            p.PrintBeginLine("Id = (long)new ").Print(typeName).Print("(id).")
                                .PrintIf(kind.signed, "IdSigned", "IdUnsigned")
                                .PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }

                WriteConstructor_IdKind_IdString(ref p, "Serializable");
                WriteConstructor_IdKindString_IdString(ref p, "Serializable");
                WriteConstructor_IdKindString_IdLong(ref p, "Serializable");
                WriteConstructor_IdKind_IdSpan(ref p, "Serializable", true);
                WriteConstructor_IdKindSpan_IdSpan(ref p, "Serializable", true);
                WriteConstructor_IdKindSpan_IdLong(ref p, "Serializable");

                p.PrintBeginLine("public readonly bool TryConvert(out ").Print(typeName).PrintEndLine(" result)");
                p.OpenScope();
                {
                    if (KindRefs.Count < 1)
                    {
                        p.PrintBeginLine("result = new((").Print(IdRawUnsignedTypeName).PrintEndLine(")Id);");
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

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly bool Equals(Serializable other)");
                p.OpenScope();
                {
                    p.PrintLine("return Kind == other.Kind && Id == other.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override bool Equals(object obj)");
                p.OpenScope();
                {
                    p.PrintLine("return obj is Serializable other && Kind == other.Kind && Id == other.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly int CompareTo(Serializable other)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ((").Print(RawTypeName).Print(")this).CompareTo((")
                        .Print(RawTypeName).PrintEndLine(")other);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly override int GetHashCode()");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ((").Print(RawTypeName).PrintEndLine(")this).GetHashCode();");
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteToString(ref p, true);
                WriteToDisplayString(ref p, true);
                WriteToFixedString(ref p, true);
                WriteToDisplayFixedString(ref p, true);
                WriteGetIdStringFast(ref p, true);
                WriteGetIdDisplayStringFast(ref p, true);
                WriteGetIdFixedString(ref p, true);
                WriteGetIdDisplayFixedString(ref p, true);

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static explicit operator ").Print(RawTypeName).PrintEndLine("(Serializable value)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return new Union { id = (").Print(IdRawUnsignedTypeName)
                        .PrintEndLine(")value.Id, kind = value.Kind }.raw;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool operator ==(Serializable left, Serializable right)");
                p.OpenScope();
                {
                    p.PrintLine("return left.Kind == right.Kind && left.Id == right.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public static bool operator !=(Serializable left, Serializable right)");
                p.OpenScope();
                {
                    p.PrintLine("return left.Kind != right.Kind || left.Id != right.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var op in s_comparerOps)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
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
                    p.PrintLine(FIELD_OFFSET, 0);
                    p.PrintBeginLine("public ").Print(RawTypeName).PrintEndLine(" raw;");
                    p.PrintEndLine();

                    p.PrintLine(FIELD_OFFSET, 0);
                    p.PrintBeginLine("public ").Print(IdRawUnsignedTypeName).Print(" id").PrintEndLine(";");
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
