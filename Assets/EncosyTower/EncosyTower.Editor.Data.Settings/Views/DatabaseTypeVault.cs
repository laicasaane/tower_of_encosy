using System;
using System.Reflection;
using EncosyTower.Collections;
using EncosyTower.Data.Authoring;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Data.Settings.Views
{
    internal record DatabaseType(Type Type, string Name);

    internal static class DatabaseTypeVault
    {
        public static readonly ArrayMap<Type, int> TypeToIndexMap = new();
        public static readonly FasterList<DatabaseType> Types = new();

        public static void Initialize()
        {
            var map = TypeToIndexMap;
            var types = Types;

            map.Clear();
            types.Clear();

            var candidates = TypeCache.GetTypesWithAttribute<DatabaseAttribute>();
            var capacity = candidates.Count;

            map.EnsureCapacity(capacity);
            types.IncreaseCapacityTo(capacity + 1);
            types.Add(new(null, Constants.UNDEFINED));

            foreach (var candidate in candidates)
            {
                if (candidate.IsClass == false
                    || candidate.IsAbstract
                    || candidate.ContainsGenericParameters
                )
                {
                    continue;
                }

                var hideAttrib = candidate.GetCustomAttribute<HideInInspector>();

                if (hideAttrib != null)
                {
                    continue;
                }

                if (map.TryAdd(candidate, types.Count) == false)
                {
                    continue;
                }

                var name = ObjectNames.NicifyVariableName(candidate.Name);
                types.Add(new(candidate, name));
            }
        }
    }
}
