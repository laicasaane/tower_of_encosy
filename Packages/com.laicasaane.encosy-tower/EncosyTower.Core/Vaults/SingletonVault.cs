#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using EncosyTower.Logging;
using EncosyTower.Types;

namespace EncosyTower.Vaults
{
    public class SingletonVault<TBase> : IDisposable
        where TBase : class
    {
        private readonly Dictionary<TypeHash, TBase> _singletons = new();

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
#if __ENCOSY_VALIDATION__
                DevLoggerAPI.LogError($"An instance of {Type<T>.Value.Name} has already been existing");
#endif

                return false;
            }

            _singletons.Add(Type<T>.Hash, new T());
            return true;
        }

        public bool TryAdd<T>(T instance)
            where T : TBase
        {
            if (instance == null)
            {
#if __ENCOSY_VALIDATION__
                throw new ArgumentNullException(nameof(instance));
#else
                return false;
#endif
            }

            if (_singletons.ContainsKey(Type<T>.Hash))
            {
#if __ENCOSY_VALIDATION__
                DevLoggerAPI.LogError($"An instance of {Type<T>.Value} has already been existing");
#endif

                return false;
            }

            _singletons.Add(Type<T>.Hash, instance);
            return true;
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
#if __ENCOSY_VALIDATION__
                else
                {
                    throw new InvalidCastException(
                        $"Cannot cast an instance of type {obj.GetType()} to {Type<T>.Value}" +
                        $"even though it is registered for {Type<T>.Value}"
                    );
                }
#endif
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
#if __ENCOSY_VALIDATION__
                else
                {
                    throw new InvalidCastException(
                        $"Cannot cast an instance of type {obj.GetType()} to {Type<T>.Value}"
                    );
                }
#endif
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
    }
}
