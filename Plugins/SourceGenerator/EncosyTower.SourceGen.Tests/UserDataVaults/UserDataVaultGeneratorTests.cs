using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.UserDataVaults;

namespace EncosyTower.SourceGen.Tests.UserDataVaults;

[TestClass]
public class UserDataVaultGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.UserDataVaults
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class UserDataVaultAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public class UserDataAccessorAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task UserDataVaultGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<UserDataVaultGenerator>();

    [TestMethod]
    public Task UserDataVaultGenerator_VaultAttribute_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<UserDataVaultGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserDataVault]
                public partial class SampleVault { }
            }
            """);

    [TestMethod]
    public Task UserDataVaultGenerator_AccessorAttribute_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<UserDataVaultGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserDataAccessor]
                public partial class SampleAccessor { }
            }
            """);
}
