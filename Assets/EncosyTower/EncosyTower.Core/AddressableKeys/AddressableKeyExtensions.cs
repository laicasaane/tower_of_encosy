#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        public static GameObject Instantiate(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix)
                .GetValueOrDefault();
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
            var result = InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix);
            return result.HasValue ? result.GetValueOrThrow().GetComponent<TComponent>() : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<GameObject> TryInstantiate(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix);
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
            var result = InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix);

            if (result.HasValue == false)
            {
                return Option.None;
            }

            return result.GetValueOrThrow().TryGetComponent<TComponent>(out var comp)
                ? comp : Option.None;
        }

        private static Option<GameObject> InstantiateInternal(
              AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            if (key.IsValid == false) return Option.None;

            var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);
            var go = handle.WaitForCompletion();

            if (go.IsInvalid())
            {
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

            return go;

        }
    }
}

#endif
