using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests.TypeModeling;

internal static class TypeModelingTestHelper
{
    private static readonly IReadOnlyList<MetadataReference> s_references = BuildReferences();

    internal static INamedTypeSymbol GetTypeSymbol(string source, string typeName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10));

        var compilation = CSharpCompilation.Create(
            assemblyName: "TypeModelingTestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: s_references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var symbol = compilation.GetTypeByMetadataName(typeName)
            ?? throw new System.InvalidOperationException(
                $"Type '{typeName}' not found in compiled source. " +
                $"Compilation errors: {string.Join(", ", compilation.GetDiagnostics())}");

        return symbol;
    }

    private static List<MetadataReference> BuildReferences()
    {
        // Resolve the directory containing mscorlib / System.Runtime for net8 runtime
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Attribute).Assembly.Location),
        };

        // Add System.Runtime if present (needed for netstandard cross-targeting refs)
        var systemRuntime = Path.Combine(runtimeDir, "System.Runtime.dll");
        if (File.Exists(systemRuntime))
            refs.Add(MetadataReference.CreateFromFile(systemRuntime));

        // Add netstandard.dll if available
        var netstandardPath = Path.Combine(runtimeDir, "netstandard.dll");
        if (File.Exists(netstandardPath))
            refs.Add(MetadataReference.CreateFromFile(netstandardPath));

        return refs;
    }
}
