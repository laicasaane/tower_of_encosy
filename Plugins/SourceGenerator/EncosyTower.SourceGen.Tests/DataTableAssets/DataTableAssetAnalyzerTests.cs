using EncosyTower.SourceGen.Analyzers.DataTableAssets;
using EncosyTower.SourceGen.Tests.Data;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.DataTableAssets;

[TestClass]
public class DataTableAssetAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DataAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DataTableAssetAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<DataTableAssetAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<DataTableAssetAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ValidIdAndDataTypes_NoDiagnostics()
        => RunAsync("""
                public struct Id { }
                public struct Data { }

                [EncosyTower.Databases.DataTableAsset]
                public partial class Asset : EncosyTower.Databases.DataTableAsset<Id, Data> { }
            """);

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public partial class Asset : EncosyTower.Databases.DataTableAsset<System.IComparable, System.IDisposable> { }
            """);

    [TestMethod]
    public Task InterfaceAsIdType_ReportsMustBeApplicable()
        => RunAsync(
              """
                  public struct Data { }

                  [EncosyTower.Databases.DataTableAsset]
                  public partial class {|#0:Asset|} : EncosyTower.Databases.DataTableAsset<System.IComparable, Data> { }
              """
            , new DiagnosticResult(DataTableAssetAnalyzer.MustBeApplicableForTypeArgument)
                .WithLocation(0)
                .WithArguments("IComparable", "TDataId")
        );

    [TestMethod]
    public Task InterfaceAsDataType_ReportsMustBeApplicable()
        => RunAsync(
              """
                  public struct Id { }

                  [EncosyTower.Databases.DataTableAsset]
                  public partial class {|#0:Asset|} : EncosyTower.Databases.DataTableAsset<Id, System.IDisposable> { }
              """
            , new DiagnosticResult(DataTableAssetAnalyzer.MustBeApplicableForTypeArgument)
                .WithLocation(0)
                .WithArguments("IDisposable", "TData")
        );

    [TestMethod]
    public Task ThreeTypeArgVariant_NoDiagnostics()
        => RunAsync("""
                public struct Id { }
                public struct Data { }
                public struct ConvertedId { }

                [EncosyTower.Databases.DataTableAsset]
                public partial class Asset : EncosyTower.Databases.DataTableAsset<Id, Data, ConvertedId> { }
            """);
}
