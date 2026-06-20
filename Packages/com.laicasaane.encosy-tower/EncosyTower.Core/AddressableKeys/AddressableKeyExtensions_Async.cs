#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

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
    Cysharp.Threading.Tasks.UniTask<(T, AsyncOperationHandle<T>)>
#else
            UnityEngine.Awaitable<(T, AsyncOperationHandle<T>)>
#endif
            LoadGetHandleAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).LoadGetHandleAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<(T, AsyncOperationHandle<T>)>>
#else
            UnityEngine.Awaitable<Option<(T, AsyncOperationHandle<T>)>>
#endif
            TryLoadGetHandleAsync<T>(this AddressableKey key, CancellationToken token = default)
                => ((AddressableKey<T>)key).TryLoadGetHandleAsync(token);

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
            return result.GetValueOrDefault().Item1;
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
            var result = await TryInstantiateGetHandleAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
            return result.GetValueOrDefault().Item1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<(GameObject, AsyncOperationHandle<GameObject>)>
#else
            UnityEngine.Awaitable<(GameObject, AsyncOperationHandle<GameObject>)>
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
            Cysharp.Threading.Tasks.UniTask<(TComponent, AsyncOperationHandle<GameObject>)>
#else
            UnityEngine.Awaitable<(TComponent, AsyncOperationHandle<GameObject>)>
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
            var result = await TryInstantiateGetHandleAsync<TComponent>(key, parent, inWorldSpace, trimCloneSuffix, token);
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
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
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
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<(GameObject, AsyncOperationHandle<GameObject>)>>
#else
            UnityEngine.Awaitable<Option<(GameObject, AsyncOperationHandle<GameObject>)>>
#endif
            TryInstantiateGetHandleAsync(
              AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
            , CancellationToken token
        )
        {
            if (key.IsValid == false)
            {
                return Option.None;
            }

            var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);

            if (handle.IsValid() == false)
            {
                handle.TryRelease();
                return Option.None;
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
                return Option.None;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                handle.TryRelease();
                return Option.None;
            }

            var go = handle.Result;

            if (go.IsInvalid())
            {
                handle.TryRelease();
                return Option.None;
            }

            if (parent is { IsValid: true, IsScene: true })
            {
                go.MoveToScene(parent.Scene);
            }

            if (trimCloneSuffix)
            {
                go.TrimCloneSuffix();
            }

            return (go, handle);

        }

        public static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<(TComponent, AsyncOperationHandle<GameObject>)>>
#else
            UnityEngine.Awaitable<Option<(TComponent, AsyncOperationHandle<GameObject>)>>
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
            var result = await TryInstantiateGetHandleAsync(key, parent, inWorldSpace, trimCloneSuffix, token);

            if (result.HasValue == false || token.IsCancellationRequested)
            {
                return Option.None;
            }

            var (go, handle) = result.GetValueOrDefault();

            if (go.TryGetComponent<TComponent>(out var comp))
            {
                return (comp, handle);
            }

            handle.TryRelease();
            return Option.None;
        }
    }
}

#endif
#endif
