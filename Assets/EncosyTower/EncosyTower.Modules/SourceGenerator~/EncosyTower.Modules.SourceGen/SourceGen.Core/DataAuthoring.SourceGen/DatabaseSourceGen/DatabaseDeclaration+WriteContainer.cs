using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    using static EncosyTower.Modules.DataAuthoring.SourceGen.Helpers;

    partial class DatabaseDeclaration
    {
        public string WriteContainer(
              Dictionary<ITypeSymbol, DataTableAssetRef> dataTableAssetRefMap
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var syntax = DatabaseRef.Syntax;
            var tables = DatabaseRef.Tables;
            var containsTables = tables.Length > 0 && dataTableAssetRefMap != null && dataMap != null;
            var databaseClassName = syntax.Identifier.Text;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, DatabaseRef.Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print($"partial class ").Print(databaseClassName)
                .Print($" : global::EncosyTower.Modules.Data.Authoring.SourceGen.IContains<{databaseClassName}.SheetContainer>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine("[global::System.Serializable]");
                p.PrintLine("[global::EncosyTower.Modules.Data.Authoring.SourceGen.GeneratedSheetContainer]");
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public partial class SheetContainer : global::EncosyTower.Modules.Data.Authoring.DataSheetContainerBase");
                p.OpenScope();
                {
                    if (containsTables)
                    {
                        foreach (var table in tables)
                        {
                            if (dataTableAssetRefMap.TryGetValue(table.Type, out var dataTableAssetRef) == false)
                            {
                                continue;
                            }

                            var dataType = dataTableAssetRef.DataType;

                            if (dataMap.ContainsKey(dataType) == false)
                            {
                                continue;
                            }

                            var typeName = GetSheetName(table, dataType);
                            var sheetName = typeName;

                            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                            p.PrintLine($"public {typeName} {sheetName} {{ get; private set; }}");
                            p.PrintEndLine();
                        }
                    }

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public SheetContainer(global::Microsoft.Extensions.Logging.ILogger logger) : base(logger)");
                    p.OpenScope();
                    {
                        if (containsTables)
                        {
                            foreach (var table in tables)
                            {
                                if (dataTableAssetRefMap.TryGetValue(table.Type, out var dataTableAssetRef) == false)
                                {
                                    continue;
                                }

                                var dataType = dataTableAssetRef.DataType;

                                if (dataMap.ContainsKey(dataType) == false)
                                {
                                    continue;
                                }

                                var typeName = GetSheetName(table, dataType);
                                var sheetName = typeName;

                                p.PrintLine($"this.{sheetName} = new {typeName}();");
                            }
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static string GetSheetName(TableRef table, ITypeSymbol dataType)
            => $"{table.Type.Name}_{dataType.Name}Sheet";
    }
}
