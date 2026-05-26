using EncosyTower.SourceGen.Analyzers.EnumTemplates;
using EncosyTower.SourceGen.Tests.EnumExtensions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.EnumTemplates;

[TestClass]
public class EnumTemplateAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.EnumExtensions;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<EnumTemplateAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<EnumTemplateAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<EnumTemplateAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task TemplateWithoutSuffix_ReportsNotEndWithTemplateSuffix()
        => RunAsync(
              """
                  [EnumTemplate]
                  public readonly struct {|#0:Foo|} { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotEndWithTemplateSuffix)
                .WithLocation(0)
        );

    [TestMethod]
    public Task TemplateWithEnumTemplateSuffix_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Foo_EnumTemplate { }
            """);

    [TestMethod]
    public Task TemplateWithTemplateSuffix_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Foo_Template { }
            """);

    [TestMethod]
    public Task EnumWithIntUnderlyingType_ReportsNotSupportUnderlyingType()
        => RunAsync(
              """
                  [EnumTemplate]
                  public readonly struct Tmpl_EnumTemplate { }

                  [{|#0:EnumMembersForTemplate(typeof(Tmpl_EnumTemplate), 0)|}]
                  public enum FruitInt : int { Apple, Orange }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotSupportUnderlyingType)
                .WithLocation(0)
        );

    [TestMethod]
    public Task EnumWithUintUnderlyingType_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Tmpl_EnumTemplate { }

                [EnumMembersForTemplate(typeof(Tmpl_EnumTemplate), 0)]
                public enum FruitUint : uint { Apple, Orange }
            """);

    [TestMethod]
    public Task TemplateWithNullTypeArg_ReportsMustBeTypeOfExpression()
        => RunAsync(
              """
                  [EnumTemplate]
                  [{|#0:EnumTemplateMembersFromEnum(null, 0)|}]
                  public readonly struct Tmpl_EnumTemplate { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.MustBeTypeOfExpression)
                .WithLocation(0)
        );

    [TestMethod]
    public Task TemplateWithValidTypeArg_NoDiagnostics()
        => RunAsync("""
                public enum SomeEnum : uint { A, B }

                [EnumTemplate]
                [EnumTemplateMembersFromEnum(typeof(SomeEnum), 0)]
                public readonly struct Tmpl_EnumTemplate { }
            """);

    [TestMethod]
    public Task TemplateWithUnboundGenericMember_ReportsNotSupportUnboundGenericType()
        => RunAsync(
              """
                  public class Generic<T> { }

                  [EnumTemplate]
                  [{|#0:EnumTemplateMemberFromType(typeof(Generic<>), 0)|}]
                  public readonly struct Tmpl_EnumTemplate { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotSupportUnboundGenericType)
                .WithLocation(0)
                .WithArguments("TestProject.Generic<>")
        );

    [TestMethod]
    public Task TemplateWithClosedGenericMember_NoDiagnostics()
        => RunAsync("""
                public class Generic<T> { }

                [EnumTemplate]
                [EnumTemplateMemberFromType(typeof(Generic<int>), 0)]
                public readonly struct Tmpl_EnumTemplate { }
            """);

    [TestMethod]
    public Task NestedTemplateInsideOuter_NoDiagnostics()
        => RunAsync("""
                public class Outer
                {
                    [EnumTemplate]
                    public readonly struct Inner_Template { }
                }
            """);
}
