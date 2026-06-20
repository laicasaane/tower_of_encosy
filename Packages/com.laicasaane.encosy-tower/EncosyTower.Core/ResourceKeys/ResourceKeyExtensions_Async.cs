#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using Error = ResourceKeyError;

#if UNITASK
    using UnityTaskGameObject = Cysharp.Threading.Tasks.UniTask<GameObject>;
    using UnityTaskGameObjectOpt = Cysharp.Threading.Tasks.UniTask<Option<GameObject>>;
    using UnityTaskGameObjectResult = Cysharp.Threading.Tasks.UniTask<Result<GameObject, ResourceKeyError>>;
    using UnityTaskInstancedAndPrefabResult = Cysharp.Threading.Tasks.UniTask<Result<InstancedAndPrefab, ResourceKeyError>>;
#else
    using UnityTaskGameObject = UnityEngine.Awaitable<GameObject>;
    using UnityTaskGameObjectOpt = UnityEngine.Awaitable<Option<GameObject>>;
    using UnityTaskGameObjectResult = UnityEngine.Awaitable<Result<GameObject, ResourceKeyError>>;
    using UnityTaskInstancedAndPrefabResult = UnityEngine.Awaitable<Result<InstancedAndPrefab, ResourceKeyError>>;
#endif

    using UnityObject = UnityEngine.Object;

    public static partial class ResourceKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<T>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadAsync<T>(this ResourceKey key, CancellationToken token = default) where T : UnityObject
                => ((ResourceKey<T>)key).LoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync<T>(this ResourceKey key, CancellationToken token = default) where T : UnityObject
                => ((ResourceKey<T>)key).TryLoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<T, ResourceKeyError>>
#else
            UnityEngine.Awaitable<Result<T, ResourceKeyError>>
#endif
            LoadOrErrorAsync<T>(this ResourceKey key, CancellationToken token = default) where T : UnityObject
                => ((ResourceKey<T>)key).LoadOrErrorAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<GameObject>
#else
            UnityEngine.Awaitable<GameObject>
#endif
            InstantiateAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).InstantiateAsync(parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<GameObject>>
#else
            UnityEngine.Awaitable<Option<GameObject>>
#endif
            TryInstantiateAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).TryInstantiateAsync(parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<GameObject, ResourceKeyError>>
#else
            UnityEngine.Awaitable<Result<GameObject, ResourceKeyError>>
#endif
            InstantiateOrErrorAsync(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return ((ResourceKey<GameObject>)key).InstantiateOrErrorAsync(
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
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).InstantiateAsync<TComponent>(
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
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).TryInstantiateAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<TComponent, ResourceKeyError>>
#else
            UnityEngine.Awaitable<Result<TComponent, ResourceKeyError>>
#endif
            InstantiateOrErrorAsync<TComponent>(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            return ((ResourceKey<GameObject>)key).InstantiateOrErrorAsync<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTaskGameObject InstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TComponent>
#else
            UnityEngine.Awaitable<TComponent>
#endif
            InstantiateAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTaskGameObjectOpt TryInstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsync(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TComponent>>
#else
            UnityEngine.Awaitable<Option<TComponent>>
#endif
            TryInstantiateAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTaskGameObjectResult InstantiateOrErrorAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateOrErrorAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.TryGetValue(out var value))
            {
                return value.Instanced;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((ResourceKey)key);
        }

        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Result<TComponent, Error>>
#else
            UnityEngine.Awaitable<Result<TComponent, Error>>
#endif
            InstantiateOrErrorAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateOrErrorAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.TryGetError(out var error))
            {
                return error;
            }

            if (result.TryGetValue(out var value) == false)
            {
                return Error.InvalidObject((ResourceKey)key);
            }

            if (value.Instanced.TryGetComponent<TComponent>(out var comp))
            {
                return comp;
            }

            UnityObject.Destroy(value.Instanced);
            return Error.MissingComponent((ResourceKey)key, value.Prefab, typeof(TComponent));
        }

        private static async UnityTaskInstancedAndPrefabResult InstantiateOrErrorAsyncInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((ResourceKey)key);
            }

            var loadResult = await key.LoadOrErrorAsync(token);

            if (loadResult.TryGetError(out var loadError))
            {
                return loadError;
            }

            if (loadResult.TryGetValue(out var prefab) == false)
            {
                return Error.InvalidObject((ResourceKey)key);
            }

            if (token.IsCancellationRequested)
            {
                return Error.CancelledRequest((ResourceKey)key);
            }

            try
            {
                var go = UnityObject.Instantiate(prefab, parent.Transform, inWorldSpace);

                if (go.IsInvalid())
                {
                    return Error.InvalidInstantiation((ResourceKey)key, prefab);
                }

                if (parent is { IsValid: true, IsScene: true })
                {
                    go.MoveToScene(parent.Scene);
                }

                if (token.IsCancellationRequested)
                {
                    UnityObject.Destroy(go);
                    return Error.CancelledRequest((ResourceKey)key);
                }

                if (trimCloneSuffix)
                {
                    go.TrimCloneSuffix();
                }

                return new InstancedAndPrefab(go, prefab);
            }
            catch (Exception ex)
            {
                return Error.Exception((ResourceKey)key, ex);
            }
        }
    }
}

#endif
