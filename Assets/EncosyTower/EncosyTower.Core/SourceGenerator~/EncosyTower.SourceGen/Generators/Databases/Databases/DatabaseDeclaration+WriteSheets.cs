using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    partial class DatabaseDeclaration
    {
        public string WriteSheet(
              DataTableAssetRef dataTableAssetRef
            , DataDeclaration dataTypeDeclaration
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , ITypeSymbol objectType
        )
        {
            var idType = dataTableAssetRef.IdType;
            var dataType = dataTableAssetRef.DataType;
            var tableType = dataTableAssetRef.TableType;
            var idTypeFullName = idType.ToFullName();
            var nestedDataTypeFullNames = dataTableAssetRef.NestedDataTypes;
            var horizontalListMap = DatabaseRef.HorizontalListMap;
            var databaseClassName = DatabaseRef.Syntax.Identifier.Text;
            var databaseTypeKeyword = DatabaseRef.Symbol.IsValueType ? "struct" : "class";

            var sheetName = GetSheetName(tableType, dataType);
            var sheetDataTypeName = $"{sheetName}.__{dataType.Name}";
            var sheetIdTypeName = dataMap.TryGetValue(idType, out var idTypeDeclaration)
                ? $"{sheetName}.__{idType.Name}"
                : idTypeFullName;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, DatabaseRef.Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();
            p.PrintEndLine();

            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.Print(DIRECTIVE).PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print($"partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseClassName)
                .Print($" : {ICONTAINS}<{databaseClassName}.{sheetName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(SERIALIZABLE);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .Print($"public abstract partial class {sheetName}")
                    .Print($" : global::Cathei.BakingSheet.Sheet<{sheetIdTypeName}, {sheetDataTypeName}>")
                    .PrintEndLine();
                p.OpenScope();
                {
                    var typeFullName = dataTypeDeclaration.FullName;
                    var typeName = dataTypeDeclaration.Symbol.Name;

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {typeFullName}[] To{typeName}Array()");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (this.Items == null || this.Count == 0)");
                        p = p.IncreasedIndent();
                        p.PrintLine($"return new {typeFullName}[0];");
                        p = p.DecreasedIndent();
                        p.PrintEndLine();

                        p.PrintLine("var rows = this.Items;");
                        p.PrintLine("var count = this.Count;");
                        p.PrintLine($"var result = new {typeFullName}[count];");
                        p.PrintEndLine();

                        p.PrintLine("for (var i = 0; i < count; i++)");
                        p.OpenScope();
                        {
                            p.PrintLine($"result[i] = (rows[i] ?? __{typeName}.Default).To{typeName}();");
                        }
                        p.CloseScope();

                        p.PrintEndLine();
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    idTypeDeclaration?.WriteCode(
                          ref p
                        , dataMap
                        , horizontalListMap
                        , tableType
                        , objectType
                        , idType: null
                    );

                    dataTypeDeclaration.WriteCode(
                          ref p
                        , dataMap
                        , horizontalListMap
                        , tableType
                        , objectType
                        , idTypeDeclaration?.Symbol ?? idType
                    );

                    foreach (var nestedFullName in nestedDataTypeFullNames)
                    {
                        if (dataMap.TryGetValue(nestedFullName, out var nestedDataDeclaration))
                        {
                            nestedDataDeclaration?.WriteCode(
                                  ref p
                                , dataMap
                                , horizontalListMap
                                , tableType
                                , objectType
                                , idType: null
                            );
                        }
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();

            p.Print("#endif").PrintEndLine().PrintEndLine();
            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}
