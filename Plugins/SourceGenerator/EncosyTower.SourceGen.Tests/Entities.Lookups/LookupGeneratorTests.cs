using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Entities.Lookups;

namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

[TestClass]
public class LookupGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Entities
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class LookupAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task LookupGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<LookupGenerator>();

    [TestMethod]
    public Task LookupGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<LookupGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Entities.Lookup]
                public partial struct SampleLookup { }
            }
            """);
}
