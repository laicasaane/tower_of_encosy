#if UNITY_MATHEMATICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EncosyTower.Collections;
using EncosyTower.Debugging;
using EncosyTower.Loaders;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

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

#if UNITY_6000_2_OR_NEWER
            GameObjectId gameObjectId = defaultGo;
            TransformId transformId = defaultGo.transform;
#else
            GameObjectId gameObjectId = defaultGo;
            TransformId transformId = defaultGo.transform;
#endif

            defaultGo.SetActive(false);

            SceneManager.MoveGameObjectToScene(defaultGo, scene);

            _transformArray.Add((int)transformId);
            _positions.Add(default);
            _goInfoMap.Add(transformId, new GameObjectInfo {
                gameObjectId = gameObjectId,
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

        public void Prepool(TId id, int amount, ReturningStrategy strategy)
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

            pool.Prepool(amount, strategy);

            IncreaseCapacityBy(amount);
        }

        public void Prepool(ReadOnlySpan<TId> ids, int amount, ReturningStrategy strategy)
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

                pool.Prepool(amount, strategy);
                count += 1;
            }

            if (count > 0)
            {
                IncreaseCapacityBy(count * amount);
            }
        }

        public bool Rent(TId id, int amount, ref NativeList<GameObjectInfo> result, RentingStrategy strategy)
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

            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            pool.Rent(gameObjectIds, transformIds, strategy);

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
                        gameObjectId = gameObjectIds[i],
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

                transformArray.Add((int)transformId);
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

        public void Return(
              TId id
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Return(gameObjectIds, transformIds, strategy);
        }

        public void Return(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (id, range) in idToRangeMap)
            {
                var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.Return(
                      gameObjectIds.Slice(start, length)
                    , transformIds.Slice(start, length)
                    , strategy
                );
            }
        }

        public void Return(
              TId id
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Return(gameObjectIds, strategy);
        }

        public void Return(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (id, range) in idToRangeMap)
            {
                var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(id, out var pool) == false)
                {
                    continue;
                }

                pool.Return(
                      gameObjectIds.Slice(start, length)
                    , strategy
                );
            }
        }

        public void Return(
              TId id
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(id, out var pool) == false)
            {
                return;
            }

            pool.Return(transformIds, strategy);
        }

        public void Return(
              NativeHashMap<TId, Range> idToRangeMap
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
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

                pool.Return(
                      transformIds.Slice(start, length)
                    , strategy
                );
            }
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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
