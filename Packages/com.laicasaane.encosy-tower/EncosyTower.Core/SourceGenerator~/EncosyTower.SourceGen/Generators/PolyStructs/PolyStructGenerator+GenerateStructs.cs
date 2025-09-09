﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyStructs
{
    partial class PolyStructGenerator
    {
        private static void GenerateStructs(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , bool outputSourceGenFiles
            , ImmutableArray<StructRef> structRefs
        )
        {
            foreach (var structRef in structRefs)
            {
                try
                {
                    var syntax = structRef.Syntax;
                    var syntaxTree = syntax.SyntaxTree;
                    var compilation = compilationCandidate.compilation;
                    var assemblyName = compilation.Assembly.Name;

                    var source = WriteStruct(structRef);

                    context.OutputSource(
                          outputSourceGenFiles
                        , syntax
                        , source
                        , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, structRef.Identifier)
                        , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
                    );
                }
                catch (Exception e)
                {
                    if (e is OperationCanceledException)
                    {
                        throw;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                          s_errorDescriptor_1
                        , structRef.Syntax.GetLocation()
                        , e.ToUnityPrintableString()
                    ));
                }
            }
        }

        private static string WriteStruct(StructRef structRef)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, structRef.Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintLine($"partial struct {structRef.Syntax.Identifier.Text}");
            p.OpenScope();
            {
                WriteConstructor(ref p, structRef);
                WriteDeconstruct(ref p, structRef);

                foreach (var interfaceRef in structRef.Interfaces.Values)
                {
                    WriteImplicitToStruct(ref p, interfaceRef, structRef);
                    WriteImplicitFromStruct(ref p, interfaceRef, structRef);
                }
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteConstructor(
              ref Printer p
            , StructRef structRef
        )
        {
            var fields = structRef.Fields;

            if (fields.Length < 1)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {structRef.Syntax.Identifier.Text}(");
            p = p.IncreasedIndent();
            {
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var comma = i > 0 ? ", " : "  ";

                    p.PrintLine($"{comma}{field.Type.ToFullName()} {field.Name}");
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                foreach (var field in structRef.Fields)
                {
                    p.PrintLine($"this.{field.Name} = {field.Name};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteDeconstruct(
              ref Printer p
            , StructRef structRef
        )
        {
            var fields = structRef.Fields;

            if (fields.Length < 1)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public void Deconstruct(");
            p = p.IncreasedIndent();
            {
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var comma = i > 0 ? ", out " : "  out ";

                    p.PrintLine($"{comma}{field.Type.ToFullName()} {field.Name}");
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                foreach (var field in structRef.Fields)
                {
                    p.PrintLine($"{field.Name} = this.{field.Name};");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteImplicitToStruct(
              ref Printer p
            , InterfaceRef interfaceRef
            , StructRef structRef
        )
        {
            var mergedStructName = $"{interfaceRef.FullContainingNameWithDot}{interfaceRef.StructName}";

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static implicit operator {structRef.Syntax.Identifier.Text}(in {mergedStructName} value)");
            p.OpenScope();
            {
                var fields = structRef.Fields;

                if (fields.Length < 1)
                {
                    p.PrintLine($"return new {structRef.Syntax.Identifier.Text}();");
                }
                else
                {
                    p.PrintLine($"return new {structRef.Syntax.Identifier.Text}(");
                    p = p.IncreasedIndent();
                    {
                        for (var i = 0; i < fields.Length; i++)
                        {
                            var field = fields[i];
                            var comma = i > 0 ? ", " : "  ";

                            p.PrintLine($"{comma}value.{field.MergedName}");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine(");");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteImplicitFromStruct(
              ref Printer p
            , InterfaceRef interfaceRef
            , StructRef structRef
        )
        {
            var mergedStructName = $"{interfaceRef.FullContainingNameWithDot}{interfaceRef.StructName}";
            var @in = structRef.Symbol.IsReadOnly ? "in " : "";
            var typeId = interfaceRef.Verbose ? structRef.Identifier : structRef.Name;

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public static implicit operator {mergedStructName}({@in}{structRef.Syntax.Identifier.Text} value)");
            p.OpenScope();
            {
                var fields = structRef.Fields;

                p.PrintLine($"return new {mergedStructName} {{");
                p = p.IncreasedIndent();
                {
                    p.PrintLine($"typeId = {mergedStructName}.TypeId.{typeId},");

                    foreach (var field in fields)
                    {
                        p.PrintLine($"{field.MergedName} = value.{field.Name},");
                    }
                }
                p = p.DecreasedIndent();
                p.PrintLine("};");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
