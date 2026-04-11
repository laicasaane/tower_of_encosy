using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.TypeModeling.Models;

namespace EncosyTower.SourceGen.Tests.TypeModeling;

[TestClass]
public class ModelExtractorTests
{
    private const string SHARED_SOURCE = """
        using System;
        namespace TestNs {
            [Obsolete("deprecated")]
            public class Target : IDisposable {
                public int PublicField;
                private int _privateField;
                public string Name { get; set; }
                public void DoWork() { }
                public static int Helper(int x) => x;
                public void Dispose() { }
            }
        }
        """;

    [TestMethod]
    public void TypeModel_EqualsSelf_WhenSameSource()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: true);

        var m1 = symbol.ToModel(CancellationToken.None, opts);
        var m2 = symbol.ToModel(CancellationToken.None, opts);

        Assert.AreEqual(m1, m2);
        Assert.AreEqual(m1.GetHashCode(), m2.GetHashCode());
    }

    [TestMethod]
    public void TypeModel_NotEqual_WhenFieldAdded()
    {
        const string SOURCE_A = """
            namespace TestNs {
                public class Target {
                    public int FieldA;
                }
            }
            """;

        const string SOURCE_B = """
            namespace TestNs {
                public class Target {
                    public int FieldA;
                    public int FieldB;
                }
            }
            """;

        var opts = new ModelOptions(ModelParts.All, includeNonPublic: true);

        var m1 = TypeModelingTestHelper.GetTypeSymbol(SOURCE_A, "TestNs.Target")
            .ToModel(CancellationToken.None, opts);
        var m2 = TypeModelingTestHelper.GetTypeSymbol(SOURCE_B, "TestNs.Target")
            .ToModel(CancellationToken.None, opts);

        Assert.AreNotEqual(m1, m2);
    }

    [TestMethod]
    public void TypeModel_Fields_Count_IncludesNonPublic()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        // IncludeNonPublic=true, IncludeCompilerGenerated=false (default)
        // → PublicField + _privateField (backing field for Name excluded)
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: true, includeCompilerGenerated: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        Assert.AreEqual(2, model.Fields.Count);
    }

    [TestMethod]
    public void TypeModel_Fields_ContainsExpectedNames()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: true, includeCompilerGenerated: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        var names = model.Fields.Select(f => f.Name).ToArray();
        CollectionAssert.Contains(names, "PublicField");
        CollectionAssert.Contains(names, "_privateField");
    }

    [TestMethod]
    public void TypeModel_IncludeNonPublic_False_OnlyPublicFields()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: false, includeCompilerGenerated: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        Assert.AreEqual(1, model.Fields.Count);
        Assert.AreEqual("PublicField", model.Fields[0].Name);
    }

    [TestMethod]
    public void TypeModel_ExcludesCompilerGeneratedFields_ByDefault()
    {
        // Auto-property generates a backing field named <Name>k__BackingField
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: true, includeCompilerGenerated: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        var names = model.Fields.Select(f => f.Name).ToArray();
        Assert.IsFalse(names.Any(n => n.StartsWith("<", StringComparison.Ordinal)),
            "Backing fields should be excluded when IncludeCompilerGenerated=false.");
    }

    [TestMethod]
    public void TypeModel_Methods_Count()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        // All public methods: DoWork, Helper, Dispose — 3 ordinary methods
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        Assert.AreEqual(3, model.Methods.Count);
    }

    [TestMethod]
    public void TypeModel_Attributes_Populated()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.All, includeNonPublic: false);
        var model = symbol.ToModel(CancellationToken.None, opts);

        Assert.IsTrue(model.Attributes.Count > 0, "Target has [Obsolete]; Attributes should be populated.");

        var attr = model.Attributes[0];
        StringAssert.Contains(attr.FullName, "ObsoleteAttribute");
    }

    [TestMethod]
    public void TypeModel_ModelParts_Fields_OnlyFieldsPopulated()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var opts = new ModelOptions(ModelParts.Fields, includeNonPublic: true);
        var model = symbol.ToModel(CancellationToken.None, opts);

        Assert.IsTrue(model.Fields.Count > 0, "Fields should be populated when ModelParts.Fields is set.");
        Assert.AreEqual(0, model.Methods.Count, "Methods should be empty when ModelParts.Fields only.");
        Assert.AreEqual(0, model.Properties.Count, "Properties should be empty when ModelParts.Fields only.");
        Assert.AreEqual(0, model.Attributes.Count, "Attributes should be empty when ModelParts.Fields only.");
        Assert.AreEqual(0, model.Events.Count, "Events should be empty when ModelParts.Fields only.");
        Assert.AreEqual(0, model.Constructors.Count, "Constructors should be empty when ModelParts.Fields only.");
        Assert.AreEqual(0, model.Interfaces.Count, "Interfaces should be empty when ModelParts.Fields only.");
    }

    [TestMethod]
    [ExpectedException(typeof(OperationCanceledException))]
    public void TypeModel_Cancellation_Throws()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Must throw OperationCanceledException on the first ThrowIfCancellationRequested call
        symbol.ToModel(cts.Token);
    }

    [TestMethod]
    public void TypeModel_Name_And_FullName_And_Namespace()
    {
        var symbol = TypeModelingTestHelper.GetTypeSymbol(SHARED_SOURCE, "TestNs.Target");
        var model = symbol.ToModel(CancellationToken.None);

        Assert.AreEqual("Target", model.Name);
        Assert.AreEqual("TestNs", model.Namespace);
        Assert.IsTrue(model.FullName.Contains("Target"),
            $"FullName expected to contain 'Target', was '{model.FullName}'.");
    }

    [TestMethod]
    public void TypeModel_IsUnboundGenericType_False_ForNonGenericType()
    {
        const string SOURCE = """
            namespace TestNs {
                public struct TestStruct { }
            }
            """;

        var symbol = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.TestStruct");
        var model = symbol.ToModel(CancellationToken.None);

        Assert.IsFalse(model.IsUnboundGenericType);
        Assert.IsFalse(model.IsGeneric);
    }

    [TestMethod]
    public void TypeModel_IsUnboundGenericType_True_ForUnboundGeneric()
    {
        const string SOURCE = """
            namespace TestNs {
                public class Generic<T> { }
            }
            """;

        var symbol = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.Generic`1");
        // Construct the unbound generic form: Generic<>
        var unbound = symbol.ConstructUnboundGenericType();
        var model = unbound.ToModel(CancellationToken.None);

        Assert.IsTrue(model.IsUnboundGenericType);
        Assert.IsTrue(model.IsGeneric);
    }

    [TestMethod]
    public void TypeModel_EnumUnderlyingSpecialType_Correct_ForByteEnum()
    {
        const string SOURCE = """
            namespace TestNs {
                public enum TestEnum : byte { A, B, C }
            }
            """;

        var symbol = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.TestEnum");
        var model = symbol.ToModel(CancellationToken.None);

        Assert.IsTrue(model.IsEnum);
        Assert.AreEqual(Microsoft.CodeAnalysis.SpecialType.System_Byte, model.EnumUnderlyingSpecialType);
    }

    [TestMethod]
    public void TypeModel_EnumUnderlyingSpecialType_None_ForNonEnum()
    {
        const string SOURCE = """
            namespace TestNs {
                public struct TestStruct { }
            }
            """;

        var symbol = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.TestStruct");
        var model = symbol.ToModel(CancellationToken.None);

        Assert.IsFalse(model.IsEnum);
        Assert.AreEqual(Microsoft.CodeAnalysis.SpecialType.None, model.EnumUnderlyingSpecialType);
    }

    [TestMethod]
    public void FieldModel_ConstantValueNumeric_Correct_ForEnumFields()
    {
        const string SOURCE = """
            namespace TestNs {
                public enum TestEnum { A = 0, B = 5, C = 10 }
            }
            """;

        var symbol = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.TestEnum");
        var opts = new ModelOptions(ModelParts.Fields, includeNonPublic: true);
        var model = symbol.ToModel(CancellationToken.None, opts);

        // Enum fields: A=0, B=5, C=10 (plus compiler-generated "value__" field — excluded by default)
        var fieldB = model.Fields.FirstOrDefault(f => f.Name == "B");
        Assert.IsTrue(fieldB.HasConstantValue, "Field B should have HasConstantValue=true.");
        Assert.AreEqual(5UL, fieldB.ConstantValueNumeric, "Field B should have ConstantValueNumeric=5.");

        var fieldC = model.Fields.FirstOrDefault(f => f.Name == "C");
        Assert.IsTrue(fieldC.HasConstantValue);
        Assert.AreEqual(10UL, fieldC.ConstantValueNumeric);
    }
}
