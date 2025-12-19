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

namespace EncosyTower.Pooling
{
    using Location = GameObjectPrefab.Location;
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

    public sealed class GameObjectPool : IDisposable
    {
        private readonly FasterList<GameObject> _unusedGameObjects;
        private readonly FasterList<GameObjectId> _unusedGameObjectIds;
        private readonly FasterList<TransformId> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        private GameObjectPrefab _prefab;

        public GameObjectPool()
        {
            _unusedGameObjects = new();
            _unusedGameObjectIds = new();
            _unusedTransformIds = new();
            _objectList = new();
        }

        public RentingStrategy RentingStrategy { get; set; }

        public ReturningStrategy ReturningStrategy { get; set; }

        public int UnusedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unusedGameObjects.Count;
        }

        public GameObjectPrefab Prefab
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _prefab;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _prefab = value ?? throw new NullReferenceException(nameof(Prefab));
        }

        public bool TrimCloneSuffix { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(int amount)
            => Prepool(amount, default);

        public bool Prepool(int amount, ReturningStrategy strategy)
        {
            if (_prefab == null)
            {
                throw new NullReferenceException(nameof(Prefab));
            }

            if (amount <= 0)
            {
                return false;
            }

            if (amount <= 1)
            {
                return PrepoolOne(new(ReturningStrategy, strategy));
            }
            else
            {
                return PrepoolMany(amount, new(ReturningStrategy, strategy));
            }
        }

        private bool PrepoolOne(ReturnOperation operation)
        {
            var go = _prefab.Instantiate(Location.Parent, Location.PoolScene, Location.Scene);

            if (go.IsInvalid())
            {
                return false;
            }

            if (TrimCloneSuffix)
            {
                go.TrimCloneSuffix();
            }

            _unusedGameObjects.IncreaseCapacityBy(1);
            _unusedGameObjectIds.IncreaseCapacityBy(1);
            _unusedTransformIds.IncreaseCapacityBy(1);

            _unusedGameObjects.Add(go);

#if UNITY_6000_2_OR_NEWER
            _unusedGameObjectIds.Add(go.GetEntityId());
            _unusedTransformIds.Add(go.transform.GetEntityId());
#else
            _unusedGameObjectIds.Add(go.GetInstanceID());
            _unusedTransformIds.Add(go.transform.GetInstanceID());
#endif

            if (operation.ShouldDeactivate())
            {
                go.SetActive(false);
            }

            return true;
        }

        private bool PrepoolMany(int amount, ReturnOperation operation)
        {
            if (_prefab.Instantiate(amount
                , Allocator.Temp
                , out NativeArray<GameObjectId> gameObjectIds
                , out NativeArray<TransformId> transformIds
                , Location.Parent, Location.PoolScene, Location.Scene
            ) == false)
            {
                return false;
            }

            UnityObjectAPI.ConvertGameObjectIdsToObjectList(gameObjectIds, _objectList);

            var unusedGameObjects = _unusedGameObjects;
            unusedGameObjects.IncreaseCapacityBy(amount);
            _unusedGameObjectIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            var gameObjects = _objectList.AsReadOnlySpan();
            var objectsLength = gameObjects.Length;
            var trimCloneSuffix = TrimCloneSuffix;

            for (var i = 0; i < objectsLength; i++)
            {
                var gameObject = (gameObjects[i] as GameObject).AssumeValid();

                if (trimCloneSuffix)
                {
                    gameObject.TrimCloneSuffix();
                }

                unusedGameObjects.Add(gameObject);
            }

            _unusedGameObjectIds.AddRange(gameObjectIds.AsReadOnlySpan());
            _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

            _objectList.Clear();

            if (operation.ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds.Reinterpret<EntityId>(), false);
            }

            return true;
        }

        public void ReleaseInstances(int keep, Action<GameObject> onReleased = null)
        {
            keep = Mathf.Max(keep, 0);

            var removeCount = UnusedCount - keep;

            if (removeCount < 1)
            {
                return;
            }

            var gameObjectSpan = _unusedGameObjects.AsSpan()[keep..];

            for (var i = gameObjectSpan.Length - 1; i >= 0; i--)
            {
                var gameObject = gameObjectSpan[i].AssumeValid();
                onReleased?.Invoke(gameObject);
                UnityObject.Destroy(gameObject);
            }

            _unusedGameObjects.RemoveRange(keep, removeCount);
            _unusedGameObjectIds.RemoveRange(keep, removeCount);
            _unusedTransformIds.RemoveRange(keep, removeCount);
        }

        public GameObject RentGameObject(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var result = _unusedGameObjects[last].AssumeValid();

            _unusedGameObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(result);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                result.SetActive(true);
            }

            return result;
        }

        public Transform RentTransform(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var result = _unusedGameObjects[last].AssumeValid();

            _unusedGameObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(result);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                result.SetActive(true);
            }

            return result.transform.AssumeValid();
        }

        public GameObjectId RentGameObjectId(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var gameObject = _unusedGameObjects[last].AssumeValid();
            var result = _unusedGameObjectIds[last];

            _unusedGameObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(gameObject);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                gameObject.SetActive(true);
            }

            return result;
        }

        public TransformId RentTransformId(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var gameObject = _unusedGameObjects[last].AssumeValid();
            var result = _unusedTransformIds[last];

            _unusedGameObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(gameObject);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                gameObject.SetActive(true);
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

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
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

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(
              Span<Transform> transforms
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = transforms.Length;

            Checks.IsTrue(length == gameObjectIds.Length && length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(
              Span<GameObject> gameObjects
            , Span<Transform> transforms
            , Span<GameObjectId> gameObjectIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == transforms.Length && length == gameObjectIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
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

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<GameObject> gameObjects, Span<Transform> transforms, RentingStrategy strategy)
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == transforms.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<GameObject> gameObjects, RentingStrategy strategy)
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length > 0, "array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);

            _unusedGameObjects.CopyTo(startIndex, gameObjects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<Transform> transforms, RentingStrategy strategy)
        {
            var length = transforms.Length;

            Checks.IsTrue(length > 0, "array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(length, Allocator.Temp);

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            UnityObjectAPI.ConvertTransformIdsToTransforms(transformIds, transforms, _objectList);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<GameObjectId> gameObjectIds, RentingStrategy strategy)
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length > 0, "\"gameObjectIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Rent(Span<TransformId> transformIds, RentingStrategy strategy)
        {
            var length = transformIds.Length;

            Checks.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds), true);
            }
        }

        public void Return(GameObject gameObject, ReturningStrategy strategy)
        {
            if (gameObject.IsInvalid())
            {
                return;
            }

            gameObject = gameObject.AssumeValid();
            var transform = gameObject.transform.AssumeValid();

            Prefab.MoveToScene(gameObject, true);

            _unusedGameObjects.Add(gameObject);

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

            Prefab.MoveToScene(gameObject, true);

            _unusedGameObjects.Add(gameObject);

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

            var unusedGameObjects = _unusedGameObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjects.Count + length;

            unusedGameObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = gameObjects[i].AssumeValid();
                var transform = transforms[i].AssumeValid();

                unusedGameObjects.Add(gameObject);

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

            Prefab.MoveToScene(returnedGameObjectIds, true);

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

            var unusedGameObjects = _unusedGameObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjects.Count + length;

            unusedGameObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = gameObjects[i].AssumeValid();
                var transform = gameObject.transform.AssumeValid();

                unusedGameObjects.Add(gameObject);

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

            Prefab.MoveToScene(returnedGameObjectIds, true);

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

            var unusedGameObjects = _unusedGameObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjects.Count + length;

            unusedGameObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var transform = transforms[i].AssumeValid();
                var gameObject = transform.gameObject.AssumeValid();

                unusedGameObjects.Add(gameObject);

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

            Prefab.MoveToScene(returnedGameObjectIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
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

            var unusedGameObjects = _unusedGameObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjects.Count + length;

            unusedGameObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, EntityId>(gameObjectIds);
            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var gameObject = (objects[i] as GameObject).AssumeValid();
                var transform = gameObject.transform.AssumeValid();

                unusedGameObjects.Add(gameObject);
                unusedGameObjectIds.Add(gameObjectIdBuffer[gameObjectIdCount++] = reGameObjectIds[i]);

#if UNITY_6000_2_OR_NEWER
                unusedTransformIds.Add(transform.GetEntityId());
#else
                unusedTransformIds.Add(transform.GetInstanceID());
#endif
            }

            _objectList.Clear();

            var returnedGameObjectIds = gameObjectIdBuffer.GetSubArray(0, gameObjectIdCount);

            Prefab.MoveToScene(returnedGameObjectIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Return(ReadOnlySpan<TransformId> transformIds, ReturningStrategy strategy)
        {
            var length = transformIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.IncreaseCapacityTo(length);

            UnityObjectAPI.ConvertTransformIdsToObjectList(transformIds, _objectList);

            var unusedGameObjects = _unusedGameObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjects.Count + length;

            unusedGameObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var reTransformIds = MemoryMarshal.Cast<TransformId, EntityId>(transformIds);
            var gameObjectIdBuffer = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var gameObjectIdCount = 0;

            for (var i = 0; i < length; i++)
            {
                var transform = (objects[i] as Transform).AssumeValid();
                var gameObject = transform.gameObject.AssumeValid();

                unusedGameObjects.Add(gameObject);

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

            Prefab.MoveToScene(returnedGameObjectIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(returnedGameObjectIds.Reinterpret<EntityId>(), false);
            }
        }

        public void Dispose()
        {
            _prefab = null;
            _unusedGameObjectIds.Clear();
            _unusedTransformIds.Clear();
            _unusedGameObjects.Clear();
            _objectList.Clear();
        }
    }
}
