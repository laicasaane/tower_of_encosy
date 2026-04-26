using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Entities.TypeHandles;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

[TestClass]
public class TypeHandleDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Entities
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public class TypeHandleAttribute : System.Attribute
            {
                public TypeHandleAttribute(System.Type type, bool isReadOnly = false) { }
            }
        }
        """;

    [TestMethod]
    public Task TypeHandleGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<TypeHandleGenerator>();

    [TestMethod]
    public Task TypeHandleGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<TypeHandleGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Entities.TypeHandle(typeof(int))]
                public partial struct SampleHandle { }
            }
            """);
}
