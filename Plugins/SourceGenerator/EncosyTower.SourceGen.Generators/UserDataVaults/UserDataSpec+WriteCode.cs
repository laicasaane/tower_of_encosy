namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial struct UserDataSpec
    {
        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataGenerator\", \"{SourceGenVersion.VALUE}\")]";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial ").Print(typeKeyword).Print(" ").Print(typeName);

                if (generateInterface)
                {
                    p.Print(" : ETUV.IUserData");
                }

                p.PrintEndLine();
                p.OpenScope();
                {
                    var generatesId = memberId.ShouldGenerate;
                    var generatesVersion = memberVersion.ShouldGenerate;

                    if (generatesId)
                    {
                        WriteProperty(ref p, "Id", memberId);

                        if (generatesVersion)
                        {
                            p.PrintEndLine();
                        }
                    }

                    if (generatesVersion)
                    {
                        WriteProperty(ref p, "Version", memberVersion);
                    }
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private static void WriteProperty(ref Printer p, string name, MemberDefinition member)
        {
            var modifier = member.type == MemberDefinitionType.DefinedInBaseTypeAsAbstract
                ? "public override string "
                : "public string ";

            p.PrintBeginLine(modifier).Print(name);

            if (member.isField == false)
            {
                p.PrintEndLine(" { get; set; }");
                return;
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ")
                    .PrintIf(member.type == MemberDefinitionType.DefinedInBaseType, "base", "this")
                    .Print(".").Print(member.name).PrintEndLine(";");
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("set => ")
                    .PrintIf(member.type == MemberDefinitionType.DefinedInBaseType, "base", "this")
                    .Print(".").Print(member.name).PrintEndLine(" = value;");
            }
            p.CloseScope();
        }
    }
}
