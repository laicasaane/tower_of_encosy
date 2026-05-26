using EncosyTower.SourceGen.Analyzers.UnionIds;
using EncosyTower.SourceGen.Tests.EnumExtensions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.UnionIds;

[TestClass]
public class UnionIdAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<UnionIdAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<UnionIdAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<UnionIdAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Plain { public int v; }
            """);

    [TestMethod]
    public Task ValidUnionIdAndKind_NoDiagnostics()
        => RunAsync("""
                public struct K { public int v; }

                [EncosyTower.UnionIds.UnionId]
                [EncosyTower.UnionIds.UnionIdKind(typeof(K), 0UL)]
                public struct Id { public int v; }
            """);

    [TestMethod]
    public Task DuplicateKindName_ReportsSameKindIsIgnored()
        => RunAsync(
              """
                  public struct K1 { public int v; }
                  public struct K2 { public int v; }

                  [EncosyTower.UnionIds.UnionId]
                  [EncosyTower.UnionIds.UnionIdKind(typeof(K1), 0UL, "Same")]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(K2), 1UL, "Same")|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.SameKindIsIgnored)
                .WithLocation(0)
                .WithArguments("Same", "TestProject.K1")
        );

    [TestMethod]
    public Task UnionIdStructWithManagedField_ReportsMustBeUnmanagedType()
        => RunAsync(
              """
                  [EncosyTower.UnionIds.UnionId]
                  public struct {|#0:Id|} { public string s; }
              """
            , new DiagnosticResult(UnionIdAnalyzer.MustBeUnmanagedType)
                .WithLocation(0)
        );

    [TestMethod]
    public Task KindForUnionIdSelf_ReportsKindTypeCannotBeIdType()
        => RunAsync(
              """
                  [{|#0:EncosyTower.UnionIds.KindForUnionId(typeof(Self), 0UL)|}]
                  public struct Self { public int v; }
              """
            , new DiagnosticResult(UnionIdAnalyzer.KindTypeCannotBeIdType)
                .WithLocation(0)
                .WithArguments("TestProject.Self")
        );

    [TestMethod]
    public Task DuplicateKindType_ReportsTypeAlreadyDeclared()
        => RunAsync(
              """
                  public struct K { public int v; }

                  [EncosyTower.UnionIds.UnionId]
                  [EncosyTower.UnionIds.UnionIdKind(typeof(K), 0UL)]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(K), 1UL)|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.TypeAlreadyDeclared)
                .WithLocation(0)
                .WithArguments("TestProject.K")
        );

    [TestMethod]
    public Task SixteenByteKind_ReportsKindSizeMustBeSmallerThan16Bytes()
        => RunAsync(
              """
                  public struct BigKind { public ulong a; public ulong b; }

                  [EncosyTower.UnionIds.UnionId]
                  [{|#0:EncosyTower.UnionIds.UnionIdKind(typeof(BigKind), 0UL, "Big")|}]
                  public struct Id { }
              """
            , new DiagnosticResult(UnionIdAnalyzer.KindSizeMustBeSmallerThan16Bytes)
                .WithLocation(0)
                .WithArguments("Big")
        );
}
