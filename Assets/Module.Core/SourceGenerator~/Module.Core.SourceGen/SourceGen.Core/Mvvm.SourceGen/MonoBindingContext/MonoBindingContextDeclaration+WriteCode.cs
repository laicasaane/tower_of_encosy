using Module.Core.SourceGen;

namespace Module.Core.Mvvm.MonoBindingContextSourceGen
{
    internal partial class MonoBindingContextDeclaration
    {
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"Module.Core.Mvvm.MonoBindingContextGenerator\", \"1.2.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public string WriteCode()
        {
            var className = Syntax.Identifier.Text;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintLine("#if UNITY_EDITOR");
            p = p.IncreasedIndent();
            p.PrintEndLine();

            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("partial class ").PrintEndLine(className);
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("private static class InspectorContextMenu");
                p.OpenScope();
                {
                    p.PrintBeginLine("[global::UnityEditor.MenuItem(\"CONTEXT/")
                        .Print(className).PrintEndLine("/Make Binding Context\")]");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("static void MakeBindingContext(global::UnityEditor.MenuCommand command)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("if (command.context is ").Print(className).PrintEndLine(" target)");
                        p.OpenScope();
                        {
                            p.PrintLine("Setup(target);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("private static global::UnityEngine.MonoBehaviour Setup(")
                        .Print(className).PrintEndLine(" target)");
                    p.OpenScope();
                    {
                        p.PrintLine("var comp = global::UnityEditor.Undo.AddComponent<global::Module.Core.Mvvm.Unity.ViewBinding.MonoBindingContext>(target.gameObject);");
                        p.PrintLine("comp.InitializeManually(target);");
                        p.PrintLine("return comp;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
            }
            p.CloseScope();

            p = p.DecreasedIndent();

            p.PrintLine("#endif");
            p.PrintEndLine();

            return p.Result;
        }
    }
}
