#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable IDE0303 // Simplify collection initialization

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace EncosyTower.SourceGen.Tests;

/// <summary>
/// Helper for running source generator smoke tests.
/// </summary>
internal static class GeneratorTestHelper
{
    /// <summary>
    /// Runs <typeparamref name="TGenerator"/> on empty input via <see cref="CSharpSourceGeneratorTest{TSourceGenerator,TVerifier}"/>.
    /// Asserts no generator-exception diagnostics and skips generated-source comparison.
    /// </summary>
    internal static Task RunSmokeTestAsync<TGenerator>(string source = "")
        where TGenerator : new()
    {
        var test = new CSharpSourceGeneratorTest<TGenerator, DefaultVerifier>
        {
            TestCode = source,
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
        };

        return test.RunAsync();
    }

    /// <summary>
    /// Runs <typeparamref name="TGenerator"/> using the raw <see cref="CSharpGeneratorDriver"/>.
    /// Only asserts that the generator produces no error-level diagnostics of its own.
    /// Use this variant when the generated code references external assemblies not available
    /// in the test compilation (e.g., Unity, EncosyTower runtime types).
    /// </summary>
    internal static Task RunDriverSmokeTestAsync<TGenerator>(string source = "")
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
              source
            , CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
        );

        IEnumerable<PortableExecutableReference> references = UnityDllPaths.All
            .Select(p => MetadataReference.CreateFromFile(p))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
              "TestAssembly"
            , new[] { syntaxTree }
            , references.ToImmutableArray()
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new TGenerator();
        var driver = (CSharpGeneratorDriver)CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();

        var generatorErrors = System.Linq.Enumerable.SelectMany(
              runResult.Results
            , r => r.Exception is null ? r.Diagnostics : ImmutableArray.Create(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                          "GENTEST001"
                        , "Generator threw exception"
                        , r.Exception.ToString()
                        , "Test"
                        , DiagnosticSeverity.Error
                        , isEnabledByDefault: true
                    )
                    , Location.None
                )
            )
        ).Where(static d => d.Severity == DiagnosticSeverity.Error);

        foreach (var error in generatorErrors)
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                $"Generator produced error: {error}");
        }

        return Task.CompletedTask;
    }

    internal static string[] RunDriverAndGetGeneratedSources<TGenerator>(string source = "")
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
              source
            , CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
        );

        IEnumerable<PortableExecutableReference> references = UnityDllPaths.All
            .Select(p => MetadataReference.CreateFromFile(p))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
              "TestAssembly"
            , new[] { syntaxTree }
            , references.ToImmutableArray()
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new TGenerator();
        var driver = (CSharpGeneratorDriver)CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();

        var generatorErrors = System.Linq.Enumerable.SelectMany(
              runResult.Results
            , r => r.Exception is null ? r.Diagnostics : ImmutableArray.Create(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                          "GENTEST001"
                        , "Generator threw exception"
                        , r.Exception.ToString()
                        , "Test"
                        , DiagnosticSeverity.Error
                        , isEnabledByDefault: true
                    )
                    , Location.None
                )
            )
        ).Where(static d => d.Severity == DiagnosticSeverity.Error);

        foreach (var error in generatorErrors)
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                $"Generator produced error: {error}");
        }

        return runResult.Results
            .SelectMany(static x => x.GeneratedSources)
            .Select(static x => x.SourceText.ToString())
            .ToArray();
    }
}
