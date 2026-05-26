using EncosyTower.SourceGen.Analyzers.Entities.Stats;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Stats;

[TestClass]
public class StatDataDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = StatsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<StatDataDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<StatDataDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<StatDataDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task NonGenericStructWithVariantType_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                public partial struct Hp { }
            """);

    [TestMethod]
    public Task NonGenericStructWithEnumType_NoDiagnostics()
        => RunAsync("""
                public enum SampleKind : byte { A, B }

                [EncosyTower.Entities.Stats.StatData(typeof(SampleKind))]
                public partial struct Kind { }
            """);

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { }
            """);

    [TestMethod]
    public Task ClassWithStatData_ReportsMustBeStruct()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                  public partial class {|#0:Hp|} { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustBeStruct)
                .WithLocation(0)
                .WithArguments("Hp")
        );

    [TestMethod]
    public Task GenericStruct_ReportsMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                  public partial struct {|#0:Hp|}<T> { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.MustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Hp")
        );

    [TestMethod]
    public Task StatVariantTypeNone_ReportsMustNotBeNone()
        => RunAsync(
              """
                  [{|#0:EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.None)|}]
                  public partial struct Hp { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.StatVariantTypeMustNotBeNone)
                .WithLocation(0)
                .WithArguments("Hp")
        );

    [TestMethod]
    public Task TypeofNonEnum_ReportsTypeofArgMustBeEnum()
        => RunAsync(
              """
                  public class NotAnEnum { }

                  [{|#0:EncosyTower.Entities.Stats.StatData(typeof(NotAnEnum))|}]
                  public partial struct Hp { }
              """
            , new DiagnosticResult(StatDataDiagnosticAnalyzer.TypeofArgMustBeEnum)
                .WithLocation(0)
                .WithArguments("TestProject.NotAnEnum")
        );
}