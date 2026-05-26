using EncosyTower.SourceGen.Analyzers.Entities.Stats;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Stats;

[TestClass]
public class StatCollectionDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = StatsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<StatCollectionDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<StatCollectionDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<StatCollectionDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ValidCollection_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public partial struct Sys { }

                [EncosyTower.Entities.Stats.StatCollection(typeof(Sys))]
                public partial struct Coll { }
            """);

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { }
            """);

    [TestMethod]
    public Task ClassWithStatCollection_ReportsMustBeStruct()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatCollection(typeof(int))]
                  public partial class {|#0:Coll|} { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.MustBeStruct)
                .WithLocation(0)
                .WithArguments("Coll")
        );

    [TestMethod]
    public Task GenericStruct_ReportsMustNotBeNonGeneric()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatCollection(typeof(int))]
                  public partial struct {|#0:Coll|}<T> { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.MustNotBeNonGeneric)
                .WithLocation(0)
                .WithArguments("Coll")
        );

    [TestMethod]
    public Task SystemTypeMissingStatSystem_ReportsStatSystemAttributeRequired()
        => RunAsync(
              """
                  public class Plain { }

                  [{|#0:EncosyTower.Entities.Stats.StatCollection(typeof(Plain))|}]
                  public partial struct Coll { }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.StatSystemAttributeRequired)
                .WithLocation(0)
                .WithArguments("TestProject.Plain")
        );

    [TestMethod]
    public Task TypeIdOffsetMaxWithStatDataMember_ReportsOverflow()
        => RunAsync(
              """
                  [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                  public partial struct Sys { }

                  [EncosyTower.Entities.Stats.StatCollection(typeof(Sys), 4294967295u)]
                  public partial struct {|#0:Coll|}
                  {
                      [EncosyTower.Entities.Stats.StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              """
            , new DiagnosticResult(StatCollectionDiagnosticAnalyzer.TypeIdOffsetOverflow)
                .WithLocation(0)
                .WithArguments(4294967295u, 1, "Coll")
        );

    [TestMethod]
    public Task TypeIdOffsetMaxWithoutStatData_NoOverflow()
        => RunAsync("""
                [EncosyTower.Entities.Stats.StatSystem(EncosyTower.Entities.Stats.StatDataSize.Size4)]
                public partial struct Sys { }

                [EncosyTower.Entities.Stats.StatCollection(typeof(Sys), 4294967295u)]
                public partial struct Coll { }
            """);
}