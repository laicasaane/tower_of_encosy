#if UNITY_MATHEMATICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Debugging;
using EncosyTower.Loaders;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    public abstract partial class SceneObjectIdManager<TKey, TId> : MonoBehaviour
        where TKey : ITryLoadAsync<GameObject>
        where TId : unmanaged, IEquatable<TId>
    {
        [SerializeField]
        private Vector3 _defaultPosition;

        private readonly Dictionary<TId, SceneObjectIdPool> _poolMap = new();
        private NativeHashMap<TransformId, GameObjectInfo> _goInfoMap;
        private TransformAccessArray _transformArray;
        private NativeList<float3> _positions;

        public TransformAccessArray TransformArray => _transformArray;

        public NativeList<float3> Positions => _positions;

        public virtual bool TrimCloneSuffix => false;

        protected abstract IReadOnlyCollection<PoolEntry<TId, TKey>> GetEntries();

        protected abstract int EstimateCapacity(int poolCount);

        private void CreateDefaultGameObject(Scene scene)
        {
            var defaultGo = new GameObject("<default>");
            var instanceId = defaultGo.GetInstanceID();
            var transformId = defaultGo.transform.GetInstanceID();
            defaultGo.SetActive(false);

            SceneManager.MoveGameObjectToScene(defaultGo, scene);

            _transformArray.Add(transformId);
            _positions.Add(default);
            _goInfoMap.Add(transformId, new GameObjectInfo {
                instanceId = instanceId,
                transformId = transformId,
            });
        }

        private void OnDestroy()
        {
            foreach (var pool in _poolMap.Values)
            {
                pool.Dispose();
            }

            _poolMap.Clear();
            DisposeNativeCollections();
        }

        private void DisposeNativeCollections()
        {
            _poolMap.Clear();

            if (_transformArray.isCreated)
            {
                _transformArray.Dispose();
            }

            _transformArray = default;

            if (_goInfoMap.IsCreated)
            {
                _goInfoMap.Dispose();
            }

            _goInfoMap = default;

            if (_positions.IsCreated)
            {
                _positions.Dispose();
            }

            _positions = default;
        }

        private void IncreaseCapacityBy(int amount)
        {
            var transformArray = _transformArray;
            var capacity = transformArray.length + amount;

            if (capacity <= transformArray.capacity)
            {
                return;
            }

            transformArray.capacity = capacity;
            _goInfoMap.Capacity = capacity;
            _positions.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepool(TId id, int amount)
            => Prepool(id, amount, default);

        public void Prepool(TId id, int amount, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Prepool(amount, pooledStrategy);

            IncreaseCapacityBy(amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepool(ReadOnlySpan<TId> ids, int amount)
            => Prepool(ids, amount, default);

        public void Prepool(ReadOnlySpan<TId> ids, int amount, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            var count = 0;

            foreach (var id in ids)
            {
                if (_poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.Prepool(amount, pooledStrategy);
                count += 1;
            }

            if (count > 0)
            {
                IncreaseCapacityBy(count * amount);
            }
        }

        public bool Rent(TId id, int amount, ref NativeList<GameObjectInfo> result)
        {
            AssertInitialization(this);

            if (result.IsCreated == false)
            {
                return false;
            }

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return false;
            }

            var instanceIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            pool.Rent(instanceIds, transformIds, true);

            var transformArray = _transformArray;
            var goInfoMap = _goInfoMap;
            var positions = _positions;
            var defaultPosition = _defaultPosition;
            var defaultRotation = quaternion.identity;
            var newTransforms = new NativeList<GameObjectInfo>(amount, Allocator.Temp);

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];

                if (goInfoMap.TryGetValue(transformId, out var goInfo))
                {
                    var transformArrayIndex = goInfo.transformArrayIndex;
                    result.Add(goInfo);
                    positions[transformArrayIndex] = defaultPosition;
                    transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, defaultRotation);
                }
                else
                {
                    newTransforms.Add(new GameObjectInfo {
                        instanceId = instanceIds[i],
                        transformId = transformId,
                    });
                }
            }

            var newTransformsLength = newTransforms.Length;

            for (var i = 0; i < newTransformsLength; i++)
            {
                var transform = newTransforms[i];
                var transformId = transform.transformId;
                var index = transform.transformArrayIndex = transformArray.length;

                transformArray.Add(transformId);
                transformArray[index].SetPositionAndRotation(defaultPosition, defaultRotation);

                goInfoMap.Add(transformId, transform);
                result.Add(transform);
            }

            if (newTransformsLength > 0)
            {
                _positions.AddReplicate(defaultPosition, newTransformsLength);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(TId id, ReadOnlySpan<int> instanceIds, ReadOnlySpan<int> transformIds)
            => Return(id, instanceIds, transformIds, default);

        public void Return(
              TId id
            , ReadOnlySpan<int> instanceIds
            , ReadOnlySpan<int> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Return(instanceIds, transformIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<int> instanceIds
            , ReadOnlySpan<int> transformIds
        )
        {
            Return(idToRangeMap, instanceIds, transformIds, default);
        }

        public void Return(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<int> instanceIds
            , ReadOnlySpan<int> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (id, range) in idToRangeMap)
            {
                var (start, length) = range.GetOffsetAndLength(instanceIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.Return(
                      instanceIds.Slice(start, length)
                    , transformIds.Slice(start, length)
                    , pooledStrategy
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnInstanceIds(TId id, ReadOnlySpan<int> instanceIds)
            => ReturnInstanceIds(id, instanceIds, default);

        public void ReturnInstanceIds(TId id, ReadOnlySpan<int> instanceIds, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.ReturnInstanceIds(instanceIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnInstanceIds(NativeHashMap<TId, Range> idToRangeMap, ReadOnlySpan<int> instanceIds)
            => ReturnInstanceIds(idToRangeMap, instanceIds, PooledGameObjectStrategy.Default);

        public void ReturnInstanceIds(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<int> instanceIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (id, range) in idToRangeMap)
            {
                var (start, length) = range.GetOffsetAndLength(instanceIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.ReturnInstanceIds(
                      instanceIds.Slice(start, length)
                    , pooledStrategy
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnTransformIds(TId id, ReadOnlySpan<int> transformIds)
            => ReturnTransformIds(id, transformIds, default);

        public void ReturnTransformIds(TId id, ReadOnlySpan<int> transformIds, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.ReturnTransformIds(transformIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnTransformIds(NativeHashMap<TId, Range> idToRangeMap, ReadOnlySpan<int> transformIds)
            => Return(idToRangeMap, transformIds, default);

        public void ReturnTransformIds(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<int> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (id, range) in idToRangeMap)
            {
                var (start, length) = range.GetOffsetAndLength(transformIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.ReturnTransformIds(
                      transformIds.Slice(start, length)
                    , pooledStrategy
                );
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void AssertInitialization(SceneObjectIdManager<TKey, TId> pooler)
        {
            Checks.IsTrue(pooler._transformArray.isCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._goInfoMap.IsCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._positions.IsCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._poolMap.Count > 0, "Pooler must be initialized first!");
        }
    }
}

#endif
