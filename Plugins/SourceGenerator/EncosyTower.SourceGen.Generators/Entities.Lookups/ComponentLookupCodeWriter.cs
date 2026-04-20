namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class ComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "UE.ComponentLookup<";
        private const string PR_SYSTEM_HANDLE = "UE.SystemHandle";
        private const string PR_REFRW = "UE.RefRW<";
        private const string PR_REFRO = "UE.RefRO<";

        protected override string LookupTypePrefix => PR_LOOKUP;

        protected override string GetLookupMethod => "GetComponentLookup";

        protected override void WriteConcreteMethods(ref Printer p, in LookupSpec spec)
        {
            var writeLookupCommon = false;

            foreach (var typeRef in spec.typeRefs)
            {
                var typeName = typeRef.typeName;
                var lookupField = GetLookupFieldName(typeRef);

                if (writeLookupCommon == false)
                {
                    writeLookupCommon = true;

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public bool EntityExists(")
                        .Print(PR_ENTITY).Print(" entity)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".EntityExists(entity);");
                    p.PrintEndLine();
                }

                WriteBeginRegion(ref p, typeRef.typeShortName);

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result, out bool entityExists)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(systemHandle);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void DidChange(")
                    .Print(PR_ENTITY).Print(" entity, uint version, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".DidChange(entity, version);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(PR_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData, out bool entityExists)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(PR_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(PR_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(PR_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(PR_ENTITY).Print(" entity, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[entity] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[systemHandle] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_ENTITY).Print(" entity, out ").Print(PR_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, out ").Print(PR_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, ").Print(typeName)
                        .Print(" _, out ").Print(PR_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(PR_ENTITY).Print(" entity, out ").Print(PR_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void SetComponentDataOptional(")
                        .Print(PR_ENTITY).Print(" entity, ")
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
                        .Print(PR_SYSTEM_HANDLE).Print(" systemHandle, ")
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
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefRO(")
                    .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(PR_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(PR_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
