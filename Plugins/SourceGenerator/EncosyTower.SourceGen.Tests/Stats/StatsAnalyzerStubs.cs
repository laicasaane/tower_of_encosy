namespace EncosyTower.SourceGen.Tests.Stats;

internal static class StatsAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Entities.Stats
        {
            public enum StatDataSize : byte
            {
                Size2 = 2,
                Size4 = 4,
                Size6 = 6,
                Size8 = 8,
                Size12 = 12,
                Size16 = 16,
            }

            public enum StatUserDataSize : byte
            {
                Size1 = 1,
                Size2 = 2,
                Size4 = 4,
            }

            public enum StatVariantType : byte
            {
                None = 0,
                Bool = 1,
                Int = 2,
                Float = 3,
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class StatSystemAttribute : System.Attribute
            {
                public StatSystemAttribute(StatDataSize maxDataSize) { }
                public StatSystemAttribute(StatDataSize maxDataSize, StatUserDataSize maxUserDataSize) { }
                public StatDataSize MaxDataSize { get; }
                public StatUserDataSize MaxUserDataSize { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false, Inherited = false)]
            public sealed class StatDataAttribute : System.Attribute
            {
                public StatDataAttribute(StatVariantType valueType) { }
                public StatDataAttribute(System.Type enumType) { }
                public StatVariantType ValueType { get; }
                public System.Type EnumType { get; }
                public bool SingleValue { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false, Inherited = false)]
            public sealed class StatCollectionAttribute : System.Attribute
            {
                public StatCollectionAttribute(System.Type statSystemType) { }
                public StatCollectionAttribute(System.Type statSystemType, uint typeIdOffset) { }
                public System.Type StatSystemType { get; }
                public uint TypeIdOffset { get; }
            }
        }
        """;
}