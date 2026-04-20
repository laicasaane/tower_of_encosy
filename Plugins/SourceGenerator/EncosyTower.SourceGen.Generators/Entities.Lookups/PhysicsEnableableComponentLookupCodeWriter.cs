namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal sealed class PhysicsEnableableComponentLookupCodeWriter : LookupCodeWriter
    {
        private const string PR_LOOKUP = "LP.PhysicsComponentLookup<";
        private const string PR_SAFE_ENTITY = "LP.SafeEntity";
        private const string PR_REFRW = "UE.RefRW<";

        protected override string LookupTypePrefix => PR_LOOKUP;

        protected override string GetLookupMethod => "GetComponentLookup";

        protected override void WriteConcreteMethods(ref Printer p, in LookupSpec spec)
        {
            foreach (var typeRef in spec.typeRefs)
            {
                var typeName = typeRef.typeName;
                var lookupField = GetLookupFieldName(typeRef);

                WriteBeginRegion(ref p, typeRef.typeShortName);

                WriteAttributes(ref p);
                p.PrintBeginLine("public void HasComponent(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".HasComponent(entity);");
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
                p.PrintBeginLine("public bool TryGetComponent(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine(".TryGetComponent(entity, out componentData);");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public ").Print(typeName).Print(" GetComponentData(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" _)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void GetComponentData(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => componentData = ").Print(lookupField)
                    .PrintEndLine("[entity];");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void SetComponentData(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, ")
                    .Print(typeName).Print(" componentData)")
                    .Print(" => ").Print(lookupField)
                    .PrintEndLine("[entity] = componentData;");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_REFRW)
                        .Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void GetRefRW(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_REFRW).Print(typeName).Print("> result)")
                        .Print(" => result = ").Print(lookupField)
                        .PrintEndLine(".GetRW(entity);");
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_REFRW)
                        .Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(PR_REFRW).Print(typeName).PrintEndLine(">();");
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

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void GetRefRWOptional(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, ").Print(typeName)
                        .Print(" _, out ").Print(PR_REFRW).Print(typeName).PrintEndLine("> result)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var lookup = ").Print(lookupField).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine("if (lookup.HasComponent(entity) == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result = new ").Print(PR_REFRW).Print(typeName).PrintEndLine(">();");
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

                p.PrintLine("#region    ENABLEABLE API");
                p.PrintLine("#endregion ==============");
                p.PrintEndLine();

                WriteAttributes(ref p);
                p.PrintBeginLine("public void IsComponentEnabled(")
                    .Print(PR_SAFE_ENTITY).Print(" entity, out ").Print(PR_BOOL)
                    .Print(typeName).Print("> result)")
                    .Print(" => result = (").Print(PR_BOOL).Print(typeName)
                    .Print(">)").Print(lookupField)
                    .PrintEndLine(".IsEnabled(entity);");
                p.PrintEndLine();

                if (typeRef.isReadOnly == false)
                {
                    WriteAttributes(ref p);
                    p.PrintBeginLine("public void SetComponentEnabled(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, ").Print(PR_BOOL)
                        .Print(typeName).Print("> value)")
                        .Print(" => ").Print(lookupField)
                        .PrintEndLine(".SetEnabled(entity, value);");
                    p.PrintEndLine();

                    WriteAttributesNoInline(ref p);
                    p.PrintBeginLine("public void SetComponentEnabledOptional(")
                        .Print(PR_SAFE_ENTITY).Print(" entity, ").Print(PR_BOOL)
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

                WriteEndRegion(ref p, typeRef.typeShortName);
            }
        }
    }
}
