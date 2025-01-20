using EncosyTower.Modules.SourceGen;

namespace EncosyTower.Modules.Data.SourceGen
{
    using static EncosyTower.Modules.Data.SourceGen.Helpers;

    partial class DataTableAssetDeclaration
    {
        private const string IINITIALIZABLE = "global::EncosyTower.Modules.IInitializable";

        public string WriteCode()
        {
            var syntax = TypeRef.Syntax;
            var idTypeName = TypeRef.IdType.ToFullName();
            var dataTypeName = TypeRef.DataType.ToFullName();
            var dataTypeHasInitializeMethod = TypeRef.DataType.ImplementsInterface(IINITIALIZABLE);

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
                p.PrintLine($"protected override {idTypeName} GetId(in {dataTypeName} entry)");
                p.OpenScope();
                {
                    p.PrintLine($"return entry.Id;");
                }
                p.CloseScope();
                p.PrintEndLine();

                if (dataTypeHasInitializeMethod)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"protected override void Initialize(ref {dataTypeName} entry)");
                    p.OpenScope();
                    {
                        p.PrintLine($"entry.Initialize();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}
