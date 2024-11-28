using System;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.InternalUnionSourceGen
{
    partial class InternalUnionDeclaration
    {
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
                    var source = GetSourceForInternalUnionForValueType(typeRef, compilation.Assembly.Name);
                    var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);

                    var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                          sourceFilePath
                        , syntax
                        , source
                        , context.CancellationToken
                    );

                    context.AddSource(
                          syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, typeRef.Symbol.ToValidIdentifier())
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
                        , typeRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private static string GetSourceForInternalUnionForValueType(TypeRef typeRef, string assemblyName)
        {
            var symbol = typeRef.Symbol;
            var typeName = symbol.ToFullName();
            var identifier = symbol.ToValidIdentifier();
            var internalUnionName = $"Union__{identifier}";
            var unionName = $"Union<{typeName}>";

            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Modules.Unions.__Internal.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("static partial class InternalUnions");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine(STRUCT_LAYOUT);
                    p.PrintBeginLine()
                        .Print("private partial struct ").Print(internalUnionName)
                        .Print($" : global::EncosyTower.Modules.Unions.IUnion<{typeName}>")
                        .PrintEndLine();
                    p.OpenScope();
                    {
                        p = WriteFields(typeName, unionName, p);
                        p = WriteConstructors(typeName, internalUnionName, unionName, p);
                        p = WriteValidateTypeIdMethod(typeName, unionName, p);
                        p = WirteImplicitConversions(typeName, internalUnionName, unionName, p);
                        p = WriteConverterClass(typeName, internalUnionName, unionName, p);
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;

            #region METHODS
            #endregion ====

            static Printer WriteFields(string typeName, string unionName, Printer p)
            {
                p.PrintLine(META_OFFSET).PrintLine(GENERATED_CODE).PrintLine("[Preserve]");
                p.PrintLine($"public readonly {unionName} Union;");
                p.PrintEndLine();

                p.PrintLine(DATA_OFFSET).PrintLine(GENERATED_CODE).PrintLine("[Preserve]");
                p.PrintLine($"public readonly {typeName} Value;");
                p.PrintEndLine();

                return p;
            }

            static Printer WriteConstructors(string typeName, string internalUnionName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine($"public {internalUnionName}({typeName} value)");
                p.OpenScope();
                {
                    p.PrintLine($"this.Union = new {UNION_TYPE}({UNION_TYPE_KIND}.ValueType, {unionName}.TypeId);");
                    p.PrintLine("this.Value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine($"public {internalUnionName}(in {unionName} union) : this()");
                p.OpenScope();
                {
                    p.PrintLine("this.Union = union;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine($"public {internalUnionName}(in {UNION_TYPE} union) : this()");
                p.OpenScope();
                {
                    p.PrintLine("ValidateTypeId(union);");
                    p.PrintLine("this.Union = union;");
                }
                p.CloseScope();
                p.PrintEndLine();
                return p;
            }

            static Printer WriteValidateTypeIdMethod(string typeName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintLine($"private static void ValidateTypeId(in {UNION_TYPE} union)");
                p.OpenScope();
                {
                    p.PrintLine($"if (union.TypeId != {unionName}.TypeId)");
                    p.OpenScope();
                    {
                        p.PrintLine("ThrowIfInvalidCast(union);");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(DOES_NOT_RETURN);
                p.PrintLine($"private static void ThrowIfInvalidCast(in {UNION_TYPE} union)");
                p.OpenScope();
                {
                    p.PrintLine("var type = global::EncosyTower.Modules.TypeIdExtensions.ToType(union.TypeId);");
                    p.PrintEndLine();

                    p.PrintLine("throw new global::System.InvalidCastException");
                    p.OpenScope("(");
                    {
                        p.PrintLine($"$\"Cannot cast {{type}} to {{typeof({typeName})}}\"");
                    }
                    p.CloseScope(");");
                }
                p.CloseScope();
                p.PrintEndLine();

                return p;
            }

            static Printer WirteImplicitConversions(string typeName, string internalUnionName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                p.PrintLine($"public static implicit operator {internalUnionName}({typeName} value) => new {internalUnionName}(value);");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                p.PrintLine($"public static implicit operator {UNION_TYPE}(in {internalUnionName} value) => value.Union;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                p.PrintLine($"public static implicit operator {unionName}(in {internalUnionName} value) => value.Union;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                p.PrintLine($"public static implicit operator {internalUnionName}(in {unionName} value) => new {internalUnionName}(value);");
                p.PrintEndLine();
                return p;
            }

            static Printer WriteConverterClass(string typeName, string internalUnionName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                p.PrintBeginLine()
                    .Print("public sealed class Converter")
                    .Print($" : global::EncosyTower.Modules.Unions.Converters.IUnionConverter<{typeName}>")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine("[Preserve]");
                    p.PrintLine("public static readonly Converter Default = new Converter();");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("private Converter() { }");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                    p.PrintLine($"public {UNION_TYPE} ToUnion({typeName} value) => new {internalUnionName}(value);");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine("[Preserve]");
                    p.PrintLine($"public {unionName} ToUnionT({typeName} value) => new {internalUnionName}(value).Union;");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine($"public {typeName} GetValue(in {UNION_TYPE} union)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId != {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine("ThrowIfInvalidCast();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine($"var temp = new {internalUnionName}(union);");
                        p.PrintLine("return temp.Value;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine($"public bool TryGetValue(in {UNION_TYPE} union, out {typeName} result)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {internalUnionName}(union);");
                            p.PrintLine("result = temp.Value;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("result = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine($"public bool TrySetValueTo(in {UNION_TYPE} union, ref {typeName} result)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {internalUnionName}(union);");
                            p.PrintLine("result = temp.Value;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine($"public string ToString(in {UNION_TYPE} union)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {internalUnionName}(union);");
                            p.PrintLine("return temp.Value.ToString();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return global::EncosyTower.Modules.TypeIdExtensions.ToType(union.TypeId).ToString();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine("[Preserve]");
                    p.PrintLine("[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]");
                    p.PrintLine("private static void ThrowIfInvalidCast()");
                    p.OpenScope();
                    {
                        p.PrintLine("throw new global::System.InvalidCastException");
                        p.OpenScope("(");
                        {
                            p.PrintLine($"$\"Cannot get value of {{typeof({typeName})}} from the input union.\"");
                        }
                        p.CloseScope(");");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();
                return p;
            }
        }
    }
}
