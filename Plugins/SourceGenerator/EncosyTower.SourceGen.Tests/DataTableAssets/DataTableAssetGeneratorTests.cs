using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.DataTableAssets;

namespace EncosyTower.SourceGen.Tests.DataTableAssets;

[TestClass]
public class DataTableAssetGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Databases
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class DataTableAssetAttribute : System.Attribute
            {
                public DataTableAssetAttribute(System.Type dataType) { }
            }
        }
        """;

    [TestMethod]
    public Task DataTableAssetGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DataTableAssetGenerator>();

    [TestMethod]
    public Task DataTableAssetGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DataTableAssetGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                public struct SampleData { public int Id; }

                [EncosyTower.Databases.DataTableAsset(typeof(SampleData))]
                public partial class SampleDataTableAsset { }
            }
            """);
}
