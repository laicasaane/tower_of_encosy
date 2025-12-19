#if UNITY_MATHEMATICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
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

    public partial class SceneObjectPoolBehaviour<TKey, TKeyComparer> : MonoBehaviour, IDisposable
#if !(UNITASK || UNITY_6000_0_OR_NEWER)
        where TKey : ITryLoad<GameObject>
        where TKeyComparer : IEqualityComparer<TKey>, new()
#endif
    {
        private SceneObjectPoolContext _context;
        private Dictionary<TKey, PoolRecord> _poolMap;

        public TransformAccessArray TransformArray => _context._transformArray;

        public NativeList<float3> Positions => _context._positions;

        public NativeList<float3> Scales => _context._scales;

        public NativeList<quaternion> Rotations => _context._rotations;

        protected virtual bool TrimCloneSuffix => false;

        protected virtual bool HideDefaultGameObject => false;

        protected void Initialize([NotNull] IReadOnlyCollection<Entry> entries, int desiredJobCount = -1)
        {
            Dispose();

            var poolMap = _poolMap = new(entries.Count, new TKeyComparer());
            var trimCloneSuffix = TrimCloneSuffix;
            var initialCapacity = 1u;
            Option<Scene> firstScene = Option.None;

            foreach (var (key, scene, entryCapacity, position, rotation, scale) in entries)
            {
                if (poolMap.ContainsKey(key))
                {
                    continue;
                }

                var prefabOpt = key.TryLoad();

                if (prefabOpt.TryGetValue(out var prefab) == false)
                {
                    continue;
                }

                if (firstScene.HasValue == false)
                {
                    firstScene = scene;
                }

                initialCapacity += entryCapacity;

                var pool = new SceneObjectPool(entryCapacity) {
                    Scene = scene,
                    Source = prefab,
                    TrimCloneSuffix = trimCloneSuffix,
                };

                AddToMap(poolMap, key, pool, position, rotation, scale);
            }

            var defaultScene = firstScene.GetValueOrDefault(gameObject.scene);

            _context = new SceneObjectPoolContext();
            _context.Initialize((int)initialCapacity, desiredJobCount);
            _context.CreateDefaultGameObject(defaultScene, HideDefaultGameObject, GetType());

            OnInitialize();
        }

        protected bool Register(
              [NotNull] TKey key
            , Scene scene
            , uint initialCapacity
            , Option<Vector3> defaultPosition = default
            , Option<Vector3> defaultRotation = default
            , Option<Vector3> defaultScale = default
        )
        {
            AssertInitialization(this);

            if (_poolMap.ContainsKey(key))
            {
                return false;
            }

            var prefabOpt = key.TryLoad();

            if (prefabOpt.TryGetValue(out var prefab) == false)
            {
                return false;
            }

            var pool = new SceneObjectPool(initialCapacity) {
                Scene = scene,
                Source = prefab,
                TrimCloneSuffix = TrimCloneSuffix,
            };

            AddToMap(_poolMap, key, pool, defaultPosition, defaultRotation, defaultScale);

            _context.IncreaseCapacityBy((int)initialCapacity);
            return true;
        }

        public void Dispose()
        {
            if (_poolMap != null)
            {
                foreach (var record in _poolMap.Values)
                {
                    record.Pool.Dispose();
                }

                _poolMap.Clear();
            }

            _poolMap = null;
            _context?.Dispose();
            _context = null;
        }

        private void IncreaseCapacityBy(int amount)
        {
            _context.IncreaseCapacityBy(amount);
        }

        public void Prepool([NotNull] TKey key, int amount, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            record.Pool.Prepool(amount, strategy);

            IncreaseCapacityBy(amount);
        }

        public void Prepool(ReadOnlySpan<TKey> keys, int amount, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            var count = 0;

            foreach (var key in keys)
            {
                if (_poolMap.TryGetValue(key, out var record) == false)
                {
                    continue;
                }

                record.Pool.Prepool(amount, strategy);
                count += 1;
            }

            if (count > 0)
            {
                IncreaseCapacityBy(count * amount);
            }
        }

        public GameObject RentGameObject([NotNull] TKey key, RentingStrategy strategy)
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return null;
            }

            var (pool, position, rotation, scale) = record;

            unsafe
            {
                Span<GameObjectInfo> result = stackalloc GameObjectInfo[1];
                _context.Rent(pool, position, rotation, scale, result, strategy);
                return result[0].gameObjectId.ToObject().GetValueOrThrow();
            }
        }

        public bool Rent([NotNull] TKey key, int amount, [NotNull] List<GameObject> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return false;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return false;
            }

            var (pool, position, rotation, scale) = record;
            var span = result.AsListFast().AddReplicateNoInit(amount);
            _context.Rent(pool, position, rotation, scale, span, strategy);
            return true;
        }

        public bool Rent([NotNull] TKey key, int amount, [NotNull] List<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return false;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return false;
            }

            var (pool, position, rotation, scale) = record;
            var span = result.AsListFast().AddReplicateNoInit(amount);
            _context.Rent(pool, position, rotation, scale, span, strategy);
            return true;
        }

        public bool Rent([NotNull] TKey key, int amount, ref NativeList<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            if (result.IsCreated == false)
            {
                return false;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return false;
            }

            var (pool, position, rotation, scale) = record;
            var span = result.InsertRangeSpan(result.Length, amount);
            _context.Rent(pool, position, rotation, scale, span, strategy);
            return true;
        }

        public bool Rent([NotNull] TKey key, Span<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            var amount = result.Length;

            if (amount < 1)
            {
                return false;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return false;
            }

            var (pool, position, rotation, scale) = record;
            _context.Rent(pool, position, rotation, scale, result, strategy);
            return true;
        }

        public void Return([NotNull] TKey key, GameObject gameObject, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (gameObject == false)
            {
                return;
            }

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            unsafe
            {
                Span<GameObjectId> gameObjectIds = stackalloc GameObjectId[1];
                Span<TransformId> transformIds = stackalloc TransformId[1];

#if UNITY_6000_2_OR_NEWER
                gameObjectIds[0] = gameObject.GetEntityId();
                transformIds[0] = gameObject.transform.GetEntityId();
#else
                gameObjectIds[0] = gameObject.GetInstanceID();
                transformIds[0] = gameObject.transform.GetInstanceID();
#endif

                record.Pool.Return(gameObjectIds, transformIds, strategy);
            }
        }

        public void Return([NotNull] TKey key, ReadOnlySpan<GameObject> gameObjects, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            record.Pool.Return(gameObjects, strategy);
        }

        public void Return(
              [NotNull] TKey key
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            record.Pool.Return(gameObjectIds, transformIds, strategy);
        }

        public void Return(
              ReadOnlySpan<(TKey, Range)> keyRanges
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (key, range) in keyRanges)
            {
                var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(key, out var record) == false)
                {
                    continue;
                }

                record.Pool.Return(
                      gameObjectIds.Slice(start, length)
                    , transformIds.Slice(start, length)
                    , strategy
                );
            }
        }

        public void Return(
              [NotNull] TKey key
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            record.Pool.Return(gameObjectIds, strategy);
        }

        public void Return(
              ReadOnlySpan<(TKey, Range)> keyRanges
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (key, range) in keyRanges)
            {
                var (start, length) = range.GetOffsetAndLength(gameObjectIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(key, out var record) == false)
                {
                    continue;
                }

                record.Pool.Return(
                      gameObjectIds.Slice(start, length)
                    , strategy
                );
            }
        }

        public void Return(
              [NotNull] TKey key
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            if (_poolMap.TryGetValue(key, out var record) == false)
            {
                return;
            }

            record.Pool.Return(transformIds, strategy);
        }

        public void Return(
              ReadOnlySpan<(TKey, Range)> keyRanges
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

            var poolMap = _poolMap;

            foreach (var (key, range) in keyRanges)
            {
                var (start, length) = range.GetOffsetAndLength(transformIds.Length);

                if (length < 1)
                {
                    continue;
                }

                if (poolMap.TryGetValue(key, out var record) == false)
                {
                    continue;
                }

                record.Pool.Return(
                      transformIds.Slice(start, length)
                    , strategy
                );
            }
        }

        protected virtual void OnInitialize() { }

        protected void OnDestroy()
        {
            Dispose();
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void AssertInitialization(SceneObjectPoolBehaviour<TKey, TKeyComparer> behaviour)
        {
            const string MESSAGE = "SceneObjectPoolBehaviour must be initialized first!";

            Checks.IsTrue(behaviour._poolMap != null, MESSAGE);
            Checks.IsTrue(behaviour._context != null, MESSAGE);
            SceneObjectPoolContext.AssertInitialization(behaviour._context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddToMap(
              Dictionary<TKey, PoolRecord> poolMap
            , TKey key
            , SceneObjectPool pool
            , Option<Vector3> defaultPosition
            , Option<Vector3> defaultRotation
            , Option<Vector3> defaultScale
        )
        {
            poolMap[key] = new PoolRecord(
                  pool
                , defaultPosition.GetValueOrDefault(float3.zero)
                , defaultRotation.GetValueOrDefault(float3.zero)
                , defaultScale.GetValueOrDefault(new float3(1f))
            );
        }
    }
}

#endif
