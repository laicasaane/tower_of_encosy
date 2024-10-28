using EncosyTower.Modules.SourceGen;

namespace EncosyTower.Modules.Entities.SourceGen
{
    internal abstract class LookupCodeWriter
    {
        protected const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        protected const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Entities.SourceGen.LookupGenerator\", \"1.0.0\")]";
        protected const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        protected const string ENTITY = "global::Unity.Entities.Entity";
        protected const string BOOL = "global::EncosyTower.Modules.Bool<";

        public string Write(LookupDeclaration declaration)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, declaration.Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("partial struct ")
                .Print(declaration.Syntax.Identifier.Text)
                .Print(" : EncosyTower.Modules.Entities.Lookups.ILookups")
                .PrintEndLine();

            WriteInterfaces(ref p, declaration);

            p.OpenScope();
            {
                WriteStructBody(ref p, declaration);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteInterfaces(ref Printer p, LookupDeclaration declaration)
        {
            if (declaration.TypeRefs.Length < 1)
            {
                return;
            }

            p = p.IncreasedIndent();

            foreach (var typeRef in declaration.TypeRefs)
            {
                if (typeRef.IsReadOnly)
                {
                    p.PrintBeginLine(", ").Print(declaration.InterfaceLookupRO);
                }
                else
                {
                    p.PrintBeginLine(", ").Print(declaration.InterfaceLookupRW);
                }

                p.Print("<").Print(typeRef.Symbol.ToFullName()).PrintEndLine(">");
            }

            p = p.DecreasedIndent();
        }

        protected static void WriteFields(ref Printer p, LookupDeclaration declaration, string lookup)
        {
            foreach (var typeRef in declaration.TypeRefs)
            {
                if (typeRef.IsReadOnly)
                {
                    p.PrintBeginLine("[global::Unity.Collections.ReadOnly] internal ");
                }
                else
                {
                    p.PrintBeginLine("/*           Read-Write           */ internal ");
                }

                p.Print(lookup)
                    .Print(typeRef.Symbol.ToFullName())
                    .Print("> ").Print(GetLookupFieldName(typeRef))
                    .PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        protected static void WriteConstructor(ref Printer p, LookupDeclaration declaration, string getLookup)
        {
            Write(ref p, declaration, getLookup, "ref global::Unity.Entities.SystemState state", "state");
            Write(ref p, declaration, getLookup, "global::Unity.Entities.SystemBase system", "system");

            static void Write(ref Printer p, LookupDeclaration declaration, string getLookup, string arg, string variable)
            {
                p.PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ")
                    .Print(declaration.Syntax.Identifier.Text)
                    .Print("(").Print(arg).PrintEndLine(")");
                p.OpenScope();
                {
                    for (var i = 0; i < declaration.TypeRefs.Length; i++)
                    {
                        var typeRef = declaration.TypeRefs[i];
                        var fieldName = GetLookupFieldName(typeRef);
                        var isReadOnly = typeRef.IsReadOnly ? "true" : "false";

                        p.PrintBeginLine(fieldName).Print(" = ")
                            .Print(variable).Print(".").Print(getLookup).Print("<")
                            .Print(typeRef.Symbol.ToFullName())
                            .Print(">(").Print(isReadOnly)
                            .PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        protected static void WriteUpdateMethods(ref Printer p, LookupDeclaration declaration)
        {
            Write(ref p, declaration, "ref global::Unity.Entities.SystemState state", "ref state");
            Write(ref p, declaration, "global::Unity.Entities.SystemBase system", "system");

            static void Write(ref Printer p, LookupDeclaration declaration, string arg0, string arg1)
            {
                p.PrintLineIf(declaration.TypeRefs.Length < 2, GENERATED_CODE);
                p.PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void Update(").Print(arg0).PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var typeRef in declaration.TypeRefs)
                    {
                        p.PrintBeginLine(GetLookupFieldName(typeRef))
                            .Print(".Update(").Print(arg1).PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        protected static void WriteBeginRegion(ref Printer p, string typeName)
        {
            p.PrintBeginLine("#region    ").PrintEndLine(typeName);
            p.PrintEndLine();
        }

        protected static void WriteEndRegion(ref Printer p, string typeName)
        {
            p.PrintBeginLine("#endregion ").PrintEndLine(typeName);
            p.PrintEndLine();
        }

        protected static string GetLookupFieldName(LookupDeclaration.TypeRef typeRef)
            => $"_lookup_{typeRef.Symbol.ToValidIdentifier()}";

        protected static string GetLookupVarName(LookupDeclaration.TypeRef typeRef)
            => $"lookup_{typeRef.Symbol.ToValidIdentifier()}";

        protected abstract void WriteStructBody(ref Printer p, LookupDeclaration declaration);
    }
}