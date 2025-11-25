namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    partial struct StatDataDefinition
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.EnumExtensions.EnumExtensionsGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        private const string ISTAT_DATA = "global::EncosyTower.Entities.Stats.IStatData";
        private const string STAT_VARIANT = "global::EncosyTower.Entities.Stats.StatVariant";
        private const string STAT_VARIANT_TYPE = "global::EncosyTower.Entities.Stats.StatVariantType";
        private const string LOG_ERROR = "global::EncosyTower.Logging.StaticDevLogger.LogError";
        private const string IF_DEBUG = "#if UNITY_EDITOR || DEVELOPMENT_BUILD";
        private const string ENDIF_DEBUG = "#endif";

        public readonly string WriteCode()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("partial struct ").Print(typeName).Print(" : ").PrintEndLine(ISTAT_DATA);
                p.OpenScope();
                {
                    if (singleValue)
                    {
                        p.PrintLine(GENERATED_CODE);
                        p.PrintBeginLine("public ").Print(valueFullTypeName).PrintEndLine(" value;");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public ").Print(typeName).Print("(")
                            .Print(valueFullTypeName).PrintEndLine(" value)");
                        p.OpenScope();
                        {
                            p.PrintLine("this.value = value;");
                        }
                        p.CloseScope();
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

                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public ").Print(typeName).Print("(")
                            .Print(valueFullTypeName).PrintEndLine(" value)");
                        p.OpenScope();
                        {
                            p.PrintLine("this.baseValue = value;");
                            p.PrintLine("this.currentValue = value;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public ").Print(typeName).Print("(")
                            .Print(valueFullTypeName).Print(" baseValue, ")
                            .Print(valueFullTypeName).PrintEndLine(" currentValue)");
                        p.OpenScope();
                        {
                            p.PrintLine("this.baseValue = baseValue;");
                            p.PrintLine("this.currentValue = currentValue;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    WriteIStatData(ref p);
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteIStatData(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly bool IsValuePair");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").PrintIf(singleValue, "false", "true").PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public readonly ").Print(STAT_VARIANT_TYPE).PrintEndLine(" ValueType");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(STAT_VARIANT_TYPE).Print(".").Print(valueTypeName).PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
