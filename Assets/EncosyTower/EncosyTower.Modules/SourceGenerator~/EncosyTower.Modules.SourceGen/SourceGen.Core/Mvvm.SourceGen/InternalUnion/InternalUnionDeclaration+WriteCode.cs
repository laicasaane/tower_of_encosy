using System;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.InternalUnionSourceGen
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    partial class InternalUnionDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Mvvm.InternalUnionGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string STRUCT_LAYOUT = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
        public const string META_OFFSET = "[global::System.Runtime.InteropServices.FieldOffset(global::EncosyTower.Modules.Unions.UnionBase.META_OFFSET)]";
        public const string DATA_OFFSET = "[global::System.Runtime.InteropServices.FieldOffset(global::EncosyTower.Modules.Unions.UnionBase.DATA_OFFSET)]";
        public const string UNION_TYPE = "global::EncosyTower.Modules.Unions.Union";
        public const string UNION_DATA_TYPE = "global::EncosyTower.Modules.Unions.UnionData";
        public const string UNION_TYPE_KIND = "global::EncosyTower.Modules.Unions.UnionTypeKind";
        public const string DOES_NOT_RETURN = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]";
        public const string GENERATED_INTERNAL_UNIONS = "[global::EncosyTower.Modules.Unions.SourceGen.GeneratedInternalUnions]";

        public const string GENERATOR_NAME = nameof(InternalUnionGenerator);

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
                var source = GetSourceForInternalClass(ValueTypeRefs, RefTypeRefs, assemblyName);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME);

                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                var fileName = $"InternalUnions_{assemblyName}";

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

        private static string GetSourceForInternalClass(
              ImmutableArray<TypeRef> valueTypes
            , ImmutableArray<TypeRef> refTypes
            , string assemblyName
        )
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Modules.Unions.__Internal.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Contains auto-generated unions for types that are the type of either");
                p.PrintLine("/// [ObservableProperty] properties or the parameter of [RelayCommand] methods.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// Automatically register these unions");
                p.PrintLine("/// to <see cref=\"EncosyTower.Modules.Unions.Converters.UnionConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// These unions are not intended to be used directly by user-code");
                p.PrintLine("/// thus they are declared <c>private</c> inside this class.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_INTERNAL_UNIONS).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine("public static partial class InternalUnions");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("static InternalUnions()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Register all unions inside this class");
                    p.PrintLine("/// to <see cref=\"EncosyTower.Modules.Unions.Converters.UnionConverter\"/>");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("public static void Register() => Init();");
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var typeRef in valueTypes)
                        {
                            WriteTryRegister(ref p, typeRef);
                        }

                        p.PrintEndLine();

                        foreach (var typeRef in refTypes)
                        {
                            WriteTryRegister(ref p, typeRef);
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

            static void WriteTryRegister(ref Printer p, TypeRef typeRef)
            {
                var symbol = typeRef.Symbol;
                var typeName = symbol.ToFullName();
                var simpleTypeName = symbol.ToSimpleName();
                var identifier = symbol.ToValidIdentifier();
                var converterDefault = $"Union__{identifier}.Converter.Default";

                if (typeRef.UnmanagedSize.HasValue)
                {
                    p.PrintBeginLine("if (").Print(UNION_DATA_TYPE).Print(".BYTE_COUNT >= ")
                        .Print(typeRef.UnmanagedSize.Value.ToString())
                        .PrintEndLine(")");
                    p.OpenScope();
                }

                p.OpenScope($"Register<{typeName}>({converterDefault}");
                {
                    p.Print("#if UNITY_EDITOR && MODULE_CORE_MVVM_LOG_INTERNAL_UNIONS_REGISTRIES").PrintEndLine();
                    p.PrintBeginLine(", \"").Print(simpleTypeName).PrintEndLine("\"");
                    p.Print("#endif").PrintEndLine();
                }

                p.CloseScope(");");

                if (typeRef.UnmanagedSize.HasValue)
                {
                    p.CloseScope();
                }

                p.PrintEndLine();
            }
        }

        private static void WriteRegisterMethod(ref Printer p)
        {
            p.Print("#if !UNITY_EDITOR || !MODULE_CORE_MVVM_LOG_INTERNAL_UNIONS_REGISTRIES").PrintEndLine();
            p.PrintLine(AGGRESSIVE_INLINING);
            p.Print("#endif").PrintEndLine();
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
            p.OpenScope("private static void Register<T>(");
            {
                p.PrintLine("  global::EncosyTower.Modules.Unions.Converters.IUnionConverter<T> converter");
                p.Print("#if UNITY_EDITOR && MODULE_CORE_MVVM_LOG_INTERNAL_UNIONS_REGISTRIES").PrintEndLine();
                p.PrintLine(", string typeName");
                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope(")");
            p.OpenScope();
            {
                p.Print("#if UNITY_EDITOR && MODULE_CORE_MVVM_LOG_INTERNAL_UNIONS_REGISTRIES").PrintEndLine();
                p.PrintLine("var result =");
                p.Print("#endif").PrintEndLine();

                p.PrintLine("global::EncosyTower.Modules.Unions.Converters.UnionConverter.TryRegister<T>(converter);");
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR && MODULE_CORE_MVVM_LOG_INTERNAL_UNIONS_REGISTRIES").PrintEndLine();

                p.PrintLine("if (result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.Log(\"Register internal union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("global::UnityEngine.Debug.LogError(\"Cannot register internal union for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();

                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
