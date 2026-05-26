using EncosyTower.SourceGen.Analyzers.Mvvm.RelayCommands;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EncosyTower.SourceGen.Tests.Mvvm.RelayCommands;

[TestClass]
public class RelayCommandDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = MvvmAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<RelayCommandDiagnosticAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<RelayCommandDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = "",
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
    {
        var test = new CSharpAnalyzerTest<RelayCommandDiagnosticAnalyzer, DefaultVerifier>
        {
            TestCode = STUB_ATTRIBUTES,
        };

        return test.RunAsync();
    }

    [TestMethod]
    public Task ObservableObjectClassWithoutRelayCommand_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    private void Plain() { }
                }
            """);

    [TestMethod]
    public Task ZeroParameterRelayCommand_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run() { }
                }
            """);

    [TestMethod]
    public Task SingleByValueParameter_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run(int value) { }
                }
            """);

    [TestMethod]
    public Task SingleInParameter_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run(in int value) { }
                }
            """);

    [TestMethod]
    public Task TwoParameters_ReportsTooManyParameters()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private void {|#0:Run|}(int a, int b) { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.TooManyParameters)
                .WithLocation(0)
                .WithArguments("Run", 2)
        );

    [TestMethod]
    public Task ThreeParameters_ReportsTooManyParameters()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private void {|#0:Run|}(int a, int b, int c) { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.TooManyParameters)
                .WithLocation(0)
                .WithArguments("Run", 3)
        );

    [TestMethod]
    public Task RefParameter_ReportsInvalidParameterRefKind()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private void {|#0:Run|}(ref int value) { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.InvalidParameterRefKind)
                .WithLocation(0)
                .WithArguments("Run", "Ref")
        );

    [TestMethod]
    public Task OutParameter_ReportsInvalidParameterRefKind()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private void {|#0:Run|}(out int value) { value = 0; }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.InvalidParameterRefKind)
                .WithLocation(0)
                .WithArguments("Run", "Out")
        );

    [TestMethod]
    public Task MissingObservableObject_ReportsOnFirstAttribute()
        => RunAsync(
              """
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand|}]
                      private void Run() { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.MissingObservableObject)
                .WithLocation(0)
                .WithArguments("Vm")
        );

    [TestMethod]
    public Task MissingObservableObject_MultipleRelayMethods_ReportsOnce()
        => RunAsync(
              """
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand|}]
                      private void First() { }

                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private void Second() { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.MissingObservableObject)
                .WithLocation(0)
                .WithArguments("Vm")
        );

    [TestMethod]
    public Task CanExecute_MissingMethod_ReportsCanExecuteNotFound()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "Missing")|}]
                      private void Run() { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.CanExecuteNotFound)
                .WithLocation(0)
                .WithArguments("Missing", "Run", "Vm")
        );

    [TestMethod]
    public Task CanExecute_PresentMethod_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")]
                    private void Run() { }

                    private bool CanRun() => true;
                }
            """);

    [TestMethod]
    public Task CanExecute_NonBoolReturn_ReportsWrongReturnType()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")|}]
                      private void Run() { }

                      private int CanRun() => 1;
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.CanExecuteWrongReturnType)
                .WithLocation(0)
                .WithArguments("CanRun", "Run")
        );

    [TestMethod]
    public Task CanExecute_BoolReturn_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")]
                    private void Run(int value) { }

                    private bool CanRun(int value) => true;
                }
            """);

    [TestMethod]
    public Task CanExecute_ParameterCountMismatch_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")|}]
                      private void Run(int value) { }

                      private bool CanRun(int a, int b) => true;
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.CanExecuteParameterCountMismatch)
                .WithLocation(0)
                .WithArguments("CanRun", "Run", 2, 1)
        );

    [TestMethod]
    public Task CanExecute_ZeroParametersAlwaysAllowed_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")]
                    private void Run(int value) { }

                    private bool CanRun() => true;
                }
            """);

    [TestMethod]
    public Task CanExecute_ParameterTypeMismatch_Reports()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [{|#0:EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")|}]
                      private void Run(int value) { }

                      private bool CanRun(string value) => true;
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.CanExecuteParameterTypeMismatch)
                .WithLocation(0)
                .WithArguments("CanRun", "Run")
        );

    [TestMethod]
    public Task CanExecute_MatchingParameterTypes_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand(CanExecute = "CanRun")]
                    private void Run(int value) { }

                    private bool CanRun(int value) => true;
                }
            """);

    [TestMethod]
    public Task StaticMethod_ReportsStaticNotSupported()
        => RunAsync(
              """
                  [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                  public partial class Vm
                  {
                      [EncosyTower.Mvvm.Input.RelayCommand]
                      private static void {|#0:Run|}() { }
                  }
              """
            , new DiagnosticResult(RelayCommandDiagnosticAnalyzer.StaticNotSupported)
                .WithLocation(0)
                .WithArguments("Run")
        );

    [TestMethod]
    public Task InstanceMethod_NoStaticDiagnostic()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run() { }
                }
            """);

    [TestMethod]
    public Task GenericClassWithRelayCommand_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm<T>
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run(T value) { }
                }
            """);

    [TestMethod]
    public Task PartialClassWithRelayCommand_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Vm
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Run() { }
                }

                public partial class Vm
                {
                    private bool CanRun() => true;
                }
            """);

    [TestMethod]
    public Task NestedClassWithRelayCommand_NoDiagnostics()
        => RunAsync("""
                public class Outer
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                    public partial class Inner
                    {
                        [EncosyTower.Mvvm.Input.RelayCommand]
                        private void Run() { }
                    }
                }
            """);
}
