using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseSpec
    {
        public readonly string WriteSheet(in SheetSpec sheet)
        {
            var databaseTypeName = this.databaseTypeName;
            var databaseTypeKeyword = this.databaseTypeKeyword;
            var dataMap = new Dictionary<string, DataSpec>(allDataModels.Count, System.StringComparer.Ordinal);

            foreach (var dm in allDataModels)
            {
                dataMap[dm.fullName] = dm;
            }

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
                .Print(" : ").Print(PR_ICONTAINS).Print("<").Print(databaseTypeName).Print(".").Print(sheetName).Print(">")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .Print("public abstract partial class ").Print(sheetName)
                    .Print(" : CBS.Sheet<").Print(sheetIdTypeName).Print(", ").Print(sheetDataTypeName).Print(">")
                    .Print(", ETDBA.IDataSheet")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine("public void Initialize(CBS.SheetConvertingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnInitialize(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("partial void OnInitialize(CBS.SheetConvertingContext context);");
                    p.PrintEndLine();

                    p.PrintBeginLine("public ").Print(dataTypeFullName).Print("[] To")
                        .Print(dataTypeSimpleName).PrintEndLine("Array()");
                    p.OpenScope();
                    {
                        p.PrintLine("if (this.Items == null || this.Count == 0)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("return new ").Print(dataTypeFullName).PrintEndLine("[0];");
                        p.PrintEndLine();

                        p.PrintLine("var rows = this.Items;");
                        p.PrintLine("var count = this.Count;");
                        p.PrintBeginLine("var result = new ").Print(dataTypeFullName).PrintEndLine("[count];");
                        p.PrintEndLine();

                        p.PrintLine("for (var i = 0; i < count; i++)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result[i] = (rows[i] ?? __")
                                .Print(dataTypeSimpleName).Print(".Default).To")
                                .Print(dataTypeSimpleName).PrintEndLine("();");
                        }
                        p.CloseScope();

                        p.PrintEndLine();
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

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

                    if (hasIdTypeDeclaration && dataMap.TryGetValue(idTypeFullName, out var idDataModel))
                    {
                        idDataModel.WriteCode(
                              ref p
                            , dataMap
                            , horizontalListMap
                            , tableTypeFullName
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

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
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
