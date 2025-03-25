using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    partial class EnumTemplateDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.EnumTemplates.EnumTemplateGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string IENUM_TEMPLATE = "global::EncosyTower.EnumExtensions.IEnumTemplate<{0}>";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        private readonly static (string, string)[] s_equalityOps = new[] { ("==", ""), ("!=", "!") };

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
            p.PrintBeginLine("[global::EncosyTower.EnumExtensions.SourceGen.GeneratedFromEnumTemplate(typeof(")
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
            WriteAdditionalWrapper(ref p);
            WriteAdditionalExtensions(ref p);

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
                    foreach (var attribute in memberRef.attributes)
                    {
                        p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                    }

                    p.PrintBeginLine(member.name).Print(" = ")
                        .Print(member.order.ToString()).PrintEndLine(",");
                }

                isNext = true;
            }
        }

        private void WriteAdditionalWrapper(ref Printer p)
        {
            var memberRefs = MemberRefs;
            var map = MemberIndexMap;

            p.PrintBeginLine("partial struct ").PrintEndLine(ExtensionsRef.ExtensionsWrapperName);
            p = p.IncreasedIndent();
            {
                var isFirst = true;

                foreach (var kvp in map)
                {
                    var typeSymbol = kvp.Key;
                    var otherName = typeSymbol.ToFullName();
                    var indexOrIndices = kvp.Value;

                    if (indexOrIndices.Indices is { })
                    {
                        p.PrintBeginLineIf(isFirst, ":", ",")
                            .Print(" global::System.IEquatable<").Print(otherName).PrintEndLine(">");

                        isFirst = false;
                    }
                }
            }
            p = p.DecreasedIndent();
            p.OpenScope();
            {
                foreach (var kvp in map)
                {
                    var typeSymbol = kvp.Key;
                    var thisName = ExtensionsRef.ExtensionsWrapperName;
                    var otherName = typeSymbol.ToFullName();
                    var extensionsName = ExtensionsRef.ExtensionsName;
                    var indexOrIndices = kvp.Value;

                    if (indexOrIndices.Indices is { } indices)
                    {
                        WriteXXXMethods(
                              ref p
                            , memberRefs
                            , thisName
                            , otherName
                            , indices
                            , extensionsName
                        );
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteXXXMethods(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string thisName
            , string otherName
            , List<int> indices
            , string extensionsName
        )
        {
            foreach (var index in indices)
            {
                var memberRef = memberRefs[index];

                if (memberRef.isComment)
                {
                    continue;
                }

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public readonly bool Equals(").Print(otherName).Print(" other)")
                    .Print(" => ").Print(extensionsName)
                    .PrintEndLine(".Equals(Value, other);");
                p.PrintEndLine();

                foreach (var (op1, op2) in s_equalityOps)
                {
                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(thisName).Print(" left, ").Print(otherName).Print(" right)")
                        .Print(" => ").Print(op2).Print(extensionsName)
                        .PrintEndLine(".Equals(left.Value, right);");
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public static bool operator ").Print(op1)
                        .Print("(").Print(otherName).Print(" left, ").Print(thisName).Print(" right)")
                        .Print(" => ").Print(op2).Print(extensionsName)
                        .PrintEndLine(".Equals(right.Value, left);");
                    p.PrintEndLine();
                }

                break;
            }
        }

        private void WriteAdditionalExtensions(ref Printer p)
        {
            var memberRefs = MemberRefs;
            var map = MemberIndexMap;
            var @this = ExtensionsRef.ParentIsNamespace ? "this " : "";
            var thisName = EnumName;
            var underTypeName = UnderlyingTypeName;

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
                        WriteXXXExtensions(ref p, memberRefs, @this, thisName, otherName, indices, underTypeName);
                        WriteTryConvertXXXExtensions(ref p, memberRefs, @this, thisName, otherName, indices);
                    }
                    else if (indexOrIndices.Index.HasValue)
                    {
                        var index = indexOrIndices.Index.Value;

                        WriteToXXXMethod(ref p, memberRefs, @this, thisName, otherName, index);
                    }
                }

                if (memberRefs.Count > 0)
                {
                    WriteTryConvertExtension(ref p, memberRefs, @this, thisName);
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

        private static void WriteXXXExtensions(
              ref Printer p
            , List<EnumMemberRef> memberRefs
            , string @this
            , string thisName
            , string otherName
            , List<int> indices
            , string underlyingTypeName
        )
        {
            foreach (var index in indices)
            {
                var memberRef = memberRefs[index];

                if (memberRef.isComment)
                {
                    continue;
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool Equals(")
                    .Print(@this).Print(thisName).Print(" self, ").Print(otherName).PrintEndLine(" other)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var valueSelf = (").Print(underlyingTypeName).PrintEndLine(")self;");
                    p.PrintBeginLine("var valueOther = (").Print(underlyingTypeName).PrintEndLine(")other;");
                    p.PrintLine($"return (valueSelf - {memberRef.baseOrder}) == valueOther;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ").Print(thisName).Print(" To").Print(thisName)
                    .Print("(").Print(@this).Print(otherName).PrintEndLine(" self)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var value = (").Print(underlyingTypeName).PrintEndLine(")self;");
                    p.PrintBeginLine("return (").Print(thisName)
                        .Print(")(").Print(underlyingTypeName)
                        .PrintEndLine($")(value + {memberRef.baseOrder});");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void Convert(")
                    .Print(@this).Print(thisName).Print(" self, out ").Print(otherName).PrintEndLine(" result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("var value = (").Print(underlyingTypeName).PrintEndLine(")self;");
                    p.PrintBeginLine("result = (").Print(otherName).Print(")(")
                        .Print(ToTypeName(memberRef.underlyingType))
                        .PrintEndLine($")(value - {memberRef.baseOrder});");
                }
                p.CloseScope();
                p.PrintEndLine();
                break;
            }
        }

        private static void WriteTryConvertXXXExtensions(
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

        private static void WriteTryConvertExtension(
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
