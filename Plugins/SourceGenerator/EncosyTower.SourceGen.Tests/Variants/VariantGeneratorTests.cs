using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Variants;

namespace EncosyTower.SourceGen.Tests.Variants;

[TestClass]
public class VariantStructGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Variants
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class VariantAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task VariantStructGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<VariantStructGenerator>();

    [TestMethod]
    public Task VariantStructGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<VariantStructGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Variants.Variant]
                public partial struct SampleVariant { }
            }
            """);
}

[TestClass]
public class VariantRegistrationGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Variants
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class VariantAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task VariantRegistrationGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<VariantRegistrationGenerator>();

    [TestMethod]
    public Task VariantRegistrationGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<VariantRegistrationGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Variants.Variant]
                public partial struct SampleVariant { }
            }
            """);
}

[TestClass]
public class VariantsInternalVariantGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Variants
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
            public class VariantAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task InternalVariantGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<InternalVariantGenerator>();

    [TestMethod]
    public Task InternalVariantGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<InternalVariantGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Variants.Variant]
                public partial struct SampleVariant { }
            }
            """);
}
