using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    partial class InternalStringAdapterDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters.InternalStringAdapterGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string IADAPTER = "global::EncosyTower.Mvvm.ViewBinding.IAdapter";
        private const string ADAPTER_ATTRIBUTE = "[global::EncosyTower.Mvvm.ViewBinding.Adapter(sourceType: typeof({0}), destType: typeof(string), order: 1)]";
        private const string LABEL_ATTRIBUTE = "[global::EncosyTower.Annotations.Label(\"{0}\", \"{1}\")]";
        private const string VARIANT = "global::EncosyTower.Variants.Variant";
        private const string CACHED_VARIANT_CONVERTER = "global::EncosyTower.Variants.Converters.CachedVariantConverter";
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
                var fileName = $"InternalStringAdapters_{assemblyName}";
                var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, fileName, syntax);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileName);

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , WriteAdapter(TypeRefs, assemblyName)
                    , hintName
                    , sourceFilePath
                );
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

            p.PrintLine($"namespace EncosyTower.Mvvm.ViewBinding.__InternalStringAdapters.{assemblyName.ToValidIdentifier()}");
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
                        p.PrintLine($"private readonly {CACHED_VARIANT_CONVERTER}<{typeName}> _converter = {CACHED_VARIANT_CONVERTER}<{typeName}>.Default;");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintLine($"public {VARIANT} Convert(in {VARIANT} variant)");
                        p.OpenScope();
                        {
                            p.PrintLine("return this._converter.ToString(variant);");
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
