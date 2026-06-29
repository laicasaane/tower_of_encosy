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

    /// <summary>
    /// Minimal references (core + netstandard) for running a generator against self-contained stub sources
    /// that do not depend on Unity or EncosyTower runtime assemblies.
    /// </summary>
    internal static IEnumerable<MetadataReference> CoreReferences { get; } = new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("netstandard").Location),
        MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
    };

    /// <summary>
    /// Runs <typeparamref name="TGenerator"/> against <paramref name="source"/> compiled with the supplied
    /// <paramref name="references"/>, returning the generated source texts. Asserts the generator itself
    /// produced no error diagnostics (binding errors in the input compilation are ignored, since stub code
    /// may reference types only present in the generated output).
    /// </summary>
    internal static string[] RunDriverAndGetGeneratedSources<TGenerator>(
          string source
        , IEnumerable<MetadataReference> references
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
              source
            , CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
        );

        var compilation = CSharpCompilation.Create(
              "TestAssembly"
            , new[] { syntaxTree }
            , references.ToImmutableArray()
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return RunDriver<TGenerator>(compilation);
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

        return RunDriver<TGenerator>(compilation);
    }

    /// <summary>
    /// Compiles <paramref name="source"/> into an in-memory assembly and returns it as a reference, so a
    /// second compilation can exercise cross-assembly scenarios. Throws if the source fails to compile.
    /// </summary>
    internal static MetadataReference CompileToReference(
          string source
        , IEnumerable<MetadataReference> references
        , string assemblyName
    )
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
              source
            , CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10)
        );

        var compilation = CSharpCompilation.Create(
              assemblyName
            , new[] { syntaxTree }
            , references.ToImmutableArray()
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var stream = new System.IO.MemoryStream();
        var result = compilation.Emit(stream);

        if (result.Success == false)
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(
                $"Reference compilation '{assemblyName}' failed:\n{string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
        }

        stream.Position = 0;
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static string[] RunDriver<TGenerator>(CSharpCompilation compilation)
        where TGenerator : IIncrementalGenerator, new()
    {
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
