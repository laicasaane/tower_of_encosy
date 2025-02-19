using System.Collections.Immutable;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    partial class DatabaseDeclaration
    {
        private const string DATABASE_ASSET_NAME = nameof(DATABASE_ASSET_NAME);
        private const string TABLE_ASSET_NAME = nameof(TABLE_ASSET_NAME);

        public string WriteDatabase(
              string databaseAssetName
            , NamingStrategy databaseNamingStrategy
            , ImmutableArray<TableRef> tables
        )
        {
            var syntax = DatabaseRef.Syntax;
            var isDatabaseStruct = DatabaseRef.Symbol.IsValueType;
            var databaseTypeName = syntax.Identifier.Text;
            var databaseTypeKeyword = isDatabaseStruct ? "struct" : "class";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (isDatabaseStruct)
            {
                p.PrintLine(STRUCT_LAYOUT_AUTO);
            }

            p.PrintBeginLine($"partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print(" : ").PrintEndLine(IDATABASE);
            p.OpenScope();
            {
                {
                    var fieldValue = databaseAssetName.ToNamingCase(databaseNamingStrategy);

                    p.PrintLine(GENERATED_ASSET_NAME).PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public const string ").Print(DATABASE_ASSET_NAME)
                        .Print(" = \"").Print(fieldValue).PrintEndLine("\";");
                    p.PrintEndLine();
                }

                foreach (var table in tables)
                {
                    var typeName = table.Type.Name;
                    var name = table.PropertyName;
                    var fieldName = $"{StringUtils.ToSnakeCase(name).ToUpper()}_{TABLE_ASSET_NAME}";
                    var fieldValue = $"{typeName}_{name}".ToNamingCase(table.NamingStrategy);

                    p.PrintLine(GENERATED_ASSET_NAME).PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public const string ").Print(fieldName)
                        .Print(" = \"").Print(fieldValue).PrintEndLine("\";");
                    p.PrintEndLine();
                }

                p.PrintBeginLine("private readonly ").Print(DATABASE_ASSET).PrintEndLine(" _asset;");
                p.PrintEndLine();

                p.PrintBeginLine("public ").Print(databaseTypeName)
                    .Print("(").Print(DATABASE_ASSET).Print(" asset)");

                if (isDatabaseStruct)
                {
                    p.PrintEndLine(" : this()");
                }

                p.OpenScope();
                {
                    p.PrintLine("_asset = asset;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public ")
                    .PrintIf(isDatabaseStruct, "readonly ")
                    .Print(DATABASE_ASSET).PrintEndLine(" Asset");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _asset;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly void Initialize()");
                p.OpenScope();
                {
                    p.PrintLine("_asset.Initialize();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly void Deinitialize()");
                p.OpenScope();
                {
                    p.PrintLine("_asset.Deinitialize();");
                }
                p.CloseScope();
                p.PrintEndLine();

                foreach (var table in tables)
                {
                    var tableTypeName = table.Type.ToFullName();
                    var tableName = StringUtils.ToSnakeCase(table.PropertyName).ToUpper();
                    var tableNameConstField = $"{tableName}_{TABLE_ASSET_NAME}";

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("private ")
                        .PrintIf(isDatabaseStruct, "readonly ")
                        .Print(tableTypeName)
                        .Print(" Get_").Print(table.PropertyName).PrintEndLine("()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return _asset.GetDataTableAsset<").Print(tableTypeName).Print(">(")
                            .Print(tableNameConstField).PrintEndLine(").ValueOrDefault();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static ")
                    .PrintIf(isDatabaseStruct, "implicit", "explicit")
                    .Print(" operator ").Print(databaseTypeName)
                    .Print("(").Print(DATABASE_ASSET).PrintEndLine(" asset)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return new ").Print(databaseTypeName).PrintEndLine("(asset);");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        public static string GetTableAssetName(TableRef table)
            => $"{table.Type.Name}_{table.PropertyName}";
    }
}
