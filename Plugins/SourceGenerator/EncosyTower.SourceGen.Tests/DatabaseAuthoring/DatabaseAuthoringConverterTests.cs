using EncosyTower.SourceGen.Analyzers.DatabaseAuthoring;
using EncosyTower.SourceGen.Tests.Databases;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.DatabaseAuthoring;

[TestClass]
public class DatabaseAuthoringConverterTests
{
    private const string STUB_ATTRIBUTES = DatabaseAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Data;\n    using EncosyTower.Data.Authoring;\n    using EncosyTower.Databases;\n    using EncosyTower.Databases.Authoring;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DatabaseAuthoringDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = Wrap(body),
        };

        foreach (var diag in expected)
        {
            test.ExpectedDiagnostics.Add(diag);
        }

        return test.RunAsync();
    }

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
    {
        var test = new CSharpAnalyzerTest<DatabaseAuthoringDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<DatabaseAuthoringDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ValidMemberConverter_NoDiagnostics()
        => RunAsync("""
                public class IntStringConv { public string Convert(int x) => string.Empty; }

                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData>
                {
                    [DataAuthoringConverter(typeof(IntStringConv))]
                    public string Foo { get; set; }
                }

                [Database]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);

    [TestMethod]
    public Task StaticConvertWrongReturnType_OnMember_ReportsInvalidStaticConvertMethodReturnType()
        => RunAsync(
              """
                  public class BadStaticReturn { public static string Convert(int x) => string.Empty; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData>
                  {
                      [{|#0:DataAuthoringConverter(typeof(BadStaticReturn))|}]
                      public int Foo { get; set; }
                  }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidStaticConvertMethodReturnType)
                .WithLocation(0)
                .WithArguments("BadStaticReturn", "int")
        );

    [TestMethod]
    public Task InstanceConvertWrongReturnType_OnMember_ReportsInvalidInstancedConvertMethodReturnType()
        => RunAsync(
              """
                  public class BadInstanceReturn { public string Convert(int x) => string.Empty; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData>
                  {
                      [{|#0:DataAuthoringConverter(typeof(BadInstanceReturn))|}]
                      public int Foo { get; set; }
                  }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidInstancedConvertMethodReturnType)
                .WithLocation(0)
                .WithArguments("BadInstanceReturn", "int")
        );

    [TestMethod]
    public Task StaticVoidConvert_InArray_ReportsInvalidStaticConvertMethod()
        => RunAsync(
              """
                  public class BadStaticVoid { public static void Convert(int x) { } }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(BadStaticVoid))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidStaticConvertMethod)
                .WithLocation(0)
                .WithArguments("BadStaticVoid", "")
        );

    [TestMethod]
    public Task InstanceVoidConvert_InArray_ReportsInvalidInstancedConvertMethod()
        => RunAsync(
              """
                  public class BadInstanceVoid { public void Convert(int x) { } }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(BadInstanceVoid))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidInstancedConvertMethod)
                .WithLocation(0)
                .WithArguments("BadInstanceVoid", "")
        );

    [TestMethod]
    public Task NullDataAuthoringConverterArg_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData>
                  {
                      [{|#0:DataAuthoringConverter(null)|}]
                      public int Foo { get; set; }
                  }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.NotTypeOfExpression)
                .WithLocation(0)
        );

    [TestMethod]
    public Task NullInDatabaseArray_ReportsNotTypeOfExpressionAt()
        => RunAsync(
              """
                  public class GoodConv { public int Convert(string s) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(GoodConv), null)|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.NotTypeOfExpressionAt)
                .WithLocation(0)
                .WithArguments(1)
        );

    [TestMethod]
    public Task AbstractConverter_ReportsAbstractTypeNotSupported()
        => RunAsync(
              """
                  public abstract class AbstractConv { public int Convert(int x) => x; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(AbstractConv))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.AbstractTypeNotSupported)
                .WithLocation(0)
                .WithArguments("AbstractConv")
        );

    [TestMethod]
    public Task UnboundGenericConverter_ReportsOpenGenericTypeNotSupported()
        => RunAsync(
              """
                  public class GenConv<T> { public int Convert(T t) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(GenConv<>))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.OpenGenericTypeNotSupported)
                .WithLocation(0)
                .WithArguments("GenConv")
        );

    [TestMethod]
    public Task TwoConvertersSameReturnType_ReportsConverterAmbiguity()
        => RunAsync(
              """
                  public class C1 { public int Convert(string s) => 0; }
                  public class C2 { public int Convert(double d) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(C1), typeof(C2))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.ConverterAmbiguity)
                .WithLocation(0)
                .WithArguments("C2", "C1", "int", 1)
        );

    [TestMethod]
    public Task TwoConvertersDifferentReturnTypes_NoDiagnostics()
        => RunAsync("""
                public class C1 { public int Convert(string s) => 0; }
                public class C2 { public string Convert(double d) => string.Empty; }

                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData> { }

                [Database(typeof(C1), typeof(C2))]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);

    [TestMethod]
    public Task ConflictingDataPropertyConverters_ReportsDuplicate()
        => RunAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }
                  public class ConvB { public int Convert(double d) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [{|#0:ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))|}]
                  [{|#1:ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvB))|}]
                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.DuplicateDataPropertyConverter)
                .WithLocation(0)
                .WithArguments("Foo", "global::TestProject.MyData", "all tables")
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.DuplicateDataPropertyConverter)
                .WithLocation(1)
                .WithArguments("Foo", "global::TestProject.MyData", "all tables")
        );

    [TestMethod]
    public Task RedundantDataPropertyConverter_ReportsRedundant()
        => RunAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [{|#0:ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))|}]
                  [{|#1:ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))|}]
                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.RedundantDataPropertyConverter)
                .WithLocation(0)
                .WithArguments("global::TestProject.ConvA", "Foo", "global::TestProject.MyData", "all tables")
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.RedundantDataPropertyConverter)
                .WithLocation(1)
                .WithArguments("global::TestProject.ConvA", "Foo", "global::TestProject.MyData", "all tables")
        );

    [TestMethod]
    public Task ConflictingTableConverters_SameSourceType_ReportsDuplicate()
        => RunAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }
                  public class ConvB { public int Convert(string s) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [{|#0:ConverterForTable("Items", typeof(ConvA))|}]
                  [{|#1:ConverterForTable("Items", typeof(ConvB))|}]
                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.DuplicateTableConverter)
                .WithLocation(0)
                .WithArguments("string", "table \"Items\"")
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.DuplicateTableConverter)
                .WithLocation(1)
                .WithArguments("string", "table \"Items\"")
        );

    [TestMethod]
    public Task RedundantTableConverter_ByNameAndByType_ReportsRedundant()
        => RunAsync(
              """
                  public class ConvA { public int Convert(string s) => 0; }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [{|#0:ConverterForTable("Items", typeof(ConvA))|}]
                  [{|#1:ConverterForTable(typeof(MyTable), typeof(ConvA))|}]
                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.RedundantTableConverter)
                .WithLocation(0)
                .WithArguments("global::TestProject.ConvA", "string", "table \"Items\"")
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.RedundantTableConverter)
                .WithLocation(1)
                .WithArguments("global::TestProject.ConvA", "string", "table \"Items\"")
        );

    [TestMethod]
    public Task TableScopedDataPropertyConverter_DoesNotConflictWithUnscoped()
        => RunAsync("""
                public class ConvA { public int Convert(string s) => 0; }
                public class ConvB { public int Convert(double d) => 0; }

                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData> { }

                [Database]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvA))]
                [ConverterForDataProperty(typeof(MyData), "Foo", typeof(ConvB), "Items")]
                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);
}
