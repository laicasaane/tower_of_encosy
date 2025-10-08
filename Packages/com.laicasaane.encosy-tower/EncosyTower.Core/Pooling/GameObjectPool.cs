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

    public sealed class GameObjectPool : IDisposable
    {
        private readonly FasterList<GameObject> _unusedObjects;
        private readonly FasterList<int> _unusedGameObjectIds;
        private readonly FasterList<int> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        private GameObjectPrefab _prefab;

        public GameObjectPool()
        {
            _unusedObjects = new();
            _unusedGameObjectIds = new();
            _unusedTransformIds = new();
            _objectList = new();
        }

        public RentingStrategy RentingStrategy { get; set; }

        public ReturningStrategy ReturningStrategy { get; set; }

        public int UnusedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unusedObjects.Count;
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

            _unusedObjects.IncreaseCapacityBy(1);
            _unusedGameObjectIds.IncreaseCapacityBy(1);
            _unusedTransformIds.IncreaseCapacityBy(1);

            _unusedObjects.Add(go);

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
                , out NativeArray<int> gameObjectIds
                , out NativeArray<int> transformIds
                , Location.Parent, Location.PoolScene, Location.Scene
            ) == false)
            {
                return false;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, amount);

            Resources.InstanceIDToObjectList(gameObjectIds, _objectList);

            var unusedObjects = _unusedObjects;
            unusedObjects.IncreaseCapacityBy(amount);
            _unusedGameObjectIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            var objects = _objectList.AsReadOnlySpan();
            var objectsLength = objects.Length;
            var trimCloneSuffix = TrimCloneSuffix;

            for (var i = 0; i < objectsLength; i++)
            {
                var go = objects[i] as GameObject;

                if (go.IsInvalid())
                {
                    continue;
                }

                if (trimCloneSuffix)
                {
                    go.TrimCloneSuffix();
                }

                unusedObjects.Add(go);
            }

            _unusedGameObjectIds.AddRange(gameObjectIds.AsReadOnlySpan());
            _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

            _objectList.Clear();

            if (operation.ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds, false);
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

            var objectSpan = _unusedObjects.AsSpan()[keep..];

            for (var i = objectSpan.Length - 1; i >= 0; i--)
            {
                var go = objectSpan[i];
                onReleased?.Invoke(go);
                UnityObject.Destroy(go);
            }

            _unusedObjects.RemoveRange(keep, removeCount);
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
            var result = _unusedObjects[last];

            _unusedObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(result);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                result.SetActive(true);
            }

            return result;
        }

        public GameObjectId RentGameObjectId(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var obj = _unusedObjects[last];
            var result = _unusedGameObjectIds[last];

            _unusedObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(obj);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                obj.SetActive(true);
            }

            return (GameObjectId)result;
        }

        public TransformId RentTransformId(RentingStrategy strategy)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var obj = _unusedObjects[last];
            var result = _unusedTransformIds[last];

            _unusedObjects.RemoveAt(last);
            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(obj);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                obj.SetActive(true);
            }

            return (TransformId)result;
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

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, reGameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, reTransformIds);

            _unusedObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(reGameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, true);
            }
        }

        public void Rent(
              Span<GameObject> objects
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , RentingStrategy strategy
        )
        {
            var length = objects.Length;

            Checks.IsTrue(length == gameObjectIds.Length && length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var startIndex = UnusedCount - length;

            _unusedObjects.CopyTo(startIndex, objects);
            _unusedGameObjectIds.CopyTo(startIndex, reGameObjectIds);
            _unusedTransformIds.CopyTo(startIndex, reTransformIds);

            _unusedObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(reGameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, true);
            }
        }

        public void Rent(Span<GameObject> objects, RentingStrategy strategy)
        {
            var length = objects.Length;

            Checks.IsTrue(length > 0, "\"objects\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedObjects.CopyTo(startIndex, objects);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds, true);
            }
        }

        public void Rent(Span<GameObjectId> gameObjectIds, RentingStrategy strategy)
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length > 0, "\"gameObjectIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var startIndex = UnusedCount - length;

            _unusedGameObjectIds.CopyTo(startIndex, reGameObjectIds);

            _unusedObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(reGameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, true);
            }
        }

        public void Rent(Span<TransformId> transformIds, RentingStrategy strategy)
        {
            var length = transformIds.Length;

            Checks.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, reTransformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedObjects.RemoveRange(startIndex, length);
            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            Prefab.MoveToScene(gameObjectIds);

            if (new RentOperation(RentingStrategy, strategy).ShouldActivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds, true);
            }
        }

        public void Return(GameObject instance, ReturningStrategy strategy)
        {
            if (instance.IsInvalid())
            {
                return;
            }

            instance = instance.AlwaysValid();

            Prefab.MoveToScene(instance, true);

            _unusedObjects.Add(instance);

#if UNITY_6000_2_OR_NEWER
            _unusedGameObjectIds.Add(instance.GetEntityId());
            _unusedTransformIds.Add(instance.transform.GetEntityId());
#else
            _unusedGameObjectIds.Add(instance.GetInstanceID());
            _unusedTransformIds.Add(instance.transform.GetInstanceID());
#endif

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                instance.SetActive(false);
            }
        }

        public void Return(ReadOnlySpan<GameObject> objects, ReturningStrategy strategy)
        {
            var length = objects.Length;

            if (length < 1)
            {
                return;
            }

            var unusedObjects = _unusedObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var postIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            var postIdsLength = 0;

            for (var i = 0; i < length; i++)
            {
                var obj = objects[i];

                if (obj.IsInvalid())
                {
                    continue;
                }

                unusedObjects.Add(obj);

#if UNITY_6000_2_OR_NEWER
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetEntityId());
                unusedTransformIds.Add(obj.transform.GetEntityId());
#else
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
                unusedTransformIds.Add(obj.transform.GetInstanceID());
#endif
            }

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, false);
            }
        }

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReturningStrategy strategy)
        {
            var length = gameObjectIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var gameObjectIdArray = NativeArray.CreateFast<int>(length, Allocator.Temp);
            reGameObjectIds.CopyTo(gameObjectIdArray);

            Resources.InstanceIDToObjectList(gameObjectIdArray, _objectList);

            var unusedObjects = _unusedObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var postIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            var postIdsLength = 0;

            for (var i = 0; i < length; i++)
            {
                var obj = objects[i] as GameObject;

                if (obj.IsInvalid())
                {
                    continue;
                }

                unusedObjects.Add(obj);
                unusedGameObjectIds.Add(postIds[postIdsLength++] = reGameObjectIds[i]);

#if UNITY_6000_2_OR_NEWER
                unusedTransformIds.Add(obj.transform.GetEntityId());
#else
                unusedTransformIds.Add(obj.transform.GetInstanceID());
#endif
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, false);
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
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var transformIdArray = NativeArray.CreateFast<int>(length, Allocator.Temp);
            reTransformIds.CopyTo(transformIdArray);

            Resources.InstanceIDToObjectList(transformIdArray, _objectList);

            var unusedObjects = _unusedObjects;
            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpan();
            var postIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            var postIdsLength = 0;

            for (var i = 0; i < length; i++)
            {
                var transform = objects[i] as Transform;

                if (transform.IsInvalid())
                {
                    continue;
                }

                var obj = transform.gameObject;

                if (obj.IsInvalid())
                {
                    continue;
                }

                unusedObjects.Add(obj);

#if UNITY_6000_2_OR_NEWER
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetEntityId());
#else
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
#endif

                unusedTransformIds.Add(reTransformIds[i]);
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);

            if (new ReturnOperation(ReturningStrategy, strategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, false);
            }
        }

        public void Dispose()
        {
            _prefab = null;
            _unusedGameObjectIds.Clear();
            _unusedTransformIds.Clear();
            _unusedObjects.Clear();
            _objectList.Clear();
        }
    }
}
