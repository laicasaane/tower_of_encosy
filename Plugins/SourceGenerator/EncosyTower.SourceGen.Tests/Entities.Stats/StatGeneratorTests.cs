using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Entities.Stats;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatDataGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Entities.Stats
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class StatDataAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task StatDataGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatDataGenerator>();

    [TestMethod]
    public Task StatDataGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatDataGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Entities.Stats.StatData]
                public partial struct SampleStatData { }
            }
            """);
}

[TestClass]
public class StatCollectionGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Entities.Stats
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class StatCollectionAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task StatCollectionGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatCollectionGenerator>();

    [TestMethod]
    public Task StatCollectionGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatCollectionGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Entities.Stats.StatCollection]
                public partial struct SampleStatCollection { }
            }
            """);
}

[TestClass]
public class StatSystemGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Entities.Stats
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class StatSystemAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task StatSystemGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatSystemGenerator>();

    [TestMethod]
    public Task StatSystemGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<StatSystemGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Entities.Stats.StatSystem]
                public partial struct SampleStatSystem { }
            }
            """);
}
