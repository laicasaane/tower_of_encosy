using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    [Generator]
    internal sealed class PhysicsBufferLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Modules.Entities.IPhysicsBufferLookups";

        protected override LookupCodeWriter CodeWriter => new PhysicsBufferLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new PhysicsBufferLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_PHYSICS_BUFFER_LOOKUPS_01"
                , "Physics Buffer Lookups Generator Error"
                , "This error indicates a bug in the Physics Buffer Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Entities.IPhysicsBufferLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class PhysicsBufferLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IBufferElementData";

        public override string InterfaceLookupRO => "global::EncosyTower.Modules.Entities.Lookups.IPhysicsBufferLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Modules.Entities.Lookups.IPhysicsBufferLookupRW";

        public PhysicsBufferLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class PhysicsBufferLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Latios.Psyshock.PhysicsBufferLookup<";
        private const string SAFE_ENTITY = "global::Latios.Psyshock.SafeEntity";
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

                if (typeRef.IsReadOnly == false)
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
