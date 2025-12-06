namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatDataDefinition
    {
        private const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Entities.Stats.StatDataGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";

        private const string ISTAT_DATA = $"IStatData";
        private const string STAT_VARIANT = $"StatVariant";
        private const string STAT_VARIANT_TYPE = $"StatVariantType";

        private const string LOG_ERROR = "StaticDevLogger.LogError";
        private const string IF_DEBUG = "#if UNITY_EDITOR || DEVELOPMENT_BUILD";
        private const string ENDIF_DEBUG = "#endif";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("partial struct ").Print(typeName).Print(" : ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    WriteFields(ref p);
                    WriteConstructors(ref p);
                    WriteIStatData(ref p);

                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteFields(ref Printer p)
        {
            if (singleValue)
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public ").Print(valueFullTypeName).PrintEndLine(" value;");
                p.PrintEndLine();
            }
            else
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public ").Print(valueFullTypeName).PrintEndLine(" baseValue;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public ").Print(valueFullTypeName).PrintEndLine(" currentValue;");
                p.PrintEndLine();
            }
        }

        private readonly void WriteConstructors(ref Printer p)
        {
            var @in = size > 8 ? "in " : "";

            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName).Print("(")
                .Print(@in).Print(valueFullTypeName).PrintEndLine(" value) : this()");
            p.OpenScope();
            {
                p.PrintLineIf(
                      singleValue
                    , "this.value = value;"
                    , "this.baseValue = this.currentValue = value;"
                );
            }
            p.CloseScope();
            p.PrintEndLine();

            if (singleValue)
            {
                return;
            }

            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(typeName).Print("(")
                .Print(@in).Print(valueFullTypeName).Print(" baseValue, ")
                .Print(@in).Print(valueFullTypeName).PrintEndLine(" currentValue) : this()");
            p.OpenScope();
            {
                p.PrintLine("this.baseValue = baseValue;");
                p.PrintLine("this.currentValue = currentValue;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteIStatData(ref Printer p)
        {
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly bool IsValuePair");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").PrintIf(singleValue, "false", "true").PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(STAT_VARIANT_TYPE).PrintEndLine(" ValueType");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(STAT_VARIANT).PrintEndLine(" BaseValue");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("readonly get");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ")
                        .PrintIf(isEnum, "(")
                        .PrintIf(isEnum, underlyingTypeName)
                        .PrintIf(isEnum, ")")
                        .Print("this.").PrintIf(singleValue, "value", "baseValue").PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("set");
                p.OpenScope();
                {
                    p.Print(IF_DEBUG).PrintEndLine();
                    p.PrintBeginLine("if (value.Type == ")
                        .Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(")");
                    p.Print(ENDIF_DEBUG).PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").PrintIf(singleValue, "value", "baseValue")
                            .Print(" = ")
                            .PrintIf(isEnum, "(")
                            .PrintIf(isEnum, valueFullTypeName)
                            .PrintIf(isEnum, ")")
                            .Print("value.").Print(valueTypeName)
                            .PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.Print(IF_DEBUG).PrintEndLine();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(LOG_ERROR).Print("($\"The setter of '").Print(typeName).Print(".BaseValue' ")
                            .Print("expects a value of type '").Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName)
                            .Print("' but receives a value of type '{").Print("value.Type").PrintEndLine("}'\");");
                    }
                    p.CloseScope();
                    p.Print(ENDIF_DEBUG).PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(STAT_VARIANT).PrintEndLine(" CurrentValue");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("readonly get");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ")
                        .PrintIf(isEnum, "(")
                        .PrintIf(isEnum, underlyingTypeName)
                        .PrintIf(isEnum, ")")
                        .Print("this.").PrintIf(singleValue, "value", "currentValue").PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("set");
                p.OpenScope();
                {
                    p.Print(IF_DEBUG).PrintEndLine();
                    p.PrintBeginLine("if (value.Type == ")
                        .Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(")");
                    p.Print(ENDIF_DEBUG).PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").PrintIf(singleValue, "value", "currentValue")
                            .Print(" = ")
                            .PrintIf(isEnum, "(")
                            .PrintIf(isEnum, valueFullTypeName)
                            .PrintIf(isEnum, ")")
                            .Print("value.").Print(valueTypeName)
                            .PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.Print(IF_DEBUG).PrintEndLine();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(LOG_ERROR).Print("($\"The setter of '").Print(typeName).Print(".CurrentValue' ")
                            .Print("expects a value of type '").Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName)
                            .Print("' but receives a value of type '{").Print("value.Type").PrintEndLine("}'\");");
                    }
                    p.CloseScope();
                    p.Print(ENDIF_DEBUG).PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteStatIndexXmlDoc(ref Printer p, string typeName)
        {
            p.PrintBeginLine("/// ").PrintEndLine("<summary>");
            p.PrintBeginLine("/// ").PrintEndLine("The index associated with a specific DynamicBuffer&lt;Stat&gt;.");
            p.PrintBeginLine("/// ").PrintEndLine("<br/>");
            p.PrintBeginLine("/// ").Print("It shoud be assigned upon the creation of a Stat&lt;").Print(typeName).PrintEndLine("&gt;.");
            p.PrintBeginLine("/// ").PrintEndLine("</summary>");
            p.PrintBeginLine("/// ").PrintEndLine("<remarks>");
            p.PrintBeginLine("/// ").PrintEndLine("A valid index should be greater than 0.");
            p.PrintBeginLine("/// ").PrintEndLine("Index 0 equals to the first element in DynamicBuffer&lt;Stat&gt;");
            p.PrintBeginLine("/// ").PrintEndLine("whose type is <see cref=\"StatVariantType.None\"/>.");
            p.PrintBeginLine("/// ").PrintEndLine("<br/>");
            p.PrintBeginLine("/// ").Print("Use <see cref=\"MakeStatHandleFor\"/> ").PrintEndLine("to compose a <see cref=\"StatHandle{T}\"/> ");
            p.PrintBeginLine("/// ").Print("which associates <see cref=\"").Print(typeName).PrintEndLine("\"/> with the <paramref name=\"entity\"/>.");
            p.PrintBeginLine("/// ").PrintEndLine("</remarks>");
        }
    }
}
