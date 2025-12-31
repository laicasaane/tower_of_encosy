using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Debugging;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    using UnityObject = UnityEngine.Object;

#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

#if UNITY_6000_3_OR_NEWER
    using EntityId = UnityEngine.EntityId;
#else
    using EntityId = System.Int32;
#endif

    public sealed class SceneObjectPool : IDisposable
    {
        private readonly FasterList<GameObjectId> _unusedGameObjectIds;
        private readonly FasterList<TransformId> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        private Scene _scene;
        private GameObject _source;

        public SceneObjectPool(uint initialCapacity)
        {
            var capacity = (int)initialCapacity;
            _unusedGameObjectIds = new(capacity);
            _unusedTransformIds = new(capacity);
            _objectList = new(capacity);
        }

        public RentingStrategy RentingStrategy { get; set; }

        public ReturningStrategy ReturningStrategy { get; set; }

        public int UnusedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unusedTransformIds.Count;
        }

        public GameObject Source
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue(value.IsValid(), "Cannot assign invalid value to Source.");
                _source = value;
            }
        }

        public Scene Scene
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scene;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue(value.IsValid(), "Cannot assign invalid value to Scene.");
                _scene = value;
            }
        }

        public bool TrimCloneSuffix { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepool(int amount)
            => Prepool(amount, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepool(int amount, ReturningStrategy strategy)
        {
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);
            Prepool(gameObjectIds, transformIds, strategy);
        }

        public void Prepool(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            Checks.IsTrue(_source.IsValid(), "Source must be initialized before Prepool is called.");
            Checks.IsTrue(_scene.IsValid(), "Scene must be initialized before Prepool is called.");
            Checks.IsTrue(gameObjectIds.IsCreated, "gameObjectIds must be created before passing into Prepool.");
            Checks.IsTrue(transformIds.IsCreated, "transformIds must be created before passing into Prepool.");

            var amount = gameObjectIds.Length;

            Checks.IsTrue(amount == transformIds.Length, "arrays do not have the same size");

            if (amount <= 0)
            {
                return;
            }

            GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                  _source.GetEntityId()
#else
                  _source.GetInstanceID()
#endif
                , amount
                , gameObjectIds.Reinterpret<EntityId>()
                , transformIds.Reinterpret<EntityId>()
                , _scene
            );

            if (TrimCloneSuffix)
            {
                TrimCloneSuffixFrom(gameObjectIds, _objectList);
                _objectList.Clear();
            }

            _unusedGameObjectIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            _unusedGameObjectIds.AddRange(gameObjectIds.AsReadOnlySpan());
            _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Prepool(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            Checks.IsTrue(_source.IsValid(), "Source must be initialized before Prepool is called.");
            Checks.IsTrue(_scene.IsValid(), "Scene must be initialized before Prepool is called.");

            var amount = gameObjectIds.Length;

            Checks.IsTrue(amount == transformIds.Length, "arrays do not have the same size");

            if (amount <= 0)
            {
                return;
            }

            var gameObjectIdArray = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIdArray = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                  _source.GetEntityId()
#else
                  _source.GetInstanceID()
#endif
                , amount
                , gameObjectIdArray.Reinterpret<EntityId>()
                , transformIdArray.Reinterpret<EntityId>()
                , _scene
            );

            if (TrimCloneSuffix)
            {
                TrimCloneSuffixFrom(gameObjectIdArray, _objectList);
                _objectList.Clear();
            }

            gameObjectIdArray.AsReadOnlySpan().CopyTo(gameObjectIds);
            transformIdArray.AsReadOnlySpan().CopyTo(transformIds);

            _unusedGameObjectIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            _unusedGameObjectIds.AddRange(gameObjectIds);
            _unusedTransformIds.AddRange(transformIds);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIdArray.Reinterpret<EntityId>(), false);
            }
        }

        /// <summary>
        /// Prepool the difference between <paramref name="estimatedAmount"/> and <see cref="UnusedCount"/>.
        /// </summary>
        /// <param name="estimatedAmount">The estimated amount to be prepooled.</param>
        /// <returns>The actual amount to be prepooled. The value is either a positive number or zero.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PrepoolDifference(int estimatedAmount)
            => PrepoolDifference(estimatedAmount, default);

        /// <inheritdoc cref="PrepoolDifference(int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PrepoolDifference(int estimatedAmount, ReturningStrategy strategy)
        {
            var diffAmount = Mathf.Max(estimatedAmount - UnusedCount, 0);

            if (diffAmount > 0)
            {
                Prepool(diffAmount, strategy);
            }

            return diffAmount;
        }

        public int PrepoolDifference(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            Checks.IsTrue(gameObjectIds.IsCreated, "gameObjectIds must be created before passing into Prepool.");
            Checks.IsTrue(transformIds.IsCreated, "transformIds must be created before passing into Prepool.");
            return PrepoolDifference(gameObjectIds.AsSpan(), transformIds.AsSpan(), strategy);
        }

        public int PrepoolDifference(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            var estimatedAmount = gameObjectIds.Length;

            Checks.IsTrue(estimatedAmount == transformIds.Length, "arrays do not have the same size");

            var diffAmount = Mathf.Max(estimatedAmount - UnusedCount, 0);

            if (diffAmount > 0)
            {
                var gameObjectIdArray = NativeArray.CreateFast<GameObjectId>(diffAmount, Allocator.Temp);
                var transformIdArray = NativeArray.CreateFast<TransformId>(diffAmount, Allocator.Temp);

                Prepool(gameObjectIdArray, transformIdArray, strategy);

                gameObjectIdArray.AsReadOnlySpan().CopyTo(gameObjectIds);
                transformIdArray.AsReadOnlySpan().CopyTo(transformIds);
            }

            return diffAmount;
        }

        public void ReleaseInstances(int keep, Action<GameObject> onReleased = null)
        {
            keep = Mathf.Max(keep, 0);

            var removeCount = UnusedCount - keep;

            if (removeCount < 1)
            {
                return;
            }

            var unusedGameObjectIds = _unusedGameObjectIds.AsSpan()[keep..];
            var length = unusedGameObjectIds.Length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            unusedGameObjectIds.CopyTo(gameObjectIds);

            UnityObjectAPI.ConvertGameObjectIdsToObjectList(gameObjectIds, _objectList);

            var objects = _objectList.AsReadOnlySpan();

            for (var i = objects.Length - 1; i >= 0; i--)
            {
                var gameObject = (objects[i] as GameObject).AssumeValid();

                onReleased?.Invoke(gameObject);
                UnityObject.Destroy(gameObject);
            }

            _objectList.Clear();

            _unusedGameObjectIds.RemoveRange(keep, removeCount);
            _unusedTransformIds.RemoveRange(keep, removeCount);
        }

        public GameObject RentGameObject(RentingStrategy strategy)
        {
            PrepoolDifference(1);

            var last = UnusedCount - 1;
            var result = _unusedGameObjectIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                var ids = NativeArray.CreateFast<EntityId>(1, Allocator.Temp);
                ids[0] = (EntityId)result;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return result.ToObject().GetValueOrThrow();
        }

        public Transform RentTransform(RentingStrategy strategy)
        {
            PrepoolDifference(1);

            var last = UnusedCount - 1;
            var result = _unusedTransformIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                var ids = NativeArray.CreateFast<EntityId>(1, Allocator.Temp);
                ids[0] = (EntityId)result;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return result.ToObject().GetValueOrThrow();
        }

        public GameObjectId RentGameObjectId(RentingStrategy strategy)
        {
            PrepoolDifference(1);

            var last = UnusedCount - 1;
            var result = _unusedGameObjectIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                var ids = NativeArray.CreateFast<EntityId>(1, Allocator.Temp);
                ids[0] = (EntityId)result;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return result;
        }

        public TransformId RentTransformId(RentingStrategy strategy)
        {
            PrepoolDifference(1);

            var last = UnusedCount - 1;
            var id = _unusedGameObjectIds[last];
            var result = _unusedTransformIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                var ids = NativeArray.CreateFast<EntityId>(1, Allocator.Temp);
                ids[0] = (EntityId)id;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return result;
        }

        public void Rent(
              Span<GameObject> gameObjects
            , Span<Transform> transforms
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(
                  length == transforms.Length && length == gameObjectIds.Length && length == transformIds.Length
                , "arrays do not have the same size"
            );

            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);
            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(
              Span<GameObject> gameObjects
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == gameObjectIds.Length && length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(
              Span<Transform> transforms
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = transformIds.Length;

            Checks.IsTrue(length == gameObjectIds.Length && length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(
              Span<GameObject> gameObjects
            , Span<Transform> transforms
            , Span<GameObjectId> gameObjectIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(
                length == transforms.Length
                && length == gameObjectIds.Length
                , "arrays do not have the same size"
            );

            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);
            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(
              Span<GameObject> gameObjects
            , Span<Transform> transforms
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == transforms.Length && length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);
            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(Span<GameObject> gameObjects, Span<Transform> transforms, RentingStrategy strategy)
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == transforms.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);
            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(Span<GameObject> gameObjects, RentingStrategy strategy)
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length > 0, "array does not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertGameObjectIdsToGameObjects(gameObjectIds, gameObjects, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(Span<Transform> transforms, RentingStrategy strategy)
        {
            var length = transforms.Length;

            Checks.IsTrue(length > 0, "array does not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }

            _objectList.Clear();
        }

        public void Rent(Span<GameObjectId> gameObjectIds, Span<TransformId> transformIds, RentingStrategy strategy)
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<GameObjectId> gameObjectIds, RentingStrategy strategy)
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length > 0, "\"gameObjectIds\" array does not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<TransformId> transformIds, RentingStrategy strategy)
        {
            var length = transformIds.Length;

            Checks.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            PrepoolDifference(length);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        /// <summary>
        /// Remove a number of <see cref="GameObjectId"/> and <see cref="TransformId"/> from the pool.
        /// </summary>
        /// <param name="estimatedAmount">The amount to be removed.</param>
        /// <returns>The actual amount to be removed.</returns>
        /// <remarks>
        /// The items are always removed from the end of the internal containers.
        /// </remarks>
        public int RemoveUnused(int estimatedAmount)
        {
            var startIndex = Mathf.Max(UnusedCount - estimatedAmount, 0);
            var length = UnusedCount - startIndex;

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            return length;
        }

        /// <summary>
        /// Remove a number of <see cref="GameObjectId"/> and <see cref="TransformId"/> from the pool.
        /// </summary>
        /// <param name="gameObjectIds">
        /// The buffer to contain the removed values of <see cref="GameObjectId"/>.
        /// It must have the same length as <paramref name="transformIds"/>.
        /// </param>
        /// <param name="transformIds">
        /// The buffer to contain the removed values of <see cref="TransformId"/>.
        /// It must have the same length as <paramref name="gameObjectIds"/>.
        /// </param>
        /// <returns>The actual amount to be removed.</returns>
        /// <remarks>
        /// The items are always removed from the end of the internal containers.
        /// </remarks>
        public int RemoveUnused(Span<GameObjectId> gameObjectIds, Span<TransformId> transformIds)
        {
            var estimatedAmount = gameObjectIds.Length;

            Checks.IsTrue(estimatedAmount == transformIds.Length, "arrays do not have the same size");

            var startIndex = Mathf.Max(UnusedCount - estimatedAmount, 0);
            var length = UnusedCount - startIndex;

            _unusedTransformIds.CopyTo(startIndex, transformIds[..length]);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds[..length]);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            return length;
        }

        public void Return(GameObject gameObject, ReturningStrategy strategy)
        {
            if (gameObject.IsInvalid())
            {
                return;
            }

            gameObject = gameObject.AssumeValid();
            var transform = gameObject.transform.AssumeValid();

#if UNITY_6000_2_OR_NEWER
            _unusedGameObjectIds.Add(gameObject.GetEntityId());
            _unusedTransformIds.Add(transform.GetEntityId());
#else
            _unusedGameObjectIds.Add(gameObject.GetInstanceID());
            _unusedTransformIds.Add(transform.GetInstanceID());
#endif

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                gameObject.SetActive(false);
            }
        }

        public void Return(Transform transform, ReturningStrategy strategy)
        {
            if (transform.IsInvalid())
            {
                return;
            }

            transform = transform.AssumeValid();
            var gameObject = transform.gameObject.AssumeValid();

#if UNITY_6000_2_OR_NEWER
            _unusedGameObjectIds.Add(gameObject.GetEntityId());
            _unusedTransformIds.Add(transform.GetEntityId());
#else
            _unusedGameObjectIds.Add(gameObject.GetInstanceID());
            _unusedTransformIds.Add(transform.GetInstanceID());
#endif

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                gameObject.SetActive(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(GameObjectId gameObjectId, ReturningStrategy strategy)
        {
            if (gameObjectId.IsValid == false)
            {
                return;
            }

            Return(gameObjectId.ToObject().GetValueOrThrow(), strategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(TransformId transformId, ReturningStrategy strategy)
        {
            if (transformId.IsValid == false)
            {
                return;
            }

            Return(transformId.ToObject().GetValueOrThrow(), strategy);
        }

        public void Return(
              ReadOnlySpan<GameObject> gameObjects
            , ReadOnlySpan<Transform> transforms
            , ReturningStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == transforms.Length, "arrays do not have the same size");

            if (length < 1)
            {
                return;
            }

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = gameObjects[i].AssumeValid();
                var transform = transforms[i].AssumeValid();

#if UNITY_6000_2_OR_NEWER
                var gameObjectId = gameObject.GetEntityId();
                var transformId = transform.GetEntityId();
#else
                var gameObjectId = gameObject.GetInstanceID();
                var transformId = transform.GetInstanceID();
#endif

                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = gameObjectId);
                unusedTransformIds.Add(transformId);
            }

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Return(ReadOnlySpan<GameObject> gameObjects, ReturningStrategy strategy)
        {
            var length = gameObjects.Length;

            if (length < 1)
            {
                return;
            }

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = gameObjects[i].AssumeValid();
                var transform = gameObject.transform.AssumeValid();

#if UNITY_6000_2_OR_NEWER
                var gameObjectId = gameObject.GetEntityId();
                var transformId = transform.GetEntityId();
#else
                var gameObjectId = gameObject.GetInstanceID();
                var transformId = transform.GetInstanceID();
#endif

                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = gameObjectId);
                unusedTransformIds.Add(transformId);
            }

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Return(ReadOnlySpan<Transform> transforms, ReturningStrategy strategy)
        {
            var length = transforms.Length;

            if (length < 1)
            {
                return;
            }

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var transform = transforms[i].AssumeValid();
                var gameObject = transform.gameObject.AssumeValid();

#if UNITY_6000_2_OR_NEWER
                var gameObjectId = gameObject.GetEntityId();
                var transformId = transform.GetEntityId();
#else
                var gameObjectId = gameObject.GetInstanceID();
                var transformId = transform.GetInstanceID();
#endif

                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = gameObjectId);
                unusedTransformIds.Add(transformId);
            }

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length == transformIds.Length, "arrays do not have the same size");

            if (length < 1)
            {
                return;
            }

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            unusedGameObjectIds.AddRange(gameObjectIds);
            unusedTransformIds.AddRange(transformIds);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), false);
            }
        }

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReturningStrategy strategy)
        {
            var length = gameObjectIds.Length;

            if (length < 1)
            {
                return;
            }

            UnityObjectAPI.ConvertGameObjectIdsToObjectList(gameObjectIds, _objectList);

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds);
            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = (objects[i] as GameObject).AssumeValid();

                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = reGameObjectIds[i]);

