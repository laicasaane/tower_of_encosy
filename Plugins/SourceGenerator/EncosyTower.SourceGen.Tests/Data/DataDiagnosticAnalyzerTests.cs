using EncosyTower.SourceGen.Analyzers.Data;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public class DataDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = DataAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DataDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<DataDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<DataDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ImmutableMinimal_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Data.Data]
                public partial class Foo
                {
                    [UnityEngine.SerializeField] private int _x;
                }
            """);

    [TestMethod]
    public Task MutableWithPublicFieldAndSetter_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Data.Data]
                [EncosyTower.Data.DataMutable]
                public partial class Foo
                {
                    [UnityEngine.SerializeField] public int _x;

                    public int Y { get; set; }
                }
            """);

    [TestMethod]
    public Task ClassWithoutDataAttribute_NoDiagnostics()
        => RunAsync("""
                public partial class Foo
                {
                    [UnityEngine.SerializeField] public int _x;

                    public int Y { get; set; }
                }
            """);

    [TestMethod]
    public Task ImmutableWithFieldPolicy_ReportsCannotDecorateImmutableDataWithFieldPolicyAttribute()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  [EncosyTower.Data.DataFieldPolicy(EncosyTower.Data.DataFieldPolicy.Private)]
                  public partial class {|#0:Foo|} { }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.CannotDecorateImmutableDataWithFieldPolicyAttribute)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task ImmutableWithPublicSerializedField_ReportsImmutableDataFieldMustBePrivate()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  public partial class Foo
                  {
                      [UnityEngine.SerializeField] public int {|#0:_x|};
                  }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.ImmutableDataFieldMustBePrivate)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task ImmutableWithPublicSetter_ReportsOnlyPrivateOrInitOnlySetterIsAllowed()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  public partial class Foo
                  {
                      public int {|#0:X|} { get; set; }
                  }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.OnlyPrivateOrInitOnlySetterIsAllowed)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task MutableWithoutPropertySettersAndPublicSetter_ReportsOnlyPrivateOrInitOnlySetterIsAllowed()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  [EncosyTower.Data.DataMutable(EncosyTower.Data.DataMutableOptions.WithoutPropertySetters)]
                  public partial class Foo
                  {
                      public int {|#0:X|} { get; set; }
                  }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.OnlyPrivateOrInitOnlySetterIsAllowed)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task CollectionIdField_ReportsCollectionIsNotApplicableForProperty()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  public partial class Foo
                  {
                      [UnityEngine.SerializeField] private int[] {|#0:_id|};
                  }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.CollectionIsNotApplicableForProperty)
                .WithLocation(0)
                .WithArguments("", "Id")
        );

    [TestMethod]
    public Task CollectionIdProperty_ReportsCollectionIsNotApplicableForProperty()
        => RunAsync(
              """
                  [EncosyTower.Data.Data]
                  public partial class Foo
                  {
                      [EncosyTower.Data.DataProperty]
                      public int[] {|#0:Id|} { get; private set; }
                  }
              """
            , new DiagnosticResult(DataDiagnosticAnalyzer.CollectionIsNotApplicableForProperty)
                .WithLocation(0)
                .WithArguments("", "Id")
        );
}
