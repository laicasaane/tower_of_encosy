using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.EnumTemplates;

namespace EncosyTower.SourceGen.Tests.EnumTemplates;

[TestClass]
public class EnumTemplateGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class EnumTemplateAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class EnumMembersForTemplateAttribute : System.Attribute
            {
                public EnumMembersForTemplateAttribute(System.Type templateType) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class TypeAsEnumMemberForTemplateAttribute : System.Attribute
            {
                public TypeAsEnumMemberForTemplateAttribute(System.Type templateType) { }
            }
        }
        """;

    [TestMethod]
    public Task EnumTemplateGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<EnumTemplateGenerator>();

    [TestMethod]
    public Task EnumTemplateGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<EnumTemplateGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.EnumExtensions.EnumTemplate]
                public enum StatusTemplate { Active, Inactive }
            }
            """);
}
