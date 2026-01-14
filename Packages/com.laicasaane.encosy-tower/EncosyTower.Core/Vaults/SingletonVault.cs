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

namespace EncosyTower.Vaults
{
    public class SingletonVault<TBase> : IDisposable
    {
        private readonly ConcurrentDictionary<TypeHash, TBase> _singletons = new();

        static SingletonVault()
        {
            ThrowIfNotReferenceType();
        }

        public bool Contains<T>()
            where T : TBase
            => _singletons.ContainsKey(Type<T>.Hash);

        public bool Contains<T>(T instance)
            where T : TBase
            => _singletons.TryGetValue(Type<T>.Hash, out var obj)
               && ReferenceEquals(obj, instance);

        public bool TryAdd<T>()
            where T : TBase, new()
        {
            if (_singletons.ContainsKey(Type<T>.Hash))
            {
                LogError_InstanceAlreadyExists<T>();
                return false;
            }

            return _singletons.TryAdd(Type<T>.Hash, new T());
        }

        public bool TryAdd<T>(T instance)
            where T : TBase
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
            where T : TBase, new()
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
            where T : TBase
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

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfNotReferenceType()
        {
            if (typeof(TBase).IsValueType)
            {
                throw new InvalidOperationException(
                    $"{nameof(SingletonVault<TBase>)} does not accept type '{typeof(TBase)}' " +
                    $"because it is not a reference type."
                );
            }
        }

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowCannotCastEvenRegistered<T>(TBase obj)
        {
            throw new InvalidCastException(
                $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}" +
                $"even though it is registered for {typeof(T)}"
            );
        }

        [Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowCannotCast<T>(TBase obj)
        {
            throw new InvalidCastException(
                $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}"
            );
        }
    }
}
