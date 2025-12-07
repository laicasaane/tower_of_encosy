#if UNITY_MATHEMATICS

using System;
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

#if UNITY_6000_3_OR_NEWER
    using EntityId = UnityEngine.EntityId;
#else
    using EntityId = System.Int32;
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

            _transformArray.Add((EntityId)transformId);
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

        public void Prepool(int amount, ReturningStrategy strategy)
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

            _pool.Prepool(amount, strategy);

            IncreaseCapacityBy(amount);
        }

        public bool Rent(int amount, ref NativeList<GameObjectInfo> result, RentingStrategy strategy)
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

            _pool.Rent(gameObjectIds, transformIds, strategy);

            var transformArray = _transformArray;
            var goInfoMap = _goInfoMap;
            var positions = _positions;
            var defaultPosition = _defaultPosition;
            var defaultRotation = quaternion.identity;
            var newInfoList = new NativeList<GameObjectInfo>(amount, Allocator.Temp);

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
                    newInfoList.Add(new GameObjectInfo {
                        gameObjectId = gameObjectIds[i],
                        transformId = transformId,
                    });
                }
            }

            var newInfoLength = newInfoList.Length;

            for (var i = 0; i < newInfoLength; i++)
            {
                var goInfo = newInfoList[i];
                var transformId = goInfo.transformId;
                var transformArrayIndex = goInfo.transformArrayIndex = transformArray.length;

                transformArray.Add((EntityId)transformId);
                transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, defaultRotation);

                goInfoMap.Add(transformId, goInfo);
                result.Add(goInfo);
            }

            if (newInfoLength > 0)
            {
                _positions.AddReplicate(defaultPosition, newInfoLength);
            }

            return true;
        }

        public bool Rent(Span<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            var amount = result.Length;

            if (amount < 1)
            {
                return false;
            }

            if (_pool == null)
            {
                return false;
            }

            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            _pool.Rent(gameObjectIds, transformIds, strategy);

            var transformArray = _transformArray;
            var goInfoMap = _goInfoMap;
            var positions = _positions;
            var defaultPosition = _defaultPosition;
            var defaultRotation = quaternion.identity;
            var newInfoList = new NativeList<(int resultIndex, GameObjectInfo)>(amount, Allocator.Temp);

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];

                if (goInfoMap.TryGetValue(transformId, out var goInfo))
                {
                    var transformArrayIndex = goInfo.transformArrayIndex;
                    result[i] = goInfo;
                    positions[transformArrayIndex] = defaultPosition;
                    transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, defaultRotation);
                }
                else
                {
                    newInfoList.Add((i, new GameObjectInfo {
                        gameObjectId = gameObjectIds[i],
                        transformId = transformId,
                    }));
                }
            }

            var newInfoLength = newInfoList.Length;

            for (var i = 0; i < newInfoLength; i++)
            {
                var (resultIndex, goInfo) = newInfoList[i];
                var transformId = goInfo.transformId;
                var transformArrayIndex = goInfo.transformArrayIndex = transformArray.length;

                transformArray.Add((EntityId)transformId);
                transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, defaultRotation);

                goInfoMap.Add(transformId, goInfo);
                result[resultIndex] = goInfo;
            }

            if (newInfoLength > 0)
            {
                _positions.AddReplicate(defaultPosition, newInfoLength);
            }

            return true;
        }

        public void Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(gameObjectIds, transformIds, strategy);
        }

        public void Return(
              Range range
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
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
                , strategy
            );
        }

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(gameObjectIds, strategy);
        }

        public void Return(
              Range range
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReturningStrategy strategy
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
                , strategy
            );
        }

        public void Return(ReadOnlySpan<TransformId> transformIds, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (_pool == null)
            {
                return;
            }

            _pool.Return(transformIds, strategy);
        }

        public void Return(
              Range range
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
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
                , strategy
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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
