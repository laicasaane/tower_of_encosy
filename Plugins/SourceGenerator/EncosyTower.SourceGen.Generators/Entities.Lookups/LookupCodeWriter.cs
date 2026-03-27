namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal abstract class LookupCodeWriter
    {
        protected const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        protected const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Entities.LookupGenerator\", \"{SourceGenVersion.VALUE}\")]";
        protected const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        protected const string ENTITY = "Entity";
        protected const string BOOL = "Bool<";

        public static LookupCodeWriter GetWriter(LookupKind kind)
        {
            return kind switch {
                LookupKind.Buffer                   => new BufferLookupCodeWriter(),
                LookupKind.Component                => new ComponentLookupCodeWriter(),
                LookupKind.EnableableBuffer         => new EnableableBufferLookupCodeWriter(),
                LookupKind.EnableableComponent      => new EnableableComponentLookupCodeWriter(),
                LookupKind.PhysicsBuffer            => new PhysicsBufferLookupCodeWriter(),
                LookupKind.PhysicsComponent         => new PhysicsComponentLookupCodeWriter(),
                LookupKind.PhysicsEnableableComponent => new PhysicsEnableableComponentLookupCodeWriter(),
                _                                   => new BufferLookupCodeWriter(),
            };
        }

        public string Write(LookupDefinition definition)
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            p.PrintBeginLine("partial struct ")
                .Print(definition.structName)
                .Print(" : ILookups")
                .PrintEndLine();

            WriteInterfaces(ref p, definition);

            p.OpenScope();
            {
                WriteStructBody(ref p, definition);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteInterfaces(ref Printer p, LookupDefinition definition)
        {
            if (definition.typeRefs.Count < 1)
            {
                return;
            }

            p = p.IncreasedIndent();

            foreach (var typeRef in definition.typeRefs)
            {
                if (typeRef.isReadOnly)
                {
                    p.PrintBeginLine(", ").Print(definition.interfaceLookupRO);
                }
                else
                {
                    p.PrintBeginLine(", ").Print(definition.interfaceLookupRW);
                }

                p.Print("<").Print(typeRef.typeName).PrintEndLine(">");
            }

            p = p.DecreasedIndent();
        }

        protected static void WriteFields(ref Printer p, LookupDefinition definition, string lookup)
        {
            foreach (var typeRef in definition.typeRefs)
            {
                if (typeRef.isReadOnly)
                {
                    p.PrintBeginLine("[ReadOnly] internal ");
                }
                else
                {
                    p.PrintBeginLine("/*           Read-Write           */ internal ");
                }

                p.Print(lookup)
                    .Print(typeRef.typeName)
                    .Print("> ").Print(GetLookupFieldName(typeRef))
                    .PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        protected static void WriteConstructor(ref Printer p, LookupDefinition definition, string getLookup)
        {
            Write(ref p, definition, getLookup, "ref SystemState state", "state");
            Write(ref p, definition, getLookup, "SystemBase system", "system");

            static void Write(ref Printer p, LookupDefinition definition, string getLookup, string arg, string variable)
            {
                p.PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ")
                    .Print(definition.structName)
                    .Print("(").Print(arg).PrintEndLine(")");
                p.OpenScope();
                {
                    for (var i = 0; i < definition.typeRefs.Count; i++)
                    {
                        var typeRef = definition.typeRefs[i];
                        var fieldName = GetLookupFieldName(typeRef);
                        var isReadOnly = typeRef.isReadOnly ? "true" : "false";

                        p.PrintBeginLine(fieldName).Print(" = ")
                            .Print(variable).Print(".").Print(getLookup).Print("<")
                            .Print(typeRef.typeName)
                            .Print(">(").Print(isReadOnly)
                            .PrintEndLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        protected static void WriteUpdateMethods(ref Printer p, LookupDefinition definition)
        {
            Write(ref p, definition, "ref SystemState state", "ref state");
            Write(ref p, definition, "SystemBase system", "system");

            static void Write(ref Printer p, LookupDefinition definition, string arg0, string arg1)
            {
                p.PrintLineIf(definition.typeRefs.Count < 2, GENERATED_CODE);
                p.PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public void Update(").Print(arg0).PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var typeRef in definition.typeRefs)
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

        protected static string GetLookupFieldName(TypeRefModel typeRef)
            => $"_lookup_{typeRef.typeIdentifier}";

        protected static string GetLookupVarName(TypeRefModel typeRef)
            => $"lookup_{typeRef.typeIdentifier}";

        protected static void WriteAttributes(ref Printer p)
        {
            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
        }

        protected static void WriteAttributesNoInline(ref Printer p)
        {
            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
        }

        protected abstract void WriteStructBody(ref Printer p, LookupDefinition definition);
    }
}
