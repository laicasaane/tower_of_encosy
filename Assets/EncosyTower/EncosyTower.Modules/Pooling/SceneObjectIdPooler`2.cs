#if UNITY_MATHEMATICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EncosyTower.Modules.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    public abstract partial class SceneObjectIdPooler<TKey, TId> : MonoBehaviour
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

        public void Prepool(TId id, int amount)
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

            pool.Prepool(amount);
            IncreaseCapacityBy(amount);
        }

        public void Prepool(ReadOnlySpan<TId> ids, int amount)
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

                pool.Prepool(amount);
                count += 1;
            }

            if (count > 0)
            {
                IncreaseCapacityBy(count * amount);
            }
        }

        public bool RentTransforms(
              TId id
            , int amount
            , ref NativeList<GameObjectInfo> transforms
        )
        {
            AssertInitialization(this);

            if (transforms.IsCreated == false)
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
                    transforms.Add(goInfo);
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
                transforms.Add(transform);
            }

            if (newTransformsLength > 0)
            {
                _positions.AddReplicate(defaultPosition, newTransformsLength);
            }

            return true;
        }

        public void ReturnTransforms(
              TId id
            , NativeArray<int> instanceIds
            , NativeArray<int> transformIds
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Return(instanceIds, transformIds);
        }

        public void ReturnTransforms(
              NativeHashMap<TId, Range> idToRangeMap
            , NativeArray<int> instanceIds
            , NativeArray<int> transformIds
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

                pool.Return(instanceIds.GetSubArray(start, length), transformIds.GetSubArray(start, length));
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void AssertInitialization(SceneObjectIdPooler<TKey, TId> pooler)
        {
            Assert.IsTrue(pooler._transformArray.isCreated, "Pooler must be initialized first!");
            Assert.IsTrue(pooler._goInfoMap.IsCreated, "Pooler must be initialized first!");
            Assert.IsTrue(pooler._positions.IsCreated, "Pooler must be initialized first!");
            Assert.IsTrue(pooler._poolMap.Count > 0, "Pooler must be initialized first!");
        }
    }
}

#endif
