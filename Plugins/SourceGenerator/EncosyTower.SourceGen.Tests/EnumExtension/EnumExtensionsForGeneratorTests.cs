using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.EnumExtension;

[TestClass]
public class EnumExtensionsForGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class EnumExtensionsForAttribute : System.Attribute
            {
                public EnumExtensionsForAttribute(System.Type enumType) { }
            }
        }
        """;

    [TestMethod]
    public Task EnumExtensionsForGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<EnumExtensionsForGenerator>();

    [TestMethod]
    public Task EnumExtensionsForGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<EnumExtensionsForGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                public enum Color { Red, Green, Blue }

                [EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(Color))]
                public partial struct ColorExtensions { }
            }
            """);
}
