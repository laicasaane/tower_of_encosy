namespace EncosyTower.Samples.Data
{
    using EncosyTower.Databases;
    using EncosyTower.Naming;

    [Database(NameCasing.SnakeLower, AssetName = $"{nameof(SampleDatabase)}Asset")]
    public readonly partial struct SampleDatabase
    {
        [Table] public readonly StringTableAsset Strings => Get_Strings();

        [Table] public readonly HeroTableAsset Heroes => Get_Heroes();

        [Table] public readonly EnemyTableAsset Enemies => Get_Enemies();
    }
}

#if UNITY_EDITOR && BAKING_SHEET
namespace EncosyTower.Samples.DatabaseAuthoring
{
#pragma warning disable IDE1006 // Naming Styles

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using Cathei.BakingSheet;
    using EncosyTower.Common;
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Samples.Data;
    using EncosyTower.StringIds;

    [AuthorDatabase(typeof(SampleDatabase), typeof(StringToEnemyTypeConverter))]
    public readonly partial struct SampleDatabaseAuthoring
    {
        partial class SheetContainer
        {
            public StringVault StringVault { get; private set; }

            protected override void OnBeforePreprocess(SheetConvertingContext context)
            {
                var vault = StringVault ??= new(1024);

                foreach (var sheet in HeroDataSheets)
                {
                    foreach (var item in sheet)
                    {
                        var str = item.Name;

                        if (str.IsEmptyOrWhiteSpace() == false)
                        {
                            var _ = vault.GetOrMakeId(str);
                        }
                    }
                }

                foreach (var sheet in EnemyDataSheets)
                {
                    foreach (var item in sheet)
                    {
                        var str = item.Name;

                        if (str.IsEmptyOrWhiteSpace() == false)
                        {
                            var _ = vault.GetOrMakeId(str);
                        }
                    }
                }
            }
        }

        partial class StringDataSheet
        {
            partial void OnPreprocess(SheetConvertingContext context)
            {
                if (context.Container is not SheetContainer container)
                {
                    return;
                }

                var vault = container.StringVault;
                var count = vault.Count;

                for (var i = 0; i < count; i++)
                {
                    var str = vault[i];

                    if (vault.TryGetId(str, out var id) == false)
                    {
                        continue;
                    }

                    Add(new __StringData {
                        Id = StringIdValueConverter.Convert(id),
                        Value_Manual = str,
                    });
                }
            }
        }

        partial class HeroDataSheet
        {
            partial void OnProcess(SheetConvertingContext context)
            {
                if (context.Container is not SheetContainer container)
                {
                    return;
                }

                ProcessorAPI.ProcessStringKey(this, container.StringVault);
            }

            partial class __HeroData : IWithStringName
            {
            }
        }

        partial class EnemyDataSheet
        {
            partial void OnProcess(SheetConvertingContext context)
            {
                if (context.Container is not SheetContainer container)
                {
                    return;
                }

                ProcessorAPI.ProcessStringKey(this, container.StringVault);
            }

            partial class __EnemyData : IWithStringName
            {
            }
        }

        internal interface IWithStringName
        {
            string Name { get; set; }

            uint Name_Manual { get; set; }
        }

        internal static class ProcessorAPI
        {
            public static void ProcessStringKey<T>(IEnumerable<T> items, StringVault vault)
                where T : IWithStringName
            {
                foreach (var item in items)
                {
                    if (vault.TryGetId(item.Name, out var id))
                    {
                        item.Name_Manual = StringIdValueConverter.Convert(id);
                    }
                }
            }
        }
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

#pragma warning restore IDE1006 // Naming Styles
}
#endif
