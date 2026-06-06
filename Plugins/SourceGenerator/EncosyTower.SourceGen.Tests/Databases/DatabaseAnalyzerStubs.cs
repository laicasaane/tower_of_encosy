namespace EncosyTower.SourceGen.Tests.Databases;

internal static class DatabaseAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Naming
        {
            public enum NameCasing
            {
                Pascal = 0,
                Camel = 1,
                SnakeLower = 2,
                SnakeUpper = 3,
                KebabLower = 4,
                KebabUpper = 5,
            }
        }

        namespace EncosyTower.Data
        {
            public interface IData { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public sealed class DataAttribute : System.Attribute { }
        }

        namespace EncosyTower.Data.Authoring
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class DataAuthoringConverterAttribute : System.Attribute
            {
                public DataAuthoringConverterAttribute(System.Type type) { }
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
                public DatabaseAttribute(EncosyTower.Naming.NameCasing nameCasing, params System.Type[] converters) { }
                public string AssetName { get; set; }
                public bool WithInstanceAPI { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class TableAttribute : System.Attribute
            {
                public TableAttribute(params System.Type[] converters) { }
                public TableAttribute(EncosyTower.Naming.NameCasing nameCasing, params System.Type[] converters) { }
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
