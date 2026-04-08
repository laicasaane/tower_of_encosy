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
                    .Print(" : ").PrintEndLine(PR_DATA_SHEET_CONTAINER_BASE);
                p.OpenScope();
                {
                    foreach (var typeName in typeNames)
                    {
                        p.PrintBeginLine("public ").Print(typeName).Print(" ")
                            .Print(typeName).PrintEndLine(" { get; set; }");
                        p.PrintEndLine();
                    }

                    foreach (var assetRefList in assetRefLists)
                    {
                        var baseTypeName = $"{assetRefList.tableTypeSimpleName}_{assetRefList.dataTypeSimpleName}Sheet";
                        var fieldNames = assetRefList.fieldNames;
                        var count = fieldNames.Count;

                        p.PrintBeginLine("public RefList<").Print(baseTypeName).Print("> ")
                            .Print(baseTypeName).PrintEndLine("s");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("get => new RefList<").Print(baseTypeName).PrintEndLine(">(");
                            p = p.IncreasedIndent();
                            {
                                var lastIndex = count - 1;
                                for (var i = 0; i < count; i++)
                                {
                                    var uniqueSheet = $"{baseTypeName}__{fieldNames[i]}";
                                    p.PrintBeginLine("this.").Print(uniqueSheet);

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
                            p = p.DecreasedIndent();
                            p.PrintLine(");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
;
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
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteRefList(ref p, maxFieldOfSameTable);

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
                var uniqueSheetName = table.uniqueSheetName;
                var baseSheetName = table.baseSheetName;
                var idTypeFullName = table.idTypeFullName;
                var dataTypeFullName = table.dataTypeFullName;
                var tableTypeFullName = table.typeFullName;
                var assetName = table.assetName;

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(string.Format(PR_TABLE_NAMING, table.propertyName, table.namingStrategy));
                p.PrintLine(string.Format(PR_GENERATED_SHEET_ATTRIBUTE, idTypeFullName, dataTypeFullName, tableTypeFullName, assetName));
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class ").Print(uniqueSheetName)
                    .Print(" : ").Print(baseSheetName).PrintEndLine(" { }");
                p.PrintEndLine();
            }
        }

        private static void WriteRefList(ref Printer p, int count)
        {
            if (count < 1)
            {
                return;
            }

            const string PR_STRUCT_LAYOUT = "SRIS.StructLayout";
            const string PR_EXPLICIT = "SRIS.LayoutKind.Explicit";
            const string PR_FIXED_ARRAY = "ETC.FixedArray";
            const string PR_GC_HANDLE = "SRIS.GCHandle";
            const string PR_GC_HANDLE_WEAK = "SRIS.GCHandleType.Weak";

            p.PrintLine("public readonly struct RefList<T> where T : class");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ").Print(PR_FIXED_ARRAY)
                    .Print("<").Print(PR_GC_HANDLE).PrintEndLine(", RefListCapacity> _array;");
                p.PrintEndLine();

                p.PrintLine("private readonly int _length;");
                p.PrintEndLine();

                for (var c = 1; c <= count; c++)
                {
                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public RefList(");

                    for (var i = 0; i < c; i++)
                    {
                        p.PrintIf(i > 0, ", ").Print("T p").Print(i);
                    }

                    p.PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("_array = new ").Print(PR_FIXED_ARRAY)
                            .Print("<").Print(PR_GC_HANDLE).PrintEndLine(", RefListCapacity>(default);");
                        p.PrintEndLine();

                        p.PrintBeginLine("_length = ").Print(c).PrintEndLine(";");
                        p.PrintEndLine();

                        for (var i = 0; i < c; i++)
                        {
                            p.PrintBeginLine("_array[").Print(i).Print("] = ").Print(PR_GC_HANDLE)
                                .Print(".Alloc(p").Print(i).Print(", ").Print(PR_GC_HANDLE_WEAK).PrintEndLine(");");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintLine("get => _length;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly T this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(PR_GC_HANDLE).PrintEndLine(" item = _array[index];");
                        p.PrintLine("return item.IsAllocated ? (T)item.Target : default;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("[").Print(PR_STRUCT_LAYOUT).Print("(")
                    .Print(PR_EXPLICIT).Print(", Size = ").Print(count).PrintEndLine(" * 8)]");
                p.PrintLine("private readonly struct RefListCapacity { }");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
