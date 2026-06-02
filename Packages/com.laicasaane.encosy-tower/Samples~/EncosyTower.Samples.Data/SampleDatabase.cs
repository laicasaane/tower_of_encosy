namespace EncosyTower.Samples.Data
{
    using EncosyTower.Databases;
    using EncosyTower.Naming;

    [Database(NameCasing.SnakeLower, AssetName = $"{nameof(SampleDatabase)}Asset")]
    public readonly partial struct SampleDatabase
    {
        [Table] public readonly HeroTableAsset Heroes => Get_Heroes();

        [Table] public readonly EnemyTableAsset Enemies => Get_Enemies();
    }
}

#if UNITY_EDITOR && BAKING_SHEET
namespace EncosyTower.Samples.DatabaseAuthoring
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Samples.Data;

    [AuthorDatabase(typeof(SampleDatabase), typeof(StringToEnemyTypeConverter))]
    public readonly partial struct SampleDatabaseAuthoring { }

    public readonly struct StringToEnemyTypeConverter
    {
        private static readonly Dictionary<string, EnemyType> s_map;

        static StringToEnemyTypeConverter()
        {
            var map = s_map = new Dictionary<string, EnemyType>(StringComparer.OrdinalIgnoreCase);
            var type = typeof(EnemyType);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsSpecialName)
                {
                    continue;
                }

                var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttribute != null ? descriptionAttribute.Description : field.Name;
                var value = (EnemyType)field.GetValue(null);
                map.TryAdd(description, value);
                map.TryAdd(field.Name, value);
            }
        }

        public static EnemyType Convert(string value)
        {
            s_map.TryGetValue(value, out var result);
            return result;
        }
    }
}
#endif
