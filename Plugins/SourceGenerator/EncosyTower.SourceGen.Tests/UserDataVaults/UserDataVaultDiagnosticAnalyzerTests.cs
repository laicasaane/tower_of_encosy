using EncosyTower.SourceGen.Analyzers.UserDataVaults;
using EncosyTower.SourceGen.Tests.Data;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.UserDataVaults;

[TestClass]
public class UserDataVaultDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DataAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<UserDataVaultDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<UserDataVaultDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<UserDataVaultDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public class Foo
                {
                    public Foo(int x) { }
                }
            """);

    [TestMethod]
    public Task ValidWithAccessorImpl_NoDiagnostics()
        => RunAsync("""
                public class MyAccessor : EncosyTower.UserDataVaults.IUserDataAccessor { }

                [EncosyTower.UserDataVaults.UserDataAccessor(typeof(int))]
                public class Foo
                {
                    public Foo(MyAccessor a) { }
                }
            """);

    [TestMethod]
    public Task ValidWithStoreImpl_NoDiagnostics()
        => RunAsync("""
                public class MyData : EncosyTower.UserDataVaults.IUserData { }

                public class MyStore : EncosyTower.UserDataVaults.UserDataStoreBase<MyData> { }

                [EncosyTower.UserDataVaults.UserDataAccessor(typeof(int))]
                public class Foo
                {
                    public Foo(MyStore s) { }
                }
            """);

    [TestMethod]
    public Task AbstractClass_ReportsMustNotBeAbstract()
        => RunAsync(
              """
                  [EncosyTower.UserDataVaults.UserDataAccessor(typeof(int))]
                  public abstract class {|#0:Foo|}
                  {
                      public Foo(EncosyTower.UserDataVaults.IUserDataAccessor a) { }
                  }
              """
            , new DiagnosticResult(UserDataVaultDiagnosticAnalyzer.MustNotBeAbstract)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task UnsupportedConstructorParam_ReportsConstructorContainsUnsupportedType()
        => RunAsync(
              """
                  [EncosyTower.UserDataVaults.UserDataAccessor(typeof(int))]
                  public class Foo
                  {
                      public Foo(int {|#0:x|}) { }
                  }
              """
            , new DiagnosticResult(UserDataVaultDiagnosticAnalyzer.ConstructorContainsUnsupportedType)
                .WithLocation(0)
                .WithArguments("Foo", "x")
        );

    [TestMethod]
    public Task LargestConstructorAtNonZeroIndex_ReportsMustHaveOnlyOneConstructor()
        => RunAsync(
              """
                  public class MyAccessor : EncosyTower.UserDataVaults.IUserDataAccessor { }

                  [EncosyTower.UserDataVaults.UserDataAccessor(typeof(int))]
                  public class {|#0:Foo|}
                  {
                      public Foo() { }

                      public Foo(MyAccessor a) { }
                  }
              """
            , new DiagnosticResult(UserDataVaultDiagnosticAnalyzer.MustHaveOnlyOneConstructor)
                .WithLocation(0)
                .WithArguments("Foo")
        );
}
