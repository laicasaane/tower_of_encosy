using EncosyTower.SourceGen.Analyzers.EnumExtensions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.EnumExtensions;

[TestClass]
public class EnumExtensionsAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<EnumExtensionsAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<EnumExtensionsAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<EnumExtensionsAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public static class Plain { }
            """);

    [TestMethod]
    public Task StaticClassWithEnumTypeArg_NoDiagnostics()
        => RunAsync("""
                public enum Color { Red, Green, Blue }

                [EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(Color))]
                public static class ColorExt { }
            """);

    [TestMethod]
    public Task NonStaticClassWithNonEnumTypeArg_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(int))]
                public class NotStatic { }
            """);

    [TestMethod]
    public Task StaticClassWithNonEnumTypeArg_ReportsTypeArgumentMustBeEnum()
        => RunAsync(
              """
                  [{|#0:EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(int))|}]
                  public static class IntExt { }
              """
            , new DiagnosticResult(EnumExtensionsAnalyzer.TypeArgumentMustBeEnum)
                .WithLocation(0)
                .WithArguments("int")
        );
}
