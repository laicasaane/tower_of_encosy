using EncosyTower.SourceGen.Analyzers.Databases;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Databases;

[TestClass]
public class DatabaseDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DatabaseAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Data;\n    using EncosyTower.Databases;\n    using EncosyTower.Databases.Authoring;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DatabaseDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<DatabaseDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<DatabaseDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ValidTableProperty_NoDiagnostics()
        => RunAsync("""
                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData> { }

                [Database]
                public partial class MyDatabase
                {
                    [Table]
                    public MyTable Items { get; }
                }
            """);

    [TestMethod]
    public Task AbstractTablePropertyType_ReportsAbstractTypeNotSupported()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public abstract class AbstractTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [{|#0:Table|}]
                      public AbstractTable Items { get; }
                  }
              """
            , new DiagnosticResult(TableDiagnosticDescriptors.AbstractTypeNotSupported)
                .WithLocation(0)
                .WithArguments("AbstractTable")
        );

    [TestMethod]
    public Task GenericTablePropertyType_ReportsGenericTypeNotSupported()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class GenericTable<T> : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [{|#0:Table|}]
                      public GenericTable<int> Items { get; }
                  }
              """
            , new DiagnosticResult(TableDiagnosticDescriptors.GenericTypeNotSupported)
                .WithLocation(0)
                .WithArguments("GenericTable")
        );

    [TestMethod]
    public Task NonDataTableAssetTablePropertyType_ReportsMustBeDerivedFromDataTableAsset()
        => RunAsync(
              """
                  public class NotATable { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [{|#0:Table|}]
                      public NotATable Items { get; }
                  }
              """
            , new DiagnosticResult(TableDiagnosticDescriptors.MustBeDerivedFromDataTableAsset)
                .WithLocation(0)
                .WithArguments("NotATable")
        );

    [TestMethod]
    public Task ValidHorizontal_NoDiagnostics()
        => RunAsync("""
                public class MyData : IData { }

                public class MyTable : DataTableAsset<int, MyData> { }

                [Database]
                public partial class MyDatabase
                {
                    [Table]
                    [Horizontal(typeof(MyData), "Value")]
                    public MyTable Items { get; }
                }
            """);

    [TestMethod]
    public Task HorizontalNullTargetType_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [Table]
                      [{|#0:Horizontal(null, "Value")|}]
                      public MyTable Items { get; }
                  }
              """
            , new DiagnosticResult(HorizontalDiagnosticDescriptors.NotTypeOfExpression)
                .WithLocation(0)
        );

    [TestMethod]
    public Task HorizontalAbstractTargetType_ReportsAbstractTypeNotSupported()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public abstract class AbstractTarget : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [Table]
                      [{|#0:Horizontal(typeof(AbstractTarget), "Value")|}]
                      public MyTable Items { get; }
                  }
              """
            , new DiagnosticResult(HorizontalDiagnosticDescriptors.AbstractTypeNotSupported)
                .WithLocation(0)
                .WithArguments("AbstractTarget")
        );

    [TestMethod]
    public Task HorizontalNonIDataTargetType_ReportsNotImplementIData()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class PlainTarget { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [Table]
                      [{|#0:Horizontal(typeof(PlainTarget), "Value")|}]
                      public MyTable Items { get; }
                  }
              """
            , new DiagnosticResult(HorizontalDiagnosticDescriptors.NotImplementIData)
                .WithLocation(0)
                .WithArguments("PlainTarget")
        );

    [TestMethod]
    public Task HorizontalEmptyPropertyName_ReportsInvalidPropertyName()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class MyTable : DataTableAsset<int, MyData> { }

                  [Database]
                  public partial class MyDatabase
                  {
                      [Table]
                      [{|#0:Horizontal(typeof(MyData), "")|}]
                      public MyTable Items { get; }
                  }
              """
            , new DiagnosticResult(HorizontalDiagnosticDescriptors.InvalidPropertyName)
                .WithLocation(0)
        );
}
