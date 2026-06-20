namespace EncosyTower.SourceGen.Generators.UnionIds
{
    partial class UnionIdDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.UnionIds.UnionIdGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string STRUCT_LAYOUT_SIZE = "[SRIS.StructLayout(SRIS.LayoutKind.Explicit, Size = {0})]";
        private const string STRUCT_LAYOUT = "[SRIS.StructLayout(SRIS.LayoutKind.Explicit)]";
        private const string FIELD_OFFSET = "[SRIS.FieldOffset({0})]";
        private const string UNION = "[SRCS.Union]";
        private const string ODIN_PROPERTY_ORDER = "[global::Sirenix.OdinInspector.PropertyOrder({0})]";
        private const string ODIN_SHOW_IN_INSPECTOR = "[global::Sirenix.OdinInspector.ShowInInspector]";
        private const string ODIN_SHOW_IF = "[global::Sirenix.OdinInspector.ShowIf(nameof(Kind), IdKind.{0})]";
        private const string ODIN_LABEL = "[global::Sirenix.OdinInspector.LabelText(\"{0}\")]";
        private const string INSPECTOR_NAME = "[UE.InspectorName(\"{0}\")]";
        private const string DESCRIPTION = "[SCM.Description(\"{0}\")]";
        private const string SERIALIZABLE = "[S.Serializable]";
        private const string NON_SERIALIZED = "[field: S.NonSerialized]";
        private const string SERIALIZE_FIELD = "[UE.SerializeField]";
        private const string VALIDATION_ATTRIBUTES = "[UE.HideInCallstack, SD.StackTraceHidden, " +
            "SD.Conditional(\"UNITY_EDITOR\"), SD.Conditional(\"DEVELOPMENT_BUILD\")]";

        private readonly static string[] s_operators = new[] { "==", "!=", "<", "<=", ">", ">=" };
        private readonly static string[] s_comparerOps = new[] { "<", "<=", ">", ">=" };
        private readonly static (string, string)[] s_equalityOps = new[] { ("==", ""), ("!=", "!") };

        public string WriteCode()
        {
            var typeName = SimpleName;

            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine(UNION);
            p.PrintLine(string.Format(STRUCT_LAYOUT_SIZE, TypeSize));
            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintLine("[SCM.TypeConverter(typeof(TypeConverter))]");
            p.PrintBeginLine("partial struct ").Print(typeName).Print(" : ETUI.IUnionId<")
                .Print(RawTypeName).Print(", ").Print(typeName)
                .Print(">, SRCS.IUnion, ET.IHasValue")
                .PrintEndLine();

            p = p.IncreasedIndent();
            {
                if (References.unityCollections && string.IsNullOrEmpty(FixedStringType) == false)
                {
                    p.PrintBeginLine(", ETCon.IToFixedString").PrintEndLine();
                    p.PrintBeginLine(", ETCon.IToDisplayFixedString").PrintEndLine();
                    p.PrintBeginLine(", ETCon.IToFixedString<").Print(FixedStringType).PrintEndLine(">");
                    p.PrintBeginLine(", ETCon.IToDisplayFixedString<").Print(FixedStringType).PrintEndLine(">");
                    p.PrintBeginLine(", ETCon.ITryParse<").Print(typeName).PrintEndLine(">");
                    p.PrintBeginLine(", ETCon.ITryParseSpan<").Print(typeName).PrintEndLine(">");
                }

                foreach (var kind in KindRefs)
                {
                    if (kind.equality.Strategy != EqualityStrategy.Default)
                    {
                        p.PrintBeginLine(", S.IEquatable<").Print(kind.fullName).PrintEndLine(">");
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
                WriteUnionPattern(ref p, typeName);
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
                WriteTryFormat(ref p);
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
                WriteIsCastableMethods(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // IdKind");
            p.OpenScope();
            {
                WriteIdKindEnum(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // Serializable");
            p.OpenScope();
            {
                WriteSerializable(ref p, typeName);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("partial struct ").Print(typeName).PrintEndLine(" // TypeConverter");
            p.OpenScope();
            {
                WriteTypeConverter(ref p, typeName);
            }
            p.CloseScope();
            p.PrintEndLine();

            WriteIdEnumExtensions(ref p, typeName);

            p.PrintBeginLine("partial struct ").Print(typeName).Print(" { }")
                .PrintEndLine(" // IdKindExtensions");
            p.PrintEndLine();
            {
                KindExtensionsRef.WriteCode(ref p);
            }

            p.PrintBeginLine("partial struct ").Print(typeName).Print(" { }")
                .Print(" // ").Print(typeName).PrintEndLine("Enumeration");
            p.PrintEndLine();
            {
                WriteEnumeration(ref p);
            }

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
                        var kindFullName = kind.fullName;

                        p.PrintLine(FIELD_OFFSET, 0);
                        p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                        p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                        p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                        p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                        p.PrintLineIf(this.References.odin, ODIN_SHOW_IF, (object)kindName);
                        p.PrintBeginLine("public readonly ").Print(kindFullName).Print(" Id_").Print(kindName).PrintEndLine(";");
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

                p.PrintBeginLine("public ").Print(typeName).Print("(").Print(kind.fullName).PrintEndLine(" id) : this()");
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
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(typeName)
                    .Print("(").Print(kind.fullName).PrintEndLine(" id)");
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
                .PrintEndLine(" : this(kind, S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
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
                .PrintEndLine(" : this(S.MemoryExtensions.AsSpan(kind), S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)");
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
                .PrintEndLine(" : this(S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
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
                .PrintEndLine(" : this(S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
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
                .PrintEndLine(" : this(S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)");
            p.OpenScope();
            {
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructor_IdKind_IdSpan(ref Printer p, string typeName, bool isSerializableStruct)
        {
            p.PrintBeginLine("public ").Print(typeName).Print("(IdKind kind")
                .Print(", S.ReadOnlySpan<char> id")
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
                            var kindFullName = kind.fullName;

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
                                            p.PrintBeginLineIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
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
            p.PrintBeginLine("public ").Print(typeName).Print("(S.ReadOnlySpan<char> kind")
                .Print(", S.ReadOnlySpan<char> id")
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
                            var kindFullName = kind.fullName;

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
                                            p.PrintBeginLineIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLineIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
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
            p.PrintBeginLine("public ").Print(typeName).Print("(S.ReadOnlySpan<char> kind")
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
            p.PrintBeginLine("public ").Print(typeName).Print("(S.ReadOnlySpan<char> kind, ")
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
            p.PrintBeginLine("public ").Print(typeName).Print("(S.ReadOnlySpan<char> kind, ")
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

        private void WriteUnionPattern(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public object").PrintIf(EnableNullable, "?").PrintEndLine(" Value");
            p.OpenScope();
            {
                p.PrintLine("get => Kind switch");
                p.OpenScope();
                {
                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;
                        p.PrintBeginLine("IdKind.").Print(kindName).Print(" => Id_").Print(kindName).PrintEndLine(",");
                    }

                    p.PrintBeginLine("_ => null").PrintEndLine(",");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public bool HasValue");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(KindExtensionsRef.ExtensionsName).PrintEndLine(".IsDefined(Kind);");
            }
            p.CloseScope();
            p.PrintEndLine();

            foreach (var kind in KindRefs)
            {
                var fullName = kind.fullName;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static explicit operator ").Print(fullName)
                    .Print("(").Print(typeName).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return value.GetValueOrThrow(ET.GenericT.T<")
                        .Print(fullName).PrintEndLine(">());");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            foreach (var kind in KindRefs)
            {
                var fullName = kind.fullName;
                var name = kind.name;

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(fullName)
                    .Print(" GetValueOrThrow(ET.T<").Print(fullName).PrintEndLine("> _)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ThrowIfUncastable(Kind, IdKind.").Print(name).PrintEndLine(");");
                    p.PrintEndLine();

                    p.PrintBeginLine("return Id_").Print(name).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(fullName)
                    .Print(" GetValueOrDefault(ET.T<").Print(fullName).Print("> _ = default, ")
                    .Print(fullName).PrintEndLine(" @default = default)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (IsCastable(Kind, IdKind.").Print(name).PrintEndLine("))");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return Id_").Print(name).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return @default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public bool TryGetValue(out ").Print(fullName).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (Kind == IdKind.").Print(name).PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("value = Id_").Print(name).PrintEndLine(";");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("value = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
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
                    .Print("(S.ReadOnlySpan<char> str, out ").Print(kind.fullName)
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
                p.PrintLine("return TryParse(S.MemoryExtensions.AsSpan(str), out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_Span(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> str, out ")
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

                p.PrintLine("var ranges = ETSE.SpanAPI.Split(str, SEPARATOR, 2);");
                p.PrintEndLine();

                p.PrintLine("S.Range? kindRange = default;");
                p.PrintLine("S.Range? idRange = default;");
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
                p.PrintLine("return TryParse(kind, S.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);");
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
                p.PrintLine("return TryParse(S.MemoryExtensions.AsSpan(kind), S.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);");
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
                p.PrintLine("return TryParse(S.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);");
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
                p.PrintLine("return TryParse(S.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryParse_IdKind_IdSpan(ref Printer p, string typeName)
        {
            p.PrintBeginLine("public bool TryParse(IdKind kind, S.ReadOnlySpan<char> id, out ")
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
                            var kindFullName = kind.fullName;

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
                                                .PrintIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
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
                                        p.PrintBeginLine("result = new((").Print(kindFullName).PrintEndLine(")idValue);");
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
            p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> kind, S.ReadOnlySpan<char> id, out ")
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
                            var kindFullName = kind.fullName;

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
                                                .PrintIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
                                                .PrintEndLine(".TryParse(id, out var idValue);");
                                        }
                                        else if (tryParse.ParamCount == 4)
                                        {
                                            p.PrintBeginLine("var idResult = ")
                                                .PrintIf(tryParse.IsStatic, kindFullName, $"default({kindFullName})")
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
                                        p.PrintBeginLine("result = new((").Print(kindFullName).PrintEndLine(")idValue);");
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
            p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> kind, ")
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
            p.PrintBeginLine("public bool TryParse(S.ReadOnlySpan<char> kind, ")
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

        private void WriteTryFormat(ref Printer p)
        {
            if (GenerateTryFormat == false)
            {
                return;
            }

            p.PrintLine("public bool TryFormat(S.Span<char> destination, out int charsWritten)");
            p.OpenScope();
            {
                if (References.unityCollections)
                {
                    p.PrintBeginLine("if (ETCol.EncosyFixedStringExtensions.TryFormat(")
                        .PrintEndLine("Kind.ToFixedString(), destination, out var kindCharsWritten) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("charsWritten = 0;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("if (ETCol.EncosyFixedStringExtensions.TryFormat(")
                        .Print("GetIdFixedString(), destination[..kindCharsWritten]")
                        .PrintEndLine(", out var idCharsWritten) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("charsWritten = 0;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("charsWritten = kindCharsWritten + idCharsWritten;");
                    p.PrintLine("return true;");
                }
                else
                {
                    p.PrintLine("var kind = S.MemoryExtensions.AsSpan(Kind.ToStringFast());");
                    p.PrintLine("var id = S.MemoryExtensions.AsSpan(GetIdStringFast());");
                    p.PrintEndLine();

                    p.PrintLine("if (kind.TryCopyTo(destination) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("charsWritten = 0;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("if (id.TryCopyTo(destination[..kind.Length]) == false)");
                    p.OpenScope();
                    {
                        p.PrintLine("charsWritten = 0;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("charsWritten = kind.Length + id.Length;");
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
                var kindFullName = kind.fullName;
                var kindFullNameFromNullable = kind.fullNameFromNullable;
                var equality = kind.equality;

                if (equality.Strategy == EqualityStrategy.Default)
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool Equals(").Print(kindFullName).PrintEndLine(" other)");
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
                                p.Print(" && ").Print(kindFullNameFromNullable)
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
                                p.Print(" && ").Print(kindFullName)
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
                            .Print(" && SCG.EqualityComparer<").Print(kindFullNameFromNullable)
                            .Print(">.Default.Equals(Id_").Print(kindName).PrintEndLine(".Value, other.Value);");
                    }
                    else
                    {
                        p.PrintBeginLine("return Kind == IdKind.").Print(kindName)
                            .Print(" && SCG.EqualityComparer<").Print(kindFullName)
                            .Print(">.Default.Equals(Id_").Print(kindName).PrintEndLine(", other);");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var (op1, op2) in s_equalityOps)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(typeName).Print(" left, ").Print(kindFullName).PrintEndLine(" right)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ").Print(op2).PrintEndLine("left.Equals(right);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(kindFullName).Print(" left, ").Print(typeName).PrintEndLine(" right)");
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
            var kindExtensionsName = KindExtensionsRef.ExtensionsName;

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
                                    .Print(" => $\"{")
                                    .Print(kindExtensionsName).Print(".Names.").Print(kindName)
                                    .Print("}{SEPARATOR}{");

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
                                    .Print(" => $\"{")
                                    .Print(kindExtensionsName).Print(".Names.").Print(kindName)
                                    .Print("}{SEPARATOR}{Id_").Print(kindName)
                                    .PrintEndLine("}\",");
                            }
                        }

                        var idField = isSerializableStruct ? "Id" : "IdUnsigned";

                        p.PrintBeginLine("_ => $\"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{")
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
                            .PrintEndLine(".ToStringFast(value, false);");
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
            var kindExtensionsName = KindExtensionsRef.ExtensionsName;

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
                                    .Print(" => $\"{")
                                    .Print(kindExtensionsName).Print(".DisplayNames.").Print(kindName)
                                    .Print("}{SEPARATOR}{");

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
                                    .Print(" => $\"{")
                                    .Print(kindExtensionsName).Print(".DisplayNames.").Print(kindName)
                                    .Print("}{SEPARATOR}{Id_").Print(kindName)
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

                        p.PrintBeginLine("_ => $\"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{")
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
                                .PrintEndLine(".ToDisplayStringFast(value, false);");
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

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly TFixedString ToFixedString<TFixedString>()");
            p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                .PrintEndLine("UC.IUTF8Bytes");
            p.WithIncreasedIndent().PrintBeginLine("=> ETCol.EncosyFixedStringExtensions")
                .PrintEndLine(".CastTo<TFixedString>(ToFixedString());");
            p.PrintEndLine();

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "UC.FixedStringMethods.Append(ref fs, Id);"
                        , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
                }
                else
                {
                    p.PrintLine("UC.FixedStringMethods.Append(ref fs, Kind.ToFixedString(false));");
                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, '")
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
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, ")
                                        .Print(kind.enumExtensionsName).Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine(", false));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToFixedString))
                                {
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, Id_")
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
                                , "UC.FixedStringMethods.Append(ref fs, Id);"
                                , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public TFixedString ToDisplayFixedString<TFixedString>()");
            p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                .PrintEndLine("UC.IUTF8Bytes");
            p.WithIncreasedIndent().PrintBeginLine("=> ETCol.EncosyFixedStringExtensions")
                .PrintEndLine(".CastTo<TFixedString>(ToDisplayFixedString());");
            p.PrintEndLine();

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" ToDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "UC.FixedStringMethods.Append(ref fs, Id);"
                        , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
                    );
                }
                else
                {
                    p.PrintLine("UC.FixedStringMethods.Append(ref fs, Kind.ToDisplayFixedString(false));");
                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, '")
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
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, ")
                                        .Print(kind.enumExtensionsName).Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine(", false));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString))
                                {
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, Id_")
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
                                , "UC.FixedStringMethods.Append(ref fs, Id);"
                                , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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
                                    .PrintEndLine(", false),");
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
                                    .PrintEndLine(", false),");
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

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly TFixedString GetIdFixedString<TFixedString>()");
            p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                .PrintEndLine("UC.IUTF8Bytes");
            p.OpenScope();
            {
                p.PrintLine("TFixedString result = default;");
                p.PrintLine("UC.FixedStringMethods.Append(ref result, GetIdFixedString());");
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "UC.FixedStringMethods.Append(ref fs, Id);"
                        , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, ")
                                        .Print(kind.enumExtensionsName).Print(".ToFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine(", false));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToFixedString))
                                {
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, Id_")
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
                                , "UC.FixedStringMethods.Append(ref fs, Id);"
                                , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly TFixedString GetIdDisplayFixedString<TFixedString>()");
            p.WithIncreasedIndent().PrintBeginLine("where TFixedString : unmanaged, UC.INativeList<byte>, ")
                .PrintEndLine("UC.IUTF8Bytes");
            p.OpenScope();
            {
                p.PrintLine("TFixedString result = default;");
                p.PrintLine("UC.FixedStringMethods.Append(ref result, GetIdDisplayFixedString());");
                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public readonly ").Print(FixedStringType).PrintEndLine(" GetIdDisplayFixedString()");
            p.OpenScope();
            {
                p.PrintBeginLine("var fs = new ").Print(FixedStringType).PrintEndLine("();");

                if (KindRefs.Count < 1)
                {
                    p.PrintLineIf(
                          isSerializableStruct
                        , "UC.FixedStringMethods.Append(ref fs, Id);"
                        , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, ")
                                        .Print(kind.enumExtensionsName).Print(".ToDisplayFixedString(Id_")
                                        .Print(kindName)
                                        .PrintEndLine(", false));");
                                }
                                else if (kind.toStringMethods.HasFlag(ToStringMethods.ToDisplayFixedString))
                                {
                                    p.PrintBeginLine("UC.FixedStringMethods.Append(ref fs, Id_")
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
                                , "UC.FixedStringMethods.Append(ref fs, Id);"
                                , "UC.FixedStringMethods.Append(ref fs, IdUnsigned);"
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
                .PrintEndLine("IdKind kind, SCG.ICollection<string> result)");
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
                            p.PrintBeginLine("ETColE.EncosyICollectionExtensions.AddRangeFast(result, ")
                                .Print(kind.enumExtensionsName)
                                .PrintEndLine(".Names.AsSpan());");

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
                .PrintEndLine("IdKind kind, SCG.ICollection<string> result)");
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
                            p.PrintBeginLine("ETColE.EncosyICollectionExtensions.AddRangeFast(result, ")
                                .Print(kind.enumExtensionsName)
                                .PrintEndLine(".DisplayNames.AsSpan());");

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
                        p.PrintLine("result = S.Array.Empty<string>();");
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
                .PrintEndLine("IdKind kind, out S.ReadOnlyMemory<string> result)");
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
                        p.PrintLine("result = S.Array.Empty<string>();");
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
                .PrintEndLine("IdKind kind, out S.ReadOnlyMemory<string> result)");
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
                        p.PrintLine("result = S.Array.Empty<string>();");
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
                .Print("IdKind kind, UC.AllocatorManager.AllocatorHandle allocator, ")
                .Print("out UC.NativeArray<").Print(FixedStringType)
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
                            p.PrintBeginLine("result = ").Print(kind.enumExtensionsName)
                                .Print(".FixedNames.ToNativeArray<").Print(FixedStringType)
                                .PrintEndLine(">(allocator);");
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
                        p.PrintBeginLine("result = UC.CollectionHelper.CreateNativeArray<")
                            .Print(FixedStringType)
                            .PrintEndLine(">(0, allocator, UC.NativeArrayOptions.UninitializedMemory);");
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
                .Print("IdKind kind, UC.AllocatorManager.AllocatorHandle allocator, ")
                .Print("out UC.NativeArray<").Print(FixedStringType)
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
                            p.PrintBeginLine("result = ").Print(kind.enumExtensionsName)
                                .Print(".FixedDisplayNames.ToNativeArray<").Print(FixedStringType)
                                .PrintEndLine(">(allocator);");
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
                        p.PrintBeginLine("result = UC.CollectionHelper.CreateNativeArray<")
                            .Print(FixedStringType)
                            .PrintEndLine(">(0, allocator, UC.NativeArrayOptions.UninitializedMemory);");
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

        private void WriteIsCastableMethods(ref Printer p)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("private static bool IsCastable(IdKind a, IdKind b)");
            p.OpenScope();
            {
                p.PrintLine("return a == b;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine("private static void ThrowIfUncastable")
                .PrintEndLine("(IdKind source, IdKind target)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCastable(source, target) == false)");
                p.OpenScope();
                {
                    p.PrintLine("throw new S.InvalidCastException(");
                    p.WithIncreasedIndent().PrintBeginLine("$\"")
                        .Print("Cannot cast '").Print(SimpleName).Print("' into '{target}' ")
                        .Print("because it currently stores a '{source}'.")
                        .PrintEndLine("\"");
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
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
                .Print("ETS.ParsableStructConverter<")
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
            p.PrintLine(UNION);
            p.PrintLine(SERIALIZABLE);
            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintBeginLine("public partial struct Serializable ")
                .Print(": SRCS.IUnion, ETUI.ISerializableUnionId<")
                .Print(RawTypeName).Print(", ").Print(typeName).Print(", Serializable")
                .PrintEndLine(">");

            if (References.unityCollections && string.IsNullOrEmpty(FixedStringType) == false)
            {
                p.PrintBeginLine(", ETCon.IToFixedString").PrintEndLine();
                p.PrintBeginLine(", ETCon.IToDisplayFixedString").PrintEndLine();
                p.PrintBeginLine(", ETCon.IToFixedString<").Print(FixedStringType).PrintEndLine(">");
                p.PrintBeginLine(", ETCon.IToDisplayFixedString<").Print(FixedStringType).PrintEndLine(">");
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

                        p.PrintBeginLine("public Serializable").Print("(").Print(kind.fullName).PrintEndLine(" id) : this()");
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

                if (KindRefs.Count > 0)
                {
                    var order = 1;

                    var label = DisplayNameForId;
                    var hasLabel = string.IsNullOrEmpty(label) == false;

                    foreach (var kind in KindRefs)
                    {
                        var kindName = kind.name;

                        p.PrintLineIf(hasLabel, DESCRIPTION, (object)label);
                        p.PrintLineIf(hasLabel && this.References.odin, ODIN_LABEL, (object)label);
                        p.PrintLineIf(References.odin, ODIN_SHOW_IN_INSPECTOR);
                        p.PrintLineIf(References.odin, ODIN_PROPERTY_ORDER, order);
                        p.PrintLineIf(this.References.odin, ODIN_SHOW_IF, (object)kindName);
                        p.PrintLine(NON_SERIALIZED);
                        p.PrintBeginLine("public ").Print(kind.fullName).Print(" Id_").PrintEndLine(kindName);
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

                WriteUnionPattern(ref p, "Serializable");

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

        private void WriteIdEnumExtensions(ref Printer p, string typeName)
        {
            foreach (var extensions in IdEnumExtensionsRefs)
            {
                p.PrintBeginLine("partial struct ").Print(typeName)
                    .Print(" // ").PrintEndLine(extensions.ExtensionsName);
                p.OpenScope();
                {
                    extensions.WriteCode(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteEnumeration(ref Printer p)
        {
            var typeName = SimpleName;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
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
            var typeName = SimpleName;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static void CopyTo(S.Span<")
                .Print(typeName)
                .PrintEndLine("> dest)");
            p.OpenScope();
            {
                p.PrintBeginLine("if (dest.Length < Length) ")
                    .PrintEndLine("throw new S.ArgumentOutOfRangeException(nameof(dest));");
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
            var typeName = SimpleName;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static bool TryCopyTo(S.Span<")
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
            var typeName = SimpleName;
            var idEnums = IdEnumExtensionsRefs;
            var idEnumsCount = idEnums.Count;

            p.PrintBeginLine("public static void AddTo<TCollection>([SDCA.NotNull] ")
                .PrintEndLine("TCollection dest)");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("where TCollection : SCG.ICollection<")
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
