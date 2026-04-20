namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class EnableableBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "UE.BufferLookup<";
        private const string PR_BUFFER = "UE.DynamicBuffer<";
        private const string PR_ENABLED_REFRW = "UE.EnabledRefRW<";
        private const string PR_ENABLED_REFRO = "UE.EnabledRefRO<";

        protected override string LookupTypePrefix => PR_LOOKUP;

        protected override string GetLookupMethod => "GetBufferLookup";

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
                p.PrintBeginLine("public void HasBuffer(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasBuffer(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result, out bool entityExists)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity, out entityExists);");
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
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetBuffer(entity, out bufferData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BUFFER)
                    .Print(typeName).Print("> bufferData, out bool entityExists)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetBuffer(entity, out bufferData, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(PR_BUFFER)
                    .Print(typeName).Print("> GetBuffer(")
                    .Print(PR_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetBuffer(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => bufferData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                p.PrintLine("#region    ENABLEABLE API");
                p.PrintLine("#endregion ==============");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsBufferEnabled(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsBufferEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetBufferEnabled(")
                        .Print(PR_ENTITY).Print(" entity, ").Print(PR_BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetBufferEnabled(entity, value);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRW(")
                        .Print(PR_ENTITY).Print(" entity, out ").Print(PR_ENABLED_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetEnabledRefRW<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRW(")
                        .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_ENABLED_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetEnabledRefRW<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRWOptional(")
                        .Print(PR_ENTITY).Print(" entity, out ").Print(PR_ENABLED_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetComponentEnabledRefRWOptional<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRWOptional(")
                        .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_ENABLED_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetComponentEnabledRefRWOptional<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefRO(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_ENABLED_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefRO<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefRO(")
                    .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(PR_ENABLED_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefRO<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefROOptional(")
                    .Print(PR_ENTITY).Print(" entity, out ").Print(PR_ENABLED_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefROOptional(")
                    .Print(PR_ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(PR_ENABLED_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