#if UNITY_6000_2_OR_NEWER
                unusedTransformIds.Add(gameObject.transform.GetEntityId());
#else
                unusedTransformIds.Add(gameObject.transform.GetInstanceID());
#endif
            }

            _objectList.Clear();

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), true);
            }
        }

        public void Return(ReadOnlySpan<TransformId> transformIds, ReturningStrategy strategy)
        {
            var length = transformIds.Length;

            if (length < 1)
            {
                return;
            }

            UnityObjectAPI.ConvertTransformIdsToObjectList(transformIds, _objectList);

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var reTransformIds = MemoryMarshal.Cast<TransformId, EntityId>(transformIds);
            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var transform = (objects[i] as Transform).AssumeValid();
                var gameObject = (transform.gameObject).AssumeValid();

#if UNITY_6000_2_OR_NEWER
                var gameObjectId = gameObject.GetEntityId();
#else
                var gameObjectId = gameObject.GetInstanceID();
#endif

                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = gameObjectId);
                unusedTransformIds.Add(reTransformIds[i]);
            }

            _objectList.Clear();

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), true);
            }
        }

        public void Dispose()
        {
            _source = null;
            _scene = default;
            _unusedGameObjectIds.Clear();
            _unusedTransformIds.Clear();
            _objectList.Clear();
        }

        private static void TrimCloneSuffixFrom(Span<GameObjectId> gameObjectIds, List<UnityObject> objectList)
        {
            if (gameObjectIds.Length < 1)
            {
                return;
            }

            UnityObjectAPI.ConvertGameObjectIdsToObjectList(gameObjectIds, objectList);
            TrimCloneSuffixFrom(objectList);
        }

        private static void TrimCloneSuffixFrom(NativeArray<GameObjectId> gameObjectIds, List<UnityObject> objectList)
        {
            if (gameObjectIds.IsCreated == false || gameObjectIds.Length < 1)
            {
                return;
            }

            UnityObjectAPI.ConvertGameObjectIdsToObjectList(gameObjectIds, objectList);
            TrimCloneSuffixFrom(objectList);
        }

        private static void TrimCloneSuffixFrom(List<UnityObject> objectList)
        {
            var objects = objectList.AsReadOnlySpan();

            for (var i = objects.Length - 1; i >= 0; i--)
            {
                var gameObject = (objects[i] as GameObject).AssumeValid();
                gameObject.TrimCloneSuffix();
            }
        }
    }
}
