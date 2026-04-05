using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.DatabaseAuthoring;

namespace EncosyTower.SourceGen.Tests.DatabaseAuthoring;

[TestClass]
public class DatabaseAuthoringGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Databases.Authoring
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class AuthorDatabaseAttribute : System.Attribute
            {
                public AuthorDatabaseAttribute(System.Type databaseType) { }
            }
        }
        """;

    [TestMethod]
    public Task DatabaseAuthoringGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DatabaseAuthoringGenerator>();

    [TestMethod]
    public Task DatabaseAuthoringGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<DatabaseAuthoringGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                public class SampleDatabase { }

                [EncosyTower.Databases.Authoring.AuthorDatabase(typeof(SampleDatabase))]
                public partial class SampleDatabaseAuthoring { }
            }
            """);
}
