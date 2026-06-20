using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using UnityObject = UnityEngine.Object;
    using Error = ResourceKeyError;

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
        public static Result<T, Error> LoadOrError<T>(this ResourceKey key) where T : UnityObject
            => ((ResourceKey<T>)key).LoadOrError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((ResourceKey<GameObject>)key).Instantiate(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<GameObject> TryInstantiate(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((ResourceKey<GameObject>)key).TryInstantiate(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<GameObject, Error> InstantiateOrError(
              this ResourceKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((ResourceKey<GameObject>)key).InstantiateOrError(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = InstantiateOrError(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value.GetValueOrDefault();
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
            var result = InstantiateOrError<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<GameObject> TryInstantiate(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = InstantiateOrError(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value;
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
            var result = InstantiateOrError<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<GameObject, Error> InstantiateOrError(
              this ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            var result = InstantiateOrErrorInternal(key, parent, inWorldSpace, trimCloneSuffix);

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

        public static Result<TComponent, Error> InstantiateOrError<TComponent>(
              this ResourceKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = InstantiateOrErrorInternal(key, parent, inWorldSpace, trimCloneSuffix);

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

        private static Result<InstancedAndPrefab, Error> InstantiateOrErrorInternal(
              ResourceKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((ResourceKey)key);
            }

            var loadResult = key.LoadOrError();

            if (loadResult.TryGetError(out var loadError))
            {
                return loadError;
            }

            if (loadResult.TryGetValue(out var prefab) == false)
            {
                return Error.InvalidObject((ResourceKey)key);
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
