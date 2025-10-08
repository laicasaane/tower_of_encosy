#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling.Native
{
    using BurstHint = Unity.Burst.CompilerServices.Hint;

#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

    public struct NativeGameObjectPool : IDisposable
    {
        private readonly float3 _defaultPosition;
        private readonly float3 _defaultScale;
        private readonly quaternion _defaultRotation;
        private readonly EntityId _prefabId;

        private TransformAccessArray _transformArray;
        private NativeList<float3> _positions;
        private NativeList<float3> _scales;
        private NativeList<quaternion> _rotations;

        private NativeList<GameObjectId> _unusedGameObjectIds;
        private NativeList<TransformId> _unusedTransformIds;
        private NativeList<int> _unusedTransformArrayIndices;

        public NativeGameObjectPool(
              EntityId prefabId
            , int capacity
            , in float3 defaultPosition
            , in quaternion defaultRotation
            , in float3 defaultScale
            , Option<Scene> rentScene = default
            , Option<Scene> returnScene = default
        )
        {
            _prefabId = prefabId;
            _defaultPosition = defaultPosition;
            _defaultRotation = defaultRotation;
            _defaultScale = defaultScale;

            RentScene = rentScene;
            ReturnScene = returnScene;

            capacity = math.max(capacity, 4);

            TransformAccessArray.Allocate(capacity, -1, out _transformArray);
            _positions = new(capacity, Allocator.Persistent);
            _rotations = new(capacity, Allocator.Persistent);
            _scales = new(capacity, Allocator.Persistent);

            _unusedGameObjectIds = new(capacity, Allocator.Persistent);
            _unusedTransformIds = new(capacity, Allocator.Persistent);
            _unusedTransformArrayIndices = new(capacity, Allocator.Persistent);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _transformArray.isCreated;
        }

        public readonly NativeArray<float3> Positions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _positions.AsArray();
        }

        public readonly NativeArray<quaternion> Rotations
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rotations.AsArray();
        }

        public readonly NativeArray<float3> Scales
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _scales.AsArray();
        }

        public readonly ScheduleOnlyTransformArray TransformArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_transformArray);
        }

        public Option<Scene> RentScene { get; set; }

        public Option<Scene> ReturnScene { get; set; }

        public void Dispose()
        {
            _transformArray.Dispose();
            _positions.Dispose();
            _rotations.Dispose();
            _scales.Dispose();

            _unusedGameObjectIds.Dispose();
            _unusedTransformIds.Dispose();
            _unusedTransformArrayIndices.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepool(int amount)
            => Prepool(amount, NativeReturningOptions.Everything);

        public void Prepool(int amount, NativeReturningOptions options)
        {
            if (amount < 1)
            {
                return;
            }

            IncreaseCapacityBy(amount);

            var gameObjectIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            GameObject.InstantiateGameObjects(_prefabId, amount, gameObjectIds, transformIds);

            if (options.Contains(NativeReturningOptions.Deactivate))
            {
                GameObject.SetGameObjectsActive(gameObjectIds, false);
            }

            _unusedGameObjectIds.AddRange(gameObjectIds.Reinterpret<GameObjectId>());
            _unusedTransformIds.AddRange(transformIds.Reinterpret<TransformId>());

            _positions.AddReplicate(_defaultPosition, amount);
            _rotations.AddReplicate(_defaultRotation, amount);
            _scales.AddReplicate(_defaultScale, amount);

            var transformArray = _transformArray;
            var transformArrayIndices = _unusedTransformArrayIndices;

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];
                var arrayIndex = transformArray.length;

                transformArray.Add(transformId);
                transformArrayIndices.Add(arrayIndex);
            }

            if (options.Contains(NativeReturningOptions.MoveToScene) && ReturnScene.TryGetValue(out var scene))
            {
                SceneManager.MoveGameObjectsToScene(gameObjectIds, scene);
            }
        }

        public bool Rent(int amount, NativeList<GameObjectInfo> result, NativeRentingOptions options)
        {
            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            if (BurstHint.Unlikely(result.IsCreated == false))
            {
                return false;
            }

            result.ResizeUninitialized(amount);
            RentInternal(result.AsArray().Slice(), options);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(NativeArray<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(result.Slice(), options);

        public bool Rent(NativeSlice<GameObjectInfo> result, NativeRentingOptions options)
        {
            var amount = result.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            RentInternal(result, options);

            return true;
        }

        public bool Rent(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            RentInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds.Reinterpret<int>()
            );

            return true;
        }

        public bool Rent(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , Span<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            RentInternal(
                  NativeArray.CreateFrom(gameObjectIds, Allocator.Temp)
                , NativeArray.CreateFrom(transformIds, Allocator.Temp)
                , NativeArray.CreateFrom(arrayIndices, Allocator.Temp)
                , options
                , default
            );

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeList<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items.AsArray().Slice(), options);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeArray<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items.Slice(), options);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(ReadOnlySpan<GameObjectInfo> items, NativeReturningOptions options)
            => Return(NativeArray.CreateFrom(items, Allocator.Temp).Slice(), options);

        public bool Return(NativeSlice<GameObjectInfo> items, NativeReturningOptions options)
        {
            var amount = items.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            var itemSlice = items.Slice();
            var gameObjectIds = itemSlice.SliceWithStride<GameObjectId>(GameObjectInfo.OFFSET_GAMEOBJECT_ID);
            var transformIds = itemSlice.SliceWithStride<TransformId>(GameObjectInfo.OFFSET_TRANSFORM_ID);
            var arrayIndices = itemSlice.SliceWithStride<int>(GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX);

            return Return(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
            );
        }

        public bool Return(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                return false;
            }

            if (BurstHint.Unlikely(gameObjectIds.Length < 1))
            {
                return false;
            }

            ReturnInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds.Reinterpret<int>()
            );

            return true;
        }

        public bool Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReadOnlySpan<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                return false;
            }

            var returnedGameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var returnedTransformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);
            var returnedArrayIndices = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            gameObjectIds.CopyTo(returnedGameObjectIds.AsSpan());
            transformIds.CopyTo(returnedTransformIds.AsSpan());
            arrayIndices.CopyTo(returnedArrayIndices.AsSpan());

            ReturnInternal(
                  returnedGameObjectIds
                , returnedTransformIds
                , returnedArrayIndices
                , options
                , returnedGameObjectIds.Reinterpret<int>()
            );

            return true;
        }

        public bool Return(
              NativeSlice<GameObjectId> gameObjectIds
            , NativeSlice<TransformId> transformIds
            , NativeSlice<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                return false;
            }

            if (BurstHint.Unlikely(gameObjectIds.Length < 1))
            {
                return false;
            }

            ReturnInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , default
            );

            return true;
        }

        private void IncreaseCapacityBy(int amount)
        {
            var capacity = _transformArray.length + amount;

            if (_transformArray.capacity < capacity)
            {
                _transformArray.capacity = capacity;
            }

            if (_unusedGameObjectIds.Capacity < capacity)
            {
                _unusedGameObjectIds.Capacity = capacity;
            }

            if (_unusedTransformIds.Capacity < capacity)
            {
                _unusedTransformIds.Capacity = capacity;
            }

            if (_unusedTransformArrayIndices.Capacity < capacity)
            {
                _unusedTransformArrayIndices.Capacity = capacity;
            }

            if (_positions.Capacity < capacity)
            {
                _positions.Capacity = capacity;
            }

            if (_rotations.Capacity < capacity)
            {
                _rotations.Capacity = capacity;
            }

            if (_scales.Capacity < capacity)
            {
                _scales.Capacity = capacity;
            }
        }

        private void RentInternal(NativeSlice<GameObjectInfo> result, NativeRentingOptions options)
        {
            var slice = result.Slice();
            var gameObjectIds = slice.SliceWithStride<GameObjectId>(GameObjectInfo.OFFSET_GAMEOBJECT_ID);
            var transformIds = slice.SliceWithStride<TransformId>(GameObjectInfo.OFFSET_TRANSFORM_ID);
            var arrayIndices = slice.SliceWithStride<int>(GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX);

            RentInternal(gameObjectIds, transformIds, arrayIndices, options, default);
        }

        private void RentInternal(
              NativeSlice<GameObjectId> gameObjectIds
            , NativeSlice<TransformId> transformIds
            , NativeSlice<int> arrayIndices
            , NativeRentingOptions options
            , NativeArray<int> gameObjectIdsToMove
        )
        {
            var amount = gameObjectIds.Length;

            Prepool(amount - _unusedGameObjectIds.Length);

            var startIndex = _unusedGameObjectIds.Length - amount;
            var unusedGameObjectIds = _unusedGameObjectIds.AsArray();
            var unusedTransformIds = _unusedTransformIds.AsArray();
            var unusedArrayIndices = _unusedTransformArrayIndices.AsArray();

            gameObjectIds.CopyFrom(unusedGameObjectIds.Slice(startIndex, amount));
            transformIds.CopyFrom(unusedTransformIds.Slice(startIndex, amount));
            arrayIndices.CopyFrom(unusedArrayIndices.Slice(startIndex, amount));

            if (gameObjectIdsToMove.IsCreated == false)
            {
                var ids = unusedGameObjectIds.Reinterpret<int>().Slice(startIndex, amount);
                gameObjectIdsToMove = NativeArray.CreateFrom(ids, Allocator.Temp);
            }

            _unusedGameObjectIds.RemoveRange(startIndex, amount);
            _unusedTransformIds.RemoveRange(startIndex, amount);
            _unusedTransformArrayIndices.RemoveRange(startIndex, amount);

            if (options.Contains(NativeRentingOptions.Activate))
            {
                GameObject.SetGameObjectsActive(gameObjectIdsToMove, true);
            }

            if (options.Contains(NativeRentingOptions.MoveToScene) == false
                || RentScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove, scene);
        }

        private void ReturnInternal(
              NativeSlice<GameObjectId> gameObjectIds
            , NativeSlice<TransformId> transformIds
            , NativeSlice<int> arrayIndices
            , NativeReturningOptions options
            , NativeArray<int> gameObjectIdsToMove
        )
        {
            var amount = gameObjectIds.Length;
            var startIndex = _unusedGameObjectIds.Length;
            var newLength = startIndex + amount;

            _unusedGameObjectIds.ResizeUninitialized(newLength);
            _unusedTransformIds.ResizeUninitialized(newLength);
            _unusedTransformArrayIndices.ResizeUninitialized(newLength);

            var unusedGameObjectIds = _unusedGameObjectIds.AsArray();
            var unusedTransformIds = _unusedTransformIds.AsArray();
            var unusedArrayIndices = _unusedTransformArrayIndices.AsArray();

            unusedGameObjectIds.Slice(startIndex, amount).CopyFrom(gameObjectIds);
            unusedTransformIds.Slice(startIndex, amount).CopyFrom(transformIds);
            unusedArrayIndices.Slice(startIndex, amount).CopyFrom(arrayIndices);

            if (gameObjectIdsToMove.IsCreated == false)
            {
                var ids = unusedGameObjectIds.Reinterpret<int>().AsSpan().Slice(startIndex, amount);
                gameObjectIdsToMove = NativeArray.CreateFrom(ids, Allocator.Temp);
            }

            if (options.Contains(NativeReturningOptions.Deactivate))
            {
                GameObject.SetGameObjectsActive(gameObjectIdsToMove, false);
            }

            if (options.Contains(NativeReturningOptions.MoveToScene) == false
                || ReturnScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove, scene);
        }
    }
}

#endif
