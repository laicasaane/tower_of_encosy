namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class EnableableBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "BufferLookup<";
        private const string BUFFER = "DynamicBuffer<";
        private const string ENABLED_REFRW = "EnabledRefRW<";
        private const string ENABLED_REFRO = "EnabledRefRO<";

        protected override void WriteStructBody(ref Printer p, LookupDefinition definition)
        {
            WriteFields(ref p, definition, LOOKUP);
            WriteConstructor(ref p, definition, "GetBufferLookup");
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
                p.PrintBeginLine("public void HasBuffer(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasBuffer(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result, out bool entityExists)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity, out entityExists);");
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
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(ENTITY).Print(" entity, out ").Print(BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetBuffer(entity, out bufferData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(ENTITY).Print(" entity, out ").Print(BUFFER)
                    .Print(typeName).Print("> bufferData, out bool entityExists)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetBuffer(entity, out bufferData, out entityExists);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(BUFFER)
                    .Print(typeName).Print("> GetBuffer(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetBuffer(")
                    .Print(ENTITY).Print(" entity, out ").Print(BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => bufferData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                p.PrintLine("#region    ENABLEABLE API");
                p.PrintLine("#endregion ==============");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsBufferEnabled(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsBufferEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetBufferEnabled(")
                        .Print(ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetBufferEnabled(entity, value);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRW(")
                        .Print(ENTITY).Print(" entity, out ").Print(ENABLED_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetEnabledRefRW<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRW(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(ENABLED_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetEnabledRefRW<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRWOptional(")
                        .Print(ENTITY).Print(" entity, out ").Print(ENABLED_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetComponentEnabledRefRWOptional<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetEnabledRefRWOptional(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(ENABLED_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .Print(".GetComponentEnabledRefRWOptional<").Print(typeName)
                        .PrintEndLine(">(entity);");
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefRO(")
                    .Print(ENTITY).Print(" entity, out ").Print(ENABLED_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefRO<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefRO(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(ENABLED_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefRO<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefROOptional(")
                    .Print(ENTITY).Print(" entity, out ").Print(ENABLED_REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefROOptional(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(ENABLED_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
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
