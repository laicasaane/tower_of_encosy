#if UNITY_MATHEMATICS

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

    public abstract partial class SceneObjectIdManager<TKey> : MonoBehaviour
        where TKey : ITryLoadAsync<GameObject>
    {
        [SerializeField]
        private Vector3 _defaultPosition;

        private SceneObjectIdPool _pool;
        private NativeHashMap<TransformId, GameObjectInfo> _goInfoMap;
        private TransformAccessArray _transformArray;
        private NativeList<float3> _positions;

        public TransformAccessArray TransformArray => _transformArray;

        public NativeList<float3> Positions => _positions;

        public virtual bool TrimCloneSuffix => false;

        protected SceneObjectIdPool Pool => _pool;

        protected abstract TKey GetKey();

        protected abstract int EstimateCapacity();

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
            _pool?.Dispose();
            DisposeNativeCollections();
        }

        private void DisposeNativeCollections()
        {
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
        public void Prepool(int amount)
            => Prepool(amount, PooledGameObjectStrategy.Default);

        public void Prepool(int amount, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            if (_pool == null)
            {
                return;
            }

            _pool.Prepool(amount, pooledStrategy);

            IncreaseCapacityBy(amount);
        }

        public bool Rent(int amount, ref NativeList<GameObjectInfo> result)
        {
            AssertInitialization(this);

            if (result.IsCreated == false)
            {
                return false;
            }

            if (_pool == null)
            {
                return false;
            }

            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            _pool.Rent(gameObjectIds, transformIds, true);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReadOnlySpan<TransformId> transformIds)
            => Return(gameObjectIds, transformIds, PooledGameObjectStrategy.Default);

        public void Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(gameObjectIds, transformIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Range range, ReadOnlySpan<GameObjectId> gameObjectIds, ReadOnlySpan<TransformId> transformIds)
            => Return(range, gameObjectIds, transformIds, default);

        public void Return(
              Range range
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

            if (length < 1)
            {
                return;
            }

            if (_pool == null)
            {
                return;
            }

            _pool.Return(
                  gameObjectIds.Slice(start, length)
                , transformIds.Slice(start, length)
                , pooledStrategy
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds)
            => Return(gameObjectIds, PooledGameObjectStrategy.Default);

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(gameObjectIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Range range, ReadOnlySpan<GameObjectId> gameObjectIds)
            => Return(range, gameObjectIds, PooledGameObjectStrategy.Default);

        public void Return(
              Range range
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

            if (length < 1)
            {
                return;
            }

            if (_pool == null)
            {
                return;
            }

            _pool.Return(
                  gameObjectIds.Slice(start, length)
                , pooledStrategy
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<TransformId> transformIds)
            => Return(transformIds, PooledGameObjectStrategy.Default);

        public void Return(ReadOnlySpan<TransformId> transformIds, PooledGameObjectStrategy pooledStrategy)
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(transformIds, pooledStrategy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Range range, ReadOnlySpan<TransformId> transformIds)
            => Return(range, transformIds, PooledGameObjectStrategy.Default);

        public void Return(
              Range range
            , ReadOnlySpan<TransformId> transformIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            AssertInitialization(this);

            var (start, length) = range.GetOffsetAndLength(transformIds.Length);

            if (length < 1)
            {
                return;
            }

            if (_pool == null)
            {
                return;
            }

            _pool.Return(
                  transformIds.Slice(start, length)
                , pooledStrategy
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void AssertInitialization(SceneObjectIdManager<TKey> pooler)
        {
            Checks.IsTrue(pooler._transformArray.isCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._goInfoMap.IsCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._positions.IsCreated, "Pooler must be initialized first!");
            Checks.IsTrue(pooler._pool != null, "Pooler must be initialized first!");
        }
    }
}

#endif
