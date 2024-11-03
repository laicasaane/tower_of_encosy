using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    using static EncosyTower.Modules.DataAuthoring.SourceGen.Helpers;

    partial class DatabaseDeclaration
    {
        public const string GENERATED_SHEET_ATTRIBUTE = "[global::EncosyTower.Modules.Data.Authoring.SourceGen.GeneratedSheet(typeof({0}), typeof({1}), typeof({2}))]";

        public string WriteSheet(
              TableRef table
            , DataTableAssetRef dataTableAssetRef
            , DataDeclaration dataTypeDeclaration
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , ITypeSymbol objectType
        )
        {
            var idType = dataTableAssetRef.IdType;
            var dataType = dataTableAssetRef.DataType;
            var dataTableAssetType = dataTableAssetRef.Symbol;
            var idTypeFullName = idType.ToFullName();
            var dataTypeFullName = dataType.ToFullName();
            var dataTableAssetTypeName = dataTableAssetType.ToFullName();
            var nestedDataTypeFullNames = dataTableAssetRef.NestedDataTypes;
            var horizontalListMap = DatabaseRef.HorizontalListMap;
            var databaseClassName = DatabaseRef.Syntax.Identifier.Text;

            var sheetName = GetSheetName(table, dataType);
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

            p.PrintBeginLine()
                .Print($"partial class ").Print(databaseClassName)
                .Print($" : global::EncosyTower.Modules.Data.Authoring.SourceGen.IContains<{databaseClassName}.{sheetName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine()
                    .Print($"[global::EncosyTower.Modules.Data.Authoring.SourceGen.SheetNamingAttribute(")
                    .Print($"\"{table.SheetName}\"")
                    .Print($", global::EncosyTower.Modules.Data.Authoring.NamingStrategy.{table.NamingStrategy}")
                    .Print(")]")
                    .PrintEndLine();

                p.PrintLine("[global::System.Serializable]");
                p.PrintLine(string.Format(GENERATED_SHEET_ATTRIBUTE, idTypeFullName, dataTypeFullName, dataTableAssetTypeName));
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .Print($"public partial class {sheetName}")
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
                        , dataTableAssetType
                        , objectType
                        , idType: null
                    );

                    dataTypeDeclaration.WriteCode(
                          ref p
                        , dataMap
                        , horizontalListMap
                        , dataTableAssetType
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
                                , dataTableAssetType
                                , objectType
                                , idType: null
                            );
                        }
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}
