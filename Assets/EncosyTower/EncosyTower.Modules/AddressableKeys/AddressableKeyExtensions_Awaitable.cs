#if UNITY_ADDRESSABLES
#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.Modules.AddressableKeys
{
    public static partial class AddressableKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> LoadAsync<T>(this AddressableKey key, CancellationToken token = default)
            => ((AddressableKey<T>)key).LoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<Option<T>> TryLoadAsync<T>(this AddressableKey key, CancellationToken token = default)
            => ((AddressableKey<T>)key).TryLoadAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable<GameObject> InstantiateAsync(
              this AddressableKey<GameObject> key
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
        public static async Awaitable<TComponent> InstantiateAsync<TComponent>(
              this AddressableKey<GameObject> key
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
        public static async Awaitable<Option<GameObject>> TryInstantiateAsync(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
        {
            return await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable<Option<TComponent>> TryInstantiateAsync<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
            , CancellationToken token = default
        )
            where TComponent : Component
        {
            var result = await InstantiateAsyncInternal(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.HasValue == false)
            {
                return default;
            }

            return result.Value().TryGetComponent<TComponent>(out var comp) ? comp : default;
        }

        private static async Awaitable<Option<GameObject>> InstantiateAsyncInternal(
              AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false) return default;

            var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);

            if (handle.IsValid() == false)
            {
                return default;
            }

            while (handle.IsDone == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (token.IsCancellationRequested)
            {
                return default;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return default;
            }

            var go = handle.Result;

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
#endif
