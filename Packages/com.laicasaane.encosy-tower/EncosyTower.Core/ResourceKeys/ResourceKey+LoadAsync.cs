#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using Error = ResourceKeyError;

    partial struct ResourceKey<T> : ILoadAsync<T>, ITryLoadAsync<T>, ILoadOrErrorAsync<T, Error>
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
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync(CancellationToken token = default)
        {
            var result = await LoadOrErrorAsync(token);
            return result.Value;
        }

        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<T, ResourceKeyError>>
#else
            UnityEngine.Awaitable<Result<T, ResourceKeyError>>
#endif
             LoadOrErrorAsync(CancellationToken token = default)
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((ResourceKey)this);
            }

            try
            {
                var request = Resources.LoadAsync<T>(Value.Value);

                if (request == null)
                {
                    return Error.InvalidRequest((ResourceKey)this);
                }

                while (request.isDone == false)
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
                    return Error.CancelledRequest((ResourceKey)this);
                }

                var obj = request.asset;

                if (obj.IsInvalid())
                {
                    return Error.InvalidObject((ResourceKey)this);
                }

                if (obj.AssumeValid() is T asset)
                {
                    return asset;
                }

                return Error.InvalidObjectOfType((ResourceKey)this, typeof(T), obj.GetType());
            }
            catch (Exception ex)
            {
                return Error.Exception((ResourceKey)this, ex);
            }
        }
    }
}

#endif
