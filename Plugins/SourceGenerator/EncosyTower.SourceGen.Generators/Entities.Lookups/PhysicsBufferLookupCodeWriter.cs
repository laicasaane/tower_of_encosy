namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class PhysicsBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "LP.PhysicsBufferLookup<";
        private const string PR_SAFE_ENTITY = "LP.SafeEntity";
        private const string PR_BUFFER = "UE.DynamicBuffer<";

        protected override void WriteStructBody(ref Printer p, LookupDefinition definition)
        {
            WriteFields(ref p, definition, PR_LOOKUP);
            WriteConstructor(ref p, definition, "GetBufferLookup");
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

                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(PR_LOOKUP)
                    .Print(typeName).Print(">(in ").Print(definition.structName).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
