using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Logging;
using UnityEngine;

namespace EncosyTower.Modules.Vaults
{
    using UnityObject = UnityEngine.Object;

    public sealed partial class ObjectVault : IDisposable
    {
        private readonly Dictionary<Id2, object> _map = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _map.Clear();
        }

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<T>(Id<T> id)
            => Contains(ToId2(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd<T>(Id<T> id, [NotNull] T obj)
            => TryAdd(ToId2(id), obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove<T>(Id<T> id, out T obj)
            => TryRemove(ToId2(id), out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(Id<T> id, out T obj)
            => TryGet<T>(ToId2(id), out obj);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(PresetId id)
            => Contains((Id2)id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd<T>(PresetId id, [NotNull] T obj)
            => TryAdd((Id2)id, obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove<T>(PresetId id, out T obj)
            => TryRemove((Id2)id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(PresetId id, out T obj)
            => TryGet<T>((Id2)id, out obj);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Id2 id)
            => _map.ContainsKey(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd<T>(Id2 id, [NotNull] T obj)
        {
            ThrowIfNotReferenceType<T>();
            ThrowIfUnityObjectIsDestroyed(obj);

            var map = _map;

            if (map.ContainsKey(id))
            {
                return false;
            }

            map[id] = obj;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove<T>(Id2 id, out T obj)
        {
            if (TryGet(id, out obj))
            {
                _map.Remove(id);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(Id2 id, out T obj)
        {
            ThrowIfNotReferenceType<T>();

            if (_map.TryGetValue(id, out var weakRef))
            {
                var result = TryCast<T>(id, weakRef);
                obj = result.ValueOrDefault();
                return result.HasValue;
            }

            obj = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Id2 ToId2<T>(Id<T> id)
            => TypeId<T>.Value.ToId2(id);

        private static Option<T> TryCast<T>(Id2 id, object obj, UnityObject context = null)
        {
            if (obj == null)
            {
                ErrorIfRegisteredObjectIsNull(id, context);
                return default;
            }

            if (obj is not UnityObject unityObj)
            {
                if (obj is T objT)
                {
                    return new(objT);
                }

                goto FAILED;
            }

            if (unityObj && obj is T unityObjT)
            {
                return new(unityObjT);
            }

            if (unityObj == false)
            {
                ErrorIfRegisteredObjectIsNull(id, context);
            }

        FAILED:
            ErrorIfTypeMismatch<T>(id, obj, context);
            return default;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfNotReferenceType<T>()
        {
            if (TypeCache<T>.IsUnmanaged || TypeCache<T>.IsValueType)
            {
                throw new InvalidCastException(
                    $"\"{TypeCache<T>.Type}\" is not a reference type"
                );
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfUnityObjectIsDestroyed<T>(T obj)
        {
            if (obj is UnityObject unityObj && unityObj == false)
            {
                throw new MissingReferenceException($"Unity Object of type {typeof(T)} is either missing or destroyed.");
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeMismatch<T>(Id2 id, object obj, UnityObject context)
        {
            var message = "Id \"{0}\" is mapped to an object of type \"{1}\". " +
                  "However an object of type \"{2}\" is being requested from it. " +
                  "It might be a bug at the time of registering.";

            if (context)
            {
                DevLoggerAPI.LogErrorFormat(context, message, id, obj?.GetType(), typeof(T));
            }
            else
            {
                DevLoggerAPI.LogErrorFormat(message, id, obj?.GetType(), typeof(T));
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfRegisteredObjectIsNull(Id2 id, UnityObject context)
        {
            var message = "The object registered with id \"{0}\" is null.";

            if (context)
            {
                DevLoggerAPI.LogErrorFormat(context, message, id);
            }
            else
            {
                DevLoggerAPI.LogErrorFormat(message, id);
            }
        }
    }
}
