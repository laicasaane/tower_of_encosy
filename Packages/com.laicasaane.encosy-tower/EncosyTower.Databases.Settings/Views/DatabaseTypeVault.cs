using System;
using System.Reflection;
using EncosyTower.Collections;
using EncosyTower.Databases.Authoring;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Databases.Settings.Views
{
    internal record DatabaseRecord(Type AuthorType, Type DatabaseType, string Name);

    internal static class DatabaseTypeVault
    {
        public static readonly ArrayMap<Type, int> TypeToIndexMap = new();
        public static readonly FasterList<DatabaseRecord> Records = new();

        public static void Initialize()
        {
            var map = TypeToIndexMap;
            var records = Records;

            map.Clear();
            records.Clear();

            var authoringTypes = TypeCache.GetTypesWithAttribute<AuthorDatabaseAttribute>();
            var capacity = authoringTypes.Count;

            map.EnsureCapacity(capacity);
            records.IncreaseCapacityTo(capacity + 1);
            records.Add(new(null, null, Constants.UNDEFINED));

            foreach (var authoringType in authoringTypes)
            {
                if (authoringType.IsAbstract
                    || authoringType.ContainsGenericParameters
                )
                {
                    continue;
                }

                var hideAttrib = authoringType.GetCustomAttribute<HideInInspector>();

                if (hideAttrib != null)
                {
                    continue;
                }

#if UNITY_6000_0_OR_NEWER
                if (authoringType.GetNestedType("SheetContainer", BindingFlags.Public) == null)
                {
                    continue;
                }
#endif

                var authorAttribute = authoringType.GetCustomAttribute<AuthorDatabaseAttribute>();

                if (authorAttribute.DatabaseType is not Type databaseType)
                {
                    continue;
                }

                if (map.TryAdd(authoringType, records.Count) == false)
                {
                    continue;
                }

                var name = ObjectNames.NicifyVariableName(databaseType.Name);
                records.Add(new(authoringType, databaseType, name));
            }
        }
    }
}
