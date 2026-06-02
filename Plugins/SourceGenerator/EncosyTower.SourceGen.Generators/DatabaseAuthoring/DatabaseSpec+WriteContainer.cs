namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseSpec
    {
        public readonly string WriteContainer()
        {
            var p = Printer.DefaultLarge;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print("partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print(" : ").Print(PR_ICONTAINS).Print("<").Print(databaseTypeName).Print(".SheetContainer>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE).PrintLine(PR_GENERATED_SHEET_CONTAINER);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class SheetContainer")
                    .Print(" : ").Print(PR_DATA_SHEET_CONTAINER_BASE)
                    .Print(", ETDBA.IPostExportDatabase")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("public SheetContainer()")
                        .PrintEndLine(" : this(CBSU.UnityLogger.Default)");
                    p.PrintLine("{ }");
                    p.PrintEndLine();

                    p.PrintBeginLine("public SheetContainer(MEL.ILogger logger)")
                        .PrintEndLine(" : base(logger)");
                    p.OpenScope();
                    {
                        foreach (var typeName in typeNames)
                        {
                            p.PrintBeginLine("this.").Print(typeName).Print(" = new ").Print(typeName).PrintEndLine("();");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    foreach (var typeName in typeNames)
                    {
                        p.PrintBeginLine("public ").Print(typeName).Print(" ")
                            .Print(typeName).PrintEndLine(" { get; set; }");
                        p.PrintEndLine();
                    }

                    foreach (var sheetGroup in sheetGroups)
                    {
                        var baseSheetName = sheetGroup.baseSheetName;
                        var sheets = sheetGroup.sheets;
                        var count = sheets.Count;

                        p.PrintBeginLine("public SCG.List<").Print(baseSheetName).Print("> ")
                            .Print(baseSheetName).PrintEndLine("s");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("get => new SCG.List<").Print(baseSheetName)
                                .Print(">(").Print(count).PrintEndLine(")");
                            p.OpenScope();
                            {
                                var lastIndex = count - 1;

                                for (var i = 0; i < count; i++)
                                {
                                    var asset = sheets[i];

                                    p.PrintBeginLine("this.").Print(asset.tableName).Print("_")
                                        .Print(baseSheetName).Print("_").Print(asset.propertyName);

                                    if (i < lastIndex)
                                    {
                                        p.PrintEndLine(",");
                                    }
                                    else
                                    {
                                        p.PrintEndLine();
                                    }
                                }
                            }
                            p.CloseScope("};");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("ETDBA.IPostExportDatabase.PostExport(ETDBA.DatabaseExportingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("public void PostExport(ETDBA.DatabaseExportingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnPostExport(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("ETDBA.IPostExportDatabase.PostExport(ETDBA.DatabaseExportingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("partial void OnPostExport(ETDBA.DatabaseExportingContext context);");
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#else").PrintEndLine().PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE).PrintLine(PR_GENERATED_SHEET_CONTAINER);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintLine("public partial class SheetContainer { }");
                p.PrintEndLine();

                p.Print("#endif").PrintEndLine().PrintEndLine();

                WriteDerivedSheetClasses(ref p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private readonly void WriteDerivedSheetClasses(ref Printer p)
        {
            foreach (var table in tables)
            {
                var tableTypeName = table.typeSimpleName;
                var propertyName = table.propertyName;
                var nameCasing = table.nameCasing;
                var idTypeFullName = table.idTypeFullName;
                var dataTypeFullName = table.dataTypeFullName;
                var tableTypeFullName = table.typeFullName;
                var assetName = table.deduplicateAssetName
                    ? nameCasing.ConvertName($"{tableTypeName}_{propertyName}")
                    : nameCasing.ConvertName(tableTypeName);

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(string.Format(PR_TABLE_NAMING, table.propertyName, table.nameCasing));
                p.PrintLine(string.Format(PR_GENERATED_SHEET_ATTRIBUTE, idTypeFullName, dataTypeFullName, tableTypeFullName, assetName));
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class ").Print(table.uniqueSheetName)
                    .Print(" : ").Print(table.baseSheetName).PrintEndLine(" { }");
                p.PrintEndLine();
            }
        }
    }
}
