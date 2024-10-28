using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;

namespace EncosyTower.Modules.EnumTemplates.SourceGen
{
    partial class EnumTemplateDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.EnumTemplates.SourceGen.EnumTemplateGenerator\", \"1.0.0\")]";
        private const string IENUM_TEMPLATE = "global::EncosyTower.Modules.EnumExtensions.IEnumTemplate<{0}>";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public string WriteCode()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            WritePartialStruct(ref p);

            var accessKeyword = Accessibility.ToKeyword();

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("[global::EncosyTower.Modules.EnumExtensions.SourceGen.GeneratedFromEnumTemplate(typeof(")
                .Print(TemplateFullName).PrintEndLine("))]");
            p.PrintBeginLine(accessKeyword).Print(" enum ").Print(EnumName).Print(" : ").Print(UnderlyingTypeName)
                .PrintEndLine();
            p.OpenScope();
            {
                WriteMembers(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            ExtensionsRef.WriteCode(ref p);
            WriteConvertMethods(ref p);

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WritePartialStruct(ref Printer p)
        {
            var structName = Syntax.Identifier.Text;

            p.PrintBeginLine("partial struct ").Print(structName).Print(" ")
                .Print(": ").Print(string.Format(IENUM_TEMPLATE, EnumName))
                .PrintEndLine(" { }");
            p.PrintEndLine();
        }

        private void WriteMembers(ref Printer p)
        {
            var isNext = false;

            foreach (var memberRef in MemberRefs)
            {
                var member = memberRef.member;

                if (memberRef.isComment)
                {
                    if (isNext)
                    {
                        p.PrintEndLine();
                    }

                    p.PrintBeginLine("/// <seealso cref=\"").Print(member.name).PrintEndLine("\"/>");
                }
                else
                {
                    p.PrintBeginLine(member.name).Print(" = ")
                        .Print(member.order.ToString()).PrintEndLine(",");
                }

                isNext = true;
            }
        }

        private void WriteConvertMethods(ref Printer p)
        {
            var memberRefs = MemberRefs;
            var map = MemberIndexMap;
            var @this = ExtensionsRef.ParentIsNamespace ? "this " : "";
            var thisName = EnumName;

            p.PrintBeginLine("static partial class ").PrintEndLine(ExtensionsRef.ExtensionsName);
            p.OpenScope();
            {
                foreach (var kvp in map)
                {
                    var typeSymbol = kvp.Key;
                    var otherName = typeSymbol.ToFullName();
                    var indexOrIndices = kvp.Value;

                    if (indexOrIndices.Indices is { } indices)
                    {
                        WriteToEnumTypeXXXMethod(ref p, memberRefs, @this, thisName, otherName, indices);
                        WriteTryConvertXXXMethod(ref p, memberRefs, @this, thisName, otherName, indices);
                    }
                    else if (indexOrIndices.Index.HasValue)
                    {
                        var index = indexOrIndices.Index.Value;

                        WriteToXXXMethod(ref p, memberRefs, @this, thisName, otherName, index);
                    }
                }

                if (memberRefs.Count > 0)
                {
                    WriteTryConvertMethod(ref p, memberRefs, @this, thisName);
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToXXXMethod(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string @this
            , string thisName
            , string otherName
            , int index
        )
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static ").Print(thisName).Print(" To").Print(thisName)
                .Print("(").Print(@this).Print(otherName).PrintEndLine(" self)");
            p.OpenScope();
            {
                var memberRef = memberRefs[index];
                var member = memberRef.member;

                p.PrintBeginLine("return ").Print(thisName).Print(".").Print(member.name).PrintEndLine(";");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteToEnumTypeXXXMethod(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string @this
            , string thisName
            , string otherName
            , List<int> indices
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static ").Print(thisName).Print(" To").Print(thisName)
                .Print("(").Print(@this).Print(otherName).PrintEndLine(" self)");
            p.OpenScope();
            {
                p.PrintBeginLine("return self switch").PrintEndLine();
                p.OpenScope();
                {
                    foreach (var index in indices)
                    {
                        var memberRef = memberRefs[index];

                        if (memberRef.isComment)
                        {
                            continue;
                        }

                        var member = memberRef.member;

                        p.PrintBeginLine(otherName).Print(".").Print(member.name)
                            .Print(" => ").Print(thisName).Print(".").Print(member.name)
                            .PrintEndLine(",");
                    }

                    p.PrintLine("_ => default,");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteTryConvertXXXMethod(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string @this
            , string thisName
            , string otherName
            , List<int> indices
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool TryConvert(")
                .Print(@this).Print(thisName).Print(" self, out ").Print(otherName).PrintEndLine(" result)");
            p.OpenScope();
            {
                p.PrintLine("switch (self)");
                p.OpenScope();
                {
                    foreach (var index in indices)
                    {
                        var memberRef = memberRefs[index];

                        if (memberRef.isComment)
                        {
                            continue;
                        }

                        var member = memberRef.member;

                        p.PrintBeginLine("case ").Print(thisName).Print(".").Print(member.name).PrintEndLine(":");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = ").Print(otherName).Print(".").Print(member.name).PrintEndLine(";");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("result = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteTryConvertMethod(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string @this
            , string thisName
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public static bool TryConvert(")
                .Print(@this).Print(thisName).PrintEndLine(" self, out ulong order, out ulong value)");
            p.OpenScope();
            {
                p.PrintLine("switch (self)");
                p.OpenScope();
                {
                    foreach (var memberRef in memberRefs)
                    {
                        if (memberRef.isComment)
                        {
                            continue;
                        }

                        var member = memberRef.member;

                        p.PrintBeginLine("case ").Print(thisName).Print(".").Print(member.name).PrintEndLine(":");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("order = ").Print(memberRef.baseOrder.ToString()).PrintEndLine(";");
                            p.PrintBeginLine("value = ").Print(memberRef.value.ToString()).PrintEndLine(";");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("order = default;");
                p.PrintLine("value = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
