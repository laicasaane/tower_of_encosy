namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    partial struct DatabaseSpec
    {
        private const string PR_UNITY_EXTENSIONS = "ETUE.EncosyUnityObjectExtensions";
        private const string PR_UNITY_IS_VALID = $"{PR_UNITY_EXTENSIONS}.IsValid";
        private const string PR_UNITY_IS_INVALID = $"{PR_UNITY_EXTENSIONS}.IsInvalid";
        private const string PR_DOES_NOT_RETURN_IF_FALSE = "[SDCA.DoesNotReturnIf(false)]";
        private const string PR_STRING_VAULT = "ETSI.StringVault";
        private const string PR_STRING_ID = "ETSI.StringId";
        private const string PR_MAKE_ID = "Vault.Instance.GetOrMakeId";
        private const string PR_INITIALIZATION_BEHAVIOUR = "ETI.InitializationBehaviour";

        public const string NOT_NULL = "[SDCA.NotNull]";
        public const string INITIALIZE_ON_LOAD_METHOD = "[global::UnityEditor.InitializeOnEnterPlayMode]";
        public const string ASSET_KEY = "ETAK.AssetKey";

        public readonly string WriteCode()
        {
            var typeKeyword = isStruct ? "struct" : "class";

            var p = Printer.DefaultLarge;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (isStruct)
            {
                p.PrintLine(PR_STRUCT_LAYOUT_AUTO);
            }

            p.PrintBeginLine($"partial ").Print(typeKeyword).Print(" ").Print(typeName)
                .Print(" : ").PrintEndLine("ETDB.IDatabase");
            p.OpenScope();
            {
                WriteInstanceMembers(ref p, typeName, isStruct);
                WriteGetMethods(ref p, tables, isStruct);
                WriteConversion(ref p, isStruct, typeName);
                WriteThrows(ref p, typeName);
                WriteNames(ref p, assetName, nameCasing, tables, typeName);
                WriteKeys(ref p, tables);
                WriteIds(ref p, tables);
                WriteNestedInstanceType(ref p, withInstanceAPI, isStruct, typeName, tables);
                WriteVault(ref p, tables);
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteConversion(ref Printer p, bool isStruct, string typeName)
        {
            p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public static ")
                .PrintIf(isStruct, "implicit", "explicit")
                .Print(" operator ").Print(typeName)
                .Print("(").Print(PR_DATABASE_ASSET).PrintEndLine(" database)");
            p.OpenScope();
            {
                p.PrintBeginLine("return new ").Print(typeName).PrintEndLine("(database);");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteInstanceMembers(ref Printer p, string databaseTypeName, bool isDatabaseStruct)
        {
            p.PrintBeginLine("private readonly ").Print(PR_DATABASE_ASSET).PrintEndLine(" _database;");
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(databaseTypeName)
                .Print("(").Print(PR_DATABASE_ASSET).Print(" database)")
                .PrintEndLineIf(isDatabaseStruct, " : this()", "");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfInvalid(database);");
                p.PrintLine("_database = database;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public ").Print(databaseTypeName)
                .Print("(").Print(PR_DATABASE_ASSET).Print(" database, ")
                .Print(PR_INITIALIZATION_BEHAVIOUR).Print(" initializationBehaviour)")
                .PrintEndLineIf(isDatabaseStruct, " : this()", "");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfInvalid(database);");
                p.PrintLine("_database = database;");
                p.PrintEndLine();

                p.PrintLine("switch (initializationBehaviour)");
                p.OpenScope();
                {
                    p.PrintBeginLine("case ").Print(PR_INITIALIZATION_BEHAVIOUR).PrintEndLine(".Respect:");
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

                    p.PrintBeginLine("case ").Print(PR_INITIALIZATION_BEHAVIOUR).PrintEndLine(".Forced:");
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .PrintEndLine("bool IsValid");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintBeginLine("get => ").Print(PR_UNITY_IS_VALID).PrintEndLine("(_database);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .PrintEndLine("bool IsInitialized");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public ")
                .PrintIf(isDatabaseStruct, "readonly ")
                .Print(PR_DATABASE_ASSET).PrintEndLine(" Database");
            p.OpenScope();
            {
                p.PrintLine(PR_AGGRESSIVE_INLINING);
                p.PrintLine("get => _database;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
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

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public readonly void Deinitialize()");
            p.OpenScope();
            {
                p.PrintLine("ThrowIfNotCreated(IsValid);");
                p.PrintLine("_database.Deinitialize();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteGetMethods(ref Printer p, EquatableArray<TableSpec> tables, bool isStruct)
        {
            foreach (var table in tables)
            {
                var tableTypeName = table.typeFullName;
                var name = table.propertyName;

                p.PrintBeginLine(PR_AGGRESSIVE_INLINING).Print(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                p.PrintBeginLine("private ")
                    .PrintIf(isStruct, "readonly ")
                    .Print(tableTypeName)
                    .Print(" Get_").Print(name).PrintEndLine("()");
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
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("[SD.Conditional(\"__ENCOSY_VALIDATION__\")]");
            p.PrintLine("private static void ThrowIfInvalid(global::UnityEngine.Object asset)");
            p.OpenScope();
            {
                p.PrintBeginLine("if (").Print(PR_UNITY_IS_INVALID).PrintEndLine("(asset))");
                p.OpenScope();
                {
                    p.PrintLine("throw new S.ArgumentNullException(\"asset\");");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("[SD.Conditional(\"__ENCOSY_VALIDATION__\")]");
            p.PrintBeginLine("private static void ThrowIfNotCreated(")
                .Print(PR_DOES_NOT_RETURN_IF_FALSE)
                .PrintEndLine(" bool value)");
            p.OpenScope();
            {
                p.PrintLine("if (value == false)");
                p.OpenScope();
                {
                    p.PrintBeginLine("throw new S.InvalidOperationException(\"")
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
            , string dbAssetName
            , NameCasing dbNameCasing
            , EquatableArray<TableSpec> tables
            , string containerTypeName
        )
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public static partial class Names");
            p.OpenScope();
            {
                {
                    var fieldValue = dbNameCasing.ConvertName(dbAssetName);

                    p.PrintLine(string.Format(PR_GENERATED_ASSET_NAME, containerTypeName, PR_DATABASE_ASSET));
                    p.PrintBeginLine("public const string DATABASE = \"").Print(fieldValue).PrintEndLine("\";");
                    p.PrintEndLine();
                }

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var fullTypeName = table.typeFullName;
                        var tableTypeName = table.typeName;
                        var name = table.propertyName;
                        var fieldName = NameCasing.SnakeUpper.ConvertName(name);
                        var fieldValue = table.deduplicateAssetName
                            ? table.nameCasing.ConvertName($"{tableTypeName}_{name}")
                            : table.nameCasing.ConvertName(tableTypeName)
                            ;

                        p.PrintLine(string.Format(PR_GENERATED_ASSET_NAME, containerTypeName, fullTypeName));
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

        private static void WriteKeys(ref Printer p, EquatableArray<TableSpec> tables)
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public static partial class Keys");
            p.OpenScope();
            {
                p.PrintBeginLine("public static readonly ").Print(ASSET_KEY)
                    .Print("<").Print(PR_DATABASE_ASSET).PrintEndLine("> Database = new(Names.DATABASE);");
                p.PrintEndLine();

                p.PrintLine("public static partial class Tables");
                p.OpenScope();
                {
                    foreach (var table in tables)
                    {
                        var name = table.propertyName;
                        var tableTypeName = table.typeFullName;
                        var nameUpper = NameCasing.SnakeUpper.ConvertName(name);

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

        private static void WriteVault(ref Printer p, EquatableArray<TableSpec> tables)
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("private static partial class Vault");
            p.OpenScope();
            {
                p.PrintBeginLine("public static readonly ").Print(PR_STRING_VAULT)
                    .Print(" Instance = new(").Print($"{tables.Count + 1}").PrintEndLine(");");
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteIds(ref Printer p, EquatableArray<TableSpec> tables)
        {
            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public static partial class Ids");
            p.OpenScope();
            {
                p.PrintBeginLine("public static ").Print(PR_STRING_ID).PrintEndLine(" Database { get; private set; }");
                p.PrintEndLine();

                p.PrintLine("public static void Initialize()");
                p.OpenScope();
                {
                    p.Print("#if UNITY_EDITOR").PrintEndLine();
                    {
                        p.PrintLine("Vault.Instance.Clear();");
                    }
                    p.Print("#endif").PrintEndLine();
                    p.PrintEndLine();

                    p.PrintBeginLine("Database = ").Print(PR_MAKE_ID).PrintEndLine("(Names.DATABASE);");
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
                        var name = table.propertyName;

                        p.PrintBeginLine("public static ").Print(PR_STRING_ID).Print(" ").Print(name)
                            .PrintEndLine(" { get; private set; }");
                        p.PrintEndLine();
                    }

                    p.PrintLine("public static void Initialize()");
                    p.OpenScope();
                    {
                        foreach (var table in tables)
                        {
                            var name = table.propertyName;
                            var nameUpper = NameCasing.SnakeUpper.ConvertName(name);

                            p.PrintBeginLine(name).Print(" = ")
                                .Print(PR_MAKE_ID).Print("(Names.Tables.").Print(nameUpper).PrintEndLine(");");
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

        private static void WriteNestedInstanceType(
              ref Printer p
            , bool withInstanceAPI
            , bool isStruct
            , string typeName
            , EquatableArray<TableSpec> tables
        )
        {
            if (withInstanceAPI == false)
            {
                return;
            }

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintLine("public static partial class Instance");
            p.OpenScope();
            {
                p.PrintBeginLine("private static ").Print(typeName).PrintEndLine(" s_instance;");
                p.PrintEndLine();

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

                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                }

                p.PrintEndLine();

                p.PrintBeginLine("public static ").Print(PR_DATABASE_ASSET).PrintEndLine(" Database");
                p.OpenScope();
                {
                    p.PrintLine(PR_AGGRESSIVE_INLINING);
                    p.PrintLine("get => s_instance.Database;");
                }
                p.CloseScope();
                p.PrintEndLine();

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
                        var tableTypeName = table.typeFullName;
                        var propName = table.propertyName;

                        p.PrintBeginLine("public static ").Print(tableTypeName).Print(" ").PrintEndLine(propName);
                        p.OpenScope();
                        {
                            p.PrintLine(PR_AGGRESSIVE_INLINING);
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

        public static string GetTableAssetName(TableSpec table)
            => $"{table.typeName}_{table.propertyName}";
    }
}
