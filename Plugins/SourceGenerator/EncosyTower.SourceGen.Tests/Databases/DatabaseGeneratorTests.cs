using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Databases;

namespace EncosyTower.SourceGen.Tests.Databases;

[TestClass]
public class DatabaseGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Databases
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public class DatabaseAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task DatabaseGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DatabaseGenerator>();

    [TestMethod]
    public Task DatabaseGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DatabaseGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Databases.Database]
                public partial class SampleDatabase { }
            }
            """);
}
