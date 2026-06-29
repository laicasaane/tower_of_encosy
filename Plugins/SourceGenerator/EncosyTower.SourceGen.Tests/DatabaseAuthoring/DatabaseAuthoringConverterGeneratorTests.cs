using EncosyTower.SourceGen.Generators.DatabaseAuthoring;
using EncosyTower.SourceGen.Tests.Databases;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.DatabaseAuthoring;

[TestClass]
public class DatabaseAuthoringConverterGeneratorTests
{
    private const string STUB_ATTRIBUTES = DatabaseAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Data;\n    using EncosyTower.Databases;\n    using EncosyTower.Databases.Authoring;\n{body}\n}}\n";

    private static string[] Run(string body)
        => GeneratorTestHelper.RunDriverAndGetGeneratedSources<DatabaseAuthoringGenerator>(
              Wrap(body)
            , GeneratorTestHelper.CoreReferences
        );

    // Same data type used by two tables that resolve different converters for the same member.
    // Option B: each diverging scope must get its own abstract base sheet (LocSheet_0 / LocSheet_1).
    [TestMethod]
    public void DivergentTableConverters_SplitIntoSeparateSheets()
    {
        var combined = string.Join("\n\n", Run("""
                public struct ConvA { public static int[] Convert(string s) => null; }
                public struct ConvB { public static int[] Convert(string s) => null; }

                [Data]
                public partial struct Loc : IData
                {
                    [DataProperty] public int Id { get; set; }
                    [DataProperty(typeof(int[]))] public int[] Points { get; set; }
                }

                public partial class LocTableA : DataTableAsset<int, Loc> { }
                public partial class LocTableB : DataTableAsset<int, Loc> { }

                [Database]
                public partial struct GameDb
                {
                    [Table] public LocTableA TableA { get; }
                    [Table] public LocTableB TableB { get; }
                }

                [AuthorDatabase(typeof(GameDb))]
                [ConverterForTable(typeof(LocTableA), typeof(ConvA))]
                [ConverterForTable(typeof(LocTableB), typeof(ConvB))]
                public partial struct GameDbAuthoring { }
            """));

        StringAssert.Contains(combined, "LocSheet_0");
        StringAssert.Contains(combined, "LocSheet_1");
        StringAssert.Contains(combined, "ConvA.Convert");
        StringAssert.Contains(combined, "ConvB.Convert");
    }

    // Same data type used by two tables that resolve the SAME converter must keep sharing one sheet.
    [TestMethod]
    public void IdenticalTableConverters_ShareSingleSheet()
    {
        var combined = string.Join("\n\n", Run("""
                public struct ConvA { public static int[] Convert(string s) => null; }

                [Data]
                public partial struct Loc : IData
                {
                    [DataProperty] public int Id { get; set; }
                    [DataProperty(typeof(int[]))] public int[] Points { get; set; }
                }

                public partial class LocTableA : DataTableAsset<int, Loc> { }
                public partial class LocTableB : DataTableAsset<int, Loc> { }

                [Database]
                public partial struct GameDb
                {
                    [Table] public LocTableA TableA { get; }
                    [Table] public LocTableB TableB { get; }
                }

                [AuthorDatabase(typeof(GameDb))]
                [ConverterForTable(typeof(LocTableA), typeof(ConvA))]
                [ConverterForTable(typeof(LocTableB), typeof(ConvA))]
                public partial struct GameDbAuthoring { }
            """));

        StringAssert.Contains(combined, "LocSheet");
        Assert.IsFalse(combined.Contains("LocSheet_0"), "Non-divergent tables must not split into per-scope sheets");
    }

    // #1 ConverterForDataProperty must win over #2 ConverterForTable for the same target member.
    [TestMethod]
    public void ConverterForDataProperty_TakesPrecedenceOverConverterForTable()
    {
        var combined = string.Join("\n\n", Run("""
                public struct ConvTable { public static int[] Convert(string s) => null; }
                public struct ConvProp { public static int[] Convert(double d) => null; }

                [Data]
                public partial struct Loc : IData
                {
                    [DataProperty] public int Id { get; set; }
                    [DataProperty(typeof(int[]))] public int[] Points { get; set; }
                }

                public partial class LocTable : DataTableAsset<int, Loc> { }

                [Database]
                public partial struct GameDb
                {
                    [Table] public LocTable Items { get; }
                }

                [AuthorDatabase(typeof(GameDb))]
                [ConverterForDataProperty(typeof(Loc), "Points", typeof(ConvProp))]
                [ConverterForTable(typeof(LocTable), typeof(ConvTable))]
                public partial struct GameDbAuthoring { }
            """));

        StringAssert.Contains(combined, "ConvProp.Convert");
        Assert.IsFalse(combined.Contains("ConvTable.Convert"), "ConverterForTable must be overridden by ConverterForDataProperty");
    }

    // The documented example places the authoring type in a different assembly from the database it targets.
    [TestMethod]
    public void CrossAssembly_AuthoringResolvesConverters()
    {
        var databaseSource = $$"""
            {{STUB_ATTRIBUTES}}
            namespace DbProject
            {
                using EncosyTower.Data;
                using EncosyTower.Databases;

                [Data]
                public partial struct Loc : IData
                {
                    [DataProperty] public int Id { get; set; }
                    [DataProperty(typeof(int[]))] public int[] Points { get; set; }
                }

                public struct ConvA { public static int[] Convert(string s) => null; }

                public partial class LocTable : DataTableAsset<int, Loc> { }

                [Database]
                public partial struct GameDb
                {
                    [Table] public LocTable Items { get; }
                }
            }
            """;

        var databaseReference = GeneratorTestHelper.CompileToReference(
              databaseSource
            , GeneratorTestHelper.CoreReferences
            , "DbProject"
        );

        var references = new System.Collections.Generic.List<Microsoft.CodeAnalysis.MetadataReference>(GeneratorTestHelper.CoreReferences)
        {
            databaseReference,
        };

        var authoringSource = """
            namespace AuthProject
            {
                using EncosyTower.Databases.Authoring;

                [AuthorDatabase(typeof(DbProject.GameDb))]
                [ConverterForTable(typeof(DbProject.LocTable), typeof(DbProject.ConvA))]
                public partial struct GameDbAuthoring { }
            }
            """;

        var sources = GeneratorTestHelper.RunDriverAndGetGeneratedSources<DatabaseAuthoringGenerator>(
              authoringSource
            , references
        );

        var combined = string.Join("\n\n", sources);

        StringAssert.Contains(combined, "LocSheet");
        StringAssert.Contains(combined, "ConvA.Convert");
    }
}
