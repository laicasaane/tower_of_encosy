using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Variants
{
    internal static class InternalVariantWriteCode
    {
        public const string GENERATOR_NAME = "InternalVariantDeclaration";

        private const string INTERNAL_VARIANTS_NAMESPACE = "EncosyTower.Variants.__InternalVariants__";

        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.InternalVariantGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT = "[SRIS.StructLayout(SRIS.LayoutKind.Explicit)]";
        private const string PRESERVE = "[UES.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[UE.RuntimeInitializeOnLoadMethod(UE.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string GENERATED_INTERNAL_VARIANTS = "[ETVSG.GeneratedInternalVariants]";

        public static void WriteVariantCode(
              ref SourceProductionContext context
            , in InternalVariantSpec decl
            , string assemblyName
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
            , string projectPath = null
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hintName = $"{decl.fileHintName}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , PrintAdditionalUsings()
                    , BuildVariantSource(in decl, assemblyName)
                    , string.Empty
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
                    , decl.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static string BuildVariantSource(in InternalVariantSpec decl, string assemblyName)
        {
            var typeName = decl.fullTypeName;
            var structName = decl.structName;
            var isValueType = decl.isValueType;
            var variantName = $"ETV.Variant<{typeName}>";

            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(INTERNAL_VARIANTS_NAMESPACE)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLineIf(isValueType, STRUCT_LAYOUT);
                    p.PrintBeginLine()
                        .Print("private partial struct ").Print(structName)
                        .Print($" : ETV.IVariant<{typeName}>")
                        .PrintEndLine();

                    variantPrinter.WriteVariantBody(
                          ref p
                        , isValueType
                        , decl.hasImplicitFromStructToType
                        , typeName
                        , structName
                        , variantName
                    );
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }

        public static void WriteStaticClass(
              ref SourceProductionContext context
            , ImmutableArray<InternalVariantSpec> valueTypes
            , ImmutableArray<InternalVariantSpec> refTypes
            , string assemblyName
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
            , string projectPath = null
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hintName = $"InternalVariants__{assemblyName.ToValidIdentifier()}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , PrintAdditionalUsings()
                    , BuildStaticClassSource(valueTypes, refTypes, assemblyName)
                    , string.Empty
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
                    , Location.None
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static string BuildStaticClassSource(
              ImmutableArray<InternalVariantSpec> valueTypes
            , ImmutableArray<InternalVariantSpec> refTypes
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(INTERNAL_VARIANTS_NAMESPACE)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Contains auto-generated variants for types detected from usage of either");
                p.PrintLine($"/// <c>Variant&lt;T&gt;.GetConverter()</c> or <c>CachedVariantConverter&lt;T&gt;.Default</c>.");
                p.PrintLine("/// <br/>");
                p.PrintLine($"/// Automatically register these variants to <see cref=\"ETVC.VariantConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// These variants are not intended to be used directly by user-code");
                p.PrintLine("/// thus they are declared <c>private</c> inside this class.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_INTERNAL_VARIANTS)
                    .PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(PRESERVE);
                    p.PrintLine("static InternalVariants()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Register all variants inside this class");
                    p.PrintLine($"/// to <see cref=\"ETVC.VariantConverter\"/>.");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(PRESERVE);
                    p.PrintLine("public static void Register() => Init();");
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var decl in valueTypes)
                        {
                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.simpleTypeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );
                        }

                        p.PrintEndLine();

                        foreach (var decl in refTypes)
                        {
                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.simpleTypeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    variantPrinter.WriteRegisterMethod(ref p);
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }

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
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETT = global::EncosyTower.Types;");
            p.PrintLine("using ETV = global::EncosyTower.Variants;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using ETVSG = global::EncosyTower.Variants.SourceGen;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            return p.Result;
        }
    }
}