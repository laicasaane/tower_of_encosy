namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    using static EncosyTower.SourceGen.Generators.DataTableAssets.Helpers;

    partial struct DataTableAssetDeclaration
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
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string NAME = nameof({className});");
                p.PrintEndLine();

                if (getIdMethodIsImplemented == false)
                {
                    p.PrintBeginLine(AGGRESSIVE_INLINING).Print(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
                    p.PrintLine($"protected override {idTypeName} GetId(in {dataTypeName} entry)");
                    p.OpenScope();
                    {
                        p.PrintLine("return entry.Id;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (initializeMethodIsImplemented == false && dataTypeImplementsIInitializable)
                {
                    p.PrintBeginLine(AGGRESSIVE_INLINING).Print(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
                    p.PrintLine($"protected override void Initialize(ref {dataTypeName} entry)");
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
                    p.PrintBeginLine(AGGRESSIVE_INLINING).Print(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
                    p.PrintLine($"protected override {convertedIdTypeName} ConvertId({idTypeName} value)");
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
