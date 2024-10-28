#if UNITY_EDITOR && ANNULUS_CODEGEN && MODULE_CORE_MVVM_ADAPTERS_GENERATOR

using System;
using EncosyTower.Modules.CodeGen;
using UnityCodeGen;

namespace EncosyTower.Modules.Editor.Mvvm.Unions.Converters
{
    [Generator]
    internal class UnionConvertersGenerator : ICodeGenerator
    {
        private static readonly string[] s_unionTypes = new string[] {
            "bool",
            "byte",
            "sbyte",
            "char",
            "double",
            "float",
            "int",
            "uint",
            "long",
            "ulong",
            "short",
            "ushort",
        };

        private static readonly string[] s_unionTypeNames = new string[] {
            "Bool",
            "Byte",
            "SByte",
            "Char",
            "Double",
            "Float",
            "Int",
            "UInt",
            "Long",
            "ULong",
            "Short",
            "UShort",
        };

        private const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";

        public void Execute(GeneratorContext context)
        {
            var nameofGenerator = nameof(UnionConvertersGenerator);

            if (CodeGenAPI.TryGetOutputFolderPath(nameofGenerator, out var outputPath) == false)
            {
                context.OverrideFolderPath("Assets");
                return;
            }

            var p = Printer.DefaultLarge;
            p.PrintAutoGeneratedBlock(nameofGenerator);
            p.PrintEndLine();
            p.PrintLine(@"#pragma warning disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
");

            p.PrintLine("namespace EncosyTower.Modules.Unions.Converters");
            p.OpenScope();
            {
                var unionTypes = s_unionTypes.AsSpan();
                var unionTypeNames = s_unionTypeNames.AsSpan();

                p.PrintLine("partial class UnionConverter");
                p.OpenScope();
                {
                    p.PrintLine("static partial void TryRegisterGeneratedConverters()");
                    p.OpenScope();
                    {
                        for (var i = 0; i < unionTypes.Length; i++)
                        {
                            var typeName = unionTypeNames[i];

                            p.PrintBeginLine("TryRegister(UnionConverter").Print(typeName)
                                .PrintEndLine(".Default);");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                for (var i = 0; i < unionTypes.Length; i++)
                {
                    var type = unionTypes[i];
                    var typeName = unionTypeNames[i];

                    p.PrintBeginLine("internal sealed class UnionConverter").Print(typeName)
                        .Print(" : IUnionConverter<").Print(type).PrintEndLine(">");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("public static readonly UnionConverter")
                            .Print(typeName)
                            .Print(" Default = new UnionConverter")
                            .Print(typeName)
                            .PrintEndLine("();")
                            .PrintEndLine();

                        p.PrintBeginLine("private UnionConverter").Print(typeName).PrintEndLine("() { }")
                            .PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public Union ToUnion(").Print(type)
                            .PrintEndLine(" value) => new Union(value);")
                            .PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public Union<").Print(type).Print("> ToUnionT(")
                            .Print(type).PrintEndLine(" value) => new Union(value);")
                            .PrintEndLine();

                        p.PrintBeginLine("public ").Print(type).PrintEndLine(" GetValue(in Union union)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("if (union.TryGetValue(out ").Print(type).PrintEndLine(" result) == false)");
                            p.OpenScope();
                            {
                                p.PrintLine("ThrowIfInvalidCast();");
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintLine("return result;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public bool TryGetValue(in Union union, out ").Print(type)
                            .PrintEndLine(" result) => union.TryGetValue(out result);")
                            .PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public bool TrySetValueTo(in Union union, ref ").Print(type)
                            .PrintEndLine(" dest) => union.TrySetValueTo(ref dest);")
                            .PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public string ToString(in Union union) => union.").Print(typeName)
                            .PrintEndLine(".ToString();")
                            .PrintEndLine();

                        p.PrintLine("[HideInCallstack, DoesNotReturn]");
                        p.PrintLine("[Conditional(\"UNITY_EDITOR\"), Conditional(\"DEVELOPMENT_BUILD\")]");
                        p.PrintLine("private static void ThrowIfInvalidCast()");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("throw new InvalidCastException($\"Cannot get value of {typeof(")
                                .Print(type)
                                .PrintEndLine(")} from the input union.\");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            context.OverrideFolderPath(outputPath);
            context.AddCode($"UnionConverters.gen.cs", p.Result);
        }
    }
}

#endif