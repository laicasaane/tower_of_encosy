using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Types.Caches;

namespace EncosyTower.SourceGen.Tests.Types.Caches;

/// <summary>
/// Smoke tests for <see cref="RuntimeTypeCachesGenerator"/>, which is triggered by
/// method-call syntax on RuntimeTypeCache&lt;T&gt; rather than attributes.
/// </summary>
[TestClass]
public class RuntimeTypeCachesGeneratorTests
{
    // The generator watches for call sites like: RuntimeTypeCache<T>.GetInfo(...)
    // Provide a stub matching that static class + method pattern.
    private const string STUBS = """
        namespace EncosyTower.Types.Caches
        {
            public static class RuntimeTypeCache<T>
            {
                public static object GetInfo() => null!;
                public static System.Collections.Generic.IEnumerable<System.Type> GetTypesDerivedFrom() => null!;
                public static System.Collections.Generic.IEnumerable<System.Type> GetTypesWithAttribute<TAttr>() => null!;
            }
        }
        """;

    [TestMethod]
    public Task RuntimeTypeCachesGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<RuntimeTypeCachesGenerator>();

    [TestMethod]
    public Task RuntimeTypeCachesGenerator_WithCallSiteStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<RuntimeTypeCachesGenerator>($$"""
            {{STUBS}}
            namespace TestProject
            {
                public class SampleUsage
                {
                    public void Use()
                    {
                        var info = EncosyTower.Types.Caches.RuntimeTypeCache<int>.GetInfo();
                    }
                }
            }
            """);
}
