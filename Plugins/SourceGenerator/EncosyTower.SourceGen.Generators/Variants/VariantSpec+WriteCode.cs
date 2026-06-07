using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Variants
{
    partial struct VariantSpec
    {
        private const string GENERATOR_NAME_STRUCT = "VariantStructDeclaration";

        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.VariantStructGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string STRUCT_LAYOUT = "[SRIS.StructLayout(SRIS.LayoutKind.Explicit)]";
        private const string PRESERVE = "[UES.Preserve]";
        private const string IVARIANT_T = "ETV.IVariant<";

        public readonly void WriteVariantCode(
              ref SourceProductionContext context
            , CompilationInfo compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
            , string projectPath = null
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hintName = $"{GENERATOR_NAME_STRUCT}__{fileHintName}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);
                var variantName = $"ETV.Variant<{fullTypeName}>";

                context.OutputSource(
                      outputSourceGenFiles
                    , openingSource
                    , BuildVariantSource(variantName)
                    , closingSource
                    , hintName
                    , sourceFilePath
                    , projectPath
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      errorDescriptor
                    , location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private readonly string BuildVariantSource(string variantName)
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLineIf(isValueType, STRUCT_LAYOUT);
            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(PRESERVE);
            p.PrintBeginLine()
                .Print("partial struct ").Print(structName)
                .Print($" : {IVARIANT_T}{fullTypeName}>")
                .PrintEndLine();

            variantPrinter.WriteVariantBody(
                  ref p
                , isValueType
                , hasImplicitFromStructToType
                , fullTypeName
                , structName
                , variantName
            );

            return p.Result;
        }
    }
}
