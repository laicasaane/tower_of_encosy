namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class PhysicsBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "LP.PhysicsBufferLookup<";
        private const string PR_SAFE_ENTITY = "LP.SafeEntity";
        private const string PR_BUFFER = "UE.DynamicBuffer<";

        protected override string LookupTypePrefix => PR_LOOKUP;

        protected override string GetLookupMethod => "GetBufferLookup";

        protected override void WriteConcreteMethods(ref Printer p, in LookupSpec spec)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                var typeName = typeRef.typeName;
                var lookupField = GetLookupFieldName(typeRef);

                WriteBeginRegion(ref p, typeRef.typeShortName);

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasBuffer(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void DidChange(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, uint version, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".DidChange(entity, version);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out bufferData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(PR_BUFFER)
                    .Print(typeName).Print("> GetBuffer(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetBuffer(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => bufferData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsBufferEnabled(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetBufferEnabled(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, ").Print(PR_BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetEnabled(entity, value);");
                    p.PrintEndLine();
                }

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
