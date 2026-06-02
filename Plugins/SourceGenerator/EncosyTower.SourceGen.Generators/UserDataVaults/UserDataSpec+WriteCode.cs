namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial struct UserDataSpec
    {
        private const string PR_EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string PR_GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(PR_GENERATED_CODE).PrintEndLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial ").Print(typeKeyword).Print(" ").Print(typeName);

                if (generateInterface)
                {
                    p.Print(" : ETUV.IUserData");
                }

                p.PrintEndLine();
                p.OpenScope();
                {
                    var generatesId = RequiresGeneratedProperty(propertyId);
                    var generatesVersion = RequiresGeneratedProperty(propertyVersion);

                    if (generatesId)
                    {
                        WriteProperty(ref p, "Id", propertyId);

                        if (generatesVersion)
                        {
                            p.PrintEndLine();
                        }
                    }

                    if (generatesVersion)
                    {
                        WriteProperty(ref p, "Version", propertyVersion);
                    }
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private static void WriteProperty(ref Printer p, string name, MemberDefinitionType definitionType)
        {
            var modifier = definitionType == MemberDefinitionType.DefinedInBaseTypeAsAbstract
                ? "public override string "
                : "public string ";

            p.PrintBeginLine(modifier).Print(name).PrintEndLine(" { get; set; }");
        }
    }
}
