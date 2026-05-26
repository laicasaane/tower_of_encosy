using EncosyTower.SourceGen.Analyzers.Entities.Lookups;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

[TestClass]
public class LookupDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EntitiesAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<LookupDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<LookupDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<LookupDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task StructWithoutLookupAttribute_NoDiagnostics()
        => RunAsync("""
                public struct Foo : EncosyTower.Entities.IBufferLookups { }
            """);

    [TestMethod]
    public Task ValidBufferLookup_NoDiagnostics()
        => RunAsync("""
                public struct MyData : Unity.Entities.IBufferElementData { public int Value; }

                [EncosyTower.Entities.Lookup(typeof(MyData))]
                public struct Cache : EncosyTower.Entities.IBufferLookups { }
            """);

    [TestMethod]
    public Task ArrayTypeofArg_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  public struct MyData : Unity.Entities.IBufferElementData { public int Value; }

                  [{|#0:EncosyTower.Entities.Lookup(typeof(MyData[]))|}]
                  public struct Cache : EncosyTower.Entities.IBufferLookups { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.NotTypeOfExpression)
                .WithLocation(0)
        );

    [TestMethod]
    public Task UnboundGenericTypeofArg_ReportsOpenGenericTypeNotSupported()
        => RunAsync(
              """
                  public struct GenericData<T> : Unity.Entities.IBufferElementData where T : unmanaged { public T Value; }

                  [{|#0:EncosyTower.Entities.Lookup(typeof(GenericData<>))|}]
                  public struct Cache : EncosyTower.Entities.IBufferLookups { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.OpenGenericTypeNotSupported)
                .WithLocation(0)
                .WithArguments("GenericData")
        );

    [TestMethod]
    public Task ManagedStructTypeofArg_ReportsManagedTypeNotSupported()
        => RunAsync(
              """
                  public struct ManagedData : Unity.Entities.IBufferElementData { public string Name; }

                  [{|#0:EncosyTower.Entities.Lookup(typeof(ManagedData))|}]
                  public struct Cache : EncosyTower.Entities.IBufferLookups { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.ManagedTypeNotSupported)
                .WithLocation(0)
                .WithArguments("ManagedData")
        );

    [TestMethod]
    public Task IncompatibleInterface_ReportsIncompatInterface()
        => RunAsync(
              """
                  public struct PlainData { public int Value; }

                  [{|#0:EncosyTower.Entities.Lookup(typeof(PlainData))|}]
                  public struct Cache : EncosyTower.Entities.IBufferLookups { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.IncompatInterface)
                .WithLocation(0)
                .WithArguments("PlainData", "Unity.Entities.IBufferElementData")
        );

    [TestMethod]
    public Task EnableableBufferMissingEnableableComponent_ReportsIncompatInterface()
        => RunAsync(
              """
                  public struct OnlyBuffer : Unity.Entities.IBufferElementData { public int Value; }

                  [{|#0:EncosyTower.Entities.Lookup(typeof(OnlyBuffer))|}]
                  public struct Cache : EncosyTower.Entities.IEnableableBufferLookups { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.IncompatInterface)
                .WithLocation(0)
                .WithArguments("OnlyBuffer", "Unity.Entities.IEnableableComponent")
        );

    [TestMethod]
    public Task NoMarkerInterface_ReportsNoMarkerInterface()
        => RunAsync(
              """
                  public struct MyData : Unity.Entities.IBufferElementData { public int Value; }

                  [EncosyTower.Entities.Lookup(typeof(MyData))]
                  public struct {|#0:Cache|} { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.NoMarkerInterface)
                .WithLocation(0)
                .WithArguments("Cache")
        );

    [TestMethod]
    public Task MultipleMarkerInterfaces_ReportsMultipleMarkerInterfaces()
        => RunAsync(
              """
                  public struct MyData : Unity.Entities.IBufferElementData, Unity.Entities.IComponentData { public int Value; }

                  [EncosyTower.Entities.Lookup(typeof(MyData))]
                  public struct {|#0:Cache|}
                      : EncosyTower.Entities.IBufferLookups
                      , EncosyTower.Entities.IComponentLookups
                  { }
              """
            , new DiagnosticResult(LookupDiagnosticAnalyzer.MultipleMarkerInterfaces)
                .WithLocation(0)
                .WithArguments("Cache", 2)
        );
}
