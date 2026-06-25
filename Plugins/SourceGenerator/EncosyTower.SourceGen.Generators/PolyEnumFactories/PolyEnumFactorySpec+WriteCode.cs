using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyEnumFactories
{
    partial struct PolyEnumFactorySpec
    {
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"PolyEnumFactoryGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string UNDEFINED_NAME = "Undefined";

        public readonly string WriteCode(in CompilationInfo compilation)
        {
            var containsUndefined = cases.Any(static c => string.Equals(
                  c.identifier
                , UNDEFINED_NAME
                , StringComparison.Ordinal
            ));

            var additional = containsUndefined ? 0 : 1;
            var underlyingType = (ulong)(cases.Count + additional) switch {
                > uint.MaxValue => "ulong",
                > ushort.MaxValue => "uint",
                > byte.MaxValue => "ushort",
                _ => "byte",
            };

            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.Print("#region    TYPE").PrintEndLine();
                p.Print("#endregion ====").PrintEndLine();
                p.PrintEndLine();

                WriteWrapperHeader(ref p, "Type");
                p.OpenScope();
                {
                    WriteTypeEnum(ref p, underlyingType, containsUndefined);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    FACTORY API").PrintEndLine();
                p.Print("#endregion ===========").PrintEndLine();
                p.PrintEndLine();

                WriteWrapperHeader(ref p, "Factory API");
                p.OpenScope();
                {
                    WriteBackingFieldAndCtor(ref p);
                    WriteFactoryMethods(ref p);
                    WriteExplicitUndefinedMethod(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    TYPE API").PrintEndLine();
                p.Print("#endregion ========").PrintEndLine();
                p.PrintEndLine();

                WriteWrapperHeader(ref p, "Type API");
                p.OpenScope();
                {
                    WriteTypeMethods(ref p, underlyingType);
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private readonly void WriteTypeEnum(ref Printer p, string underlyingType, bool containsUndefined)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum Type : ").PrintEndLine(underlyingType);
            p.OpenScope();
            {
                if (containsUndefined == false)
                {
                    p.PrintBeginLine(UNDEFINED_NAME).Print(" = ").Print(enumStructTypeName)
                        .Print(".EnumCase.").Print(UNDEFINED_NAME).PrintEndLine(",");
                }

                for (var i = 0; i < cases.Count; i++)
                {
                    var c = cases[i];

                    p.PrintBeginLine(c.identifier).Print(" = ").Print(enumStructTypeName)
                        .Print(".EnumCase.").Print(c.identifier).PrintEndLine(",");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteWrapperHeader(ref Printer p, string comment)
        {
            p.PrintBeginLine();

            if (string.IsNullOrEmpty(wrapperAccessibility) == false)
            {
                p.Print(wrapperAccessibility).Print(" ");
            }

            if (string.IsNullOrEmpty(wrapperPreModifiers) == false)
            {
                p.Print(wrapperPreModifiers).Print(" ");
            }

            p.Print("partial ").Print(wrapperKindKeyword).Print(" ").Print(wrapperTypeName)
                .Print(" // ")
                .PrintEndLine(comment);
        }

        private readonly void WriteBackingFieldAndCtor(ref Printer p)
        {
            if (emitBackingField == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("private readonly ").Print(enumStructTypeName).Print(" ")
                .Print(fieldName).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("private ").Print(wrapperTypeName)
                .Print("(").Print(enumStructTypeName).Print(" value)")
                .PrintIf(isStruct, " : this()")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine("this.").Print(fieldName).PrintEndLine(" = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteFactoryMethods(ref Printer p)
        {
            for (var i = 0; i < cases.Count; i++)
            {
                var c = cases[i];

                switch (c.strategy)
                {
                    case ConstructionStrategy.Ctors:
                        WriteCtorStrategy(ref p, c);

                        if (c.emitMemberInitOverload)
                        {
                            WriteMemberInitStrategy(ref p, c);
                        }

                        break;

                    case ConstructionStrategy.MemberInit:
                        WriteMemberInitStrategy(ref p, c);
                        break;

                    case ConstructionStrategy.Default:
                        WriteDefaultStrategy(ref p, c);
                        break;
                }
            }
        }

        private readonly void WriteCtorStrategy(ref Printer p, in CaseSpec c)
        {
            for (var i = 0; i < c.ctors.Count; i++)
            {
                var ctor = c.ctors[i];

                p.PrintLine(GENERATED_CODE);
                p.PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ").Print(wrapperTypeName).Print(" ")
                    .Print(c.identifier).Print("(");

                WriteParameterList(ref p, ctor.parameters);

                p.PrintEndLine(")");
                p.OpenScope();
                {
                    p.PrintBeginLine("return new ").Print(wrapperTypeName)
                        .Print("(new ").Print(c.qualifiedName).Print("(");

                    WriteArgumentList(ref p, ctor.parameters);

                    p.PrintEndLine("));");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMemberInitStrategy(ref Printer p, in CaseSpec c)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").Print(wrapperTypeName).Print(" ")
                .Print(c.identifier).Print("(");

            for (var i = 0; i < c.initMembers.Count; i++)
            {
                if (i > 0)
                {
                    p.Print(", ");
                }

                var member = c.initMembers[i];
                p.Print(member.typeFullyQualifiedName).Print(" ").Print(member.parameterName);
            }

            p.PrintEndLine(")");
            p.OpenScope();
            {
                p.PrintBeginLine("return new ").Print(wrapperTypeName)
                    .Print("(new ").Print(c.qualifiedName).PrintEndLine(" {");

                p = p.IncreasedIndent();
                {
                    for (var i = 0; i < c.initMembers.Count; i++)
                    {
                        var member = c.initMembers[i];

                        p.PrintBeginLine().Print(member.name).Print(" = ").Print(member.parameterName);

                        if (i < c.initMembers.Count - 1)
                        {
                            p.PrintEndLine(",");
                        }
                        else
                        {
                            p.PrintEndLine();
                        }
                    }
                }
                p = p.DecreasedIndent();

                p.PrintLine("});");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteDefaultStrategy(ref Printer p, in CaseSpec c)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").Print(wrapperTypeName).Print(" ")
                .Print(c.identifier).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("return new ").Print(wrapperTypeName)
                    .Print("(default(").Print(c.qualifiedName).PrintEndLine("));");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteExplicitUndefinedMethod(ref Printer p)
        {
            if (emitExplicitUndefinedMethod == false)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ").Print(wrapperTypeName).Print(" ")
                .Print(UNDEFINED_NAME).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("return new ").Print(wrapperTypeName)
                    .Print("(default(").Print(enumStructTypeName).PrintEndLine("));");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteTypeMethods(ref Printer p, string underlyingType)
        {
            p.PrintBeginLine(AGGRESSIVE_INLINING).Print(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintLine("public bool Is(Type type)");
            p.OpenScope();
            {
                p.PrintBeginLine("return this.").Print(fieldName).Print(".GetEnumCase() == (")
                    .Print(enumStructTypeName).Print(".EnumCase)((").Print(underlyingType).PrintEndLine(")type);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteParameterList(ref Printer p, EquatableArray<ParamSpec> parameters)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                {
                    p.Print(", ");
                }

                var param = parameters[i];

                if (param.isParams)
                {
                    p.Print("params ");
                }

                switch (param.refKind)
                {
                    case RefKind.Ref: p.Print("ref "); break;
                    case RefKind.In: p.Print("in "); break;
                }

                p.Print(param.typeFullyQualifiedName).Print(" ").Print(param.name);

                if (param.hasExplicitDefaultValue && string.IsNullOrEmpty(param.defaultValueLiteral) == false)
                {
                    p.Print(" = ").Print(param.defaultValueLiteral);
                }
            }
        }

        private static void WriteArgumentList(ref Printer p, EquatableArray<ParamSpec> parameters)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                {
                    p.Print(", ");
                }

                var param = parameters[i];

                switch (param.refKind)
                {
                    case RefKind.Ref: p.Print("ref "); break;
                    case RefKind.In: p.Print("in "); break;
                }

                p.Print(param.name);
            }
        }
    }
}
