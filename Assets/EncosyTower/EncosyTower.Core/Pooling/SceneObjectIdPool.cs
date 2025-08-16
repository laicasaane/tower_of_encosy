using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Debugging;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    using UnityObject = UnityEngine.Object;

    public sealed class SceneObjectIdPool : IDisposable
    {
        private readonly FasterList<int> _unusedInstanceIds;
        private readonly FasterList<int> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        private Scene _scene;
        private GameObject _source;

        public SceneObjectIdPool()
        {
            _unusedInstanceIds = new(32);
            _unusedTransformIds = new(32);
            _objectList = new(32);
        }

        public PooledGameObjectStrategy PooledStrategy { get; set; }

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
                if (value.IsInvalid())
                {
                    throw new MissingReferenceException(nameof(Source));
                }

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
                if (value.IsValid() == false)
                {
                    throw new MissingReferenceException(nameof(Scene));
                }

                _scene = value;
            }
        }

        public bool TrimCloneSuffix { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(int amount)
            => Prepool(amount, default);

        public bool Prepool(int amount, PooledGameObjectStrategy pooledStrategy)
        {
            if (_scene.IsValid() == false)
            {
                throw new MissingReferenceException(nameof(Scene));
            }

            if (_source.IsInvalid())
            {
                throw new MissingReferenceException(nameof(Source));
            }

            if (amount <= 0)
            {
                return false;
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

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(instanceIds, false);
            }

            return true;

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

            _unusedInstanceIds.RemoveRange(keep, removeCount);
            _unusedTransformIds.RemoveRange(keep, removeCount);
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

            Checks.IsTrue(length == transformIds.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;

            _unusedInstanceIds.CopyTo(startIndex, instanceIds);
            _unusedTransformIds.CopyTo(startIndex, transformIds);

            _unusedInstanceIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentInstanceIds(Span<int> instanceIds, bool activate = false)
        {
            var length = instanceIds.Length;

            Checks.IsTrue(length > 0, "\"instanceIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedInstanceIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentTransformIds(Span<int> transformIds, bool activate = false)
        {
            var length = transformIds.Length;

            Checks.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var startIndex = UnusedCount - length;
            var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedInstanceIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(instanceIds, true);
            }
        }

        public void RentTransforms(int amount, [NotNull] FasterList<Transform> transforms, bool activate = false)
        {
            Checks.IsTrue(amount > 0, "\"amount\" must be greater than 0");

            Prepool(amount - UnusedCount);

            var startIndex = UnusedCount - amount;
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var instanceIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedInstanceIds.CopyTo(startIndex, instanceIds);

            _unusedInstanceIds.RemoveRange(startIndex, amount);
            _unusedTransformIds.RemoveRange(startIndex, amount);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<int> instanceIds, ReadOnlySpan<int> transformIds)
            => Return(instanceIds, transformIds, default);

        public void Return(
              ReadOnlySpan<int> instanceIds
            , ReadOnlySpan<int> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            var length = instanceIds.Length;

            Checks.IsTrue(length == transformIds.Length, "arrays do not have the same size");

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

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(instanceIds, false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnInstanceIds(ReadOnlySpan<int> instanceIds)
            => ReturnInstanceIds(instanceIds, default);

        public void ReturnInstanceIds(ReadOnlySpan<int> instanceIds, PooledGameObjectStrategy pooledStrategy)
        {
            var length = instanceIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            ResourceAPI.InstanceIDToObjectList(instanceIds, _objectList);

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

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnTransformIds(ReadOnlySpan<int> transformIds)
            => ReturnTransformIds(transformIds, default);

        public void ReturnTransformIds(ReadOnlySpan<int> transformIds, PooledGameObjectStrategy pooledStrategy)
        {
            var length = transformIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            ResourceAPI.InstanceIDToObjectList(transformIds, _objectList);

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

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, true);
            }
        }

        public void Dispose()
        {
            _source = null;
            _scene = default;
            _unusedInstanceIds.Clear();
            _unusedTransformIds.Clear();
            _objectList.Clear();
        }
    }
}
