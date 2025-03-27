using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Unions.GenericUnions
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    partial class GenericUnionDeclaration
    {
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string NAMESPACE = "EncosyTower.Unions";
        public const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        public const string META_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.UnionBase.META_OFFSET)]";
        public const string DATA_OFFSET = $"[global::System.Runtime.InteropServices.FieldOffset(global::{NAMESPACE}.UnionBase.DATA_OFFSET)]";
        public const string UNION_TYPE = $"global::{NAMESPACE}.Union";
        public const string UNION_DATA_TYPE = $"global::{NAMESPACE}.UnionData";
        public const string UNION_TYPE_KIND = $"global::{NAMESPACE}.UnionTypeKind";
        public const string DOES_NOT_RETURN = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        public const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Unions.GenericUnions.GenericUnionGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string CONVERTER_DEFAULT = "{0}.Converter.Default";
        public const string GENERATED_GENERIC_UNIONS = $"[global::{NAMESPACE}.SourceGen.GeneratedGenericUnions]";
        public const string GENERATOR_NAME = nameof(GenericUnionDeclaration);

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
                var fileName = $"GenericUnions_{assemblyName}";
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

        public void GenerateRedundantTypes(
              SourceProductionContext context
            , Compilation compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            foreach (var typeRef in Redundants)
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
                        , syntax
                        , WriteRedundantType(typeRef)
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

        private static string WriteRedundantType(TypeRef structRef)
        {
            var typeName = structRef.TypeArgument.ToFullName();
            var structName = structRef.Syntax.Identifier.Text;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, structRef.Syntax.Parent);
            var p = scopePrinter.printer;

            p = p.IncreasedIndent();

            p.PrintLine("/// <summary>");
            p.PrintLine($"/// A union has already been implemented for <see cref=\"{typeName}\"/.");
            p.PrintLine("/// This declaration is redundant and can be removed.");
            p.PrintLine("/// </summary>");
            p.PrintLine($"[global::System.Obsolete(\"A union has already been implemented for {typeName}. This declaration is redundant and can be removed.\")]");
            p.PrintLine($"partial struct {structName} {{ }}");

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static string WriteStaticClass(
              ImmutableArray<TypeRef> valueTypes
            , ImmutableArray<TypeRef> refTypes
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;
            var unionPrinter = new UnionPrinter(GENERATED_CODE);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace {NAMESPACE}.__GenericUnions__.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Automatically registers generic unions");
                p.PrintLine($"/// to <see cref=\"{NAMESPACE}.Converters.UnionConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_GENERIC_UNIONS)
                    .PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class GenericUnions");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("static GenericUnions()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var typeRef in valueTypes)
                        {
                            var symbol = typeRef.Symbol;
                            var typeName = typeRef.TypeArgument.ToFullName();
                            var simpleTypeName = typeRef.TypeArgument.ToSimpleName();
                            var identifier = symbol.ToFullName();
                            var converterDefault = string.Format(CONVERTER_DEFAULT, identifier);

                            unionPrinter.WriteRegister(
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
                            var typeName = typeRef.TypeArgument.ToFullName();
                            var simpleTypeName = typeRef.TypeArgument.ToSimpleName();
                            var identifier = symbol.ToFullName();
                            var converterDefault = string.Format(CONVERTER_DEFAULT, identifier);

                            unionPrinter.WriteRegister(
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

                    unionPrinter.WriteRegisterMethod(ref p);
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }

        public void GenerateUnionForValueTypes(
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
                        , syntax
                        , WriteUnion(typeRef, true)
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

        public void GenerateUnionForRefTypes(
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
                        , syntax
                        , WriteUnion(typeRef, false)
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

        private static string WriteUnion(TypeRef typeRef, bool isValueType)
        {
            var symbol = typeRef.TypeArgument;
            var typeName = symbol.ToFullName();
            var structName = typeRef.Syntax.Identifier.Text;
            var unionName = $"global::EncosyTower.Unions.Union<{typeName}>";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, typeRef.Syntax.Parent);
            var p = scopePrinter.printer;
            var unionPrinter = new UnionPrinter(GENERATED_CODE);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintLineIf(isValueType, STRUCT_LAYOUT);
                p.PrintBeginLine()
                    .Print("partial struct ").Print(structName)
                    .PrintEndLine();

                unionPrinter.WriteUnionBody(
                    ref p
                    , isValueType
                    , isValueType || typeRef.Symbol.TypeKind != TypeKind.Interface
                    , typeName
                    , structName
                    , unionName
                );
            }
            p = p.DecreasedIndent();
            return p.Result;
        }
    }
}
