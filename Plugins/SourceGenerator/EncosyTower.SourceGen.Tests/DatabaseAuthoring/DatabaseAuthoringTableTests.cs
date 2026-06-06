using EncosyTower.SourceGen.Analyzers.DatabaseAuthoring;
using EncosyTower.SourceGen.Tests.Databases;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.DatabaseAuthoring;

[TestClass]
public class DatabaseAuthoringTableTests
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
    public Task ValidConverter_NoDiagnostics()
        => RunAsync("""
                public class IntConv { public int Convert(string s) => 0; }

                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData> { }

                [Database(typeof(IntConv))]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);

    [TestMethod]
    public Task PrivateCtorConverter_ReportsMissingDefaultConstructor()
        => RunAsync(
              """
                  public class PrivateCtorConv
                  {
                      private PrivateCtorConv() { }
                      public int Convert(int x) => x;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(PrivateCtorConv))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingDefaultConstructor)
                .WithLocation(0)
                .WithArguments("PrivateCtorConv")
        );

    [TestMethod]
    public Task TwoStaticConvert_ReportsStaticConvertMethodAmbiguity()
        => RunAsync(
              """
                  public class StaticAmbig
                  {
                      public static int Convert(int x) => x;
                      public static string Convert(string s) => s;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(StaticAmbig))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.StaticConvertMethodAmbiguity)
                .WithLocation(0)
                .WithArguments("StaticAmbig")
        );

    [TestMethod]
    public Task TwoInstanceConvert_ReportsInstancedConvertMethodAmbiguity()
        => RunAsync(
              """
                  public class InstanceAmbig
                  {
                      public int Convert(int x) => x;
                      public string Convert(string s) => s;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(InstanceAmbig))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InstancedConvertMethodAmbiguity)
                .WithLocation(0)
                .WithArguments("InstanceAmbig")
        );

    [TestMethod]
    public Task NoConvertMethod_OnMember_ReportsMissingConvertMethod()
        => RunAsync(
              """
                  public class NoConvert { }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData>
                  {
                      [{|#0:DataAuthoringConverter(typeof(NoConvert))|}]
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
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingConvertMethod)
                .WithLocation(0)
                .WithArguments("NoConvert", "int")
        );

    [TestMethod]
    public Task NoConvertMethod_InArray_ReportsMissingConvertMethodReturnType()
        => RunAsync(
              """
                  public class NoConvert { }

                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [{|#0:Database(typeof(NoConvert))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingConvertMethodReturnType)
                .WithLocation(0)
                .WithArguments("NoConvert", "")
        );
}
