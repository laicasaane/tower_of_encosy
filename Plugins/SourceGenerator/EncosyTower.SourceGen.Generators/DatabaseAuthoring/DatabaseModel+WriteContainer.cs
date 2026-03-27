namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseModel
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
                .Print($" : {ICONTAINS}<{databaseTypeName}.SheetContainer>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_SHEET_CONTAINER);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class SheetContainer")
                    .Print(" : ").PrintEndLine(DATA_SHEET_CONTAINER_BASE);
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
                        .PrintEndLine(" : this(UnityLogger.Default)");
                    p.PrintLine("{ }");
                    p.PrintEndLine();

                    p.PrintBeginLine("public SheetContainer(MELogging.ILogger logger)")
                        .PrintEndLine(" : base(logger)");
                    p.OpenScope();
                    {
                        foreach (var typeName in typeNames)
                        {
                            p.PrintLine($"this.{typeName} = new {typeName}();");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteRefList(ref p, maxFieldOfSameTable);

                p.Print("#else").PrintEndLine().PrintEndLine();

                p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_SHEET_CONTAINER);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(SERIALIZABLE);
                p.PrintLine(string.Format(TABLE_NAMING, table.propertyName, table.namingStrategy));
                p.PrintLine(string.Format(GENERATED_SHEET_ATTRIBUTE, idTypeFullName, dataTypeFullName, tableTypeFullName, assetName));
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

            const string STRUCT_LAYOUT = "StructLayout";
            const string EXPLICIT = "LayoutKind.Explicit";
            const string FIXED_ARRAY = "FixedArray";
            const string GC_HANDLE = "GCHandle";
            const string WEAK = "GCHandleType.Weak";

            p.PrintLine("public readonly struct RefList<T> where T : class");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ").Print(FIXED_ARRAY)
                    .Print("<").Print(GC_HANDLE).PrintEndLine(", RefListCapacity> _array;");
                p.PrintEndLine();

                p.PrintLine("private readonly int _length;");
                p.PrintEndLine();

                for (var c = 1; c <= count; c++)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public RefList(");

                    for (var i = 0; i < c; i++)
                    {
                        p.PrintIf(i > 0, ", ").Print($"T p{i}");
                    }

                    p.PrintEndLine(")");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("_array = new ").Print(FIXED_ARRAY)
                            .Print("<").Print(GC_HANDLE).PrintEndLine(", RefListCapacity>(default);");
                        p.PrintEndLine();

                        p.PrintLine($"_length = {c};");
                        p.PrintEndLine();

                        for (var i = 0; i < c; i++)
                        {
                            p.PrintBeginLine($"_array[{i}] = ").Print(GC_HANDLE)
                                .Print($".Alloc(p{i}, ").Print(WEAK).PrintEndLine(");");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _length;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public readonly T this[int index]");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        p.PrintBeginLine(GC_HANDLE).PrintEndLine(" item = _array[index];");
                        p.PrintLine("return item.IsAllocated ? (T)item.Target : default;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine("[").Print(STRUCT_LAYOUT).Print("(")
                    .Print(EXPLICIT).PrintEndLine($", Size = {count} * 8)]");
                p.PrintLine("private readonly struct RefListCapacity { }");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
