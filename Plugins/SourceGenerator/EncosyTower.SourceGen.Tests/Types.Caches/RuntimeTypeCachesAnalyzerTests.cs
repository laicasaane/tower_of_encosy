using EncosyTower.SourceGen.Analyzers.Types.Caches;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Types.Caches;

[TestClass]
public class RuntimeTypeCachesAnalyzerTests
{
    private const string STUB_ATTRIBUTES = RuntimeTypeCachesAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Types;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<RuntimeTypeCachesAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<RuntimeTypeCachesAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<RuntimeTypeCachesAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task NoMatchingInvocation_NoDiagnostics()
        => RunAsync("""
                public class Foo
                {
                    public void M()
                    {
                        System.Console.WriteLine("hi");
                    }
                }
            """);

    [TestMethod]
    public Task GetInfoOnNonStaticClass_NoDiagnostics()
        => RunAsync("""
                public class Bar { }

                public class Foo
                {
                    public void M()
                    {
                        RuntimeTypeCache.GetInfo<Bar>();
                    }
                }
            """);

    [TestMethod]
    public Task GetInfoOnTypeParameter_ReportsTypeParameterIsNotApplicable()
        => RunAsync(
              """
                  public class Foo<T>
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetInfo<{|#0:T|}>();
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.TypeParameterIsNotApplicable)
                .WithLocation(0)
                .WithArguments("T", "GetInfo")
        );

    [TestMethod]
    public Task GetTypesDerivedFromOnClass_NoDiagnostics()
        => RunAsync("""
                public class Bar { }

                public class Foo
                {
                    public void M()
                    {
                        RuntimeTypeCache.GetTypesDerivedFrom<Bar>();
                    }
                }
            """);

    [TestMethod]
    public Task GetTypesDerivedFromOnStruct_ReportsOnlyClassOrInterfaceIsApplicable()
        => RunAsync(
              """
                  public class Foo
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetTypesDerivedFrom<{|#0:int|}>();
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.OnlyClassOrInterfaceIsApplicable)
                .WithLocation(0)
                .WithArguments("int")
        );

    [TestMethod]
    public Task GetInfoOnStaticClass_ReportsStaticClassIsNotApplicable()
        => RunAsync(
              """
                  public static class StaticBar { }

                  public class Foo
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetInfo<{|#0:StaticBar|}>();
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.StaticClassIsNotApplicable)
                .WithLocation(0)
                .WithArguments("global::TestProject.StaticBar")
            , DiagnosticResult.CompilerError("CS0718")
                .WithSpan(38, 30, 38, 48)
                .WithArguments("TestProject.StaticBar")
        );

    [TestMethod]
    public Task GetTypesDerivedFromOnSealedClass_ReportsSealedClassIsNotApplicable()
        => RunAsync(
              """
                  public sealed class SealedBar { }

                  public class Foo
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetTypesDerivedFrom<{|#0:SealedBar|}>();
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.SealedClassIsNotApplicable)
                .WithLocation(0)
                .WithArguments("global::TestProject.SealedBar")
        );

    [TestMethod]
    public Task GetTypesDerivedFromWithStringLiteralAssemblyName_NoDiagnostics()
        => RunAsync("""
                public class Bar { }

                public class Foo
                {
                    public void M()
                    {
                        RuntimeTypeCache.GetTypesDerivedFrom<Bar>("MyAssembly");
                    }
                }
            """);

    [TestMethod]
    public Task GetTypesDerivedFromWithNonConstantAssemblyName_ReportsAssemblyNameMustBeStringLiteralOrConstant()
        => RunAsync(
              """
                  public class Bar { }

                  public class Foo
                  {
                      public void M(string asm)
                      {
                          RuntimeTypeCache.GetTypesDerivedFrom<Bar>({|#0:asm|});
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.AssemblyNameMustBeStringLiteralOrConstant)
                .WithLocation(0)
        );

    [TestMethod]
    public Task GetTypesDerivedFromWithNullLiteralAssemblyName_ReportsAssemblyNameMustBeStringLiteralOrConstant()
        => RunAsync(
              """
                  public class Bar { }

                  public class Foo
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetTypesDerivedFrom<Bar>({|#0:(string)null|});
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.AssemblyNameMustBeStringLiteralOrConstant)
                .WithLocation(0)
        );

    [TestMethod]
    public Task GetTypesWithAttributeOnExternalType_NoDiagnostics()
        => RunAsync("""
                public class MyAttribute : System.Attribute { }

                public class Foo
                {
                    public void M()
                    {
                        RuntimeTypeCache.GetTypesWithAttribute<MyAttribute>();
                    }
                }
            """);

    [TestMethod]
    public Task GetTypesWithAttributeOnCachesNamespaceType_ReportsTypesFromCachesAreProhibited()
        => RunAsync(
              """
                  public class Foo
                  {
                      public void M()
                      {
                          RuntimeTypeCache.GetTypesWithAttribute<{|#0:global::EncosyTower.Types.Caches.CachedAttributeType|}>();
                      }
                  }
              """
            , new DiagnosticResult(RuntimeTypeCachesAnalyzer.TypesFromCachesAreProhibited)
                .WithLocation(0)
                .WithArguments("global::EncosyTower.Types.Caches.CachedAttributeType")
        );
}
