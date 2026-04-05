using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.PolyEnumStructs;

[TestClass]
public class PolyEnumStructGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.PolyEnumStructs
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public class PolyEnumStructAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task PolyEnumStructGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<PolyEnumStructGenerator>();

    [TestMethod]
    public Task PolyEnumStructGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunDriverSmokeTestAsync<PolyEnumStructGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct SamplePolyEnum { }
            }
            """);
}
