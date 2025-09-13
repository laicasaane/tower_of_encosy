using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Variants.InternalVariants
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using static VariantPrinter;

    partial class InternalVariantDeclaration
    {
        private const string CONVERTER_DEFAULT = "Variant__{0}.Converter.Default";
        public const string GENERATED_INTERNAL_VARIANTS = $"[global::{NAMESPACE}.SourceGen.GeneratedInternalVariants]";
        public const string GENERATOR_NAME = nameof(InternalVariantGenerator);

        public string InternalVariantsNamespace { get; set; }
            = "EncosyTower.Variants.__InternalVariants__";

        public string GeneratedCodeAttribute { get; set; }
            = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Variants.InternalVariants.InternalVariantGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public void GenerateStaticClass(
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
                var fileName = $"InternalVariants_{assemblyName}";
                var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, fileName, syntax);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , WriteStaticClass(ValueTypeRefs, RefTypeRefs, assemblyName)
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

        private string WriteStaticClass(
              ImmutableArray<TypeRef> valueTypes
            , ImmutableArray<TypeRef> refTypes
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter(GeneratedCodeAttribute);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(InternalVariantsNamespace)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Contains auto-generated variants for types that are the type of either");
                p.PrintLine("/// [ObservableProperty] properties or the parameter of [RelayCommand] methods.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// Automatically register these variants");
                p.PrintLine($"/// to <see cref=\"{NAMESPACE}.Converters.VariantConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// These variants are not intended to be used directly by user-code");
                p.PrintLine("/// thus they are declared <c>private</c> inside this class.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_INTERNAL_VARIANTS)
                    .PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("static InternalVariants()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Register all variants inside this class");
                    p.PrintLine($"/// to <see cref=\"{NAMESPACE}.Converters.VariantConverter\"/>");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("public static void Register() => Init();");
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var typeRef in valueTypes)
                        {
                            var symbol = typeRef.Symbol;
                            var typeName = symbol.ToFullName();
                            var simpleTypeName = symbol.ToSimpleName();
                            var identifier = symbol.ToValidIdentifier();
                            var converterDefault = string.Format(CONVERTER_DEFAULT, identifier);

                            variantPrinter.WriteRegister(
                                  ref p
                                , typeName
                                , simpleTypeName
                                , converterDefault
                                , typeRef.UnmanagedSize
                            );
                        }

                        p.PrintEndLine();

                        foreach (var typeRef in refTypes)
                        {
                            var symbol = typeRef.Symbol;
                            var typeName = symbol.ToFullName();
                            var simpleTypeName = symbol.ToSimpleName();
                            var identifier = symbol.ToValidIdentifier();
                            var converterDefault = string.Format(CONVERTER_DEFAULT, identifier);

                            variantPrinter.WriteRegister(
                                  ref p
                                , typeName
                                , simpleTypeName
                                , converterDefault
                                , typeRef.UnmanagedSize
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

        public void GenerateVariantForValueTypes(
              SourceProductionContext context
            , Compilation compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            foreach (var typeRef in ValueTypeRefs)
            {
                try
                {
                    var syntax = typeRef.Syntax;
                    var syntaxTree = syntax.SyntaxTree;
                    var hintName = syntaxTree.GetGeneratedSourceFileName(
                          GENERATOR_NAME
                        , syntax
                        , typeRef.Symbol.ToValidIdentifier()
                    );

                    var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                          compilation.Assembly.Name
                        , GENERATOR_NAME
                    );

                    context.OutputSource(
                          outputSourceGenFiles
                        , null
                        , syntax
                        , WriteVariant(typeRef, compilation.Assembly.Name, true)
                        , hintName
                        , sourceFilePath
                    );
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          errorDescriptor
                        , typeRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        public void GenerateVariantForRefTypes(
              SourceProductionContext context
            , Compilation compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            foreach (var typeRef in RefTypeRefs)
            {
                try
                {
                    var syntax = typeRef.Syntax;
                    var syntaxTree = syntax.SyntaxTree;
                    var hintName = syntaxTree.GetGeneratedSourceFileName(
                          GENERATOR_NAME
                        , syntax
                        , typeRef.Symbol.ToValidIdentifier()
                    );

                    var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                          compilation.Assembly.Name
                        , GENERATOR_NAME
                    );

                    context.OutputSource(
                          outputSourceGenFiles
                        , null
                        , syntax
                        , WriteVariant(typeRef, compilation.Assembly.Name, false)
                        , hintName
                        , sourceFilePath
                    );
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          errorDescriptor
                        , typeRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private string WriteVariant(TypeRef typeRef, string assemblyName, bool isValueType)
        {
            var symbol = typeRef.Symbol;
            var typeName = symbol.ToFullName();
            var identifier = symbol.ToValidIdentifier();
            var structName = $"Variant__{identifier}";
            var variantName = $"global::EncosyTower.Variants.Variant<{typeName}>";

            var p = Printer.DefaultLarge;
            var variantPrinter = new VariantPrinter(GeneratedCodeAttribute);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(InternalVariantsNamespace)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLineIf(isValueType, STRUCT_LAYOUT);
                    p.PrintBeginLine()
                        .Print("private partial struct ").Print(structName)
                        .Print($" : global::EncosyTower.Variants.IVariant<{typeName}>")
                        .PrintEndLine();

                    variantPrinter.WriteVariantBody(
                          ref p
                        , isValueType
                        , isValueType || typeRef.Symbol.TypeKind != TypeKind.Interface
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
    }
}
