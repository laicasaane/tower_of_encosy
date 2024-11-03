using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    [Generator]
    internal sealed class ComponentLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Modules.Entities.IComponentLookups";

        protected override LookupCodeWriter CodeWriter => new ComponentLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new ComponentLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_COMPONENT_LOOKUPS_01"
                , "Component Lookups Generator Error"
                , "This error indicates a bug in the Component Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Entities.IComponentLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class ComponentLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IComponentData";

        public override string InterfaceLookupRO => "global::EncosyTower.Modules.Entities.Lookups.IComponentLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Modules.Entities.Lookups.IComponentLookupRW";

        public ComponentLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class ComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Unity.Entities.ComponentLookup<";
        private const string SYSTEM_HANDLE = "global::Unity.Entities.SystemHandle";
        private const string REFRW = "global::Unity.Entities.RefRW<";
        private const string REFRO = "global::Unity.Entities.RefRO<";

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
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(systemHandle);");
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
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[systemHandle];");
                p.PrintEndLine();

                if (typeRef.IsReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(ENTITY).Print(" entity, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[entity] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentData(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                        .Print(typeName).Print(" componentData)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine("[systemHandle] = componentData;");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRW(systemHandle);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(ENTITY).Print(" entity, out ").Print(REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRefRWOptional(entity);");
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void SetComponentDataOptional(")
                        .Print(ENTITY).Print(" entity, ")
                        .Print(typeName).PrintEndLine(" componentData)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var refRW = ").Print(lookupField).PrintEndLine(".GetRefRW(entity);");
                        p.PrintEndLine();

                        p.PrintLine("if (refRW.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("refRW.ValueRW = componentData;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void SetComponentDataOptional(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ")
                        .Print(typeName).PrintEndLine(" componentData)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var refRW = ").Print(lookupField).PrintEndLine(".GetRefRW(systemHandle);");
                        p.PrintEndLine();

                        p.PrintLine("if (refRW.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("refRW.ValueRW = componentData;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefRO(")
                    .Print(ENTITY).Print(" entity, out ").Print(REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefRO(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefRO(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(ENTITY).Print(" entity, out ").Print(REFRO)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetRefROOptional(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .PrintEndLine(".GetRefROOptional(entity);");
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
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            }
        }
    }
}
