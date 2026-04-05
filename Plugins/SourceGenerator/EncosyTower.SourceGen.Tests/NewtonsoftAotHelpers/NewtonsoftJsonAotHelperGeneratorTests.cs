using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers;

namespace EncosyTower.SourceGen.Tests.NewtonsoftAotHelpers;

[TestClass]
public class NewtonsoftJsonAotHelperGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Serialization.NewtonsoftJson
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public class NewtonsoftJsonAotHelperAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task NewtonsoftJsonAotHelperGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<NewtonsoftJsonAotHelperGenerator>();

    [TestMethod]
    public Task NewtonsoftJsonAotHelperGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<NewtonsoftJsonAotHelperGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper]
                public partial class SampleAotHelper { }
            }
            """);
}
