using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using UnityObject = UnityEngine.Object;

    public static partial class ResourceKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResourceKey AsResource(this AssetKey key)
            => new(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResourceKey<T> AsResource<T>(this AssetKey<T> key) where T : UnityObject
            => new(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load<T>(this ResourceKey key) where T : UnityObject
            => ((ResourceKey<T>)key).Load();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> TryLoad<T>(this ResourceKey key) where T : UnityObject
            => ((ResourceKey<T>)key).TryLoad();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this ResourceKey<GameObject> key
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
              this ResourceKey<GameObject> key
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
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TComponent> TryInstantiate<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = InstantiateInternal(key, parent, inWorldSpace, trimCloneSuffix);

            if (result.HasValue)
            {
                if (result.GetValueOrThrow().TryGetComponent<TComponent>(out var comp))
                {
                    return comp;
                }
            }

            return default;
        }
        private static Option<GameObject> InstantiateInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            if (key.IsValid == false) return default;

            var goOpt = key.TryLoad();

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
