using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.TypeWraps;

namespace EncosyTower.SourceGen.Tests.TypeWraps;

[TestClass]
public class TypeWrapGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.TypeWraps
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public class WrapTypeAttribute : System.Attribute
            {
                public WrapTypeAttribute(System.Type wrappedType) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class WrapRecordAttribute : System.Attribute
            {
                public WrapRecordAttribute(System.Type wrappedType) { }
            }
        }
        """;

    [TestMethod]
    public Task TypeWrapGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<TypeWrapGenerator>();

    [TestMethod]
    public Task TypeWrapGenerator_WrapRecord_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<TypeWrapGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.TypeWraps.WrapRecord(typeof(string))]
                public partial class StringWrapper { }
            }
            """);
}
