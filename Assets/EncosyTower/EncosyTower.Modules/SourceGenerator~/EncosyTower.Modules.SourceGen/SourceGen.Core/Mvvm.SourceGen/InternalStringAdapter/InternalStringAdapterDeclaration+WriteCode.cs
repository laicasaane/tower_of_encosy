using System;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EncosyTower.Modules.Mvvm.InternalStringAdapterSourceGen
{
    partial class InternalStringAdapterDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Mvvm.InternalStringAdapterGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string IADAPTER = "global::EncosyTower.Modules.Mvvm.ViewBinding.IAdapter";
        private const string ADAPTER_ATTRIBUTE = "[global::EncosyTower.Modules.Mvvm.ViewBinding.Adapter(sourceType: typeof({0}), destType: typeof(string), order: 1)]";
        private const string LABEL_ATTRIBUTE = "[global::EncosyTower.Modules.Label(\"{0}\", \"{1}\")]";
        private const string UNION = "global::EncosyTower.Modules.Unions.Union";
        private const string CACHED_UNION_CONVERTER = "global::EncosyTower.Modules.Unions.Converters.CachedUnionConverter";
        private const string GENERATOR_NAME = nameof(InternalStringAdapterGenerator);

        public void GenerateAdapters(
              SourceProductionContext context
            , Compilation compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            var syntax = CompilationUnit().NormalizeWhitespace(eol: "\n");

            try
            {
                var syntaxTree = syntax.SyntaxTree;
                var assemblyName = compilation.Assembly.Name;
                var source = WriteAdapter(TypeRefs, assemblyName);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                var fileName = $"InternalStringAdapters_{assemblyName}";

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, fileName, syntax)
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      errorDescriptor
                    , syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static string WriteAdapter(ImmutableArray<TypeRef> types, string assemblyName)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Modules.Mvvm.ViewBinding.__InternalStringAdapters.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                foreach (var type in types)
                {
                    var adapterTypeName = AdapterTypeName(type);
                    var typeName = type.Symbol.ToFullName();
                    var label = $"{type.Symbol.Name} ⇒ String";

                    p.PrintLine(string.Format(ADAPTER_ATTRIBUTE, typeName));
                    p.PrintLine(string.Format(LABEL_ATTRIBUTE, label, $"Generated/{type.Symbol.ContainingNamespace}"));
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public sealed class {adapterTypeName} : {IADAPTER}");
                    p.OpenScope();
                    {
                        p.PrintLine(GENERATED_CODE);
                        p.PrintLine($"private readonly {CACHED_UNION_CONVERTER}<{typeName}> _converter = new {CACHED_UNION_CONVERTER}<{typeName}>();");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintLine($"public {UNION} Convert(in {UNION} union)");
                        p.OpenScope();
                        {
                            p.PrintLine("return this._converter.ToString(union);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            return p.Result;
        }

        private static string AdapterTypeName(TypeRef typeRef)
            => $"{typeRef.Symbol.ToValidIdentifier()}ToStringAdapter";
    }
}
