using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalVariants
{
    using InternalVariantDeclaration = Variants.InternalVariantDeclaration;
    using VariantPrinter = Variants.VariantPrinter;

    internal static class MvvmInternalVariantWriteCode
    {
        public const string GENERATOR_NAME = "MvvmInternalVariantDeclaration";

        private const string INTERNAL_VARIANTS_NAMESPACE = "EncosyTower.Mvvm.__InternalVariants__";
        private const string VARIANTS_NAMESPACE = "EncosyTower.Variants";
        private const string MVVM_NAMESPACE = "EncosyTower.Mvvm";

        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string GENERATED_INTERNAL_VARIANTS = $"[global::{VARIANTS_NAMESPACE}.SourceGen.GeneratedInternalVariants]";

        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.InternalVariants.InternalVariantGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public static void WriteVariantCode(
              ref SourceProductionContext context
            , in InternalVariantDeclaration decl
            , string assemblyName
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = $"{GENERATOR_NAME}__{decl.fileHintName}.g.cs";
            var sourceFilePath = BuildSourceFilePath(assemblyName, hintName);

            try
            {
                var source = BuildVariantSource(in decl, assemblyName);

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(context, decl.location, sourceFilePath, sourceText);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, decl.location, e.ToUnityPrintableString()));
            }
        }

        private static string BuildVariantSource(in InternalVariantDeclaration decl, string assemblyName)
        {
            var typeName = decl.fullTypeName;
            var structName = decl.structName;
            var isValueType = decl.isValueType;
            var variantName = $"global::{VARIANTS_NAMESPACE}.Variant<{typeName}>";

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
                        .Print($" : global::{VARIANTS_NAMESPACE}.IVariant<{typeName}>")
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
            , ImmutableArray<InternalVariantDeclaration> valueTypes
            , ImmutableArray<InternalVariantDeclaration> refTypes
            , string assemblyName
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = $"{GENERATOR_NAME}__InternalVariants__{assemblyName.ToValidIdentifier()}.g.cs";
            var sourceFilePath = BuildSourceFilePath(assemblyName, hintName);

            try
            {
                var source = BuildStaticClassSource(valueTypes, refTypes, assemblyName);

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(context, Location.None, sourceFilePath, sourceText);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, Location.None, e.ToUnityPrintableString()));
            }
        }

        private static string BuildStaticClassSource(
              ImmutableArray<InternalVariantDeclaration> valueTypes
            , ImmutableArray<InternalVariantDeclaration> refTypes
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
                p.PrintLine("/// Contains auto-generated variants for types detected from usage of");
                p.PrintLine($"/// <c>[ObservableProperty]</c>, <c>[RelayCommand]</c>, <c>[BindingProperty]</c>, or <c>[BindingCommand]</c>");
                p.PrintLine($"/// in <c>{MVVM_NAMESPACE}</c>.");
                p.PrintLine("/// <br/>");
                p.PrintLine($"/// Automatically register these variants to <see cref=\"{VARIANTS_NAMESPACE}.Converters.VariantConverter\"/>");
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
                    p.PrintLine($"/// to <see cref=\"{VARIANTS_NAMESPACE}.Converters.VariantConverter\"/>.");
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

        internal static string BuildSourceFilePath(string assemblyName, string hintName)
        {
            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
        }
    }
}
