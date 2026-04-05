using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.Generators.Mvvm.RelayCommands;
using EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties;
using EncosyTower.SourceGen.Generators.Mvvm.InternalVariants;
using EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters;
using EncosyTower.SourceGen.Generators.Mvvm.Binders;
using EncosyTower.SourceGen.Generators.Mvvm.MonoBinders;

namespace EncosyTower.SourceGen.Tests.Mvvm;

[TestClass]
public class RelayCommandGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class ObservableObjectAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task RelayCommandGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<RelayCommandGenerator>();

    [TestMethod]
    public Task RelayCommandGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<RelayCommandGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class SampleViewModel { }
            }
            """);
}

[TestClass]
public class ObservablePropertyGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class ObservableObjectAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task ObservablePropertyGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<ObservablePropertyGenerator>();

    [TestMethod]
    public Task ObservablePropertyGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunDriverSmokeTestAsync<ObservablePropertyGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class SampleViewModel { }
            }
            """);
}

[TestClass]
public class InternalVariantGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class ObservableObjectAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Field)]
            public class ObservablePropertyAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
            public class NotifyPropertyChangedForAttribute : System.Attribute
            {
                public NotifyPropertyChangedForAttribute(params string[] names) { }
            }
        }
        namespace EncosyTower.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class RelayCommandAttribute : System.Attribute { }
        }
        namespace EncosyTower.Mvvm.ViewBinding
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public class BindingPropertyAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class BindingCommandAttribute : System.Attribute { }
        }
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
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class SampleViewModel { }
            }
            """);
}

[TestClass]
public class InternalStringAdapterGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class ObservableObjectAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Field)]
            public class ObservablePropertyAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
            public class NotifyPropertyChangedForAttribute : System.Attribute
            {
                public NotifyPropertyChangedForAttribute(params string[] names) { }
            }
        }
        namespace EncosyTower.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class RelayCommandAttribute : System.Attribute { }
        }
        namespace EncosyTower.Mvvm.ViewBinding
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public class BindingPropertyAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public class AdapterAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task InternalStringAdapterGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<InternalStringAdapterGenerator>();

    [TestMethod]
    public Task InternalStringAdapterGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<InternalStringAdapterGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class SampleViewModel { }
            }
            """);
}

[TestClass]
public class BinderGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ViewBinding
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class BinderAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task BinderGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<BinderGenerator>();

    [TestMethod]
    public Task BinderGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunDriverSmokeTestAsync<BinderGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Mvvm.ViewBinding.Binder]
                public partial class SampleBinder { }
            }
            """);
}

[TestClass]
public class MonoBinderGeneratorTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ViewBinding.Components
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class MonoBinderAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task MonoBinderGenerator_EmptyInput_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<MonoBinderGenerator>();

    [TestMethod]
    public Task MonoBinderGenerator_WithAttributeStub_DoesNotThrow()
        => GeneratorTestHelper.RunSmokeTestAsync<MonoBinderGenerator>($$"""
            {{STUB_ATTRIBUTES}}
            namespace TestProject
            {
                [EncosyTower.Mvvm.ViewBinding.Components.MonoBinder]
                public partial class SampleMonoBinder { }
            }
            """);
}
