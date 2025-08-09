#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
#if UNITASK
    using UnityTaskGameObject = Cysharp.Threading.Tasks.UniTask<GameObject>;
    using UnityTaskGameObjectOpt = Cysharp.Threading.Tasks.UniTask<Option<GameObject>>;
#else
    using UnityTaskGameObject = UnityEngine.Awaitable<GameObject>;
    using UnityTaskGameObjectOpt = UnityEngine.Awaitable<Option<GameObject>>;
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
        public static async UnityTaskGameObject InstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.GetValueOrDefault();
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
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.HasValue ? result.GetValueOrThrow().GetComponent<TComponent>() : default;
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
            return await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
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
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.HasValue)
            {
                if (result.GetValueOrThrow().TryGetComponent<TComponent>(out var comp))
                {
                    return comp;
                }
            }

            return default;
        }

        private static async UnityTaskGameObjectOpt InstantiateAsyncInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false) return default;

            var goOpt = await key.TryLoadAsync(token);

            if (goOpt.HasValue == false || goOpt.TryGetValue(out var prefab) == false)
            {
                return default;
            }

            var go = UnityObject.Instantiate(prefab, parent.Transform, inWorldSpace);

            if (go.IsInvalid())
            {
                return default;
            }

            if (parent is { IsValid: true, IsScene: true })
            {
                go.MoveToScene(parent.Scene);
            }

            if (trimCloneSuffix)
            {
                go.TrimCloneSuffix();
            }

            return go;
        }
    }
}

#endif
