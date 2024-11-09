using System;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.GenericUnionSourceGen
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    partial class GenericUnionDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Unions.GenericUnionGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        public const string META_OFFSET = "[global::System.Runtime.InteropServices.FieldOffset(global::EncosyTower.Modules.Unions.UnionBase.META_OFFSET)]";
        public const string DATA_OFFSET = "[global::System.Runtime.InteropServices.FieldOffset(global::EncosyTower.Modules.Unions.UnionBase.DATA_OFFSET)]";
        public const string UNION_TYPE = "global::EncosyTower.Modules.Unions.Union";
        public const string UNION_DATA_TYPE = "global::EncosyTower.Modules.Unions.UnionData";
        public const string UNION_TYPE_KIND = "global::EncosyTower.Modules.Unions.UnionTypeKind";
        public const string DOES_NOT_RETURN = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]";
        public const string GENERATED_GENERIC_UNIONS = "[global::EncosyTower.Modules.Unions.SourceGen.GeneratedGenericUnions]";
        public const string PRESERVE = "[global::UnityEngine.Scripting.Preserve]";

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
                var source = WriteStaticClass(ValueTypeRefs, RefTypeRefs, assemblyName);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                var fileName = $"GenericUnions_{assemblyName}";

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, fileName, syntax)
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
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
            foreach (var structRef in Redundants)
            {
                try
                {
                    var syntax = structRef.Syntax;
                    var syntaxTree = syntax.SyntaxTree;
                    var source = WriteRedundantType(structRef);
                    var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);

                    var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                          sourceFilePath
                        , syntax
                        , source
                        , context.CancellationToken
                    );

                    context.AddSource(
                          syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, structRef.Symbol.ToValidIdentifier())
                        , outputSource
                    );

                    if (outputSourceGenFiles)
                    {
                        SourceGenHelpers.OutputSourceToFile(
                              context
                            , syntax.GetLocation()
                            , sourceFilePath
                            , outputSource
                        );
                    }
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          errorDescriptor
                        , structRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private static string WriteStaticClass(
              ImmutableArray<StructRef> valueTypes
            , ImmutableArray<StructRef> refTypes
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Modules.Unions.__Generics.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Automatically registers generic unions");
                p.PrintLine("/// to <see cref=\"EncosyTower.Modules.Unions.Converters.UnionConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_GENERIC_UNIONS).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine("public static partial class GenericUnions");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("static GenericUnions()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var structRef in valueTypes)
                        {
                            WriteRegister(ref p, structRef);
                        }

                        p.PrintEndLine();

                        foreach (var structRef in refTypes)
                        {
                            WriteRegister(ref p, structRef);
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    WriteRegisterMethod(ref p);

                    p.WritePreserveAttributeClass(GENERATED_CODE);
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;

            #region METHODS
            #endregion ====

            static void WriteRegister(ref Printer p, StructRef structRef)
            {
                var symbol = structRef.Symbol;
                var typeName = structRef.TypeArgument.ToFullName();
                var simpleTypeName = structRef.TypeArgument.ToSimpleName();
                var identifier = symbol.ToFullName();
                var converterDefault = $"{identifier}.Converter.Default";

                if (structRef.UnmanagedSize.HasValue)
                {
                    p.PrintBeginLine("if (").Print(UNION_DATA_TYPE).Print(".BYTE_COUNT >= ")
                        .Print(structRef.UnmanagedSize.Value.ToString())
                        .PrintEndLine(")");
                    p.OpenScope();
                }

                p.OpenScope($"Register<{typeName}>({converterDefault}");
                {
                    p.Print("#if UNITY_EDITOR && MODULE_MVVM_CORE_LOG_GENERIC_UNION_REGISTRIES").PrintEndLine();
                    p.PrintBeginLine(", \"").Print(simpleTypeName).PrintEndLine("\"");
                    p.Print("#endif").PrintEndLine();
                }
                p.CloseScope(");");

                if (structRef.UnmanagedSize.HasValue)
                {
                    p.CloseScope();
                }

                p.PrintEndLine();
            }
        }

        private static void WriteRegisterMethod(ref Printer p)
        {
            p.Print("#if !UNITY_EDITOR || !MODULE_MVVM_CORE_LOG_GENERIC_UNION_REGISTRIES").PrintEndLine();
            p.PrintLine(AGGRESSIVE_INLINING);
            p.Print("#endif").PrintEndLine();
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
            p.OpenScope("private static void Register<T>(");
            {
                p.PrintLine("  global::EncosyTower.Modules.Unions.Converters.IUnionConverter<T> converter");
                p.Print("#if UNITY_EDITOR && MODULE_MVVM_CORE_LOG_GENERIC_UNION_REGISTRIES").PrintEndLine();
                p.PrintLine(", string typeName");
                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope(")");
            p.OpenScope();
            {
                p.Print("#if UNITY_EDITOR && MODULE_MVVM_CORE_LOG_GENERIC_UNION_REGISTRIES").PrintEndLine();
                p.PrintLine("var result =");
                p.Print("#endif").PrintEndLine();

                p.PrintLine("global::EncosyTower.Modules.Unions.Converters.UnionConverter.TryRegister<T>(converter);");
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR && MODULE_MVVM_CORE_LOG_GENERIC_UNION_REGISTRIES").PrintEndLine();

                p.PrintLine("if (result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.Log(\"Register generic union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.LogError(\"Cannot register generic union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();

                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string WriteRedundantType(StructRef structRef)
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
    }
}
