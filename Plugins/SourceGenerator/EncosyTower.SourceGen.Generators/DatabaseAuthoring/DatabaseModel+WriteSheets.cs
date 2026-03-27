using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseModel
    {
        public readonly string WriteSheet(in SheetModel sheet)
        {
            var databaseTypeName = this.databaseTypeName;
            var databaseTypeKeyword = this.databaseTypeKeyword;

            // Rebuild dataMap (string-keyed) from allDataModels
            var dataMap = new Dictionary<string, DataModel>(allDataModels.Count, System.StringComparer.Ordinal);

            foreach (var dm in allDataModels)
            {
                dataMap[dm.fullName] = dm;
            }

            // Rebuild horizontalListMap from horizontalListEntries
            var horizontalListMap = new Dictionary<string, Dictionary<string, HashSet<string>>>(System.StringComparer.Ordinal);

            foreach (var entry in horizontalListEntries)
            {
                if (horizontalListMap.TryGetValue(entry.targetTypeFullName, out var innerMap) == false)
                {
                    horizontalListMap[entry.targetTypeFullName] = innerMap = new(System.StringComparer.Ordinal);
                }

                var propertyNames = new HashSet<string>(System.StringComparer.Ordinal);

                foreach (var pn in entry.propertyNames)
                {
                    propertyNames.Add(pn);
                }

                innerMap[entry.containingTypeFullName] = propertyNames;
            }

            var sheetName = sheet.sheetName;
            var idTypeFullName = sheet.idTypeFullName;
            var idTypeSimpleName = sheet.idTypeSimpleName;
            var dataTypeFullName = sheet.dataTypeFullName;
            var dataTypeSimpleName = sheet.dataTypeSimpleName;
            var tableTypeFullName = sheet.tableTypeFullName;

            var hasIdTypeDeclaration = dataMap.ContainsKey(idTypeFullName);
            var sheetIdTypeName = hasIdTypeDeclaration
                ? $"{sheetName}.__{idTypeSimpleName}"
                : idTypeFullName;
            var sheetDataTypeName = $"{sheetName}.__{dataTypeSimpleName}";

            var p = Printer.DefaultLarge;
            p = p.IncreasedIndent();
            p.PrintEndLine();

            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print("partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print($" : {ICONTAINS}<{databaseTypeName}.{sheetName}>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(SERIALIZABLE);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .Print($"public abstract partial class {sheetName}")
                    .Print($" : Sheet<{sheetIdTypeName}, {sheetDataTypeName}>")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine($"public {dataTypeFullName}[] To{dataTypeSimpleName}Array()");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (this.Items == null || this.Count == 0)");
                        p = p.IncreasedIndent();
                        p.PrintLine($"return new {dataTypeFullName}[0];");
                        p = p.DecreasedIndent();
                        p.PrintEndLine();

                        p.PrintLine("var rows = this.Items;");
                        p.PrintLine("var count = this.Count;");
                        p.PrintLine($"var result = new {dataTypeFullName}[count];");
                        p.PrintEndLine();

                        p.PrintLine("for (var i = 0; i < count; i++)");
                        p.OpenScope();
                        {
                            p.PrintLine($"result[i] = (rows[i] ?? __{dataTypeSimpleName}.Default).To{dataTypeSimpleName}();");
                        }
                        p.CloseScope();

                        p.PrintEndLine();
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    if (hasIdTypeDeclaration && dataMap.TryGetValue(idTypeFullName, out var idDataModel))
                    {
                        idDataModel.WriteCode(
                              ref p
                            , dataMap
                            , horizontalListMap
                            , tableTypeFullName
                        );
                    }

                    if (dataMap.TryGetValue(dataTypeFullName, out var dataModel))
                    {
                        dataModel.WriteCode(
                              ref p
                            , dataMap
                            , horizontalListMap
                            , tableTypeFullName
                            , idTypeFullName
                        );
                    }

                    foreach (var nestedFullName in sheet.nestedDataTypeFullNames)
                    {
                        if (dataMap.TryGetValue(nestedFullName, out var nestedModel))
                        {
                            nestedModel.WriteCode(
                                  ref p
                                , dataMap
                                , horizontalListMap
                                , tableTypeFullName
                            );
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#else").PrintEndLine().PrintEndLine();

                p.PrintLine(SERIALIZABLE);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public abstract partial class ").Print(sheetName).PrintEndLine(" { }");
                p.PrintEndLine();

                p.Print("#endif").PrintEndLine().PrintEndLine();
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}
