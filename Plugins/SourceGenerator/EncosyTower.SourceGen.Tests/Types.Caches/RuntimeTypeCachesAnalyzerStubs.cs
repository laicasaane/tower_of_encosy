namespace EncosyTower.SourceGen.Tests.Types.Caches;

internal static class RuntimeTypeCachesAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Types
        {
            public static class RuntimeTypeCache
            {
                public static void GetInfo<T>() { }

                public static void GetTypesDerivedFrom<T>() { }

                public static void GetTypesDerivedFrom<T>(string assemblyName) { }

                public static void GetTypesWithAttribute<T>() { }

                public static void GetTypesWithAttribute<T>(string assemblyName) { }

                public static void GetFieldsWithAttribute<T>() { }

                public static void GetFieldsWithAttribute<T>(string assemblyName) { }

                public static void GetMethodsWithAttribute<T>() { }

                public static void GetMethodsWithAttribute<T>(string assemblyName) { }
            }
        }

        namespace EncosyTower.Types.Caches
        {
            public class CachedAttributeType { }
        }
        """;
}
