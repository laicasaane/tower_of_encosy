using EncosyTower.SourceGen.Analyzers.NewtonsoftAotHelpers;
using EncosyTower.SourceGen.Tests.Data;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.NewtonsoftAotHelpers;

[TestClass]
public class NewtonsoftJsonAotHelperAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DataAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<NewtonsoftJsonAotHelperAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<NewtonsoftJsonAotHelperAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<NewtonsoftJsonAotHelperAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ConcreteClassWithBaseTypeArg_NoDiagnostics()
        => RunAsync("""
                public class Base { }

                [EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))]
                public partial class Helper { }
            """);

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public abstract class Helper { }
            """);

    [TestMethod]
    public Task GenericClassWithBaseTypeArg_NoDiagnostics()
        => RunAsync("""
                public class Base { }

                [EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))]
                public partial class Helper<T> { }
            """);

    [TestMethod]
    public Task AbstractClass_ReportsMustNotBeAbstract()
        => RunAsync(
              """
                  public class Base { }

                  [{|#0:EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))|}]
                  public abstract partial class Helper { }
              """
            , new DiagnosticResult(NewtonsoftJsonAotHelperAnalyzer.MustNotBeAbstract)
                .WithLocation(0)
                .WithArguments("Helper")
        );

    [TestMethod]
    public Task AttributeWithoutTypeArg_ReportsBaseTypeMustBeProvided()
        => RunAsync(
              """
                  [{|#0:EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper|}]
                  public partial class Helper { }
              """
            , new DiagnosticResult(NewtonsoftJsonAotHelperAnalyzer.BaseTypeMustBeProvided)
                .WithLocation(0)
                .WithArguments("Helper")
        );
}
