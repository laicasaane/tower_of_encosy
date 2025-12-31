#if UNITY_MATHEMATICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
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

    public partial class SceneObjectPoolBehaviour<TKey> : MonoBehaviour, IDisposable
#if !(UNITASK || UNITY_6000_0_OR_NEWER)
        where TKey : ITryLoad<GameObject>
#endif
    {
        [SerializeField] internal Vector3 _defaultPosition;
        [SerializeField] internal Vector3 _defaultRotation;
        [SerializeField] internal Vector3 _defaultScale = Vector3.one;

        private SceneObjectPoolContext _context;
        private SceneObjectPool _pool;

        public TransformAccessArray TransformArray => _context._transformArray;

        public NativeList<float3> Positions => _context._positions;

        public NativeList<float3> Scales => _context._scales;

        public NativeList<quaternion> Rotations => _context._rotations;

        protected virtual bool TrimCloneSuffix => false;

        protected virtual bool HideDefaultGameObject => false;

        protected SceneObjectPool Pool => _pool;

        protected void Initialize(
              [NotNull] TKey key
            , Scene scene
            , uint initialCapacity
            , int desiredJobCount = -1
        )
        {
            Dispose();

            var prefabOpt = key.TryLoad();

            if (prefabOpt.TryGetValue(out var prefab) == false)
            {
                return;
            }

            initialCapacity += 1;

            _pool = new SceneObjectPool(initialCapacity) {
                Scene = scene,
                Source = prefab,
                TrimCloneSuffix = TrimCloneSuffix,
            };

            _context = new SceneObjectPoolContext();
            _context.Initialize((int)initialCapacity, desiredJobCount);
            _context.CreateDefaultGameObject(scene, HideDefaultGameObject, GetType());

            OnInitialize();
        }

        public void Dispose()
        {
            _pool?.Dispose();
            _pool = null;
            _context?.Dispose();
            _context = null;
        }

        private void IncreaseCapacityBy(int amount)
        {
            _context.IncreaseCapacityBy(amount);
        }

        public void Prepool(int amount, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (amount < 1)
            {
                return;
            }

            _pool.Prepool(amount, strategy);

            IncreaseCapacityBy(amount);
        }

        /// <summary>
        /// Prepool the difference between <paramref name="estimatedAmount"/> and <see cref="SceneObjectPool.UnusedCount"/>.
        /// </summary>
        /// <param name="estimatedAmount">The estimated amount to be prepooled.</param>
        /// <returns>The actual amount to be prepooled. The value is either a positive number or zero.</returns>
        public int PrepoolDifference(int estimatedAmount, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            var diffAmount = _pool.PrepoolDifference(estimatedAmount, strategy);

            if (diffAmount > 0)
            {
                IncreaseCapacityBy(diffAmount);
            }

            return diffAmount;
        }

        public GameObject RentGameObject(RentingStrategy strategy)
        {
            AssertInitialization(this);

            unsafe
            {
                Span<GameObjectInfo> result = stackalloc GameObjectInfo[1];
                _context.Rent(_pool, _defaultPosition, _defaultRotation, _defaultScale, result, strategy);
                return result[0].gameObjectId.ToObject().GetValueOrThrow();
            }
        }

        public bool Rent(int amount, [NotNull] List<GameObject> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            var span = result.AsListFast().AddReplicateNoInit(amount);
            _context.Rent(_pool, _defaultPosition, _defaultRotation, _defaultScale, span, strategy);
            return true;
        }

        public bool Rent(int amount, [NotNull] List<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            var span = result.AsListFast().AddReplicateNoInit(amount);
            _context.Rent(_pool, _defaultPosition, _defaultRotation, _defaultScale, span, strategy);
            return true;
        }

        public bool Rent(int amount, ref NativeList<GameObjectInfo> result, RentingStrategy strategy)
        {
            AssertInitialization(this);

            if (result.IsCreated == false)
            {
                return false;
            }

            var span = result.InsertRangeSpan(result.Length, amount);
            _context.Rent(_pool, _defaultPosition, _defaultRotation, _defaultScale, span, strategy);
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

            _context.Rent(_pool, _defaultPosition, _defaultRotation, _defaultScale, result, strategy);
            return true;
        }

        public void Return(GameObject gameObject, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            if (gameObject == false)
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

                _pool.Return(gameObjectIds, transformIds, strategy);
            }
        }

        public void Return(ReadOnlySpan<GameObject> gameObjects, ReturningStrategy strategy)
        {
            AssertInitialization(this);

            _pool.Return(gameObjects, strategy);
        }

        public void Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReturningStrategy strategy
        )
        {
            AssertInitialization(this);

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

            _pool.Return(
                  gameObjectIds.Slice(start, length)
                , transformIds.Slice(start, length)
                , strategy
            );
        }

        public void Return(ReadOnlySpan<GameObjectId> gameObjectIds, ReturningStrategy strategy)
        {
            AssertInitialization(this);

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

            _pool.Return(
                  gameObjectIds.Slice(start, length)
                , strategy
            );
        }

        public void Return(ReadOnlySpan<TransformId> transformIds, ReturningStrategy strategy)
        {
            AssertInitialization(this);

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

            _pool.Return(
                  transformIds.Slice(start, length)
                , strategy
            );
        }

        protected virtual void OnInitialize() { }

        protected void OnDestroy()
        {
            Dispose();
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void AssertInitialization(SceneObjectPoolBehaviour<TKey> behaviour)
        {
            const string MESSAGE = "SceneObjectPoolBehaviour must be initialized first!";

            Checks.IsTrue(behaviour._pool != null, MESSAGE);
            Checks.IsTrue(behaviour._context != null, MESSAGE);
            SceneObjectPoolContext.AssertInitialization(behaviour._context);
        }
    }
}

#endif
