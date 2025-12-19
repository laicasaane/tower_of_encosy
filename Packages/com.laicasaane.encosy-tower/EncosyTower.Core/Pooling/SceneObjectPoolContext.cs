#if UNITY_MATHEMATICS

using System;
using System.Diagnostics;
using EncosyTower.Collections;
using EncosyTower.Debugging;
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

    internal sealed class SceneObjectPoolContext : IDisposable
    {
        internal NativeHashMap<TransformId, GameObjectInfo> _goInfoMap;
        internal TransformAccessArray _transformArray;
        internal NativeList<float3> _positions;
        internal NativeList<float3> _scales;
        internal NativeList<quaternion> _rotations;

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void AssertInitialization(SceneObjectPoolContext context)
        {
            const string MESSAGE = "SceneObjectPoolBehaviour must be initialized first!";

            Checks.IsTrue(context._transformArray.isCreated, MESSAGE);
            Checks.IsTrue(context._goInfoMap.IsCreated, MESSAGE);
            Checks.IsTrue(context._positions.IsCreated, MESSAGE);
            Checks.IsTrue(context._scales.IsCreated, MESSAGE);
            Checks.IsTrue(context._rotations.IsCreated, MESSAGE);
        }

        public void Initialize(int capacity, int desiredJobCount)
        {
            TransformAccessArray.Allocate(capacity, desiredJobCount, out _transformArray);
            _goInfoMap = new(capacity, Allocator.Persistent);
            _positions = new(capacity, Allocator.Persistent);
            _scales = new(capacity, Allocator.Persistent);
            _rotations = new(capacity, Allocator.Persistent);
        }

        public void IncreaseCapacityBy(int amount)
        {
            var transformArray = _transformArray;
            var capacity = transformArray.length + amount;

            if (capacity <= transformArray.capacity)
            {
                return;
            }

            transformArray.capacity = capacity;
            _goInfoMap.IncreaseCapacityTo(capacity);
            _positions.IncreaseCapacityTo(capacity);
            _scales.IncreaseCapacityTo(capacity);
            _rotations.IncreaseCapacityTo(capacity);
        }

        public void Dispose()
        {
            if (_goInfoMap.IsCreated)
            {
                _goInfoMap.Dispose();
            }

            _goInfoMap = default;

            if (_transformArray.isCreated)
            {
                _transformArray.Dispose();
            }

            _transformArray = default;

            if (_positions.IsCreated)
            {
                _positions.Dispose();
            }

            _positions = default;

            if (_scales.IsCreated)
            {
                _scales.Dispose();
            }

            _scales = default;

            if (_rotations.IsCreated)
            {
                _rotations.Dispose();
            }

            _rotations = default;
        }

        public void CreateDefaultGameObject(Scene scene, bool hideDefaultGameObject, Type behaviourType)
        {
            var defaultGo = new GameObject($"<default-gameobject-{behaviourType.Name.ToLower()}>");

#if UNITY_6000_2_OR_NEWER
            GameObjectId gameObjectId = defaultGo;
            TransformId transformId = defaultGo.transform;
#else
            GameObjectId gameObjectId = defaultGo;
            TransformId transformId = defaultGo.transform;
#endif

            defaultGo.SetActive(false);
            defaultGo.hideFlags = HideFlags.DontSave;

            SceneManager.MoveGameObjectToScene(defaultGo, scene);

            _transformArray.Add((EntityId)transformId);
            _positions.Add(Vector3.zero);
            _scales.Add(Vector3.one);
            _rotations.Add(quaternion.identity);
            _goInfoMap.Add(transformId, new GameObjectInfo {
                gameObjectId = gameObjectId,
                transformId = transformId,
            });

            if (hideDefaultGameObject)
            {
                defaultGo.hideFlags |= HideFlags.HideInHierarchy;
            }
        }

        public void Rent(
              SceneObjectPool pool
            , in float3 defaultPosition
            , in float3 defaultRotation
            , in float3 defaultScale
            , Span<GameObject> result
            , RentingStrategy strategy
        )
        {
            var amount = result.Length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            pool.Rent(result, gameObjectIds, transformIds, strategy);

            var transformArray = _transformArray;
            var goInfoMap = _goInfoMap;
            var positions = _positions;
            var scales = _scales;
            var rotations = _rotations;
            var rotation = (quaternion)Quaternion.Euler(defaultRotation);
            var newInfoList = new NativeList<(int resultIndex, GameObjectInfo)>(amount, Allocator.Temp);

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];

                if (goInfoMap.TryGetValue(transformId, out var goInfo))
                {
                    var transformArrayIndex = goInfo.transformArrayIndex;
                    positions[transformArrayIndex] = defaultPosition;
                    scales[transformArrayIndex] = defaultScale;
                    rotations[transformArrayIndex] = rotation;

                    var transform = transformArray[transformArrayIndex];
                    transform.SetLocalPositionAndRotation(defaultPosition, rotation);
                    transform.localScale = defaultScale;
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
                var (_, goInfo) = newInfoList[i];
                var transformId = goInfo.transformId;
                var transformArrayIndex = goInfo.transformArrayIndex = transformArray.length;

                transformArray.Add((EntityId)transformId);
                transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, rotation);

                goInfoMap.Add(transformId, goInfo);
            }

            if (newInfoLength > 0)
            {
                positions.AddReplicate(defaultPosition, newInfoLength);
                scales.AddReplicate(defaultScale, newInfoLength);
                rotations.AddReplicate(rotation, newInfoLength);
            }
        }

        public void Rent(
              SceneObjectPool pool
            , in float3 defaultPosition
            , in float3 defaultRotation
            , in float3 defaultScale
            , Span<GameObjectInfo> result
            , RentingStrategy strategy
        )
        {
            var amount = result.Length;
            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);

            pool.Rent(gameObjectIds, transformIds, strategy);

            var transformArray = _transformArray;
            var goInfoMap = _goInfoMap;
            var positions = _positions;
            var scales = _scales;
            var rotations = _rotations;
            var rotation = (quaternion)Quaternion.Euler(defaultRotation);
            var newInfoList = new NativeList<(int resultIndex, GameObjectInfo)>(amount, Allocator.Temp);

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];

                if (goInfoMap.TryGetValue(transformId, out var goInfo))
                {
                    var transformArrayIndex = goInfo.transformArrayIndex;
                    result[i] = goInfo;

                    positions[transformArrayIndex] = defaultPosition;
                    scales[transformArrayIndex] = defaultScale;
                    rotations[transformArrayIndex] = rotation;

                    var transform = transformArray[transformArrayIndex];
                    transform.SetLocalPositionAndRotation(defaultPosition, rotation);
                    transform.localScale = defaultScale;
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
                transformArray[transformArrayIndex].SetPositionAndRotation(defaultPosition, rotation);

                goInfoMap.Add(transformId, goInfo);
                result[resultIndex] = goInfo;
            }

            if (newInfoLength > 0)
            {
                positions.AddReplicate(defaultPosition, newInfoLength);
                scales.AddReplicate(defaultScale, newInfoLength);
                rotations.AddReplicate(rotation, newInfoLength);
            }
        }
    }
}

#endif
