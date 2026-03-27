namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class PhysicsComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "PhysicsComponentLookup<";
        private const string SAFE_ENTITY = "SafeEntity";
        private const string REFRW = "RefRW<";

        protected override void WriteStructBody(ref Printer p, LookupDefinition definition)
        {
            WriteFields(ref p, definition, LOOKUP);
            WriteConstructor(ref p, definition, "GetComponentLookup");
            WriteUpdateMethods(ref p, definition);
            WriteConcreteMethods(ref p, definition);
        }

        private static void WriteConcreteMethods(ref Printer p, LookupDefinition definition)
        {
            foreach (var typeRef in definition.typeRefs)
            {
                var typeName = typeRef.typeName;
                var lookupField = GetLookupFieldName(typeRef);

                WriteBeginRegion(ref p, typeRef.typeShortName);

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void DidChange(")
                    .Print(SAFE_ENTITY).Print(" entity, uint version, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".DidChange(entity, version);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(SAFE_ENTITY).Print(" entity, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[entity] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SAFE_ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(SAFE_ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(REFRW).Print(typeName).PrintEndLine(">();");
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = ").Print(lookupField).PrintEndLine(".GetRW(entity);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(REFRW).Print(typeName).PrintEndLine(">();");
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = ").Print(lookupField).PrintEndLine(".GetRW(entity);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(LOOKUP)
                    .Print(typeName).Print(">(in ").Print(definition.structName).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
