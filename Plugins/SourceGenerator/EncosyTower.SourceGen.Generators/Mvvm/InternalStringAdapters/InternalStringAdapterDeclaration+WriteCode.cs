using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    partial class InternalStringAdapterDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters.InternalStringAdapterGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string IADAPTER = "ETMVB.IAdapter";
        private const string ADAPTER_ATTRIBUTE = "[ETMVB.Adapter(sourceType: typeof({0}), destType: typeof(string), order: 1)]";
        private const string LABEL_ATTRIBUTE = "[ETA.Label(\"{0}\", \"{1}\")]";
        private const string VARIANT = "ETV.Variant";
        private const string CACHED_VARIANT_CONVERTER = "ETVC.CachedVariantConverter";
        private const string GENERATOR_NAME = nameof(InternalStringAdapterGenerator);

        public void GenerateAdapters(
              SourceProductionContext context
            , string assemblyName
            , bool outputSourceGenFiles
        )
        {
            var fileName = $"InternalStringAdapters_{assemblyName}";
            var stableHashCode = SourceGenHelpers.GetStableHashCode(string.Empty) & 0x7fffffff;
            var hintName = $"{fileName}__{GENERATOR_NAME}_{stableHashCode}_0.g.cs";
            var sourceFilePath = GeneratorHelpers.BuildSourceFilePath(assemblyName, hintName);

            context.OutputSource(
                  outputSourceGenFiles
                , PrintAdditionalUsings()
                , WriteAdapter(Candidates, assemblyName)
                , string.Empty
                , hintName
                , sourceFilePath
                , Location.None
            );
        }

        private static string WriteAdapter(ImmutableArray<StringAdapterSpec> types, string assemblyName)
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
                    var typeName = type.fullTypeName;
                    var label = $"{type.labelName} ⇒ String";

                    p.PrintLine(string.Format(ADAPTER_ATTRIBUTE, typeName));
                    p.PrintLine(string.Format(LABEL_ATTRIBUTE, label, $"Generated/{type.namespaceName}"));
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public sealed class {adapterTypeName} : {IADAPTER}");
                    p.OpenScope();
                    {
                        p.PrintLine($"private readonly {CACHED_VARIANT_CONVERTER}<{typeName}> _converter = {CACHED_VARIANT_CONVERTER}<{typeName}>.Default;");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
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

        private static string AdapterTypeName(StringAdapterSpec typeRef)
            => $"{typeRef.identifierName}ToStringAdapter";

        private static string PrintAdditionalUsings()
        {
            var p = Printer.Default;

            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ETA = global::EncosyTower.Annotations;");
            p.PrintLine("using ETMVB = global::EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using ETV = global::EncosyTower.Variants;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            return p.Result;
        }
    }
}
