using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    public sealed partial class ObjectVault<TId> : IDisposable
        where TId : unmanaged, IEquatable<TId>
    {
        private readonly ConcurrentDictionary<TId, object> _map = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _map.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TId id)
            => _map.ContainsKey(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd<T>(TId id, [NotNull] T obj)
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
        public bool TryRemove<T>(TId id, out Option<T> obj)
        {
            if (_map.TryRemove(id, out var weakRef))
            {
                obj = TryCast<T>(id, weakRef);
                return true;
            }

            obj = Option.None;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(TId id, out Option<T> obj)
        {
            ThrowIfNotReferenceType<T>();

            if (_map.TryGetValue(id, out var weakRef))
            {
                obj = TryCast<T>(id, weakRef);
                return true;
            }

            obj = Option.None;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(TId id, out Option<object> obj)
        {
            if (_map.TryGetValue(id, out var weakRef))
            {
                obj = TryCast(id, weakRef);
                return true;
            }

            obj = default;
            return false;
        }

        private static Option<T> TryCast<T>(TId id, object obj, UnityObject context = null)
        {
            if (obj == null)
            {
                ErrorIfRegisteredObjectIsNull(id, context);
                return Option.None;
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
            return Option.None;
        }

        private static Option<object> TryCast(TId id, object obj, UnityObject context = null)
        {
            if (obj is UnityObject unityObj)
            {
                if (unityObj.IsValid())
                {
                    return new(unityObj);
                }

                ErrorIfRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            if (obj == null)
            {
                ErrorIfRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            return new(obj);
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfNotReferenceType<T>()
        {
            var typeOfT = typeof(T);
            var isValueType = typeOfT.IsValueType;
            var isUnmanaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;

            if (isUnmanaged || isValueType)
            {
                throw new InvalidCastException(
                    $"\"{typeOfT}\" is not a reference type"
                );
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfUnityObjectIsDestroyed<T>(T obj)
        {
            if (obj is UnityObject unityObj && unityObj == false)
            {
                throw new MissingReferenceException($"Unity Object of type {typeof(T)} is either missing or destroyed.");
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeMismatch<T>(TId id, object obj, UnityObject context)
        {
            var message = "Id \"{0}\" is mapped to an object of type \"{1}\". " +
                  "However an object of type \"{2}\" is being requested from it. " +
                  "It might be a bug at the time of registering.";

            if (context)
            {
                StaticDevLogger.LogErrorFormat(context, message, id, obj?.GetType(), typeof(T));
            }
            else
            {
                StaticDevLogger.LogErrorFormat(message, id, obj?.GetType(), typeof(T));
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfRegisteredObjectIsNull(TId id, UnityObject context)
        {
            var message = "The object registered with id \"{0}\" is null.";

            if (context)
            {
                StaticDevLogger.LogErrorFormat(context, message, id);
            }
            else
            {
                StaticDevLogger.LogErrorFormat(message, id);
            }
        }
    }
}
