#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using UnityObject = UnityEngine.Object;

    public static partial class ResourceKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<T> LoadAsync<T>(this ResourceKey key, CancellationToken token = default) where T : UnityObject
            => ((ResourceKey<T>)key).LoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<T>> TryLoadAsync<T>(this ResourceKey key, CancellationToken token = default) where T : UnityObject
            => ((ResourceKey<T>)key).TryLoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<GameObject> InstantiateAsync(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.ValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<TComponent> InstantiateAsync<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.HasValue ? result.Value().GetComponent<TComponent>() : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<Option<GameObject>> TryInstantiateAsync(
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
        public static async UniTask<Option<TComponent>> TryInstantiateAsync<TComponent>(
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
                if (result.Value().TryGetComponent<TComponent>(out var comp))
                {
                    return comp;
                }
            }

            return default;
        }

        private static async UniTask<Option<GameObject>> InstantiateAsyncInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false) return default;

            var goOpt = await key.TryLoadAsync(token);

            if (goOpt.HasValue == false || goOpt.TryValue(out var prefab) == false)
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
