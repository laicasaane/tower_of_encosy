using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.UnionIds;

namespace EncosyTower.SourceGen.Tests.UnionIds;

[TestClass]
public class UnionIdGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.UnionIds
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public class UnionIdAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class KindForUnionIdAttribute : System.Attribute
            {
                public KindForUnionIdAttribute(System.Type unionIdType) { }
            }
        }
        """;

    [TestMethod]
    public Task UnionIdGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<UnionIdGenerator>();

    [TestMethod]
    public Task UnionIdGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunDriverSmokeTestAsync<UnionIdGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.UnionIds.UnionId]
                public partial struct SampleId { }
            }
            """);
}
