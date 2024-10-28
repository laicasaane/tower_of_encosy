using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Collections.Unsafe;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    using UnityObject = UnityEngine.Object;

    public class SceneObjectIdPool
    {
        private readonly Scene _scene;
        private readonly GameObject _source;

        private readonly FasterList<int> _unusedInstanceIds;
        private readonly FasterList<int> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        public SceneObjectIdPool(Scene scene, [NotNull] GameObject source)
        {
            if (scene.IsValid() == false)
            {
                throw new MissingReferenceException(nameof(scene));
            }

            if (source.IsInvalid())
            {
                throw new MissingReferenceException(nameof(source));
            }

            _source = source;
            _scene = scene;

            _unusedInstanceIds = new(32);
            _unusedTransformIds = new(32);
            _objectList = new(32);
        }

        public int UnusedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _unusedTransformIds.Count;
        }

        public GameObject Source
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source;
        }

        public Scene Scene
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scene;
        }

        public bool TrimCloneSuffix { get; set; }

        public void Prepool(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var instanceIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            GameObject.InstantiateGameObjects(
                  _source.GetInstanceID()
                , amount
                , instanceIds
                , transformIds
                , _scene
            );

            if (TrimCloneSuffix)
            {
                TrimCloneSuffixFrom(instanceIds, _objectList);
            }

            _unusedInstanceIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            _unusedInstanceIds.AddRange(instanceIds.AsReadOnlySpan());
            _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

            GameObject.SetGameObjectsActive(instanceIds, false);

            static void TrimCloneSuffixFrom(NativeArray<int> instanceIds, List<UnityObject> objectList)
            {
                if (instanceIds.IsCreated == false || instanceIds.Length < 1)
                {
                    return;
                }

                objectList.Clear();
                objectList.Capacity = Mathf.Max(objectList.Capacity, instanceIds.Length);

                Resources.InstanceIDToObjectList(instanceIds, objectList);

                var objects = objectList.AsReadOnlySpanUnsafe();

                for (var i = objects.Length - 1; i >= 0; i--)
                {
                    var go = objects[i] as GameObject;
                    go.TrimCloneSuffix();
                }
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

            var instanceIdSpan = _unusedInstanceIds.AsSpan()[keep..];
            var length = instanceIdSpan.Length;
            var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            instanceIdSpan.CopyTo(instanceIds);

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            Resources.InstanceIDToObjectList(instanceIds, _objectList);

            var objects = _objectList.AsReadOnlySpanUnsafe();

            for (var i = objects.Length - 1; i >= 0; i--)
            {
                var go = objects[i] as GameObject;

                if (go.IsInvalid())
                {
                    continue;
                }

                onReleased?.Invoke(go);
                UnityObject.Destroy(go);
            }

            _objectList.Clear();

            _unusedInstanceIds.RemoveAt(keep, removeCount);
            _unusedTransformIds.RemoveAt(keep, removeCount);
        }

        public int RentInstanceId(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var result = _unusedInstanceIds[last];

            _unusedInstanceIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (activate)
            {
                var ids = NativeArray.CreateFast<int>(1, Allocator.Temp);
                ids[0] = result;

                GameObject.SetGameObjectsActive(ids, true);
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
            var id = _unusedInstanceIds[last];
            var result = _unusedTransformIds[last];

            _unusedInstanceIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (activate)
            {
                var ids = NativeArray.CreateFast<int>(1, Allocator.Temp);
                ids[0] = id;

                GameObject.SetGameObjectsActive(ids, true);
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

            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

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

            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

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

            _unusedInstanceIds.RemoveAt(startIndex, length);
            _unusedTransformIds.RemoveAt(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentTransforms(int amount, [NotNull] FasterList<Transform> transforms, bool activate = false)
        {
            Assert.IsTrue(amount > 0, "\"amount\" must be greater than 0");

            Prepool(amount - UnusedCount);

            var startIndex = UnusedCount - amount;
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var instanceIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedInstanceIds.RemoveAt(startIndex, amount);
            _unusedTransformIds.RemoveAt(startIndex, amount);

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, amount);

            Resources.InstanceIDToObjectList(transformIds, _objectList);

            var objects = _objectList.AsReadOnlySpanUnsafe();
            transforms.AddReplicateNoInit(amount);
            var transformSpan = transforms.AsSpan();

            for (var i = 0; i < amount; i++)
            {
                transformSpan[i] = objects[i] as Transform;
            }

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }

            _objectList.Clear();
        }

        public void Return(Span<int> instanceIds, Span<int> transformIds)
        {
            var length = instanceIds.Length;

            Assert.IsTrue(length == transformIds.Length, "arrays do not have the same size");

            if (length < 1)
            {
                return;
            }

            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedInstanceIds.Count + length;

            unusedInstanceIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            unusedInstanceIds.AddRange(instanceIds);
            unusedTransformIds.AddRange(transformIds);

            GameObject.SetGameObjectsActive(instanceIds, false);
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

            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedInstanceIds.Count + length;

            unusedInstanceIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            var objects = _objectList.AsReadOnlySpanUnsafe();
            var postIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            var postIdsLength = 0;

            for (var i = 0; i < length; i++)
            {
                var go = objects[i] as GameObject;

                if (go.IsInvalid())
                {
                    continue;
                }

                unusedInstanceIds.Add(postIds[postIdsLength++] = instanceIds[i]);
                unusedTransformIds.Add(go.transform.GetInstanceID());
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
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

            var unusedInstanceIds = _unusedInstanceIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedInstanceIds.Count + length;

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

                unusedInstanceIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
                unusedTransformIds.Add(transformIds[i]);
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);
            GameObject.SetGameObjectsActive(postIds, false);
        }
    }
}
