using System.Collections.Immutable;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    partial class DatabaseDeclaration
    {
        private const string UNITY_EXTENSIONS = "global::EncosyTower.UnityExtensions.EncosyUnityObjectExtensions";
        private const string UNITY_IS_VALID = $"{UNITY_EXTENSIONS}.IsValid";
        private const string UNITY_IS_INVALID = $"{UNITY_EXTENSIONS}.IsInvalid";
        private const string DOES_NOT_RETURN_IF_FALSE = "[global::System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)]";
        private const string STRING_VAULT = "global::EncosyTower.StringIds.StringVault";
        private const string STRING_ID = "global::EncosyTower.StringIds.StringId";
        private const string MAKE_ID = "Vault.Instance.GetOrMakeId";
        private const string INITIALIZATION_BEHAVIOUR = "global::EncosyTower.Initialization.InitializationBehaviour";

        public const string NOT_NULL = "[global::System.Diagnostics.CodeAnalysis.NotNull]";
        public const string INITIALIZE_ON_LOAD_METHOD = "[global::UnityEditor.InitializeOnEnterPlayMode]";
        public const string ASSET_KEY = "global::EncosyTower.AssetKeys.AssetKey";


        public string WriteCode(ImmutableArray<TableRef> tables)
        {
            var syntax = DatabaseRef.Syntax;
            var isStruct = DatabaseRef.Symbol.IsValueType;
            var typeName = syntax.Identifier.Text;
            var typeKeyword = isStruct ? "struct" : "class";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (isStruct)
            {
                p.PrintLine(STRUCT_LAYOUT_AUTO);
            }

            p.PrintBeginLine($"partial ").Print(typeKeyword).Print(" ").Print(typeName)
                .Print(" : ").PrintEndLine(IDATABASE);
            p.OpenScope();
            {
                WriteInstanceMembers(ref p, typeName, isStruct);
                WriteGetMethods(ref p, tables, isStruct);
                WriteConversion(ref p, isStruct, typeName);
                WriteThrows(ref p, typeName);
                WriteNames(ref p, DatabaseRef, tables, typeName);
                WriteKeys(ref p, tables);
                WriteIds(ref p, tables);
                WriteNestedInstanceType(ref p, DatabaseRef, tables);
                WriteVault(ref p, tables);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteConversion(ref Printer p, bool isStruct, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public static ")
                .PrintIf(isStruct, "implicit", "explicit")
                .Print(" operator ").Print(typeName)
                .Print("(").Print(DATABASE_ASSET).PrintEndLine(" database)");
            p.OpenScope();
            {
                p.PrintBeginLine("return new ").Print(typeName).PrintEndLine("(database);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteInstanceMembers(ref Printer p, string databaseTypeName, bool isDatabaseStruct)
        {
            p.PrintBeginLine("private readonly ").Print(DATABASE_ASSET).PrintEndLine(" _database;");
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(databaseTypeName)
                .Print("(").Print(DATABASE_ASSET).Print(" database)")
                .PrintEndLineIf(isDatabaseStruct, " : this()", "");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfInvalid(database);");
                p.PrintLine("_database = database;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(databaseTypeName)
                .Print("(").Print(DATABASE_ASSET).Print(" database, ")
                .Print(INITIALIZATION_BEHAVIOUR).Print(" initializationBehaviour)")
                .PrintEndLineIf(isDatabaseStruct, " : this()", "");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfInvalid(database);");
                p.PrintLine("_database = database;");
                p.PrintEndLine();

                p.PrintLine("switch (initializationBehaviour)");
                p.OpenScope();
                {
                    p.PrintBeginLine("case ").Print(INITIALIZATION_BEHAVIOUR).PrintEndLine(".Respect:");
                    p.OpenScope();
                    {
                        p.PrintLine("if (IsInitialized == false)");
                        p.OpenScope();
                        {
                            p.PrintLine("Initialize();");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("break;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("case ").Print(INITIALIZATION_BEHAVIOUR).PrintEndLine(".Forced:");
                    p.OpenScope();
                    {
                        p.PrintLine("Deinitialize();");
                        p.PrintLine("Initialize();");
                        p.PrintLine("break;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .PrintEndLine("bool IsValid");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(UNITY_IS_VALID).PrintEndLine("(_database);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .PrintEndLine("bool IsInitialized");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("get");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCreated(IsValid);");
                    p.PrintLine("return _database.IsInitialized;");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .Print(DATABASE_ASSET).PrintEndLine(" Database");
            p.OpenScope();
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("get => _database;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly void Initialize()");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfNotCreated(IsValid);");
                p.PrintEndLine();

                p.PrintLine("Ids.Initialize();");
                p.PrintEndLine();

                p.PrintLine("_database.StringVault = Vault.Instance;");
                p.PrintLine("_database.Initialize();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public readonly void Deinitialize()");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfNotCreated(IsValid);");
                p.PrintLine("_database.Deinitialize();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteGetMethods(ref Printer p, ImmutableArray<TableRef> tables, bool isStruct)
        {
            foreach (var table in tables)
            {
                var tableTypeName = table.Type.ToFullName();
                var name = table.PropertyName;

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("private ")
                    .PrintIf(isStruct, "readonly ")
                    .Print(tableTypeName)
                    .Print(" Get_").Print(table.PropertyName).PrintEndLine("()");
                p.OpenScope();
                {
                    p.PrintLine("ThrowIfNotCreated(IsValid);");
                    p.PrintBeginLine("return _database.GetDataTableAsset<").Print(tableTypeName)
                        .Print(">(Ids.Tables.").Print(name).PrintEndLine(").GetValueOrDefault();");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteThrows(ref Printer p, string typeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("[global::System.Diagnostics.Conditional(\"__SUPERGAME_VALIDATION__\")]");
            p.PrintLine("private static void ThrowIfInvalid(global::UnityEngine.Object asset)");
            p.OpenScope();
            {
                p.PrintBeginLine("if (").Print(UNITY_IS_INVALID).PrintEndLine("(asset))");
                p.OpenScope();
                {
                    p.PrintLine("throw new global::System.ArgumentNullException(\"asset\");");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("[global::System.Diagnostics.Conditional(\"__SUPERGAME_VALIDATION__\")]");
            p.PrintBeginLine("private static void ThrowIfNotCreated(")
                .Print(DOES_NOT_RETURN_IF_FALSE)
                .PrintEndLine(" bool value)");
            p.OpenScope();
            {
                p.PrintLine("if (value == false)");
                p.OpenScope();
                {
                    p.PrintBeginLine("throw new global::System.InvalidOperationException(\"")
                        .Print(typeName)
                        .Print(" must be created using the constructor that takes a DatabaseAsset.")
                        .PrintEndLine("\");");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteNames(
              ref Printer p
            , DatabaseRef dbRef
            , ImmutableArray<TableRef> tables
            , string containerTypeName
        )
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Names");
            p.OpenScope();
            {
                {
                    var fieldValue = dbRef.AssetName.ToNamingCase(dbRef.NamingStrategy);

                    p.PrintLine(string.Format(GENERATED_ASSET_NAME, containerTypeName, DATABASE_ASSET));
                    p.PrintLine(GENERATED_CODE);
                    p.PrintBeginLine("public const string DATABASE = \"").Print(fieldValue).PrintEndLine("\";");
                    p.PrintEndLine();
                }

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var fullTypeName = table.Type.ToFullName();
                        var typeName = table.Type.Name;
                        var name = table.PropertyName;
                        var fieldName = StringUtils.ToSnakeCase(name).ToUpper();
                        var fieldValue = $"{typeName}_{name}".ToNamingCase(table.NamingStrategy);

                        p.PrintLine(string.Format(GENERATED_ASSET_NAME, containerTypeName, fullTypeName));
                        p.PrintLine(GENERATED_CODE);
                        p.PrintBeginLine("public const string ").Print(fieldName)
                            .Print(" = \"").Print(fieldValue).PrintEndLine("\";");
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteKeys(ref Printer p, ImmutableArray<TableRef> tables)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Keys");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public static readonly ").Print(ASSET_KEY)
                    .Print("<").Print(DATABASE_ASSET).PrintEndLine("> Database = new(Names.DATABASE);");
                p.PrintEndLine();

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var name = table.PropertyName;
                        var tableTypeName = table.Type.ToFullName();
                        var nameUpper = StringUtils.ToSnakeCase(name).ToUpper();

                        p.PrintLine(GENERATED_CODE);
                        p.PrintBeginLine("public static readonly ").Print(ASSET_KEY)
                            .Print("<").Print(tableTypeName).Print("> ")
                            .Print(name).Print(" = new").Print("(Names.Tables.").Print(nameUpper).PrintEndLine(");");
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteVault(ref Printer p, ImmutableArray<TableRef> tables)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("private static partial class Vault");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public static readonly ").Print(STRING_VAULT)
                    .Print(" Instance = new(").Print($"{tables.Length + 1}").PrintEndLine(");");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteIds(ref Printer p, ImmutableArray<TableRef> tables)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("public static partial class Ids");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ").Print(STRING_ID).PrintEndLine(" Database { get; private set; }");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static void Initialize()");
                p.OpenScope();
                {
                    p.Print("#if UNITY_EDITOR").PrintEndLine();
                    {
                        p.PrintLine("Vault.Instance.Clear();");
                    }
                    p.Print("#endif").PrintEndLine();
                    p.PrintEndLine();

                    p.PrintBeginLine("Database = ").Print(MAKE_ID).PrintEndLine("(Names.DATABASE);");
                    p.PrintEndLine();

                    p.PrintLine("Tables.Initialize();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var name = table.PropertyName;

                        p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public static ").Print(STRING_ID).Print(" ").Print(name)
                            .PrintEndLine(" { get; private set; }");
                        p.PrintEndLine();
                    }

                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine("public static void Initialize()");
                    p.OpenScope();
                    {
                        foreach (var table in tables)
                        {
                            var name = table.PropertyName;
                            var nameUpper = StringUtils.ToSnakeCase(name).ToUpper();

                            p.PrintBeginLine(name).Print(" = ")
                                .Print(MAKE_ID).Print("(Names.Tables.").Print(nameUpper).PrintEndLine(");");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteNestedInstanceType(ref Printer p, DatabaseRef dbRef, ImmutableArray<TableRef> tables)
        {
            if (dbRef.WithInstanceAPI == false)
            {
                return;
            }

            var syntax = dbRef.Syntax;
            var isStruct = dbRef.Symbol.IsValueType;
            var typeName = syntax.Identifier.Text;

            p.PrintLine("public static partial class Instance");
            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("private static ").Print(typeName).PrintEndLine(" s_instance;");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static bool IsInitialized");

                if (isStruct)
                {
                    p.PrintEndLine(" => s_instance.IsValid && s_instance.IsInitialized;");
                }
                else
                {
                    p.PrintEndLine();
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        p.PrintLine("if (s_instance is not null)");
                        p.OpenScope();
                        {
                            p.PrintLine("return s_instance.IsValid && s_instance.IsInitialized;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine("return false");
                    }
                    p.CloseScope();
                }

                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static ").Print(DATABASE_ASSET).PrintEndLine(" Database");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => s_instance.Database;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("public static void Initialize(")
                    .PrintIf(isStruct == false, NOT_NULL).PrintIf(isStruct == false, " ")
                    .Print(typeName).PrintEndLine(" instance)");
                p.OpenScope();
                {
                    p.PrintLine("s_instance = instance;");
                    p.PrintLine("s_instance.Initialize();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine("public static void Deinitialize()");
                p.OpenScope();
                {
                    p.PrintLine("s_instance.Deinitialize();");
                    p.PrintLine("s_instance = default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR").PrintEndLine();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine(INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine("private static void InitWhenDomainReloadDisabled()");
                    p.OpenScope();
                    {
                        p.PrintLine("if (IsInitialized)");
                        p.OpenScope();
                        {
                            p.PrintLine("Deinitialize();");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.Print("#endif").PrintEndLine();
                p.PrintEndLine();

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var tableTypeName = table.Type.ToFullName();
                        var propName = table.PropertyName;

                        p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                        p.PrintBeginLine("public static ").Print(tableTypeName).Print(" ").PrintEndLine(propName);
                        p.OpenScope();
                        {
                            p.PrintLine(AGGRESSIVE_INLINING);
                            p.PrintBeginLine("get => s_instance.").Print(propName).PrintEndLine(";");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        public static string GetTableAssetName(TableRef table)
            => $"{table.Type.Name}_{table.PropertyName}";
    }
}
