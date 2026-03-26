namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class PhysicsBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "PhysicsBufferLookup<";
        private const string SAFE_ENTITY = "SafeEntity";
        private const string BUFFER = "DynamicBuffer<";

        protected override void WriteStructBody(ref Printer p, LookupDefinition definition)
        {
            WriteFields(ref p, definition, LOOKUP);
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
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasBuffer(entity);");
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
                p.PrintBeginLine("public bool TryGetBuffer(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out bufferData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(BUFFER)
                    .Print(typeName).Print("> GetBuffer(")
                    .Print(SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetBuffer(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BUFFER)
                    .Print(typeName).Print("> bufferData)")
                    .Print(" => bufferData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsBufferEnabled(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetBufferEnabled(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetEnabled(entity, value);");
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(LOOKUP)
                    .Print(typeName).Print(">(in ").Print(definition.structName).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.typeShortName);
            }

            static void WriteAttributes(ref Printer p)
            {
                p.PrintLine("/// <inheritdoc/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            }
        }
    }
}
