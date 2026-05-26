using EncosyTower.SourceGen.Analyzers.PolyEnumFactories;
using EncosyTower.SourceGen.Tests.EnumExtensions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.PolyEnumFactories;

[TestClass]
public class PolyEnumFactoryAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<PolyEnumFactoryAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<PolyEnumFactoryAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<PolyEnumFactoryAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public class Plain { }
            """);

    [TestMethod]
    public Task ValidPartialFactory_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Target
                {
                    public partial struct Case { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                public partial class Factory { }
            """);

    [TestMethod]
    public Task NonPartialFactory_ReportsMustBePartial()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target
                  {
                      public partial struct Case { }
                  }

                  [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                  public class {|#0:Factory|} { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.MustBePartial)
                .WithLocation(0)
                .WithArguments("Factory")
        );

    [TestMethod]
    public Task GenericFactory_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target
                  {
                      public partial struct Case { }
                  }

                  [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                  public partial class {|#0:Factory|}<T> { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.MustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Factory")
        );

    [TestMethod]
    public Task TargetWithoutPolyEnumStruct_ReportsTargetMustBePolyEnumStruct()
        => RunAsync(
              """
                  public struct Plain { }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Plain))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetMustBePolyEnumStruct)
                .WithLocation(0)
                .WithArguments("Plain")
        );

    [TestMethod]
    public Task GenericTarget_ReportsTargetMustNotBeGeneric()
        => RunAsync(
              """
                  public partial struct Target<T> { }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<>))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetMustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Target")
        );

    [TestMethod]
    public Task TargetWithoutCases_ReportsMustHaveCaseStructs()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target { }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.MustHaveCaseStructs)
                .WithLocation(0)
                .WithArguments("Target")
        );

    [TestMethod]
    public Task CaseCtorWithOutParam_ReportsCaseCtorOutParameterIgnored()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target
                  {
                      public partial struct Case
                      {
                          public {|#0:Case|}(out int x) { x = 0; }
                      }
                  }

                  [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.CaseCtorOutParameterIgnored)
                .WithLocation(0)
                .WithArguments("Case")
        );
}
