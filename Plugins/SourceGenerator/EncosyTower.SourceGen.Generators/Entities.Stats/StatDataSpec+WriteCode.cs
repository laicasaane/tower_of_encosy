namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatDataSpec
    {
        private const string PR_AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Entities.Stats.StatDataGenerator\", \"{SourceGenVersion.VALUE}\")]";

        private const string PR_ISTAT_DATA = "ETES.IStatData";
        private const string PR_STAT_VARIANT = "ETES.StatVariant";
        private const string PR_STAT_VARIANT_TYPE = "ETES.StatVariantType";

        private const string PR_LOG_ERROR = "UnityDebug.LogError";
        private const string PR_IF_DEBUG = "#if UNITY_EDITOR || DEVELOPMENT_BUILD";
        private const string PR_ENDIF_DEBUG = "#endif";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(PR_GENERATED_CODE).PrintEndLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial struct ").Print(typeName).Print(" : ").PrintEndLine(PR_ISTAT_DATA);
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
                p.PrintBeginLine("public ")
                    .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                    .Print(valueType).PrintEndLine(" value;");
                p.PrintEndLine();
            }
            else
            {
                p.PrintBeginLine("public ")
                    .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                    .Print(valueType).PrintEndLine(" baseValue;");
                p.PrintEndLine();

                p.PrintBeginLine("public ")
                    .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                    .Print(valueType).PrintEndLine(" currentValue;");
                p.PrintEndLine();
            }
        }

        private readonly void WriteConstructors(ref Printer p)
        {
            var @in = size > 8 ? "in " : "";

            p.PrintLine(PR_AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName).Print("(").Print(@in)
                .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                .Print(valueType).PrintEndLine(" value) : this()");
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

            p.PrintLine(PR_AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").Print(typeName).Print("(").Print(@in)
                .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                .Print(valueType).Print(" baseValue, ").Print(@in)
                .PrintIf(HasCustomNs, valueTypeNs).PrintIf(HasCustomNs, ".")
                .Print(valueType).PrintEndLine(" currentValue) : this()");
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
            p.PrintLine("public readonly bool IsValuePair");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").PrintIf(singleValue, "false", "true").PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public readonly ").Print(PR_STAT_VARIANT_TYPE).PrintEndLine(" ValueType");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(PR_STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(PR_STAT_VARIANT).PrintEndLine(" BaseValue");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
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

                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintLine("set");
                p.OpenScope();
                {
                    p.Print(PR_IF_DEBUG).PrintEndLine();
                    p.PrintBeginLine("if (value.Type == ")
                        .Print(PR_STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(")");
                    p.Print(PR_ENDIF_DEBUG).PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").PrintIf(singleValue, "value", "baseValue")
                            .Print(" = ");

                        if (isEnum)
                        {
                            p.Print("(").PrintIf(HasCustomNs, valueTypeNs).Print(valueType).Print(")");
                        }

                        p.Print("value.").Print(valueTypeName)
                            .PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.Print(PR_IF_DEBUG).PrintEndLine();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(PR_LOG_ERROR).Print("($\"The setter of '").Print(typeName).Print(".BaseValue' ")
                            .Print("expects a value of type '").Print(PR_STAT_VARIANT_TYPE).Print(".").Print(valueTypeName)
                            .Print("' but receives a value of type '{").Print("value.Type").PrintEndLine("}'\");");
                    }
                    p.CloseScope();
                    p.Print(PR_ENDIF_DEBUG).PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(PR_STAT_VARIANT).PrintEndLine(" CurrentValue");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintLine("readonly get");
                p.OpenScope();
                {
                    p.PrintBeginLine("return ");

                    if (isEnum)
                    {
                        p.Print("(").Print(underlyingTypeName).Print(")");
                    }

                    p.Print("this.").PrintIf(singleValue, "value", "currentValue").PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintLine("set");
                p.OpenScope();
                {
                    p.Print(PR_IF_DEBUG).PrintEndLine();
                    p.PrintBeginLine("if (value.Type == ")
                        .Print(PR_STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(")");
                    p.Print(PR_ENDIF_DEBUG).PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").PrintIf(singleValue, "value", "currentValue")
                            .Print(" = ");

                        if (isEnum)
                        {
                            p.Print("(").PrintIf(HasCustomNs, valueTypeNs).Print(valueType).Print(")");
                        }

                        p.Print("value.").Print(valueTypeName)
                            .PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.Print(PR_IF_DEBUG).PrintEndLine();
                    p.PrintLine("else");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(PR_LOG_ERROR).Print("($\"The setter of '").Print(typeName).Print(".CurrentValue' ")
                            .Print("expects a value of type '").Print(PR_STAT_VARIANT_TYPE).Print(".").Print(valueTypeName)
                            .Print("' but receives a value of type '{").Print("value.Type").PrintEndLine("}'\");");
                    }
                    p.CloseScope();
                    p.Print(PR_ENDIF_DEBUG).PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
