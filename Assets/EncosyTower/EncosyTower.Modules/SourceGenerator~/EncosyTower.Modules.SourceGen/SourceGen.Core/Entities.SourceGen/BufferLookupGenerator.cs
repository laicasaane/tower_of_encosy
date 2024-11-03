using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    [Generator]
    internal sealed class BufferLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Modules.Entities.IBufferLookups";

        protected override LookupCodeWriter CodeWriter => new BufferLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new BufferLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_BUFFER_LOOKUPS_01"
                , "Buffer Lookups Generator Error"
                , "This error indicates a bug in the Buffer Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Entities.IBufferLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class BufferLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IBufferElementData";

        public override string InterfaceLookupRO => "global::EncosyTower.Modules.Entities.Lookups.IBufferLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Modules.Entities.Lookups.IBufferLookupRW";

        public BufferLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class BufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Unity.Entities.BufferLookup<";
        private const string BUFFER = "global::Unity.Entities.DynamicBuffer<";

        protected override void WriteStructBody(ref Printer p, LookupDeclaration declaration)
        {
            WriteFields(ref p, declaration, LOOKUP);
            WriteConstructor(ref p, declaration, "GetBufferLookup");
            WriteUpdateMethods(ref p, declaration);
            WriteConcreteMethods(ref p, declaration);
        }

        private static void WriteConcreteMethods(ref Printer p, LookupDeclaration declaration)
        {
            foreach (var typeRef in declaration.TypeRefs)
            {
                var typeName = typeRef.Symbol.ToFullName();
                var lookupField = GetLookupFieldName(typeRef);

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
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(LOOKUP)
                    .Print(typeName).Print(">(in ").Print(declaration.Syntax.Identifier.Text).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                WriteEndRegion(ref p, typeRef.Symbol.Name);
            }

            static void WriteAttributes(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            }
        }
    }
}
