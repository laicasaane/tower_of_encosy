namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal abstract class LookupCodeWriter
    {
        public const string PR_AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        public const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Entities.Lookups.LookupGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_ENTITY = "UE.Entity";
        public const string PR_BOOL = "ET.Bool<";

        protected abstract string LookupTypePrefix { get; }

        protected abstract string GetLookupMethod { get; }

        public static LookupCodeWriter GetWriter(LookupKind kind)
        {
            return kind switch {
                LookupKind.Buffer => new BufferLookupCodeWriter(),
                LookupKind.Component => new ComponentLookupCodeWriter(),
                LookupKind.EnableableBuffer => new EnableableBufferLookupCodeWriter(),
                LookupKind.EnableableComponent => new EnableableComponentLookupCodeWriter(),
                LookupKind.PhysicsBuffer => new PhysicsBufferLookupCodeWriter(),
                LookupKind.PhysicsComponent => new PhysicsComponentLookupCodeWriter(),
                LookupKind.PhysicsEnableableComponent => new PhysicsEnableableComponentLookupCodeWriter(),
                _ => new BufferLookupCodeWriter(),
            };
        }

        public string Write(in LookupSpec spec)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("partial struct ")
                .Print(spec.structName)
                .Print(" : ETEL.ILookups")
                .PrintEndLine();

            WriteInterfaces(ref p, spec);

            p.OpenScope();
            {
                WriteFields(ref p, spec, LookupTypePrefix);
                WriteConstructor(ref p, spec, GetLookupMethod);
                WriteImplicitOperators(ref p, spec, LookupTypePrefix);
                WriteGetMethods(ref p, spec, LookupTypePrefix);
                WriteUpdateMethods(ref p, spec);
                WriteConcreteMethods(ref p, spec);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        protected static string GetLookupFieldName(TypeRefSpec typeRef)
            => $"_lookup_{typeRef.typeIdentifier}";

        protected static Printer WriteBeginRegion(ref Printer p, string typeName)
            => p.PrintBeginLine("#region    ").PrintEndLine(typeName)
                .PrintEndLine();

        protected static Printer WriteEndRegion(ref Printer p, string typeName)
            => p.PrintBeginLine("#endregion ").PrintEndLine(typeName)
                .PrintEndLine();

        protected static Printer WriteLookupFieldName(ref Printer p, TypeRefSpec typeRef)
            => p.Print("_lookup_").Print(typeRef.typeIdentifier);

        protected static Printer WriteAttributes(ref Printer p)
            => p.PrintLine("/// <inheritdoc/>").PrintLine(PR_AGGRESSIVE_INLINING);

        protected static Printer WriteAttributesNoInline(ref Printer p)
            => p.PrintLine("/// <inheritdoc/>");

        private static void WriteInterfaces(ref Printer p, in LookupSpec spec)
        {
            if (spec.typeRefs.Count < 1)
            {
                return;
            }

            p = p.IncreasedIndent();

            foreach (var typeRef in spec.typeRefs)
            {
                if (typeRef.isReadOnly)
                {
                    p.PrintBeginLine(", ").Print(spec.interfaceLookupRO);
                }
                else
                {
                    p.PrintBeginLine(", ").Print(spec.interfaceLookupRW);
                }

                p.Print("<").Print(typeRef.typeName).PrintEndLine(">");
            }

            p = p.DecreasedIndent();
        }

        private static void WriteFields(ref Printer p, in LookupSpec spec, string lookupTypePrefix)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                if (typeRef.isReadOnly)
                {
                    p.PrintBeginLine(" [UC.ReadOnly] internal ");
                }
                else
                {
                    p.PrintBeginLine("/*Read-Write*/ internal ");
                }

                p.Print(lookupTypePrefix)
                    .Print(typeRef.typeName)
                    .Print("> ");
                WriteLookupFieldName(ref p, typeRef);
                p.PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        private static void WriteConstructor(ref Printer p, in LookupSpec spec, string getLookupMethod)
        {
            Write(ref p, spec, getLookupMethod, "ref UE.SystemState state", "state");
            Write(ref p, spec, getLookupMethod, "UE.SystemBase system", "system");

            static void Write(ref Printer p, in LookupSpec spec, string getLookup, string arg, string variable)
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
                        var isReadOnly = typeRef.isReadOnly ? "true" : "false";

                        p.PrintBeginLine();
                        WriteLookupFieldName(ref p, typeRef);
                        p.Print(" = ").Print(variable).Print(".").Print(getLookup).Print("<")
                            .Print(typeRef.typeName)
                            .Print(">(").Print(isReadOnly)
                            .PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteUpdateMethods(ref Printer p, in LookupSpec spec)
        {
            Write(ref p, spec, "ref UE.SystemState state", "ref state");
            Write(ref p, spec, "UE.SystemBase system", "system");

            static void Write(ref Printer p, in LookupSpec spec, string arg0, string arg1)
            {
                p.PrintLineIf(spec.typeRefs.Count < 2, PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("public void Update(").Print(arg0).PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var typeRef in spec.typeRefs)
                    {
                        p.PrintBeginLine();
                        WriteLookupFieldName(ref p, typeRef);
                        p.Print(".Update(").Print(arg1).PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteGetMethods(ref Printer p, in LookupSpec spec, string lookupTypePrefix)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(lookupTypePrefix)
                    .Print(typeRef.typeName)
                    .Print("> ")
                    .Print("Get(ET.T<").Print(typeRef.typeName)
                    .Print("> _) => ");
                WriteLookupFieldName(ref p, typeRef);
                p.PrintEndLine(";");
                p.PrintEndLine();
            }
        }

        private static void WriteImplicitOperators(ref Printer p, in LookupSpec spec, string lookupTypePrefix)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(lookupTypePrefix)
                    .Print(typeRef.typeName).Print(">(in ").Print(spec.structName).Print(" value)")
                    .Print(" => value.");
                WriteLookupFieldName(ref p, typeRef);
                p.PrintEndLine(";");
                p.PrintEndLine();
            }
        }

        protected abstract void WriteConcreteMethods(ref Printer p, in LookupSpec spec);
    }
}
