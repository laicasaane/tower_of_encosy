using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial class DatabaseDeclaration
    {
        public string WriteContainer(
              Dictionary<INamedTypeSymbol, DataTableAssetRefList> assetRefListMap
            , int maxFieldOfSameTable
            , List<string> typeNames
        )
        {
            var syntax = DatabaseRef.Syntax;
            var tables = DatabaseRef.Tables;
            var databaseTypeName = syntax.Identifier.Text;
            var databaseTypeKeyword = DatabaseRef.Symbol.IsValueType ? "struct" : "class";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print($"partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print($" : {ICONTAINS}<{databaseTypeName}.SheetContainer>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(SERIALIZABLE).PrintLine(GENERATED_SHEET_CONTAINER);
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine($"public partial class SheetContainer")
                    .Print(" : ").PrintEndLine(DATA_SHEET_CONTAINER_BASE);
                p.OpenScope();
                {
                    foreach (var typeName in typeNames)
                    {
                        p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public ").Print(typeName).Print(" ")
                            .Print(typeName).PrintEndLine(" { get; set; }");
                        p.PrintEndLine();
                    }

                    foreach (var item in assetRefListMap)
                    {
                        var tableType = item.Key;
                        var dataType = item.Value.DataType;
                        var baseTypeName = GetSheetName(tableType, dataType);

                        p.PrintBeginLine("public RefList<").Print(baseTypeName).Print("> ")
                            .Print(baseTypeName).PrintEndLine("s");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("get => new RefList<").Print(baseTypeName).PrintEndLine(">(");
                            p = p.IncreasedIndent();
                            {
                                var fieldNames = item.Value.FieldNames;
                                var count = fieldNames.Count;
                                var lastIndex = count - 1;

                                for (var i = 0; i < count; i++)
                                {
                                    var typeName = GetUniqueSheetName(tableType, dataType, fieldNames[i]);
                                    p.PrintBeginLine("this.").Print(typeName);

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

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public SheetContainer()")
                        .PrintEndLine(" : this(global::Cathei.BakingSheet.Unity.UnityLogger.Default)");
                    p.PrintLine("{ }");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public SheetContainer(global::Microsoft.Extensions.Logging.ILogger logger)")
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

                WriteDerivedSheetClasses(ref p, tables);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteDerivedSheetClasses(ref Printer p, ImmutableArray<TableRef> tables)
        {
            foreach (var table in tables)
            {
                var typeName = GetUniqueSheetName(table);
                var baseTypeName = GetSheetName(table);
                var idTypeFullName = table.IdType.ToFullName();
                var dataTypeFullName = table.DataType.ToFullName();
                var tableTypeName = table.Type.ToFullName();
                var propertyName = table.PropertyName;
                var assetName = $"{table.Type.Name}_{propertyName}".ToNamingCase(table.NamingStrategy);

                p.PrintLine(SERIALIZABLE);
                p.PrintLine(string.Format(TABLE_NAMING, table.PropertyName, table.NamingStrategy));
                p.PrintLine(string.Format(GENERATED_SHEET_ATTRIBUTE, idTypeFullName, dataTypeFullName, tableTypeName, assetName));
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class ").Print(typeName)
                    .Print(" : ").Print(baseTypeName).PrintEndLine(" { }");
                p.PrintEndLine();
            }
        }

        private static void WriteRefList(ref Printer p, int count)
        {
            if (count < 1)
            {
                return;
            }

            const string STRUCT_LAYOUT = "global::System.Runtime.InteropServices.StructLayout";
            const string EXPLICIT = "global::System.Runtime.InteropServices.LayoutKind.Explicit";
            const string FIXED_ARRAY = "global::EncosyTower.Collections.FixedArray";
            const string GC_HANDLE = "global::System.Runtime.InteropServices.GCHandle";
            const string WEAK = "global::System.Runtime.InteropServices.GCHandleType.Weak";

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly struct RefList<T> where T : class");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private readonly ").Print(FIXED_ARRAY)
                    .Print("<").Print(GC_HANDLE).PrintEndLine(", RefListCapacity> _array;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE);
                p.PrintLine("private readonly int _length;");
                p.PrintEndLine();

                for (var c = 1; c <= count; c++)
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public RefList(");

                    for (var i = 0; i < c; i++)
                    {
                        if (i > 0)
                        {
                            p.Print(", ");
                        }

                        p.Print($"T p{i}");
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
                                .Print($".Alloc(p{0}, ").Print(WEAK).PrintEndLine(");");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public readonly int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _length;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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
