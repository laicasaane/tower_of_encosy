namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class BufferLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "UE.BufferLookup<";
        private const string PR_BUFFER = "UE.DynamicBuffer<";

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

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
