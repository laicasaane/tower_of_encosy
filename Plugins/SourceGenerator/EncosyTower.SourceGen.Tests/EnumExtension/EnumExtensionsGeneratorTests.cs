using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.EnumExtension;

[TestClass]
public class EnumExtensionsGeneratorTests
{
    private const string STUB_ATTRIBUTE = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class EnumExtensionsAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task EnumExtensionsGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<EnumExtensionsGenerator>();

    [TestMethod]
    public Task EnumExtensionsGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunDriverSmokeTestAsync<EnumExtensionsGenerator>($$"""
            {{STUB_ATTRIBUTE}}
            namespace TestProject
            {
                [EncosyTower.EnumExtensions.EnumExtensions]
                public enum Color { Red, Green, Blue }
            }
            """);
}
