#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using EncosyTower.Tasks;
using System;

namespace EncosyTower.AddressableKeys
{
    using Error = AddressableKeyError;

    partial struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>, ILoadOrErrorAsync<T, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<T>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<ValueHandlePair<T>>
#else
            UnityEngine.Awaitable<ValueHandlePair<T>>
#endif
            LoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<T, Error>>
#else
            UnityEngine.Awaitable<Result<T, Error>>
#endif
            LoadOrErrorAsync(CancellationToken token = default)
        {
            var result = await LoadGetHandleOrErrorAsync(token);

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
        public readonly async
#if UNITASK
    Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<T>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<T>>>
#endif
            TryLoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await LoadGetHandleOrErrorAsync(token);
            return result.Value;
        }

        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<T>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<T>, Error>>
#endif
            LoadGetHandleOrErrorAsync(CancellationToken token = default)
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)this);
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(Value.Value);

                if (handle.IsValid() == false)
                {
                    handle.TryRelease();
                    return Error.InvalidHandle((AddressableKey)this);
                }

                while (handle.IsDone == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await UnityTasks.NextFrameAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    handle.TryRelease();
                    return Error.CancelledRequest((AddressableKey)this);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.TryRelease();
                    return Error.FailedStatus((AddressableKey)this, handle.Status);
                }

                var asset = handle.Result;

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
#endif
