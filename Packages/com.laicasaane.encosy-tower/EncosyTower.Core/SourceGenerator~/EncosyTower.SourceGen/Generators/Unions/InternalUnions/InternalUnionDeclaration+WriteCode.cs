using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Unions.InternalUnions
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using static UnionPrinter;

    partial class InternalUnionDeclaration
    {
        private const string CONVERTER_DEFAULT = "Union__{0}.Converter.Default";
        public const string GENERATED_INTERNAL_UNIONS = $"[global::{NAMESPACE}.SourceGen.GeneratedInternalUnions]";
        public const string GENERATOR_NAME = nameof(InternalUnionGenerator);

        public string InternalUnionsNamespace { get; set; }
            = "EncosyTower.Unions.__InternalUnions__";

        public string GeneratedCodeAttribute { get; set; }
            = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.Unions.InternalUnions.InternalUnionGenerator\", \"{SourceGenVersion.VALUE}\")]";

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
                var fileName = $"InternalUnions_{assemblyName}";
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
            var unionPrinter = new UnionPrinter(GeneratedCodeAttribute);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(InternalUnionsNamespace)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Contains auto-generated unions for types that are the type of either");
                p.PrintLine("/// [ObservableProperty] properties or the parameter of [RelayCommand] methods.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// Automatically register these unions");
                p.PrintLine($"/// to <see cref=\"{NAMESPACE}.Converters.UnionConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// These unions are not intended to be used directly by user-code");
                p.PrintLine("/// thus they are declared <c>private</c> inside this class.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_INTERNAL_UNIONS)
                    .PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class InternalUnions");
                p.OpenScope();
                {
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("static InternalUnions()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Register all unions inside this class");
                    p.PrintLine($"/// to <see cref=\"{NAMESPACE}.Converters.UnionConverter\"/>");
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
                            var typeName = symbol.ToFullName();
                            var simpleTypeName = symbol.ToSimpleName();
                            var identifier = symbol.ToValidIdentifier();
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
                        , null
                        , syntax
                        , WriteUnion(typeRef, compilation.Assembly.Name, true)
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
                        , null
                        , syntax
                        , WriteUnion(typeRef, compilation.Assembly.Name, false)
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

        private string WriteUnion(TypeRef typeRef, string assemblyName, bool isValueType)
        {
            var symbol = typeRef.Symbol;
            var typeName = symbol.ToFullName();
            var identifier = symbol.ToValidIdentifier();
            var structName = $"Union__{identifier}";
            var unionName = $"global::EncosyTower.Unions.Union<{typeName}>";

            var p = Printer.DefaultLarge;
            var unionPrinter = new UnionPrinter(GeneratedCodeAttribute);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(InternalUnionsNamespace)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("static partial class InternalUnions");
                p.OpenScope();
                {
                    p.PrintLine(GeneratedCodeAttribute).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLineIf(isValueType, STRUCT_LAYOUT);
                    p.PrintBeginLine()
                        .Print("private partial struct ").Print(structName)
                        .Print($" : global::EncosyTower.Unions.IUnion<{typeName}>")
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
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }
    }
}
