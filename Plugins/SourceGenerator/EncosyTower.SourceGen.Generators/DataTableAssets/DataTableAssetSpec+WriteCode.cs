namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    using static EncosyTower.SourceGen.Generators.DataTableAssets.Helpers;

    partial struct DataTableAssetSpec
    {
        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print("partial class ").Print(className);
            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(PR_GENERATED_CODE);
                p.PrintLine($"public const string NAME = nameof({className});");
                p.PrintEndLine();

                if (getIdMethodIsImplemented == false)
                {
                    p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                    p.PrintLine($"protected sealed override {idTypeName} GetId(in {dataTypeName} entry)");
                    p.OpenScope();
                    {
                        p.PrintLine("return entry.Id;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (initializeMethodIsImplemented == false && dataTypeImplementsIInitializable)
                {
                    p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                    p.PrintLine($"protected sealed override void Initialize(ref {dataTypeName} entry)");
                    p.OpenScope();
                    {
                        p.PrintLine("entry.Initialize();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (convertIdMethodIsImplemented == false
                    && convertedIdTypeName != null
                    && convertExpression != null
                )
                {
                    p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                    p.PrintLine($"protected sealed override {convertedIdTypeName} ConvertId({idTypeName} value)");
                    p.OpenScope();
                    {
                        p.PrintLine($"return {convertExpression};");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            return p.Result;
        }
    }
}
