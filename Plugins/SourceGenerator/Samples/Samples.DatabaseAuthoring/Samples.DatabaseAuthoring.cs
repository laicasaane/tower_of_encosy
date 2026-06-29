namespace Samples.DatabaseAuthoring
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using EncosyTower.Databases.Authoring;
    using Samples.Data.Databases;
    using Samples.Data.Enemies;

    [AuthorDatabase(typeof(SampleDatabase), typeof(StringToEnemyTypeConverter))]
    public partial struct SampleDatabaseAuthoring
    {
    }

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

namespace Samples.Data.XDatabases.Authoring
{
    using System.Collections.Generic;
    using EncosyTower.Data;
    using EncosyTower.Databases.Authoring;

    [AuthorDatabase(typeof(GameDatabase), typeof(Vector2IntIndexConverter))]
    [ConverterForDataProperty(typeof(PlacementData), nameof(PlacementData.Size), typeof(Vector2IntConverter))]
    [ConverterForTable(typeof(PlacementTableAsset), typeof(Vector2IntIndexArrayConverter))]
    public readonly partial struct GameDatabaseAuthoring
    {
    }

    [Data]
    public partial struct Vector2IntData
    {
        [DataProperty]
        public readonly int X => Get_X();

        [DataProperty]
        public readonly int Y => Get_Y();

        public static implicit operator Vector2Int(Vector2IntData data)
            => new(data.X, data.Y);

        public static implicit operator Vector2IntData(Vector2Int position)
            => new() { _x = position.x, _y = position.y };
    }

    public readonly struct Vector2IntConverter
    {
        public static Vector2Int Convert(Vector2IntData data)
            => new() { x = data.X, y = data.Y };
    }

    public readonly struct Vector2IntIndexConverter
    {
        public static Vector2Int Convert(Vector2IntData data)
            => new() { x = data.X - 1, y = data.Y - 1 };
    }

    public readonly struct Vector2IntIndexArrayConverter
    {
        public static Vector2Int[] Convert(List<Vector2IntData> data)
        {
            var length = data.Count;
            var result = new Vector2Int[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = Vector2IntIndexConverter.Convert(data[i]);
            }

            return result;
        }
    }
}
