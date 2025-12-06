using System;
using Microsoft.CodeAnalysis;

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
                p.PrintBeginLine("partial struct ").PrintEndLine(typeName);
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintLine("public Indices indices;");
                    p.PrintEndLine();

                    WriteIndicesStruct(ref p);


                }
                p.CloseScope();

                p.Print("#region INTERNALS").PrintEndLine();
                p.Print("#endregion ======").PrintEndLine();
                p.PrintEndLine();

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
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteIndicesStruct(ref Printer p)
        {
            var count = statDataCollection.Count;

            p.PrintLine("public partial struct Indices");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const int LENGTH = {count};");
                p.PrintEndLine();

                for (var i = 0; i < count; i++)
                {
                    var statData = statDataCollection[i];

                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public StatIndex<").Print(statData.typeName).Print("> ")
                        .Print(statData.fieldName).PrintEndLine(";");
                    p.PrintEndLine();
                }

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public ref StatIndex this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => ref AsSpan()[index];");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => LENGTH;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
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

                p.PrintBeginLine(AGGRESSIVE_INLINING).Print(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly ReadOnlySpan<StatIndex> AsReadOnlySpan()");
                p.WithIncreasedIndent().PrintLine("=> AsSpan();");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
