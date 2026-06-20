#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    public static partial class AddressableKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddressableKey AsAddressable(this AssetKey key)
            => new(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddressableKey<T> AsAddressable<T>(this AssetKey<T> key)
            => new(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).Load();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> TryLoad<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).TryLoad();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T, AsyncOperationHandle<T>) LoadGetHandle<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).LoadGetHandle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<(T, AsyncOperationHandle<T>)> TryLoadGetHandle<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).TryLoadGetHandle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = TryInstantiateGetHandle(key, parent, inWorldSpace, trimCloneSuffix);
            return result.GetValueOrDefault().Item1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent Instantiate<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = TryInstantiateGetHandle<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return result.GetValueOrDefault().Item1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (GameObject, AsyncOperationHandle<GameObject>) InstantiateGetHandle(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = TryInstantiateGetHandle(key, parent, inWorldSpace, trimCloneSuffix);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (TComponent, AsyncOperationHandle<GameObject>) InstantiateGetHandle<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = TryInstantiateGetHandle<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<GameObject> TryInstantiate(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = TryInstantiateGetHandle(key, parent, inWorldSpace, trimCloneSuffix);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TComponent> TryInstantiate<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = TryInstantiateGetHandle<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        public static Option<(GameObject, AsyncOperationHandle<GameObject>)> TryInstantiateGetHandle(
              this AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            if (key.IsValid == false)
            {
                return Option.None;
            }

            var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);
            var go = handle.WaitForCompletion();

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

        public static Option<(TComponent, AsyncOperationHandle<GameObject>)> TryInstantiateGetHandle<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = TryInstantiateGetHandle(key, parent, inWorldSpace, trimCloneSuffix);

            if (result.HasValue == false)
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
