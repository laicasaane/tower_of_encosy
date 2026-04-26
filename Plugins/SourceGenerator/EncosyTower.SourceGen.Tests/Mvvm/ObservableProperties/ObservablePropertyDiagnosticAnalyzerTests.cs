using System.Threading.Tasks;
using EncosyTower.SourceGen.Analyzers.Mvvm.ObservableProperties;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Mvvm.ObservableProperties;

[TestClass]
public class ObservablePropertyDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class ObservableObjectAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
            public class ObservablePropertyAttribute : System.Attribute
            {
                public ObservablePropertyAttribute() { }
                public ObservablePropertyAttribute(string name) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true)]
            public class NotifyPropertyChangedForAttribute : System.Attribute
            {
                public NotifyPropertyChangedForAttribute(string propertyName) { }
                public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true)]
            public class NotifyCanExecuteChangedForAttribute : System.Attribute
            {
                public NotifyCanExecuteChangedForAttribute(string commandName) { }
                public NotifyCanExecuteChangedForAttribute(string commandName, params string[] otherCommandNames) { }
            }
        }
        namespace EncosyTower.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class RelayCommandAttribute : System.Attribute { }
        }
        """;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ObservablePropertyDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = Wrap(body),
        };

        foreach (var diag in expected)
        {
            test.ExpectedDiagnostics.Add(diag);
        }

        return test.RunAsync();
    }

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
    {
        var test = new CSharpAnalyzerTest<ObservablePropertyDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<ObservablePropertyDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ObservableObjectClassWithoutObservableProperty_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    private int _count;
                }
            """);

    [TestMethod]
    public Task ObservablePropertyOnField_WithObservableObject_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    private int _count;
                }
            """);

    [TestMethod]
    public Task MissingObservableObject_ReportsOnMember()
        => RunAsync(
              """
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      private int {|#0:_count|};
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.MissingObservableObject)
                .WithLocation(0)
                .WithArguments("_count", "Vm")
        );

    [TestMethod]
    public Task NotifyPropertyChangedFor_TargetMissing_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      [{|#0:EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor("Missing")|}]
                      private int _count;
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.NotifyPropertyChangedForTargetMissing)
                .WithLocation(0)
                .WithArguments("Missing", "_count", "Vm")
        );

    [TestMethod]
    public Task NotifyPropertyChangedFor_TargetPresent_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    [EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor("Total")]
                    private int _count;

                    public int Total => _count;
                }
            """);

    [TestMethod]
    public Task NotifyCanExecuteChangedFor_TargetMissing_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      [{|#0:EncosyTower.Mvvm.ComponentModel.NotifyCanExecuteChangedFor("MissingCommand")|}]
                      private int _count;
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.NotifyCanExecuteChangedForTargetMissing)
                .WithLocation(0)
                .WithArguments("MissingCommand", "_count", "Missing", "Vm")
        );

    [TestMethod]
    public Task NotifyCanExecuteChangedFor_TargetPresent_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    [EncosyTower.Mvvm.ComponentModel.NotifyCanExecuteChangedFor("RunCommand")]
                    private int _count;

                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run() { }
                }
            """);

    [TestMethod]
    public Task StaticField_ReportsStaticNotSupported()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      private static int {|#0:_count|};
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.StaticMemberNotSupported)
                .WithLocation(0)
                .WithArguments("_count")
        );

    [TestMethod]
    public Task InstanceField_NoStaticDiagnostic()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    private int _count;
                }
            """);

    [TestMethod]
    public Task ReadOnlyField_ReportsReadOnlyNotSupported()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      private readonly int {|#0:_count|};
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.ReadOnlyFieldNotSupported)
                .WithLocation(0)
                .WithArguments("_count")
        );

    [TestMethod]
    public Task MutableField_NoReadOnlyDiagnostic()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    private int _count;
                }
            """);

    [TestMethod]
    public Task DuplicateGeneratedName_TwoFields_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      private int _count;

                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      private int {|#0:m_count|};
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.DuplicateGeneratedPropertyName)
                .WithLocation(0)
                .WithArguments("m_count", "Count", "_count")
        );

    [TestMethod]
    public Task UniqueGeneratedNames_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    private int _count;

                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    private int _total;
                }
            """);

    [TestMethod]
    public Task CorrelationArgNotStringConstant_NullInArrayPosition_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      [{|#0:EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor(null, "Total")|}]
                      private int _count;

                      public int Total => _count;
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.CorrelationArgNotStringConstant)
                .WithLocation(0)
                .WithArguments("NotifyPropertyChangedFor", "_count")
        );

    [TestMethod]
    public Task CorrelationArgWellFormedString_NoNotStringConstantDiagnostic()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    [EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor("Total")]
                    private int _count;

                    public int Total => _count;
                }
            """);

    [TestMethod]
    public Task CorrelationArgEmpty_NullSingleArg_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                      [{|#0:EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor(null)|}]
                      private int _count;
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.CorrelationArgEmpty)
                .WithLocation(0)
                .WithArguments("NotifyPropertyChangedFor", "_count")
        );

    [TestMethod]
    public Task CorrelationArgNonNull_NoEmptyDiagnostic()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    [EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedFor("Total")]
                    private int _count;

                    public int Total => _count;
                }
            """);

    [TestMethod]
    public Task InvalidCustomPropertyName_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.ComponentModel.ObservableProperty("123Bad")|}]
                      private int _count;
                  }
              """
            , new DiagnosticResult(ObservablePropertyDiagnosticAnalyzer.InvalidCustomPropertyName)
                .WithLocation(0)
                .WithArguments("123Bad", "_count")
        );

    [TestMethod]
    public Task ValidCustomPropertyName_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty("Counter")]
                    private int _count;
                }
            """);
}
