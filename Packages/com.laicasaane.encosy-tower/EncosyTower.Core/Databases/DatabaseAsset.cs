using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.StringIds;
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

        private readonly Dictionary<StringId, DataTableAssetBase> _idToAsset = new();
        private readonly Dictionary<Type, DataTableAssetBase> _typeToAsset = new();
        private readonly Dictionary<Type, List<StringId>> _typeToIds = new();

        protected IReadOnlyDictionary<StringId, DataTableAssetBase> IdToAsset => _idToAsset;

        protected IReadOnlyDictionary<Type, DataTableAssetBase> TypeToAsset => _typeToAsset;

        protected ReadOnlyMemory<DataTableAssetBase> Tables => _tables;

        protected ReadOnlyMemory<DataTableAssetBase> RedundantTables => _redundantTabless;

        public bool IsInitialized { get; protected set; }

        public virtual void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            var tables = Tables.Span;
            var assetsLength = tables.Length;
            var idToAsset = _idToAsset;
            var typeToAsset = _typeToAsset;
            var typeToIds = _typeToIds;

            idToAsset.Clear();
            idToAsset.EnsureCapacity(assetsLength);

            typeToAsset.Clear();
            typeToAsset.EnsureCapacity(assetsLength);

            typeToIds.Clear();
            typeToIds.EnsureCapacity(assetsLength);

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
                var id = StringToId.MakeFromManaged(name);

                idToAsset[id] = table;

                if (typeToAsset.TryGetValue(type, out var otherAsset))
                {
                    LogWarningAmbiguousTypeAtInitialization(i, type, name, otherAsset.name, this);
                }
                else
                {
                    typeToAsset[type] = table;
                }

                if (typeToIds.TryGetValue(type, out var ids) == false)
                {
                    typeToIds[type] = ids = new(1);
                }

                ids.Add(id);
                table.Initialize();
            }

            IsInitialized = true;
        }

        public virtual void Deinitialize()
        {
            if (IsInitialized == false)
            {
                return;
            }

            IsInitialized = false;

            foreach (var asset in _idToAsset.Values)
            {
                asset.Deinitialize();
            }

            _idToAsset.Clear();
            _typeToAsset.Clear();
            _typeToIds.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] string name)
            => TryGetDataTableAsset(name, out var asset) ? asset : Option.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset([NotNull] string name, out DataTableAssetBase tableAsset)
            => TryGetDataTableAsset(StringToId.MakeFromManaged(name), out tableAsset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset(StringId id)
            => TryGetDataTableAsset(id, out var asset) ? asset : Option.None;

        public bool TryGetDataTableAsset(StringId id, out DataTableAssetBase tableAsset)
        {
            ThrowsDatabaseIsNotInitialized(IsInitialized);

            if (_idToAsset.TryGetValue(id, out var asset))
            {
                tableAsset = asset;
                return true;
            }
            else
            {
                LogErrorCannotFindAsset(id, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] Type type)
            => TryGetDataTableAsset(type, out var asset) ? asset : Option.None;

        public bool TryGetDataTableAsset([NotNull] Type type, out DataTableAssetBase tableAsset)
        {
            ThrowsDatabaseIsNotInitialized(IsInitialized);
            LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToIds, this);

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
            => TryGetDataTableAsset<T>(out var asset) ? asset : Option.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset<T>(out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowsDatabaseIsNotInitialized(IsInitialized);

            var type = typeof(T);

            LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToIds, this);

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
        public Option<T> GetDataTableAsset<T>([NotNull] string name)
            where T : DataTableAssetBase
            => TryGetDataTableAsset<T>(name, out var asset) ? asset : Option.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset<T>([NotNull] string name, out T tableAsset)
            where T : DataTableAssetBase
            => TryGetDataTableAsset<T>(StringToId.MakeFromManaged(name), out tableAsset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>(StringId id)
            where T : DataTableAssetBase
            => TryGetDataTableAsset<T>(id, out var asset) ? asset : Option.None;

        public bool TryGetDataTableAsset<T>(StringId id, out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowsDatabaseIsNotInitialized(IsInitialized);

            if (_idToAsset.TryGetValue(id, out var asset))
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
                LogErrorCannotFindAsset(id, this);
            }

            tableAsset = null;
            return false;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorAssetIsInvalid(int index, DatabaseAsset context)
        {
            StaticDevLogger.LogError(context, $"Table asset at index {index} is invalid.");
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
            StaticDevLogger.LogWarning(
                  context
                , $"DO NOT use the type '{type}' to get the asset named '{name}' (index {index}) " +
                  $"because that type has already been registered to the asset named '{otherName}'!\n" +
                  $"Please use the name '{name}' to get the correct asset!"
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowsDatabaseIsNotInitialized(bool initialized)
        {
            if (initialized)
            {
                return;
            }

            throw new InvalidOperationException(
                $"The database is not yet initialized. " +
                $"Please call '{nameof(Initialize)}' method once before using."
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorCannotFindAsset(StringId id, DatabaseAsset context)
        {
            var name = IdToString.GetManaged(id);
            var info = string.IsNullOrEmpty(name)
                ? $"id '{id}'"
                : $"name '{name}'";

            StaticDevLogger.LogError(context, $"Cannot find any table asset by {info}.");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorCannotFindAsset(Type type, DatabaseAsset context)
        {
            StaticDevLogger.LogError(context, $"Cannot find any table asset by the type '{type}'.");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorFoundAssetIsNotValidType<T>(DataTableAssetBase context)
        {
            StaticDevLogger.LogError(context, $"The table asset is not an instance of type '{typeof(T)}'");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogWarningAmbiguousTypeAtGetDataTableAsset(
              Type type
            , Dictionary<Type, List<StringId>> typeToIds
            , DatabaseAsset context
        )
        {
            if (typeToIds.TryGetValue(type, out var ids) == false || ids.Count < 2)
            {
                return;
            }

            var name = IdToString.GetManaged(ids[0]);
            var info =  string.IsNullOrEmpty(name)
                ? $"by id '{ids[0]}'"
                : $"named '{name}'";

            StaticDevLogger.LogWarning(
                  context
                , $"It is unreliable to get a table asset by the type '{type}' " +
                  $"because it is the type of multiple assets of different names.\n" +
                  $"The method overload always returns the asset {info} " +
                  $"because it is the first that was registered.\n" +
                  $"Please use the overload that takes asset names into account."
            );
        }
    }
}
