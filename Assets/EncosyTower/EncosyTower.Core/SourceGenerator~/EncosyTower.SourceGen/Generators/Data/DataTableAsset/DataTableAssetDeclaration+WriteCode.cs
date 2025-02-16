using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Data.DataTableAssets
{
    using static EncosyTower.SourceGen.Generators.Data.Helpers;

    partial class DataTableAssetDeclaration
    {
        private const string IINITIALIZABLE = "global::EncosyTower.Common.IInitializable";

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

                if (GetIdMethodIsImplemented == false)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"protected override {idTypeName} GetId(in {dataTypeName} entry)");
                    p.OpenScope();
                    {
                        p.PrintLine($"return entry.Id;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (InitializeMethodIsImplemented == false && dataTypeHasInitializeMethod)
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

                if (ConvertIdMethodIsImplemented == false
                    && TypeRef.ConvertedIdType != null
                )
                {
                    var convertedIdTypeName = TypeRef.ConvertedIdType.ToFullName();
                    var result = GetConvertExpression(TypeRef.IdType, TypeRef.ConvertedIdType, convertedIdTypeName);

                    if (string.IsNullOrEmpty(result))
                    {
                        result = GetConvertExpression(TypeRef.ConvertedIdType, TypeRef.IdType, convertedIdTypeName);
                    }

                    if (string.IsNullOrEmpty(result) == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintLine($"protected override {convertedIdTypeName} ConvertId({idTypeName} value)");
                        p.OpenScope();
                        {
                            p.PrintLine($"return {result};");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static string GetConvertExpression(ITypeSymbol from, ITypeSymbol to, string typeName)
        {
            var members = from.GetMembers();
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.IsStatic == false
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (comparer.Equals(method.ReturnType, to) == false
                    || comparer.Equals(parameters[0].Type, from) == false
                )
                {
                    continue;
                }

                if (method.Name == "op_Implicit")
                {
                    return "value";
                }

                if (method.Name == "op_Explicit")
                {
                    return $"({typeName})value";
                }
            }

            return string.Empty;
        }
    }
}
