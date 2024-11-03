using EncosyTower.Modules.SourceGen;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;

    partial class DataTableAssetDeclaration
    {
        public string WriteCode()
        {
            var syntax = TypeRef.Syntax;
            var idTypeName = TypeRef.IdType.ToFullName();
            var dataTypeName = TypeRef.DataType.ToFullName();

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print($"partial class ").Print(syntax.Identifier.Text)
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string NAME = nameof({syntax.Identifier.Text});");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"protected override {idTypeName} GetId(in {dataTypeName} data)");
                p.OpenScope();
                {
                    p.PrintLine($"return data.Id;");
                }
                p.CloseScope();
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}