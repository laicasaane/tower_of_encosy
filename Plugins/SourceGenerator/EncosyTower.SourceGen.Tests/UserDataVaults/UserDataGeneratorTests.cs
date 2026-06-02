using EncosyTower.SourceGen.Generators.UserDataVaults;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.UserDataVaults;

[TestClass]
public class UserDataGeneratorTests
{
    private const string STUB_USER_DATA = """
        namespace EncosyTower.UserDataVaults
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class UserDataAttribute : System.Attribute { }

            public interface IUserData
            {
                string Id { get; set; }

                string Version { get; set; }
            }
        }
        """;

    [TestMethod]
    public Task UserDataGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<UserDataGenerator>();

    [TestMethod]
    public void UserDataGenerator_PartialClass_GeneratesIUserDataMembers()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData { }
            }
            """);

        StringAssert.Contains(generated, "partial class PlayerData : ETUV.IUserData");
        StringAssert.Contains(generated, "public string Id { get; set; }");
        StringAssert.Contains(generated, "public string Version { get; set; }");
    }

    [TestMethod]
    public void UserDataGenerator_PartialStruct_GeneratesIUserDataMembers()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial struct PlayerData { }
            }
            """);

        StringAssert.Contains(generated, "partial struct PlayerData : ETUV.IUserData");
        StringAssert.Contains(generated, "public string Id { get; set; }");
        StringAssert.Contains(generated, "public string Version { get; set; }");
    }

    [TestMethod]
    public void UserDataGenerator_PartialRecordClass_GeneratesIUserDataMembers()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial record class PlayerData { }
            }
            """);

        StringAssert.Contains(generated, "partial record class PlayerData : ETUV.IUserData");
        StringAssert.Contains(generated, "public string Id { get; set; }");
        StringAssert.Contains(generated, "public string Version { get; set; }");
    }

    [TestMethod]
    public void UserDataGenerator_PartialRecordStruct_GeneratesIUserDataMembers()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial record struct PlayerData { }
            }
            """);

        StringAssert.Contains(generated, "partial record struct PlayerData : ETUV.IUserData");
        StringAssert.Contains(generated, "public string Id { get; set; }");
        StringAssert.Contains(generated, "public string Version { get; set; }");
    }

    [TestMethod]
    public void UserDataGenerator_ExistingId_GeneratesOnlyMissingVersion()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData
                {
                    public string Id { get; set; }
                }
            }
            """);

        Assert.AreEqual(0, CountOccurrences(generated, "public string Id { get; set; }"));
        Assert.AreEqual(1, CountOccurrences(generated, "public string Version { get; set; }"));
    }

    [TestMethod]
    public void UserDataGenerator_DirectIdWithAbstractBaseVersion_GeneratesOnlyVersionOverride()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                public abstract class BaseData
                {
                    public string Id { get; set; }

                    public abstract string Version { get; set; }
                }

                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData : BaseData
                {
                    public new string Id { get; set; }
                }
            }
            """);

        Assert.AreEqual(0, CountOccurrences(generated, "public string Id { get; set; }"));
        Assert.AreEqual(0, CountOccurrences(generated, "public override string Id { get; set; }"));
        Assert.AreEqual(1, CountOccurrences(generated, "public override string Version { get; set; }"));
    }

    [TestMethod]
    public void UserDataGenerator_ConcreteBaseProperties_GeneratesInterfaceOnly()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                public class BaseData
                {
                    public string Id { get; set; }

                    public string Version { get; set; }
                }

                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData : BaseData { }
            }
            """);

        StringAssert.Contains(generated, "partial class PlayerData : ETUV.IUserData");
        Assert.AreEqual(0, CountOccurrences(generated, "public string Id { get; set; }"));
        Assert.AreEqual(0, CountOccurrences(generated, "public string Version { get; set; }"));
    }

    [TestMethod]
    public void UserDataGenerator_AbstractBaseProperties_GeneratesOverrides()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                public abstract class BaseData
                {
                    public abstract string Id { get; set; }

                    public abstract string Version { get; set; }
                }

                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData : BaseData { }
            }
            """);

        StringAssert.Contains(generated, "partial class PlayerData : ETUV.IUserData");
        Assert.AreEqual(1, CountOccurrences(generated, "public override string Id { get; set; }"));
        Assert.AreEqual(1, CountOccurrences(generated, "public override string Version { get; set; }"));
    }

    [TestMethod]
    public void UserDataGenerator_BaseIdOnly_GeneratesOnlyMissingVersion()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                public class BaseData
                {
                    public string Id { get; set; }
                }

                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData : BaseData { }
            }
            """);

        Assert.AreEqual(0, CountOccurrences(generated, "public string Id { get; set; }"));
        Assert.AreEqual(1, CountOccurrences(generated, "public string Version { get; set; }"));
    }

    [TestMethod]
    public void UserDataGenerator_ExistingInterfaceAndMembers_GeneratesNoDuplicateMembers()
    {
        var generatedSources = RunAndGetGeneratedSources("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public partial class PlayerData : EncosyTower.UserDataVaults.IUserData
                {
                    public string Id { get; set; }

                    public string Version { get; set; }
                }
            }
            """);

        Assert.AreEqual(0, generatedSources.Length);
    }

    [TestMethod]
    public void UserDataGenerator_AbstractTarget_GeneratesNoSource()
    {
        var generatedSources = RunAndGetGeneratedSources("""
            namespace TestProject
            {
                [EncosyTower.UserDataVaults.UserData]
                public abstract partial class PlayerData { }
            }
            """);

        Assert.AreEqual(0, generatedSources.Length);
    }

    [TestMethod]
    public void UserDataGenerator_NestedType_GeneratedOutputCompilesThroughDriver()
    {
        var generated = RunAndGetSingleGeneratedSource("""
            namespace TestProject
            {
                public partial class Outer
                {
                    [EncosyTower.UserDataVaults.UserData]
                    public partial class PlayerData { }
                }
            }
            """);

        StringAssert.Contains(generated, "partial class Outer");
        StringAssert.Contains(generated, "partial class PlayerData : ETUV.IUserData");
    }

    private static string RunAndGetSingleGeneratedSource(string source)
    {
        var generatedSources = RunAndGetGeneratedSources(source);

        Assert.AreEqual(1, generatedSources.Length);

        return generatedSources[0];
    }

    private static string[] RunAndGetGeneratedSources(string source)
        => GeneratorTestHelper.RunDriverAndGetGeneratedSources<UserDataGenerator>($$"""
            {{STUB_USER_DATA}}
            {{source}}
            """);

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
