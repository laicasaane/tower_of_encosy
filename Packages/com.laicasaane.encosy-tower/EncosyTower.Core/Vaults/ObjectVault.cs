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
            where T : class
        {
            ThrowIfObjectNull(IsNotNull(obj), typeof(T));

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
            where T : class
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
            where T : class
        {
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
                    return Option.Some(objT);
                }

                goto FAILED;
            }

            if (unityObj && obj is T unityObjT)
            {
                return Option.Some(unityObjT);
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
                    return Option.Some(unityObj);
                }

                ErrorIfRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            if (obj == null)
            {
                ErrorIfRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            return Option.Some(obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsNotNull(object obj)
            => obj is UnityObject unityObj ? unityObj.IsValid() : obj != null;

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfObjectNull([DoesNotReturnIf(false)] bool isNotNull, Type type)
        {
            if (isNotNull == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentNullException CreateException(Type type)
                => typeof(UnityObject).IsAssignableFrom(type)
                    ? new("obj", new MissingReferenceException($"Unity object of type {type} is missing or destroyed."))
                    : new("obj", $"Object of type {type} is null.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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
