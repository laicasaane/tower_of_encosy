#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    using Error = AddressableKeyError;

    public static partial class AddressableKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<T>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).LoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).TryLoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<T, Error>>
#else
            UnityEngine.Awaitable<Result<T, Error>>
#endif
            LoadOrErrorAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).LoadOrErrorAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
    Cysharp.Threading.Tasks.UniTask<ValueHandlePair<T>>
#else
            UnityEngine.Awaitable<ValueHandlePair<T>>
#endif
            LoadGetHandleAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).LoadGetHandleAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<T>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<T>>>
#endif
            TryLoadGetHandleAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).TryLoadGetHandleAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<T>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<T>, Error>>
#endif
            LoadGetHandleOrErrorAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).LoadGetHandleOrErrorAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<GameObject>
#else
            UnityEngine.Awaitable<GameObject>
#endif
            InstantiateAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<GameObject>>
#else
            UnityEngine.Awaitable<Option<GameObject>>
#endif
            TryInstantiateAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<GameObject, Error>>
#else
            UnityEngine.Awaitable<Result<GameObject, Error>>
#endif
            InstantiateOrErrorAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateOrErrorAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
    Cysharp.Threading.Tasks.UniTask<ValueHandlePair<GameObject>>
#else
            UnityEngine.Awaitable<ValueHandlePair<GameObject>>
#endif
            InstantiateGetHandleAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<GameObject>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<GameObject>>>
#endif
            TryInstantiateGetHandleAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateGetHandleAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<GameObject>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<GameObject>, Error>>
#endif
            InstantiateGetHandleOrErrorAsync(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleOrErrorAsync(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TComponent>
#else
            UnityEngine.Awaitable<TComponent>
#endif
            InstantiateAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TComponent>>
#else
            UnityEngine.Awaitable<Option<TComponent>>
#endif
            TryInstantiateAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<TComponent, Error>>
#else
            UnityEngine.Awaitable<Result<TComponent, Error>>
#endif
            InstantiateOrErrorAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateOrErrorAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
    Cysharp.Threading.Tasks.UniTask<ValueHandlePair<TComponent, GameObject>>
#else
            UnityEngine.Awaitable<ValueHandlePair<TComponent, GameObject>>
#endif
            InstantiateGetHandleAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<TComponent, GameObject>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<TComponent, GameObject>>>
#endif
            TryInstantiateGetHandleAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateGetHandleAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<TComponent, GameObject>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<TComponent, GameObject>, Error>>
#endif
            InstantiateGetHandleOrErrorAsync<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleOrErrorAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<GameObject>
#else
            UnityEngine.Awaitable<GameObject>
#endif
            InstantiateAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await TryInstantiateGetHandleAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.GetValueOrDefault().Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TComponent>
#else
            UnityEngine.Awaitable<TComponent>
#endif
            InstantiateAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await TryInstantiateGetHandleAsync<TComponent>(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            return result.GetValueOrDefault().Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<ValueHandlePair<GameObject>>
#else
            UnityEngine.Awaitable<ValueHandlePair<GameObject>>
#endif
            InstantiateGetHandleAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await TryInstantiateGetHandleAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<ValueHandlePair<TComponent, GameObject>>
#else
            UnityEngine.Awaitable<ValueHandlePair<TComponent, GameObject>>
#endif
            InstantiateGetHandleAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await TryInstantiateGetHandleAsync<TComponent>(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<GameObject>>
#else
            UnityEngine.Awaitable<Option<GameObject>>
#endif
            TryInstantiateAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await TryInstantiateGetHandleAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TComponent>>
#else
            UnityEngine.Awaitable<Option<TComponent>>
#endif
            TryInstantiateAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await TryInstantiateGetHandleAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<GameObject, Error>>
#else
            UnityEngine.Awaitable<Result<GameObject, Error>>
#endif
            InstantiateOrErrorAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateGetHandleOrErrorAsync(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AddressableKey)key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<TComponent, Error>>
#else
            UnityEngine.Awaitable<Result<TComponent, Error>>
#endif
            InstantiateOrErrorAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateGetHandleOrErrorAsync<TComponent>(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AddressableKey)key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<GameObject>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<GameObject>>>
#endif
            TryInstantiateGetHandleAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token = default
        )
        {
            var result = await InstantiateGetHandleOrErrorAsync(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<TComponent, GameObject>>>
#else
            UnityEngine.Awaitable<Option<ValueHandlePair<TComponent, GameObject>>>
#endif
            TryInstantiateGetHandleAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateGetHandleOrErrorAsync<TComponent>(
                  key
                , parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );

            return result.Value;
        }

        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<GameObject>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<GameObject>, Error>>
#endif
            InstantiateGetHandleOrErrorAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token = default
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)key);
            }

            try
            {
                var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);

                if (handle.IsValid() == false)
                {
                    handle.TryRelease();
                    return Error.InvalidHandle((AddressableKey)key);
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
                    return Error.CancelledRequest((AddressableKey)key);
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.TryRelease();
                    return Error.FailedStatus((AddressableKey)key, handle.Status);
                }

                var go = handle.Result;

                if (go.IsInvalid())
                {
                    handle.TryRelease();
                    return Error.InvalidObject((AddressableKey)key);
                }

                if (parent is { IsValid: true, IsScene: true })
                {
                    go.MoveToScene(parent.Scene);
                }

                if (token.IsCancellationRequested)
                {
                    handle.TryRelease();
                    return Error.CancelledRequest((AddressableKey)key);
                }

                if (trimCloneSuffix)
                {
                    go.TrimCloneSuffix();
                }

                return new ValueHandlePair<GameObject>(go, handle);
            }
            catch (Exception ex)
            {
                return Error.Exception((AddressableKey)key, ex);
            }
        }

        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<TComponent, GameObject>, Error>>
#else
            UnityEngine.Awaitable<Result<ValueHandlePair<TComponent, GameObject>, Error>>
#endif
            InstantiateGetHandleOrErrorAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateGetHandleOrErrorAsync(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.TryGetError(out var error))
            {
                return error;
            }

            if (result.TryGetValue(out var value) == false)
            {
                return Error.Undefined((AddressableKey)key);
            }

            var (go, handle) = value;

            if (token.IsCancellationRequested)
            {
                handle.TryRelease();
                return Error.CancelledRequest((AddressableKey)key);
            }

            if (go.TryGetComponent<TComponent>(out var comp))
            {
                return new ValueHandlePair<TComponent, GameObject>(comp, handle);
            }

            handle.TryRelease();
            return Error.MissingComponent((AddressableKey)key, typeof(TComponent));
        }
    }
}

#endif
#endif
