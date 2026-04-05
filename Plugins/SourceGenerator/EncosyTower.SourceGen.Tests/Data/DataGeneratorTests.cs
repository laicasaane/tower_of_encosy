using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Data;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public class DataGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Data
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class DataAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task DataGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DataGenerator>();

    [TestMethod]
    public Task DataGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DataGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Data.Data]
                public partial struct SampleData
                {
                    public int Id;
                    public string Name;
                }
            }
            """);
}
