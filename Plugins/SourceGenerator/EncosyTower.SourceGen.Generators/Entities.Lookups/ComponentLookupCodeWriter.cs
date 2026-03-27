namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class ComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "ComponentLookup<";
        private const string SYSTEM_HANDLE = "SystemHandle";
        private const string REFRW = "RefRW<";
        private const string REFRO = "RefRO<";

        protected override void WriteStructBody(ref Printer p, LookupDefinition definition)
        {
            WriteFields(ref p, definition, LOOKUP);
            WriteConstructor(ref p, definition, "GetComponentLookup");
            WriteUpdateMethods(ref p, definition);
            WriteConcreteMethods(ref p, definition);
        }

        private static void WriteConcreteMethods(ref Printer p, LookupDefinition definition)
        {
            var writeLookupCommon = false;

            foreach (var typeRef in definition.typeRefs)
            {
                var typeName = typeRef.typeName;
                var lookupField = GetLookupFieldName(typeRef);

                if (writeLookupCommon == false)
                {
                    writeLookupCommon = true;

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public bool EntityExists(")
                        .Print(ENTITY).Print(" entity)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".EntityExists(entity);");
                    p.PrintEndLine();
                }

                WriteBeginRegion(ref p, typeRef.typeShortName);

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result, out bool entityExists)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(systemHandle);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void DidChange(")
                    .Print(ENTITY).Print(" entity, uint version, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".DidChange(entity, version);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData, out bool entityExists)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(ENTITY).Print(" entity, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[entity] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[systemHandle] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void SetComponentDataOptional(")
                        .Print(ENTITY).Print(" entity, ")
                        .Print(typeName).PrintEndLine(" componentData)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var refRW = ").Print(lookupField).PrintEndLine(".GetRefRW(entity);");
                        p.PrintEndLine();

                        p.PrintLine("if (refRW.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("refRW.ValueRW = componentData;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void SetComponentDataOptional(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                        .Print(typeName).PrintEndLine(" componentData)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var refRW = ").Print(lookupField).PrintEndLine(".GetRefRW(systemHandle);");
                        p.PrintEndLine();

                        p.PrintLine("if (refRW.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("refRW.ValueRW = componentData;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefRO(")
                    .Print(ENTITY).Print(" entity, out ").Print(REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefRO(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(ENTITY).Print(" entity, out ").Print(REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
                p.PrintEndLine();

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
