namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class BufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "BufferLookup<";
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
