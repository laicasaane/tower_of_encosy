using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

    public sealed class SceneObjectIdPool : IDisposable
    {
        private readonly FasterList<int> _unusedGameObjectIds;
        private readonly FasterList<int> _unusedTransformIds;
        private readonly List<UnityObject> _objectList;

        private Scene _scene;
        private GameObject _source;

        public SceneObjectIdPool()
        {
            _unusedGameObjectIds = new(32);
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

            var gameObjectIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                  _source.GetEntityId()
#else
                  _source.GetInstanceID()
#endif
                , amount
                , gameObjectIds
                , transformIds
                , _scene
            );

            if (TrimCloneSuffix)
            {
                TrimCloneSuffixFrom(gameObjectIds, _objectList);
            }

            _unusedGameObjectIds.IncreaseCapacityBy(amount);
            _unusedTransformIds.IncreaseCapacityBy(amount);

            _unusedGameObjectIds.AddRange(gameObjectIds.AsReadOnlySpan());
            _unusedTransformIds.AddRange(transformIds.AsReadOnlySpan());

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(gameObjectIds, false);
            }

            return true;

            static void TrimCloneSuffixFrom(NativeArray<int> gameObjectIds, List<UnityObject> objectList)
            {
                if (gameObjectIds.IsCreated == false || gameObjectIds.Length < 1)
                {
                    return;
                }

                objectList.Clear();
                objectList.Capacity = Mathf.Max(objectList.Capacity, gameObjectIds.Length);

                Resources.InstanceIDToObjectList(gameObjectIds, objectList);

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

            var unusedGameObjectIds = _unusedGameObjectIds.AsSpan()[keep..];
            var length = unusedGameObjectIds.Length;
            var gameObjectIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
            unusedGameObjectIds.CopyTo(gameObjectIds);

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            Resources.InstanceIDToObjectList(gameObjectIds, _objectList);

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

            _unusedGameObjectIds.RemoveRange(keep, removeCount);
            _unusedTransformIds.RemoveRange(keep, removeCount);
        }

        public GameObjectId RentGameObjectId(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var result = _unusedGameObjectIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (activate)
            {
                var ids = NativeArray.CreateFast<int>(1, Allocator.Temp);
                ids[0] = result;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return (GameObjectId)result;
        }

        public TransformId RentTransformId(bool activate = false)
        {
            if (UnusedCount < 1)
            {
                Prepool(1);
            }

            var last = UnusedCount - 1;
            var id = _unusedGameObjectIds[last];
            var result = _unusedTransformIds[last];

            _unusedGameObjectIds.RemoveAt(last);
            _unusedTransformIds.RemoveAt(last);

            if (activate)
            {
                var ids = NativeArray.CreateFast<int>(1, Allocator.Temp);
                ids[0] = id;

                GameObject.SetGameObjectsActive(ids, true);
            }

            return (TransformId)result;
        }

        public void Rent(Span<GameObjectId> gameObjectIds, Span<TransformId> transformIds, bool activate = false)
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

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, true);
            }
        }

        public void Rent(Span<GameObjectId> gameObjectIds, bool activate = false)
        {
            var length = gameObjectIds.Length;

            Checks.IsTrue(length > 0, "\"gameObjectIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var startIndex = UnusedCount - length;
            _unusedGameObjectIds.CopyTo(startIndex, reGameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, true);
            }
        }

        public void Rent(Span<TransformId> transformIds, bool activate = false)
        {
            var length = transformIds.Length;

            Checks.IsTrue(length > 0, "\"transformIds\" array does not have enough space to contain the items");

            Prepool(length - UnusedCount);

            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var startIndex = UnusedCount - length;
            var gameObjectIds = NativeArray.CreateFast<int>(length, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, reTransformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, length);
            _unusedTransformIds.RemoveRange(startIndex, length);

            if (activate)
            {
                GameObject.SetGameObjectsActive(gameObjectIds, true);
            }
        }

        public void Rent(int amount, [NotNull] FasterList<Transform> transforms, bool activate = false)
        {
            Checks.IsTrue(amount > 0, "\"amount\" must be greater than 0");

            Prepool(amount - UnusedCount);

            var startIndex = UnusedCount - amount;
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var gameObjectIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            _unusedTransformIds.CopyTo(startIndex, transformIds);
            _unusedGameObjectIds.CopyTo(startIndex, gameObjectIds);

            _unusedGameObjectIds.RemoveRange(startIndex, amount);
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
                GameObject.SetGameObjectsActive(gameObjectIds, true);
            }

            _objectList.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReadOnlySpan<TransformId> transformIds)
            => Return(gameObjectIds, transformIds, default);

        public void Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , PooledGameObjectStrategy pooledStrategy
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
            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
            unusedTransformIds.IncreaseCapacityTo(capacity);

            unusedGameObjectIds.AddRange(reGameObjectIds);
            unusedTransformIds.AddRange(reTransformIds);

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(reGameObjectIds, false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds)
            => Return(gameObjectIds, PooledGameObjectStrategy.Default);

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, PooledGameObjectStrategy pooledStrategy)
        {
            var length = gameObjectIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            var reGameObjectIds = MemoryMarshal.Cast<GameObjectId, int>(gameObjectIds);
            ResourceAPI.InstanceIDToObjectList(reGameObjectIds, _objectList);

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
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

                unusedGameObjectIds.Add(postIds[postIdsLength++] = reGameObjectIds[i]);

#if UNITY_6000_2_OR_NEWER
                unusedTransformIds.Add(go.transform.GetEntityId());
#else
                unusedTransformIds.Add(go.transform.GetInstanceID());
#endif
            }

            _objectList.Clear();

            postIds = postIds.GetSubArray(0, postIdsLength);

            if (new PooledGameObjectOperation(PooledStrategy, pooledStrategy).ShouldDeactivate())
            {
                GameObject.SetGameObjectsActive(postIds, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<TransformId> transformIds)
            => Return(transformIds, PooledGameObjectStrategy.Default);

        public void Return(ReadOnlySpan<TransformId> transformIds, PooledGameObjectStrategy pooledStrategy)
        {
            var length = transformIds.Length;

            if (length < 1)
            {
                return;
            }

            _objectList.Clear();
            _objectList.Capacity = Mathf.Max(_objectList.Capacity, length);

            var reTransformIds = MemoryMarshal.Cast<TransformId, int>(transformIds);
            ResourceAPI.InstanceIDToObjectList(reTransformIds, _objectList);

            var unusedGameObjectIds = _unusedGameObjectIds;
            var unusedTransformIds = _unusedTransformIds;
            var capacity = unusedGameObjectIds.Count + length;

            unusedGameObjectIds.IncreaseCapacityTo(capacity);
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

#if UNITY_6000_2_OR_NEWER
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetEntityId());
#else
                unusedGameObjectIds.Add(postIds[postIdsLength++] = obj.GetInstanceID());
#endif

                unusedTransformIds.Add(reTransformIds[i]);
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
            _unusedGameObjectIds.Clear();
            _unusedTransformIds.Clear();
            _objectList.Clear();
        }
    }
}
