using System;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.GenericUnionSourceGen
{
    partial class GenericUnionDeclaration
    {
        public void GenerateUnionForValueTypes(
              SourceProductionContext context
            , Compilation compilation
            , bool outputSourceGenFiles
            , DiagnosticDescriptor errorDescriptor
        )
        {
            foreach (var structRef in ValueTypeRefs)
            {
                try
                {
                    var syntax = structRef.Syntax;
                    var syntaxTree = syntax.SyntaxTree;
                    var source = WriteGenericUnionForValueType(structRef);
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

        private static string WriteGenericUnionForValueType(StructRef structRef)
        {
            var typeName = structRef.TypeArgument.ToFullName();
            var structName = structRef.Syntax.Identifier.Text;
            var unionName = $"Union<{typeName}>";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, structRef.Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintLine(STRUCT_LAYOUT);
            p.PrintBeginLine()
                .Print("partial struct ").Print(structName)
                .PrintEndLine();
            p.OpenScope();
            {
                p = WriteFields(typeName, unionName, p);
                p = WriteConstructors(typeName, structName, unionName, p);
                p = WriteValidateTypeIdMethod(typeName, unionName, p);
                p = WirteImplicitConversions(typeName, structName, unionName, p);
                p = WriteConverterClass(typeName, structName, unionName, p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;

            #region METHODS
            #endregion ====

            static Printer WriteFields(string typeName, string unionName, Printer p)
            {
                p.PrintLine(META_OFFSET).PrintLine(GENERATED_CODE);
                p.PrintLine($"public readonly {unionName} Union;");
                p.PrintEndLine();

                p.PrintLine(DATA_OFFSET).PrintLine(GENERATED_CODE);
                p.PrintLine($"public readonly {typeName} Value;");
                p.PrintEndLine();

                return p;
            }

            static Printer WriteConstructors(string typeName, string structName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public {structName}({typeName} value)");
                p.OpenScope();
                {
                    p.PrintLine($"this.Union = new {UNION_TYPE}({UNION_TYPE_KIND}.ValueType, {unionName}.TypeId);");
                    p.PrintLine("this.Value = value;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public {structName}(in {unionName} union) : this()");
                p.OpenScope();
                {
                    p.PrintLine("this.Union = union;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine($"public {structName}(in {UNION_TYPE} union) : this()");
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
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

            static Printer WirteImplicitConversions(string typeName, string structName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public static implicit operator {structName}({typeName} value) => new {structName}(value);");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public static implicit operator {UNION_TYPE}(in {structName} value) => value.Union;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public static implicit operator {unionName}(in {structName} value) => value.Union;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                p.PrintLine($"public static implicit operator {structName}(in {unionName} value) => new {structName}(value);");
                p.PrintEndLine();
                return p;
            }

            static Printer WriteConverterClass(string typeName, string structName, string unionName, Printer p)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintBeginLine()
                    .Print("public sealed class Converter")
                    .Print($" : global::EncosyTower.Modules.Unions.Converters.IUnionConverter<{typeName}>")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintLine("public static readonly Converter Default = new Converter();");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine("private Converter() { }");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                    p.PrintLine($"public {UNION_TYPE} ToUnion({typeName} value) => new {structName}(value);");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                    p.PrintLine($"public {unionName} ToUnionT({typeName} value) => new {structName}(value).Union;");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
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

                        p.PrintLine($"var temp = new {structName}(union);");
                        p.PrintLine("return temp.Value;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine($"public bool TryGetValue(in {UNION_TYPE} union, out {typeName} result)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {structName}(union);");
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

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine($"public bool TrySetValueTo(in {UNION_TYPE} union, ref {typeName} result)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {structName}(union);");
                            p.PrintLine("result = temp.Value;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLine($"public string ToString(in {UNION_TYPE} union)");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (union.TypeId == {unionName}.TypeId)");
                        p.OpenScope();
                        {
                            p.PrintLine($"var temp = new {structName}(union);");
                            p.PrintLine("return temp.Value.ToString();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return global::EncosyTower.Modules.TypeIdExtensions.ToType(union.TypeId).ToString();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(DOES_NOT_RETURN);
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
