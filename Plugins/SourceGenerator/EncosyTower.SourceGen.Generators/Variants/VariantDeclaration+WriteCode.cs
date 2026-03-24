using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.Variants
{
    partial struct VariantDeclaration
    {
        private const string GENERATOR_NAME_STRUCT = "VariantStructDeclaration";

        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";
        private const string VARIANTS_NAMESPACE = "EncosyTower.Variants";
        private const string IVARIANT_T = $"global::{VARIANTS_NAMESPACE}.IVariant<";

        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.VariantStructGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public readonly void WriteVariantCode(
              ref SourceProductionContext context
            , CompilationCandidateSlim compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = $"{GENERATOR_NAME_STRUCT}__{fileHintName}.g.cs";
            var sourceFilePath = VariantDeclarationWriteCode.BuildSourceFilePath(compilation.assemblyName, hintName);
            var variantName = $"global::{VARIANTS_NAMESPACE}.Variant<{fullTypeName}>";

            try
            {
                var source = BuildVariantSource(variantName);

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(context, location, sourceFilePath, sourceText);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, location, e.ToUnityPrintableString()));
            }
        }

        private readonly string BuildVariantSource(string variantName)
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var hasNamespace = string.IsNullOrEmpty(namespaceName) == false;
            var numContainingTypes = containingTypes.Count;

            if (hasNamespace)
            {
                p.PrintLine($"namespace {namespaceName}");
                p.OpenScope();
            }

            for (int i = 0; i < numContainingTypes; i++)
            {
                p.PrintLine(containingTypes[i]);
                p.OpenScope();
            }

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

            for (int i = 0; i < numContainingTypes; i++)
            {
                p.CloseScope();
            }

            if (hasNamespace)
            {
                p.CloseScope();
            }

            return p.Result;
        }
    }

    internal static class VariantDeclarationWriteCode
    {
        private const string GENERATOR_NAME_REG = "VariantRegistrationDeclaration";

        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string VARIANTS_NAMESPACE = "EncosyTower.Variants";
        private const string GENERATED_GENERIC_VARIANTS = $"[global::{VARIANTS_NAMESPACE}.SourceGen.GeneratedGenericVariants]";

        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.VariantRegistrationGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public static void WriteStaticRegistrationClass(
              ref SourceProductionContext context
            , ImmutableArray<VariantDeclaration> declarations
            , CompilationCandidateSlim compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.assemblyName;
            var hintName = $"{GENERATOR_NAME_REG}__AttributeVariants__{assemblyName.ToValidIdentifier()}.g.cs";
            var sourceFilePath = BuildSourceFilePath(assemblyName, hintName);

            try
            {
                var source = BuildRegistrationClassSource(declarations, assemblyName);

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

        private static string BuildRegistrationClassSource(
              ImmutableArray<VariantDeclaration> declarations
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace {VARIANTS_NAMESPACE}.__AttributeVariants__.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Automatically registers attribute-based variants");
                p.PrintLine($"/// to <see cref=\"{VARIANTS_NAMESPACE}.Converters.VariantConverter\"/>");
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
            , in VariantDeclaration declaration
            , CompilationCandidateSlim compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = $"{GENERATOR_NAME_REG}__Redundant__{declaration.fileHintName}.g.cs";
            var sourceFilePath = BuildSourceFilePath(compilation.assemblyName, hintName);

            try
            {
                var source = BuildRedundantTypeSource(in declaration);

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(context, declaration.location, sourceFilePath, sourceText);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, declaration.location, e.ToUnityPrintableString()));
            }
        }

        private static string BuildRedundantTypeSource(in VariantDeclaration declaration)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var hasNamespace = string.IsNullOrEmpty(declaration.namespaceName) == false;
            var numContainingTypes = declaration.containingTypes.Count;

            if (hasNamespace)
            {
                p.PrintLine($"namespace {declaration.namespaceName}");
                p.OpenScope();
            }

            for (int i = 0; i < numContainingTypes; i++)
            {
                p.PrintLine(declaration.containingTypes[i]);
                p.OpenScope();
            }

            var fullTypeName = declaration.fullTypeName;

            p.PrintLine("/// <summary>");
            p.PrintLine($"/// A variant has already been implemented for <see cref=\"{fullTypeName}\"/>.");
            p.PrintLine("/// This declaration is redundant and can be removed.");
            p.PrintLine("/// </summary>");
            p.PrintLine($"[global::System.Obsolete(\"A variant has already been implemented for {fullTypeName}. This declaration is redundant and can be removed.\")]");
            p.PrintLine($"partial struct {declaration.structName} {{ }}");

            for (int i = 0; i < numContainingTypes; i++)
            {
                p.CloseScope();
            }

            if (hasNamespace)
            {
                p.CloseScope();
            }

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
