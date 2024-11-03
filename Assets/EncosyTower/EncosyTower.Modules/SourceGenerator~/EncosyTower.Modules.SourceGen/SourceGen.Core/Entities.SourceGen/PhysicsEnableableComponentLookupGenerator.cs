using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    [Generator]
    internal sealed class PhysicsEnableableComponentLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Modules.Entities.IPhysicsEnableableComponentLookups";

        protected override LookupCodeWriter CodeWriter => new PhysicsEnableableComponentLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new PhysicsEnableableComponentLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS_01"
                , "Physics Enableable Component Lookups Generator Error"
                , "This error indicates a bug in the Physics Enableable Component Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Entities.IPhysicsEnableableComponentLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class PhysicsEnableableComponentLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IComponentData";

        protected override string Interface2 => "global::Unity.Entities.IEnableableComponent";

        public override string InterfaceLookupRO => "global::EncosyTower.Modules.Entities.Lookups.IPhysicsEnableableComponentLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Modules.Entities.Lookups.IPhysicsEnableableComponentLookupRW";

        public PhysicsEnableableComponentLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class PhysicsEnableableComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Latios.Psyshock.PhysicsComponentLookup<";
        private const string SAFE_ENTITY = "global::Latios.Psyshock.SafeEntity";
        private const string REFRW = "global::Unity.Entities.RefRW<";

        protected override void WriteStructBody(ref Printer p, LookupDeclaration declaration)
        {
            WriteFields(ref p, declaration, LOOKUP);
            WriteConstructor(ref p, declaration, "GetComponentLookup");
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
                p.PrintBeginLine("public void HasComponent(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
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
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void SetComponentData(")
                    .Print(SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity] = componentData;");
                p.PrintEndLine();

                if (typeRef.IsReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SAFE_ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(SAFE_ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(REFRW).Print(typeName).PrintEndLine(">();");
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = ").Print(lookupField).PrintEndLine(".GetRW(entity);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(REFRW).Print(typeName).PrintEndLine(">();");
                        }
                        p.CloseScope();
                        p.PrintLine("else");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = ").Print(lookupField).PrintEndLine(".GetRW(entity);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public static implicit operator ").Print(LOOKUP)
                    .Print(typeName).Print(">(in ").Print(declaration.Syntax.Identifier.Text).Print(" value)")
                    .Print(" => value.").Print(lookupField).PrintEndLine(";");
                p.PrintEndLine();

                p.PrintLine("#region    ENABLEABLE API");
                p.PrintLine("#endregion ==============");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsComponentEnabled(")
                    .Print(SAFE_ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.IsReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentEnabled(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetEnabled(entity, value);");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void SetComponentEnabledOptional(")
                        .Print(SAFE_ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).PrintEndLine("> value)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity))");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("lookup").PrintEndLine(".SetEnabled(entity, value);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteEndRegion(ref p, typeRef.Symbol.Name);
            }

            static void WriteAttributes(ref Printer p)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            }
        }
    }
}
