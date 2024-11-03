using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    [Generator]
    internal sealed class EnableableComponentLookupGenerator : LookupGenerator
    {
        protected override string Interface => "global::EncosyTower.Modules.Entities.IEnableableComponentLookups";

        protected override LookupCodeWriter CodeWriter => new EnableableComponentLookupCodeWriter();

        protected override LookupDeclaration GetDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            return new EnableableComponentLookupDeclaration(context, candidate, semanticModel);
        }

        protected override DiagnosticDescriptor ErrorDescriptor
            => new("SG_ENABLEABLE_COMPONENT_LOOKUPS_01"
                , "Enableable Component Lookups Generator Error"
                , "This error indicates a bug in the Enableable Component Lookups source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Entities.IEnableableComponentLookups"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }

    internal sealed class EnableableComponentLookupDeclaration : LookupDeclaration
    {
        protected override string Interface => "global::Unity.Entities.IComponentData";

        protected override string Interface2 => "global::Unity.Entities.IEnableableComponent";

        public override string InterfaceLookupRO => "global::EncosyTower.Modules.Entities.Lookups.IEnableableComponentLookupRO";

        public override string InterfaceLookupRW => "global::EncosyTower.Modules.Entities.Lookups.IEnableableComponentLookupRW";

        public EnableableComponentLookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        ) : base(context, candidate, semanticModel) { }
    }

    internal sealed class EnableableComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string LOOKUP = "global::Unity.Entities.ComponentLookup<";
        private const string SYSTEM_HANDLE = "global::Unity.Entities.SystemHandle";
        private const string REFRW = "global::Unity.Entities.RefRW<";
        private const string REFRO = "global::Unity.Entities.RefRO<";
        private const string ENABLED_REFRW = "global::Unity.Entities.EnabledRefRW<";
        private const string ENABLED_REFRO = "global::Unity.Entities.EnabledRefRO<";

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
                        .Print(ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(REFRW).Print(typeName).Print("> result)")
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

                p.PrintLine("#region    ENABLEABLE API");
                p.PrintLine("#endregion ==============");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsComponentEnabled(")
                    .Print(ENTITY).Print(" entity, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsComponentEnabled(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsComponentEnabled(")
                    .Print(SYSTEM_HANDLE).Print(" systemHandle, out ").Print(BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsComponentEnabled(systemHandle);");
                p.PrintEndLine();

                if (typeRef.IsReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentEnabled(")
                        .Print(ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetComponentEnabled(entity, value);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentEnabled(")
                        .Print(SYSTEM_HANDLE).Print(" systemHandle, ").Print(BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetComponentEnabled(systemHandle, value);");
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

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintBeginLine("public void SetComponentEnabledOptional(")
                        .Print(ENTITY).Print(" entity, ").Print(BOOL)
                        .Print(typeName).PrintEndLine("> value)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var refRW = ").Print(lookupField)
                            .Print(".GetComponentEnabledRefRWOptional<").Print(typeName)
                            .PrintEndLine(">(entity);");
                        p.PrintEndLine();

                        p.PrintLine("if (refRW.IsValid)");
                        p.OpenScope();
                        {
                            p.PrintLine("refRW.ValueRW = value;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
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
                    .Print(".GetComponentEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetEnabledRefROOptional(")
                    .Print(ENTITY).Print(" entity, ").Print(typeName)
                    .Print(" _, out ").Print(ENABLED_REFRO).Print(typeName).Print("> result)")
                    .Print(" => result = ").Print(lookupField)
                    .Print(".GetComponentEnabledRefROOptional<").Print(typeName)
                    .PrintEndLine(">(entity);");
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
