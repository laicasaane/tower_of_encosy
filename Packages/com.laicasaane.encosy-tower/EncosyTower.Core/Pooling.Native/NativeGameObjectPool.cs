#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
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

#if UNITY_6000_3_OR_NEWER
    using EntityId = UnityEngine.EntityId;
#else
    using EntityId = System.Int32;
#endif

    public struct NativeGameObjectPool : IDisposable
    {
        public static readonly Allocator Allocator = Allocator.Persistent;

        private NativeReference<NativePrefabInfo> _prefab;

        private TransformAccessArray _transformArray;
        private NativeList<float3> _positions;
        private NativeList<float3> _scales;
        private NativeList<quaternion> _rotations;

        private NativeList<GameObjectId> _unusedGameObjectIds;
        private NativeList<TransformId> _unusedTransformIds;
        private NativeList<int> _unusedTransformIndices;

        public NativeGameObjectPool(in NativePrefabInfo prefab, int initialCapacity = 4, int desiredJobCount = -1)
        {
            initialCapacity = math.max(initialCapacity, 4);
            desiredJobCount = math.select(-1, desiredJobCount, desiredJobCount >= 0);

            _prefab = new(Allocator, NativeArrayOptions.UninitializedMemory) {
                Value = prefab
            };

            TransformAccessArray.Allocate(initialCapacity, desiredJobCount, out _transformArray);
            _positions = new(initialCapacity, Allocator);
            _rotations = new(initialCapacity, Allocator);
            _scales = new(initialCapacity, Allocator);

            _unusedGameObjectIds = new(initialCapacity, Allocator);
            _unusedTransformIds = new(initialCapacity, Allocator);
            _unusedTransformIndices = new(initialCapacity, Allocator);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _prefab.IsCreated;
        }

        public readonly NativePrefabInfo Prefab
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _prefab.Value;
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

        public void Dispose()
        {
            if (IsCreated)
            {
                _prefab.Dispose();
                _transformArray.Dispose();
                _positions.Dispose();
                _rotations.Dispose();
                _scales.Dispose();

                _unusedGameObjectIds.Dispose();
                _unusedTransformIds.Dispose();
                _unusedTransformIndices.Dispose();
            }

            _prefab = default;
            _transformArray = default;
            _positions = default;
            _rotations = default;
            _scales = default;

            _unusedGameObjectIds = default;
            _unusedTransformIds = default;
            _unusedTransformIndices = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(int amount)
            => Prepool(amount, NativeReturningOptions.Everything, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(int amount, out NativeRentingError error)
            => Prepool(amount, NativeReturningOptions.Everything, out error);

        public bool Prepool(int amount, NativeReturningOptions options, out NativeRentingError error)
        {
            if (amount < 1)
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            IncreaseCapacityBy(amount);

            var gameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);
            var prefab = Prefab;

            GameObject.InstantiateGameObjects(
                  prefab.EntityId
                , amount
                , gameObjectIds.Reinterpret<EntityId>()
                , transformIds.Reinterpret<EntityId>()
            );

            if (options.Contains(NativeReturningOptions.Deactivate))
            {
                GameObject.SetGameObjectsActive(gameObjectIds.Reinterpret<EntityId>(), false);
            }

            _unusedGameObjectIds.AddRange(gameObjectIds.Reinterpret<GameObjectId>());
            _unusedTransformIds.AddRange(transformIds.Reinterpret<TransformId>());

            _positions.AddReplicate(prefab.Position, amount);
            _rotations.AddReplicate(prefab.Rotation, amount);
            _scales.AddReplicate(prefab.Scale, amount);

            var transformArray = _transformArray;
            var unusedTransformIndices = _unusedTransformIndices;

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];
                var arrayIndex = transformArray.length;

                transformArray.Add(transformId);
                unusedTransformIndices.Add(arrayIndex);
            }

            if (options.Contains(NativeReturningOptions.MoveToScene)
                && prefab.ReturnScene.TryGetValue(out var scene)
            )
            {
                SceneManager.MoveGameObjectsToScene(gameObjectIds.Reinterpret<EntityId>(), scene);
            }

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(int amount, NativeList<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(amount, result, options, out _);

        public bool Rent(
              int amount
            , NativeList<GameObjectInfo> result
            , NativeRentingOptions options
            , out NativeRentingError error
        )
        {
            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            if (BurstHint.Unlikely(result.IsCreated == false))
            {
                error = NativeRentingError.ResultListMustBeCreatedInAdvance;
                return false;
            }

            result.ResizeUninitialized(amount);
            RentInternal(result.AsArray().Slice(), options);

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(in NativeArray<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(result, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(in NativeArray<GameObjectInfo> result, NativeRentingOptions options, out NativeRentingError error)
            => Rent(result.Slice(), options, out error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(in NativeSlice<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(result, options, out _);

        public bool Rent(in NativeSlice<GameObjectInfo> result, NativeRentingOptions options, out NativeRentingError error)
        {
            var amount = result.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            RentInternal(result, options);

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            return Rent(gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Rent(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeRentingOptions options
            , out NativeRentingError error
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                error = NativeRentingError.ArraysMustHaveSameLength;
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            RentInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , Span<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            return Rent(gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Rent(
              Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , Span<int> arrayIndices
            , NativeRentingOptions options
            , out NativeRentingError error
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                error = NativeRentingError.ArraysMustHaveSameLength;
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            RentInternal(
                  NativeArray.CreateFrom(gameObjectIds, Allocator.Temp)
                , NativeArray.CreateFrom(transformIds, Allocator.Temp)
                , NativeArray.CreateFrom(arrayIndices, Allocator.Temp)
                , options
                , default
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeList<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeList<GameObjectInfo> items, NativeReturningOptions options, out NativeReturningError error)
            => Return(items.AsArray().Slice(), options, out error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeArray<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(NativeArray<GameObjectInfo> items, NativeReturningOptions options, out NativeReturningError error)
            => Return(items.Slice(), options, out error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(ReadOnlySpan<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(ReadOnlySpan<GameObjectInfo> items, NativeReturningOptions options, out NativeReturningError error)
            => Return(NativeArray.CreateFrom(items, Allocator.Temp).Slice(), options, out error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(in NativeSlice<GameObjectInfo> items, NativeReturningOptions options)
            => Return(items, options, out _);

        public bool Return(
              in NativeSlice<GameObjectInfo> items
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            var amount = items.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeReturningError.ArraysMustContainAnyElement;
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
                , out error
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                error = NativeReturningError.ArraysMustHaveSameLength;
                return false;
            }

            if (BurstHint.Unlikely(gameObjectIds.Length < 1))
            {
                error = NativeReturningError.ArraysMustContainAnyElement;
                return false;
            }

            ReturnInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds
            );

            error = NativeReturningError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReadOnlySpan<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReadOnlySpan<int> arrayIndices
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                error = NativeReturningError.ArraysMustHaveSameLength;
                return false;
            }

            var amount = gameObjectIds.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeReturningError.ArraysMustContainAnyElement;
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
                , returnedGameObjectIds
            );

            error = NativeReturningError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            if (BurstHint.Unlikely(
                gameObjectIds.Length != transformIds.Length || transformIds.Length != arrayIndices.Length
            ))
            {
                error = NativeReturningError.ArraysMustHaveSameLength;
                return false;
            }

            if (BurstHint.Unlikely(gameObjectIds.Length < 1))
            {
                error = NativeReturningError.ArraysMustContainAnyElement;
                return false;
            }

            ReturnInternal(
                  gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , default
            );

            error = NativeReturningError.None;
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

            if (_unusedTransformIndices.Capacity < capacity)
            {
                _unusedTransformIndices.Capacity = capacity;
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

        private void RentInternal(in NativeSlice<GameObjectInfo> result, NativeRentingOptions options)
        {
            var slice = result.Slice();
            var gameObjectIds = slice.SliceWithStride<GameObjectId>(GameObjectInfo.OFFSET_GAMEOBJECT_ID);
            var transformIds = slice.SliceWithStride<TransformId>(GameObjectInfo.OFFSET_TRANSFORM_ID);
            var arrayIndices = slice.SliceWithStride<int>(GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX);

            RentInternal(gameObjectIds, transformIds, arrayIndices, options, default);
        }

        private void RentInternal(
              in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeRentingOptions options
            , NativeArray<GameObjectId> gameObjectIdsToMove
        )
        {
            var amount = gameObjectIds.Length;

            Prepool(amount - _unusedGameObjectIds.Length);

            var startIndex = _unusedGameObjectIds.Length - amount;
            var unusedGameObjectIds = _unusedGameObjectIds.AsArray();
            var unusedTransformIds = _unusedTransformIds.AsArray();
            var unusedArrayIndices = _unusedTransformIndices.AsArray();

            gameObjectIds.CopyFrom(unusedGameObjectIds.Slice(startIndex, amount));
            transformIds.CopyFrom(unusedTransformIds.Slice(startIndex, amount));
            arrayIndices.CopyFrom(unusedArrayIndices.Slice(startIndex, amount));

            if (gameObjectIdsToMove.IsCreated == false)
            {
                var ids = unusedGameObjectIds.Slice(startIndex, amount);
                gameObjectIdsToMove = NativeArray.CreateFrom(ids, Allocator.Temp);
            }

            _unusedGameObjectIds.RemoveRange(startIndex, amount);
            _unusedTransformIds.RemoveRange(startIndex, amount);
            _unusedTransformIndices.RemoveRange(startIndex, amount);

            if (options.Contains(NativeRentingOptions.Activate))
            {
                GameObject.SetGameObjectsActive(gameObjectIdsToMove.Reinterpret<EntityId>(), true);
            }

            if (options.Contains(NativeRentingOptions.MoveToScene) == false
                || Prefab.RentScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove.Reinterpret<EntityId>(), scene);
        }

        private void ReturnInternal(
              in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeReturningOptions options
            , NativeArray<GameObjectId> gameObjectIdsToMove
        )
        {
            var amount = gameObjectIds.Length;
            var startIndex = _unusedGameObjectIds.Length;
            var newLength = startIndex + amount;

            _unusedGameObjectIds.ResizeUninitialized(newLength);
            _unusedTransformIds.ResizeUninitialized(newLength);
            _unusedTransformIndices.ResizeUninitialized(newLength);

            var unusedGameObjectIds = _unusedGameObjectIds.AsArray();
            var unusedTransformIds = _unusedTransformIds.AsArray();
            var unusedArrayIndices = _unusedTransformIndices.AsArray();

            unusedGameObjectIds.Slice(startIndex, amount).CopyFrom(gameObjectIds);
            unusedTransformIds.Slice(startIndex, amount).CopyFrom(transformIds);
            unusedArrayIndices.Slice(startIndex, amount).CopyFrom(arrayIndices);

            if (gameObjectIdsToMove.IsCreated == false)
            {
                var ids = unusedGameObjectIds.AsSpan().Slice(startIndex, amount);
                gameObjectIdsToMove = NativeArray.CreateFrom(ids, Allocator.Temp);
            }

            if (options.Contains(NativeReturningOptions.Deactivate))
            {
                GameObject.SetGameObjectsActive(gameObjectIdsToMove.Reinterpret<EntityId>(), false);
            }

            if (options.Contains(NativeReturningOptions.MoveToScene) == false
                || Prefab.ReturnScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove.Reinterpret<EntityId>(), scene);
        }
    }
}

#endif
