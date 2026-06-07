using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Variants
{
    internal static class VariantSpecWriteCode
    {
        private const string GENERATOR_NAME_REG = "VariantRegistrationDeclaration";

        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.VariantRegistrationGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string PRESERVE = "[UES.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[UE.RuntimeInitializeOnLoadMethod(UE.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string GENERATED_GENERIC_VARIANTS = "[ETVSG.GeneratedGenericVariants]";

        public static void WriteStaticRegistrationClass(
              ref SourceProductionContext context
            , ImmutableArray<VariantSpec> declarations
            , CompilationInfo compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
            , string projectPath = null
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = $"{GENERATOR_NAME_REG}__AttributeVariants__{assemblyName.ToValidIdentifier()}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , PrintAdditionalUsings()
                    , BuildRegistrationClassSource(declarations, assemblyName)
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

        private static string BuildRegistrationClassSource(
              ImmutableArray<VariantSpec> declarations
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Variants.__AttributeVariants__.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Automatically registers attribute-based variants");
                p.PrintLine($"/// to <see cref=\"ETVC.VariantConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_GENERIC_VARIANTS)
                    .PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class AttributeVariants");
                p.OpenScope();
                {
                    p.PrintLine(PRESERVE);
                    p.PrintLine("static AttributeVariants()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        var wroteValueType = false;

                        foreach (var decl in declarations)
                        {
                            if (decl.isValueType == false)
                            {
                                continue;
                            }

                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.typeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );

                            wroteValueType = true;
                        }

                        if (wroteValueType)
                        {
                            p.PrintEndLine();
                        }

                        foreach (var decl in declarations)
                        {
                            if (decl.isValueType)
                            {
                                continue;
                            }

                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.typeName
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

        public static void WriteRedundantTypeMarker(
              ref SourceProductionContext context
            , in VariantSpec declaration
            , CompilationInfo compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
            , string projectPath = null
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hintName = $"{GENERATOR_NAME_REG}__Redundant__{declaration.fileHintName}.g.cs";
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.openingSource
                    , BuildRedundantTypeSource(in declaration)
                    , declaration.closingSource
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
                    , declaration.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static string BuildRedundantTypeSource(in VariantSpec declaration)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var fullTypeName = declaration.fullTypeName;

            p.PrintLine("/// <summary>");
            p.PrintLine($"/// A variant has already been implemented for <see cref=\"{fullTypeName}\"/>.");
            p.PrintLine("/// This declaration is redundant and can be removed.");
            p.PrintLine("/// </summary>");
            p.PrintLine($"[global::System.Obsolete(\"A variant has already been implemented for {fullTypeName}. This declaration is redundant and can be removed.\")]");
            p.PrintLine($"partial struct {declaration.structName} {{ }}");

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
