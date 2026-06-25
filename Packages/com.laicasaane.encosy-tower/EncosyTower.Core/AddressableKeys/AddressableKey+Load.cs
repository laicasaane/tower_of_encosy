#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    using Error = AddressableKeyError;

    partial struct AddressableKey<T> : ILoad<T>, ITryLoad<T>, ILoadOrError<T, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ValueHandlePair<T> LoadGetHandle()
            => TryLoadGetHandle().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<T> TryLoad()
        {
            var result = TryLoadGetHandle();
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Result<T, Error> LoadOrError()
        {
            var result = LoadGetHandleOrError();

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AddressableKey)this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<ValueHandlePair<T>> TryLoadGetHandle()
            => LoadGetHandleOrError().Value;

        public readonly Result<ValueHandlePair<T>, Error> LoadGetHandleOrError()
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)this);
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(Value.Value);
                var asset = handle.WaitForCompletion();

                if (handle.IsValid() == false)
                {
                    handle.TryRelease();
                    return Error.InvalidHandle((AddressableKey)this);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.TryRelease();
                    return Error.FailedStatus((AddressableKey)this, handle.Status);
                }

                if ((asset is UnityEngine.Object obj && obj) || asset != null)
                {
                    return new ValueHandlePair<T>(asset, handle);
                }

                handle.TryRelease();
                return Error.InvalidObject((AddressableKey)this);
            }
            catch (Exception ex)
            {
                return Error.Exception((AddressableKey)this, ex);
            }
        }
    }
}

#endif
