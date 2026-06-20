#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
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
        public static Result<T, Error> LoadOrError<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).LoadOrError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueHandlePair<T> LoadGetHandle<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).LoadGetHandle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<ValueHandlePair<T>> TryLoadGetHandle<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).TryLoadGetHandle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<ValueHandlePair<T>, Error> LoadGetHandleOrError<T>(this AddressableKey key)
            => ((AddressableKey<T>)key).LoadGetHandleOrError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).Instantiate(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<GameObject> TryInstantiate(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).TryInstantiate(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<GameObject, Error> InstantiateOrError(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateOrError(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueHandlePair<GameObject> InstantiateGetHandle(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandle(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<ValueHandlePair<GameObject>> TryInstantiateGetHandle(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateGetHandle(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<ValueHandlePair<GameObject>, Error> InstantiateGetHandleOrError(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleOrError(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent Instantiate<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).Instantiate<TComponent>(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TComponent> TryInstantiate<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).TryInstantiate<TComponent>(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TComponent, Error> InstantiateOrError<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateOrError<TComponent>(parent, inWorldSpace, trimCloneSuffix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueHandlePair<TComponent, GameObject> InstantiateGetHandle<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandle<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<ValueHandlePair<TComponent, GameObject>> TryInstantiateGetHandle<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).TryInstantiateGetHandle<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<ValueHandlePair<TComponent, GameObject>, Error> InstantiateGetHandleOrError<TComponent>(
              this AddressableKey key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            return ((AddressableKey<GameObject>)key).InstantiateGetHandleOrError<TComponent>(
                  parent
                , inWorldSpace
                , trimCloneSuffix
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Instantiate(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = TryInstantiateGetHandle(key, parent, inWorldSpace, trimCloneSuffix);
            return result.GetValueOrDefault().Value;
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
            return result.GetValueOrDefault().Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueHandlePair<GameObject> InstantiateGetHandle(
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
        public static ValueHandlePair<TComponent, GameObject> InstantiateGetHandle<TComponent>(
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
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
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
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<GameObject, Error> InstantiateOrError(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
        {
            var result = InstantiateGetHandleOrError(key, parent, inWorldSpace, trimCloneSuffix);

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
        public static Result<TComponent, Error> InstantiateOrError<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = InstantiateGetHandleOrError<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);

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
        public static Option<ValueHandlePair<GameObject>> TryInstantiateGetHandle(
              this AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            var result = InstantiateGetHandleOrError(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<ValueHandlePair<TComponent, GameObject>> TryInstantiateGetHandle<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = InstantiateGetHandleOrError<TComponent>(key, parent, inWorldSpace, trimCloneSuffix);
            return result.Value;
        }

        public static Result<ValueHandlePair<GameObject>, Error> InstantiateGetHandleOrError(
              this AddressableKey<GameObject> key
            , TransformOrScene parent
            , bool inWorldSpace
            , bool trimCloneSuffix
        )
        {
            if (key.IsValid == false)
            {
                return Error.InvalidKey((AddressableKey)key);
            }

            try
            {
                var handle = Addressables.InstantiateAsync(key.Value.Value, parent.Transform, inWorldSpace);
                var go = handle.WaitForCompletion();

                if (go.IsInvalid())
                {
                    handle.TryRelease();
                    return Error.InvalidObject((AddressableKey)key);
                }

                if (parent is { IsValid: true, IsScene: true })
                {
                    go.MoveToScene(parent.Scene);
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

        public static Result<ValueHandlePair<TComponent, GameObject>, Error> InstantiateGetHandleOrError<TComponent>(
              this AddressableKey<GameObject> key
            , TransformOrScene parent = default
            , bool inWorldSpace = false
            , bool trimCloneSuffix = false
        )
            where TComponent : Component
        {
            var result = InstantiateGetHandleOrError(key, parent, inWorldSpace, trimCloneSuffix);

            if (result.TryGetError(out var error))
            {
                return error;
            }

            if (result.TryGetValue(out var value) == false)
            {
                return Error.Undefined((AddressableKey)key);
            }

            var (go, handle) = value;

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
