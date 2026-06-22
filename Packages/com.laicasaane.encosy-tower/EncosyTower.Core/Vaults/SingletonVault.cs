#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using EncosyTower.Logging;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Vaults
{
    public class SingletonVault<TBase> : IDisposable
        where TBase : class
    {
        private readonly ConcurrentDictionary<TypeHash, TBase> _singletons = new();

        public bool Contains<T>()
            where T : class, TBase
            => _singletons.ContainsKey(Type<T>.Hash);

        public bool Contains<T>(T instance)
            where T : class, TBase
            => _singletons.TryGetValue(Type<T>.Hash, out var obj)
               && ReferenceEquals(obj, instance);

        public bool TryAdd<T>()
            where T : class, TBase, new()
        {
            if (_singletons.ContainsKey(Type<T>.Hash))
            {
                LogError_InstanceAlreadyExists<T>();
                return false;
            }

            return _singletons.TryAdd(Type<T>.Hash, new T());
        }

        public bool TryAdd<T>(T instance)
            where T : class, TBase
        {
            if (instance == null)
            {
#if __ENCOSY_VALIDATION__
                throw CreateArgumentNullException_Instance();
#else
                return false;
#endif
            }

            if (_singletons.ContainsKey(Type<T>.Hash))
            {
                LogError_InstanceAlreadyExists<T>();
                return false;
            }

            return _singletons.TryAdd(Type<T>.Hash, instance);
        }

        public bool TryGetOrAdd<T>(out T instance)
            where T : class, TBase, new()
        {
            if (_singletons.TryGetValue(Type<T>.Hash, out var obj))
            {
                if (obj is T inst)
                {
                    instance = inst;
                    return true;
                }
                else
                {
                    ThrowCannotCastEvenRegistered<T>(obj);
                }
            }

            _singletons[Type<T>.Hash] = instance = new();
            return true;
        }

        public bool TryGet<T>(out T instance)
            where T : class, TBase
        {
            if (_singletons.TryGetValue(Type<T>.Hash, out var obj))
            {
                if (obj is T inst)
                {
                    instance = inst;
                    return true;
                }
                else
                {
                    ThrowCannotCast<T>(obj);
                }
            }

            instance = default;
            return false;
        }

        public void Dispose()
        {
            var singletons = _singletons;

            foreach (var (_, obj) in singletons)
            {
                if (obj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            singletons.Clear();
        }

        private static Exception CreateArgumentNullException_Instance()
            => new ArgumentNullException("instance");

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void LogError_InstanceAlreadyExists<T>()
            => StaticDevLogger.LogError($"An instance of {typeof(T)} has already been existing");

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowCannotCastEvenRegistered<T>(TBase obj)
        {
            throw new InvalidCastException(
                $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}" +
                $"even though it is registered for {typeof(T)}"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowCannotCast<T>(TBase obj)
        {
            throw new InvalidCastException(
                $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}"
            );
        }
    }
}
