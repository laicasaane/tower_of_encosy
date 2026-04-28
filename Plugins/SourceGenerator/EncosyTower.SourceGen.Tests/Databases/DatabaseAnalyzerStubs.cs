namespace EncosyTower.SourceGen.Tests.Databases;

internal static class DatabaseAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Naming
        {
            public enum NamingStrategy
            {
                Default = 0,
                CamelCase = 1,
                PascalCase = 2,
                SnakeCase = 3,
            }
        }

        namespace EncosyTower.Data
        {
            public interface IData { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public sealed class DataAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class DataConverterAttribute : System.Attribute
            {
                public DataConverterAttribute(System.Type type) { }
                public System.Type Type { get; }
            }
        }

        namespace EncosyTower.Databases
        {
            public abstract class DataTableAsset<TDataId, TData> { }

            public abstract class DataTableAsset<TDataId, TData, TConvertedId> { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)]
            public sealed class DatabaseAttribute : System.Attribute
            {
                public DatabaseAttribute(params System.Type[] converters) { }
                public DatabaseAttribute(EncosyTower.Naming.NamingStrategy namingStrategy, params System.Type[] converters) { }
                public string AssetName { get; set; }
                public bool WithInstanceAPI { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class TableAttribute : System.Attribute
            {
                public TableAttribute(params System.Type[] converters) { }
                public TableAttribute(EncosyTower.Naming.NamingStrategy namingStrategy, params System.Type[] converters) { }
            }
        }

        namespace EncosyTower.Databases.Authoring
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = true)]
            public sealed class HorizontalAttribute : System.Attribute
            {
                public HorizontalAttribute(System.Type targetType, string propertyName) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
            public sealed class AuthorDatabaseAttribute : System.Attribute
            {
                public AuthorDatabaseAttribute(System.Type databaseType) { }
                public System.Type DatabaseType { get; }
            }
        }
        """;
}
