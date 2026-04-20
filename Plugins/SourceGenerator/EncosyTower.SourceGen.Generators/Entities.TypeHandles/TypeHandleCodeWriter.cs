namespace EncosyTower.SourceGen.Generators.Entities.TypeHandles
{
    internal static class TypeHandleCodeWriter
    {
        public const string PR_AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        public const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Entities.TypeHandles.TypeHandleGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public static string Write(in TypeHandleSpec spec)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("partial struct ")
                .Print(spec.structName)
                .Print(" : ETETH.ITypeHandles")
                .PrintEndLine();

            p.OpenScope();
            {
                WriteStructBody(ref p, spec);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteStructBody(ref Printer p, in TypeHandleSpec spec)
        {
            WriteFields(ref p, spec);
            WriteConstructor(ref p, spec);
            WriteImplicitOperators(ref p, spec);
            WriteGetMethods(ref p, spec);
            WriteUpdateMethods(ref p, spec);
        }

        private static void WriteFields(ref Printer p, in TypeHandleSpec spec)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                if (TryGetHandleType(typeRef.kind, out var handleType) == false)
                {
                    continue;
                }

                if (typeRef.kind != TypeKind.SharedComponent && typeRef.isReadOnly)
                {
                    p.PrintBeginLine(" [UC.ReadOnly] internal ");
                }
                else
                {
                    p.PrintBeginLine("/*Read-Write*/ internal ");
                }

                p.Print(handleType).Print("<")
                    .Print(typeRef.typeName)
                    .Print("> ");
                WriteHandleFieldName(ref p, typeRef);
                p.PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        private static void WriteConstructor(ref Printer p, in TypeHandleSpec spec)
        {
            Write(ref p, spec, "ref UE.SystemState state", "state");
            Write(ref p, spec, "UE.SystemBase system", "system");

            static void Write(ref Printer p, in TypeHandleSpec spec, string arg, string variable)
            {
                p.PrintLineIf(spec.typeRefs.Count < 2, PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ")
                    .Print(spec.structName)
                    .Print("(").Print(arg).PrintEndLine(")");
                p.OpenScope();
                {
                    for (var i = 0; i < spec.typeRefs.Count; i++)
                    {
                        var typeRef = spec.typeRefs[i];

                        if (TryGetGetHandleMethod(typeRef.kind, out var getHandle) == false)
                        {
                            continue;
                        }

                        var isReadOnly = typeRef.isReadOnly ? "true" : "false";

                        p.PrintBeginLine();
                        WriteHandleFieldName(ref p, typeRef);
                        p.Print(" = ")
                            .Print(variable).Print(".").Print(getHandle).Print("<")
                            .Print(typeRef.typeName)
                            .Print(">(").PrintIf(typeRef.kind != TypeKind.SharedComponent, isReadOnly)
                            .PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteUpdateMethods(ref Printer p, in TypeHandleSpec spec)
        {
            Write(ref p, spec, "ref UE.SystemState state", "ref state");
            Write(ref p, spec, "UE.SystemBase system", "system");

            static void Write(ref Printer p, in TypeHandleSpec spec, string arg0, string arg1)
            {
                p.PrintLineIf(spec.typeRefs.Count < 2, PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void Update(").Print(arg0).PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var typeRef in spec.typeRefs)
                    {
                        p.PrintBeginLine();
                        WriteHandleFieldName(ref p, typeRef);
                        p.Print(".Update(").Print(arg1).PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteGetMethods(ref Printer p, in TypeHandleSpec spec)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                if (TryGetHandleType(typeRef.kind, out var handleType) == false)
                {
                    continue;
                }

                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(handleType).Print("<")
                    .Print(typeRef.typeName)
                    .Print("> ")
                    .Print("Get(ET.T<").Print(typeRef.typeName)
                    .Print("> _) => ");
                WriteHandleFieldName(ref p, typeRef);
                p.PrintEndLine(";");
                p.PrintEndLine();
            }
        }

        private static void WriteImplicitOperators(ref Printer p, in TypeHandleSpec spec)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                if (TryGetHandleType(typeRef.kind, out var handleType) == false)
                {
                    continue;
                }

                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(handleType).Print("<")
                    .Print(typeRef.typeName).Print(">(in ").Print(spec.structName).Print(" value)")
                    .Print(" => value.");
                WriteHandleFieldName(ref p, typeRef);
                p.PrintEndLine(";");
                p.PrintEndLine();
            }
        }

        private static Printer WriteHandleFieldName(ref Printer p, TypeRefSpec typeRef)
            => p.Print("_handle_").Print(typeRef.typeIdentifier);

        private static bool TryGetHandleType(TypeKind kind, out string result)
        {
            switch (kind)
            {
                case TypeKind.Buffer:
                    result = "UE.BufferTypeHandle";
                    return true;

                case TypeKind.Component:
                    result = "UE.ComponentTypeHandle";
                    return true;

                case TypeKind.SharedComponent:
                    result = "UE.SharedComponentTypeHandle";
                    return true;

                default:
                    result = null;
                    return false;
            }
        }

        private static bool TryGetGetHandleMethod(TypeKind kind, out string result)
        {
            switch (kind)
            {
                case TypeKind.Buffer:
                    result = "GetBufferTypeHandle";
                    return true;

                case TypeKind.Component:
                    result = "GetComponentTypeHandle";
                    return true;

                case TypeKind.SharedComponent:
                    result = "GetSharedComponentTypeHandle";
                    return true;

                default:
                    result = null;
                    return false;
            }
        }
    }
}
