using EncosyTower.SourceGen.Analyzers.Entities.TypeHandles;
using EncosyTower.SourceGen.Generators.Entities.TypeHandles;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

[TestClass]
public class TypeHandleDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EntitiesAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using Unity.Entities;\n    using EncosyTower.Entities;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TypeHandleDiagnosticAnalyzer, DefaultVerifier>
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
    public Task TypeHandleGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<TypeHandleGenerator>();

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
    {
        var test = new CSharpAnalyzerTest<TypeHandleDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<TypeHandleDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task TypeHandleOnComponentStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyComp : IComponentData { }

                [TypeHandle(typeof(MyComp))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task TypeHandleOnBufferStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyBuf : IBufferElementData { }

                [TypeHandle(typeof(MyBuf))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task TypeHandleOnSharedComponentStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyShared : ISharedComponentData { }

                [TypeHandle(typeof(MyShared))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task GenericContainerStruct_ReportsGenericContainerNotSupported()
        => RunAsync(
              """
                  public struct MyComp : IComponentData { }

                  [TypeHandle(typeof(MyComp))]
                  public partial struct {|#0:MyHandle|}<T> { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.GenericContainerNotSupported)
                .WithLocation(0)
                .WithArguments("MyHandle")
        );

    [TestMethod]
    public Task TypeHandleWithNullLiteral_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  [{|#0:TypeHandle(null)|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.NotTypeOfExpression)
                .WithLocation(0)
        );

    [TestMethod]
    public Task TypeHandleWithUnboundGeneric_ReportsOpenGenericTypeNotSupported()
        => RunAsync(
              """
                  public struct GenericComp<T> : IComponentData where T : unmanaged { }

                  [{|#0:TypeHandle(typeof(GenericComp<>))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.OpenGenericTypeNotSupported)
                .WithLocation(0)
                .WithArguments("GenericComp")
        );

    [TestMethod]
    public Task TypeHandleWithManagedClass_ReportsManagedTypeNotSupported()
        => RunAsync(
              """
                  public class ManagedComp : IComponentData { }

                  [{|#0:TypeHandle(typeof(ManagedComp))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.ManagedTypeNotSupported)
                .WithLocation(0)
                .WithArguments("ManagedComp")
        );

    [TestMethod]
    public Task TypeHandleWithoutMarkerInterface_ReportsMissingMarkerInterface()
        => RunAsync(
              """
                  public struct PlainStruct { }

                  [{|#0:TypeHandle(typeof(PlainStruct))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.MissingMarkerInterface)
                .WithLocation(0)
                .WithArguments("PlainStruct")
        );

    [TestMethod]
    public Task DuplicateTypeHandleAttribute_ReportsDuplicateTypeHandle()
        => RunAsync(
              """
                  public struct MyComp : IComponentData { }

                  [TypeHandle(typeof(MyComp))]
                  [{|#0:TypeHandle(typeof(MyComp))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.DuplicateTypeHandle)
                .WithLocation(0)
                .WithArguments("MyComp")
        );

    [TestMethod]
    public Task TypeHandleSharedComponentWithReadOnlyTrue_ReportsReadOnlyIgnoredForSharedComponent()
        => RunAsync(
              """
                  public struct MyShared : ISharedComponentData { }

                  [{|#0:TypeHandle(typeof(MyShared), true)|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.ReadOnlyIgnoredForSharedComponent)
                .WithLocation(0)
                .WithArguments("MyShared")
        );
}
