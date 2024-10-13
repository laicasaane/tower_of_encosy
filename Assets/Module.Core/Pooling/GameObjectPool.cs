using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Module.Core.Collections;
using Module.Core.Collections.Unsafe;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Module.Core.Pooling
{
    using Location = GameObjectPrefab.Location;
    using UnityObject = UnityEngine.Object;

    public class GameObjectPool
    {
        private readonly GameObjectPrefab _prefab;
        private readonly FasterList<GameObject> _unusedObjects;
        private readonly FasterList<int> _unusedInstanceIds;
        private readonly FasterList<int> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        public GameObjectPool([NotNull] GameObjectPrefab prefab)
        {
            _prefab = prefab;
            _unusedObjects = new();
            _unusedInstanceIds = new();
            _unusedTransformIds = new();
            _objectList = new();
        }

        public int UnusedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unusedObjects.Count;
        }

        public GameObjectPrefab Prefab
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _prefab;
        }

        public bool TrimCloneSuffix { get; set; }

        public void Prepool(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (amount <= 1)
            {
                PrepoolOne();
            }
            else
            {
                PrepoolMany(amount);
            }

            void PrepoolOne()
            {
                var go = _prefab.Instantiate(Location.Parent, Location.PoolScene, Location.Scene);

                if (go.IsInvalid())
                {
                    return;
                }

                if (TrimCloneSuffix)
                {
                    go.TrimCloneSuffix();
                }

                _unusedObjects.IncreaseCapacityBy(1);
                _unusedInstanceIds.IncreaseCapacityBy(1);
                _unusedTransformIds.IncreaseCapacityBy(1);

                _unusedObjects.Add(go);
                _unusedInstanceIds.Add(go.GetInstanceID());
                _unusedTransformIds.Add(go.transform.GetInstanceID());

                go.SetActive(false);
            }

            void PrepoolMany(int amount)
            {
                if (_prefab.Instantiate(amount
                    , Allocator.Temp
                    , out var instanceIds
                    , out var transformIds
                    , Location.Parent, Location.PoolScene, Location.Scene
                ) == false)
                {
                    return;
                }

                _objectList.Clear();
                _objectList.Capacity = Mathf.Max(_objectList.Capacity, amount);

                Resources.InstanceIDToObjectList(instanceIds, _objectList);

                var unusedObjects = _unusedObjects;
                unusedObjects.IncreaseCapacityBy(amount);
                _unusedInstanceIds.IncreaseCapacityBy(amount);
                _unusedTransformIds.IncreaseCapacityBy(amount);

                var objects = _objectList.AsReadOnlySpanUnsafe();
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

                _unusedInstanceIds.AddRange(instanceIds.AsReadOnlySpan());
                _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

                _objectList.Clear();

                GameObject.SetGameObjectsActive(instanceIds, false);
            }
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

            _unusedObjects.RemoveAt(keep, removeCount);
            _unusedInstanceIds.RemoveAt(keep, removeCount);
            _unusedTransformIds.RemoveAt(keep, removeCount);
        }

        public GameObject RentGameObject(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var result = _unusedObjects[last];

            _unusedObjects.RemoveAt(last);
            _unusedInstanceIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(result);

            if (activate)
            {
                result.SetActive(true);
            }

            return result;
        }

        public int RentInstanceId(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var obj = _unusedObjects[last];
            var result = _unusedInstanceIds[last];

            _unusedObjects.RemoveAt(last);
            _unusedInstanceIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(obj);

            if (activate)
            {
                obj.SetActive(true);
            }

            return result;
        }

        public int RentTransformId(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var obj = _unusedObjects[last];
            var result = _unusedTransformIds[last];

            _unusedObjects.RemoveAt(last);
            _unusedInstanceIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            Prefab.MoveToScene(obj);

            if (activate)
            {
                obj.SetActive(true);
            }

            return result;
        }

        public void Rent(Span<int> instanceIds, Span<int> transformIds, bool activate = false)
        {
            var length = instanceIds.Length;

            Assert.IsTrue(length == transformIds.Length, "arrays do not have the same size");
            Assert.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedObjects.RemoveAt(startIndex, length);
            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            Prefab.MoveToScene(instanceIds);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void Rent(
              Span<GameObject> objects
            , Span<int> instanceIds
            , Span<int> transformIds
            , bool activate = false
        )
        {
            var length = objects.Length;

            Assert.IsTrue(length == instanceIds.Length && length == transformIds.Length, "arrays do not have the same size");
            Assert.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            _unusedObjects.CopyTo(startIndex, objects);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedObjects.RemoveAt(startIndex, length);
            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            Prefab.MoveToScene(instanceIds);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentGameObjects(Span<GameObject> objects, bool activate = false)
        {
            var length = objects.Length;

            Assert.IsTrue(length > 0, "\"objects\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedObjects.CopyTo(startIndex, objects);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedObjects.RemoveAt(startIndex, length);
            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            Prefab.MoveToScene(instanceIds);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentInstanceIds(Span<int> instanceIds, bool activate = false)
        {
            var length = instanceIds.Length;

            Assert.IsTrue(length > 0, "\"instanceIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedObjects.RemoveAt(startIndex, length);
            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            Prefab.MoveToScene(instanceIds);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentTransformIds(Span<int> transformIds, bool activate = false)
        {
            var length = transformIds.Length;

            Assert.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedObjects.RemoveAt(startIndex, length);
            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            Prefab.MoveToScene(instanceIds);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void Return(GameObject instance)
        {
            if (instance.IsInvalid())
            {
                return;
            }

            instance = instance.AlwaysValid();

            Prefab.MoveToScene(instance, true);

            _unusedObjects.Add(instance);
            _unusedInstanceIds.Add(instance.GetInstanceID());
            _unusedTransformIds.Add(instance.transform.GetInstanceID());

            instance.SetActive(false);
        }

        public void ReturnGameObjects(ReadOnlySpan<GameObject> objects)
        {
            var length = objects.Length;

            if (length < 1)
            {
                return;
            }

            var unusedObjects = _unusedObjects;
            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedInstanceIds.IncreaseCapacityTo(capacity);
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
                unusedInstanceIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
                unusedTransformIds.Add(obj.transform.GetInstanceID());
            }

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);
            GameObject.SetGameObjectsActive(postIds, false);
        }

        public void ReturnInstanceIds(NativeArray<int> instanceIds)
        {
            var length = instanceIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            Resources.InstanceIDToObjectList(instanceIds, _objectList);

            var unusedObjects = _unusedObjects;
            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedInstanceIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpanUnsafe();
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
                unusedInstanceIds.Add(postIds[postIdsLength++] = instanceIds[i]);
                unusedTransformIds.Add(obj.transform.GetInstanceID());
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);
            GameObject.SetGameObjectsActive(postIds, false);
        }

        public void ReturnTransformIds(NativeArray<int> transformIds)
        {
            var length = transformIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            Resources.InstanceIDToObjectList(transformIds, _objectList);

            var unusedObjects = _unusedObjects;
            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedObjects.Count + length;

            unusedObjects.IncreaseCapacityTo(capacity);
            unusedInstanceIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpanUnsafe();
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
                unusedInstanceIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
                unusedTransformIds.Add(transformIds[i]);
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
            Prefab.MoveToScene(postIds, true);
            GameObject.SetGameObjectsActive(postIds, false);
        }
    }
}
