using EncosyTower.SourceGen.Analyzers.TypeWraps;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.TypeWraps;

[TestClass]
public sealed class TypeWrapDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = TypeWrapsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TypeWrapDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<TypeWrapDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<TypeWrapDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task WrapType_OnPartialStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial struct Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_OnPartialClass_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial class Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_OnNestedStruct_NoDiagnostics()
        => RunAsync("""
                public partial class Outer
                {
                    [EncosyTower.TypeWraps.WrapType(typeof(int))]
                    public partial struct Wrapper { }
                }
            """);

    [TestMethod]
    public Task WrapType_OnGenericStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial struct Wrapper<T> { }
            """);

    [TestMethod]
    public Task WrapType_OnRecord_ReportsWrapTypeOnRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial record Wrapper;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapTypeOnRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_OnRecordClass_ReportsWrapTypeOnRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial record class Wrapper(int Value);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapTypeOnRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_NullArg_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(null)|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.NotTypeOfExpression)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnPartialRecord_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapRecord]
                public partial record Wrapper(int Value);
            """);

    [TestMethod]
    public Task WrapRecord_OnPartialRecordStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapRecord]
                public partial record struct Wrapper(int Value);
            """);

    [TestMethod]
    public Task WrapRecord_OnRecordWithoutParameters_ReportsRequiresOneParameter()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordRequiresOneParameter)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnRecordWithTwoParameters_ReportsRequiresOneParameter()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper(int A, int B);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordRequiresOneParameter)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnNonRecordClass_ReportsWrapRecordOnNonRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial class Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordOnNonRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnNonRecordStruct_ReportsWrapRecordOnNonRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordOnNonRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_OnClassWithBaseClass_ReportsInheritsBaseClass()
        => RunAsync(
              """
                  public class BaseType { }

                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial class Wrapper : BaseType { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapperInheritsBaseClass)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnRecordClassWithBaseClass_ReportsInheritsBaseClass()
        => RunAsync(
              """
                  public record BaseType;

                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper(int Value) : BaseType;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapperInheritsBaseClass)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_TwoArgValidIdentifier_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int), "Inner")]
                public partial struct Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_TwoArgEmptyString_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "")
        );

    [TestMethod]
    public Task WrapType_TwoArgNullString_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), null)|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "")
        );

    [TestMethod]
    public Task WrapType_TwoArgKeyword_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "class")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "class")
        );

    [TestMethod]
    public Task WrapType_TwoArgStartsWithDigit_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "1bad")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "1bad")
        );

    [TestMethod]
    public Task WrapType_TwoArgWithSpaces_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "bad name")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "bad name")
        );
}
