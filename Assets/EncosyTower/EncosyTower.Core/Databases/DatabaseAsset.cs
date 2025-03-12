using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Databases
{
    public class DatabaseAsset : ScriptableObject, IInitializable, IDeinitializable
    {
        [SerializeField]
        internal DataTableAssetBase[] _tables = new DataTableAssetBase[0];

        [SerializeField]
        internal DataTableAssetBase[] _redundantTabless = new DataTableAssetBase[0];

        private readonly Dictionary<string, DataTableAssetBase> _nameToAsset = new();
        private readonly Dictionary<Type, DataTableAssetBase> _typeToAsset = new();
        private readonly Dictionary<Type, List<string>> _typeToNames = new();

        protected IReadOnlyDictionary<string, DataTableAssetBase> NameToAsset => _nameToAsset;

        protected IReadOnlyDictionary<Type, DataTableAssetBase> TypeToAsset => _typeToAsset;

        protected ReadOnlyMemory<DataTableAssetBase> Tables => _tables;

        protected ReadOnlyMemory<DataTableAssetBase> RedundantTables => _redundantTabless;

        public bool Initialized { get; protected set; }

        public virtual void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            var tables = Tables.Span;
            var assetsLength = tables.Length;
            var nameToAsset = _nameToAsset;
            var typeToAsset = _typeToAsset;
            var typeToNames = _typeToNames;

            nameToAsset.Clear();
            nameToAsset.EnsureCapacity(assetsLength);

            typeToAsset.Clear();
            typeToAsset.EnsureCapacity(assetsLength);

            typeToNames.Clear();
            typeToNames.EnsureCapacity(assetsLength);

            for (var i = 0; i < assetsLength; i++)
            {
                var table = tables[i];

                if (table.IsInvalid())
                {
                    LogErrorAssetIsInvalid(i, this);
                    continue;
                }

                var type = table.GetType();
                var name = table.name;

                nameToAsset[name] = table;

                if (typeToAsset.TryGetValue(type, out var otherAsset))
                {
                    LogWarningAmbiguousTypeAtInitialization(i, type, name, otherAsset.name, this);
                }
                else
                {
                    typeToAsset[type] = table;
                }

                if (typeToNames.TryGetValue(type, out var names) == false)
                {
                    typeToNames[type] = names = new(1);
                }

                names.Add(name);
                table.Initialize();
            }

            Initialized = true;
        }

        public virtual void Deinitialize()
        {
            if (Initialized == false)
            {
                return;
            }

            Initialized = false;

            foreach (var asset in _nameToAsset.Values)
            {
                asset.Deinitialize();
            }

            _nameToAsset.Clear();
            _typeToAsset.Clear();
            _typeToNames.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] string name)
            => TryGetDataTableAsset(name, out var asset) ? asset : default;

        public bool TryGetDataTableAsset([NotNull] string name, out DataTableAssetBase tableAsset)
        {
            ThrowsDatabaseIsNotInitialized();

            if (_nameToAsset.TryGetValue(name, out var asset))
            {
                tableAsset = asset;
                return true;
            }
            else
            {
                LogErrorCannotFindAsset(name, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] Type type)
            => TryGetDataTableAsset(type, out var asset) ? asset : default;

        public bool TryGetDataTableAsset([NotNull] Type type, out DataTableAssetBase tableAsset)
        {
            ThrowsDatabaseIsNotInitialized();
            LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToNames, this);

            if (_typeToAsset.TryGetValue(type, out var asset))
            {
                tableAsset = asset;
                return true;
            }
            else
            {
                LogErrorCannotFindAsset(type, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>() where T : DataTableAssetBase
            => TryGetDataTableAsset<T>(out var asset) ? asset : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset<T>(out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowsDatabaseIsNotInitialized();

            var type = typeof(T);

            LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToNames, this);

            if (_typeToAsset.TryGetValue(type, out var asset))
            {
                if (asset is T assetT)
                {
                    tableAsset = assetT;
                    return true;
                }
                else
                {
                    LogErrorFoundAssetIsNotValidType<T>(asset);
                }
            }
            else
            {
                LogErrorCannotFindAsset(type, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>([NotNull] string name) where T : DataTableAssetBase
            => TryGetDataTableAsset<T>(name, out var asset) ? asset : default;

        public bool TryGetDataTableAsset<T>([NotNull] string name, out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowsDatabaseIsNotInitialized();

            if (_nameToAsset.TryGetValue(name, out var asset))
            {
                if (asset is T assetT)
                {
                    tableAsset = assetT;
                    return true;
                }
                else
                {
                    LogErrorFoundAssetIsNotValidType<T>(asset);
                }
            }
            else
            {
                LogErrorCannotFindAsset(name, this);
            }

            tableAsset = null;
            return false;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorAssetIsInvalid(int index, DatabaseAsset context)
        {
            DevLoggerAPI.LogError(context, $"Table asset at index {index} is invalid.");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogWarningAmbiguousTypeAtInitialization(
              int index
            , Type type
            , string name
            , string otherName
            , DatabaseAsset context
        )
        {
            DevLoggerAPI.LogWarning(
                  context
                , $"DO NOT use the type '{type}' to get the asset named '{name}' (index {index}) " +
                  $"because that type has already been registered to the asset named '{otherName}'!\n" +
                  $"Please use the name '{name}' to get the correct asset!"
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowsDatabaseIsNotInitialized()
        {
            throw new InvalidOperationException(
                $"The database is not yet initialized. " +
                $"Please call '{nameof(Initialize)}' method once before using."
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorCannotFindAsset(string name, DatabaseAsset context)
        {
            DevLoggerAPI.LogError(context, $"Cannot find any table asset by the name '{name}'.");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorCannotFindAsset(Type type, DatabaseAsset context)
        {
            DevLoggerAPI.LogError(context, $"Cannot find any table asset by the type '{type}'.");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorFoundAssetIsNotValidType<T>(DataTableAssetBase context)
        {
            DevLoggerAPI.LogError(context, $"The table asset is not an instance of type '{typeof(T)}'");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogWarningAmbiguousTypeAtGetDataTableAsset(
              Type type
            , Dictionary<Type, List<string>> typeToNames
            , DatabaseAsset context
        )
        {
            if (typeToNames.TryGetValue(type, out var names) == false || names.Count < 2)
            {
                return;
            }

            DevLoggerAPI.LogWarning(
                  context
                , $"It is unreliable to get a table asset by the type '{type}' " +
                  $"because it is the type of multiple assets of different names.\n" +
                  $"The method overload always returns the asset named '{names[0]}' " +
                  $"because it is the first that was registered.\n" +
                  $"Please use the overload that takes asset names into account."
            );
        }
    }
}
