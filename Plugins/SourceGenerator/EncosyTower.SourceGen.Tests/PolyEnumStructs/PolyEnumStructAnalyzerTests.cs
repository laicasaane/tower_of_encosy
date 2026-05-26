using EncosyTower.SourceGen.Analyzers.PolyEnumStructs;
using EncosyTower.SourceGen.Tests.EnumExtensions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.PolyEnumStructs;

[TestClass]
public class PolyEnumStructAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<PolyEnumStructAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<PolyEnumStructAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<PolyEnumStructAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public partial struct Plain { }
            """);

    [TestMethod]
    public Task PolyEnumStructWithCase_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Good
                {
                    public partial struct Case
                    {
                        public void Run() { }
                    }
                }
            """);

    [TestMethod]
    public Task GenericPolyEnumStruct_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumStruct|}]
                  public partial struct Foo<T> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.MustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task PolyEnumStructWithoutCases_ReportsMustHaveCaseStructs()
        => RunAsync(
              """
                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumStruct|}]
                  public partial struct Empty { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.MustHaveCaseStructs)
                .WithLocation(0)
                .WithArguments("Empty")
        );

    [TestMethod]
    public Task IEnumCaseGenericMethod_ReportsIEnumCaseMethodMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Outer
                  {
                      public partial struct Case
                      {
                          public void Run() { }
                      }

                      public interface IEnumCase
                      {
                          void {|#0:Bar|}<T>();
                      }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.IEnumCaseMethodMustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Bar")
        );

    [TestMethod]
    public Task CaseStructGenericMethod_ReportsCaseStructMethodMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Outer
                  {
                      public partial struct Case
                      {
                          public void {|#0:Bar|}<T>() { }
                      }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseStructMethodMustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Bar", "Case")
        );
}
