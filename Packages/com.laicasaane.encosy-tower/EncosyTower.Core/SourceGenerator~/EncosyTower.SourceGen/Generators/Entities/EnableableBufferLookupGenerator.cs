using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities
{
    [Generator]
    internal sealed class EnableableBufferLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Entities.IEnableableBufferLookups";

        protected override LookupCodeWriter CodeWriter => new EnableableBufferLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new BufferLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_ENABLEABLE_BUFFER_LOOKUPS_01"
                , "Enableable Buffer Lookups Generator Error"
                , "This error indicates a bug in the Enableable Buffer Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Entities.IEnableableBufferLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class EnableableBufferLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IBufferElementData";

        protected override string Interface2 => "global::Unity.Entities.IEnableableComponent";

        public override string InterfaceLookupRO => "global::EncosyTower.Entities.Lookups.IEnableableBufferLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Entities.Lookups.IEnableableBufferLookupRW";

        public EnableableBufferLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class EnableableBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Unity.Entities.BufferLookup<";
        private const string BUFFER = "global::Unity.Entities.DynamicBuffer<";
        private const string ENABLED_REFRW = "global::Unity.Entities.EnabledRefRW<";
        private const string ENABLED_REFRO = "global::Unity.Entities.EnabledRefRO<";

        protected override void WriteStructBody(ref Printer p, LookupDeclaration declaration)
        {
            WriteFields(ref p, declaration, LOOKUP);
            WriteConstructor(ref p, declaration, "GetBufferLookup");
            WriteUpdateMethods(ref p, declaration);
            WriteConcreteMethods(ref p, declaration);
        }

        private static void WriteConcreteMethods(ref Printer p, LookupDeclaration declaration)
        {
            var writeLookupCommon = false;

            foreach (var typeRef in declaration.TypeRefs)
            {
                var typeName = typeRef.Symbol.ToFullName();
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

                WriteBeginRegion(ref p, typeRef.Symbol.Name);

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

                if (typeRef.IsReadOnly == false)
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
                    .Print(typeName).Print(">(in ").Print(declaration.Syntax.Identifier.Text).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.Symbol.Name);
            }

            static void WriteAttributes(ref Printer p)
            {
                p.PrintLine("/// <inheritdoc/>");
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            }
        }
    }
}
