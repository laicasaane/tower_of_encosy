using EncosyTower.SourceGen.Analyzers.Mvvm.MonoBinders;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Mvvm.MonoBinders;

[TestClass]
public class MonoBinderDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = MvvmAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Mvvm.ViewBinding.Components;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<MonoBinderDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<MonoBinderDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<MonoBinderDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task BinderOnUnityObjectSubclass_NoDiagnostics()
        => RunAsync("""
                public class MyComp : UnityEngine.Object { }

                [MonoBinder(typeof(MyComp))]
                public partial class MyBinder { }
            """);

    [TestMethod]
    public Task BinderOnNonUnityType_ReportsComponentTypeMustInheritUnityObject()
        => RunAsync(
              """
                  public class PlainComp { }

                  [{|#0:MonoBinder(typeof(PlainComp))|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ComponentTypeMustInheritUnityObject)
                .WithLocation(0)
                .WithArguments("TestProject.PlainComp")
        );

    [TestMethod]
    public Task BindingPropertyWithoutMonoBinder_ReportsBindingAttributeRequiresMonoBinder()
        => RunAsync(
              """
                  [{|#0:MonoBindingProperty("Foo")|}]
                  public partial class OrphanBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.BindingAttributeRequiresMonoBinder)
                .WithLocation(0)
                .WithArguments("OrphanBinder", "MonoBindingProperty")
        );

    [TestMethod]
    public Task ExcludeUnknownMember_ReportsExcludedMemberNotFound()
        => RunAsync(
              """
                  public class MyComp : UnityEngine.Object
                  {
                      public int KnownProp { get; set; }
                  }

                  [MonoBinder(typeof(MyComp))]
                  [{|#0:MonoBindingExclude("MissingMember")|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ExcludedMemberNotFound)
                .WithLocation(0)
                .WithArguments("MissingMember", "TestProject.MyComp")
        );

    [TestMethod]
    public Task ExcludeKnownMember_NoDiagnostics()
        => RunAsync("""
                public class MyComp : UnityEngine.Object
                {
                    public int KnownProp { get; set; }
                }

                [MonoBinder(typeof(MyComp))]
                [MonoBindingExclude("KnownProp")]
                public partial class MyBinder { }
            """);

    [TestMethod]
    public Task ExcludeParentSameAsComponent_ReportsExcludeParentTypeIsComponentType()
        => RunAsync(
              """
                  public class MyComp : UnityEngine.Object { }

                  [MonoBinder(typeof(MyComp))]
                  [{|#0:MonoBinderExcludeParent(typeof(MyComp))|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ExcludeParentTypeIsComponentType)
                .WithLocation(0)
                .WithArguments("TestProject.MyComp")
        );

    [TestMethod]
    public Task ExcludeParentNotInChain_ReportsExcludeParentTypeNotInHierarchy()
        => RunAsync(
              """
                  public class BaseComp : UnityEngine.Object { }
                  public class MyComp : BaseComp { }
                  public class Unrelated : UnityEngine.Object { }

                  [MonoBinder(typeof(MyComp))]
                  [{|#0:MonoBinderExcludeParent(typeof(Unrelated))|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ExcludeParentTypeNotInHierarchy)
                .WithLocation(0)
                .WithArguments("TestProject.Unrelated", "TestProject.MyComp")
        );

    [TestMethod]
    public Task ExcludeParentInChain_NoDiagnostics()
        => RunAsync("""
                public class BaseComp : UnityEngine.Object { }
                public class MyComp : BaseComp { }

                [MonoBinder(typeof(MyComp))]
                [MonoBinderExcludeParent(typeof(BaseComp))]
                public partial class MyBinder { }
            """);

    [TestMethod]
    public Task ExplicitBindingOnObsoleteMember_ReportsObsoleteExplicitMemberWithExcludeObsolete()
        => RunAsync(
              """
                  public class MyComp : UnityEngine.Object
                  {
                      [System.Obsolete]
                      public int OldProp { get; set; }
                  }

                  [MonoBinder(typeof(MyComp), ExcludeObsolete = true)]
                  [{|#0:MonoBindingProperty("OldProp")|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ObsoleteExplicitMemberWithExcludeObsolete)
                .WithLocation(0)
                .WithArguments("OldProp", "TestProject.MyComp", "MonoBindingProperty")
        );

    [TestMethod]
    public Task ExcludeObsoleteMemberRedundantly_ReportsExcludeObsoleteMemberRedundant()
        => RunAsync(
              """
                  public class MyComp : UnityEngine.Object
                  {
                      [System.Obsolete]
                      public int OldProp { get; set; }
                  }

                  [MonoBinder(typeof(MyComp), ExcludeObsolete = true)]
                  [{|#0:MonoBindingExclude("OldProp")|}]
                  public partial class MyBinder { }
              """
            , new DiagnosticResult(MonoBinderDiagnosticAnalyzer.ExcludeObsoleteMemberRedundant)
                .WithLocation(0)
                .WithArguments("OldProp")
        );
}
