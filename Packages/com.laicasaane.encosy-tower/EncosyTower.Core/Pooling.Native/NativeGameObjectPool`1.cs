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

    public struct NativeGameObjectPool<TKey> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
    {
        public static readonly Allocator Allocator = Allocator.Persistent;

        private NativeHashMap<TKey, NativePrefabInfo> _prefabMap;

        private TransformAccessArray _transformArray;
        private NativeList<float3> _positions;
        private NativeList<float3> _scales;
        private NativeList<quaternion> _rotations;

        private NativeHashMap<TKey, NativeList<GameObjectId>> _unusedGameObjectIdsMap;
        private NativeHashMap<TKey, NativeList<TransformId>> _unusedTransformIdsMap;
        private NativeHashMap<TKey, NativeList<int>> _unusedTransformIndicesMap;
        private readonly int _initialCapacity;

        public NativeGameObjectPool(int initialCapacity = 4, int desiredJobCount = -1)
        {
            _initialCapacity = initialCapacity = math.max(initialCapacity, 4);
            desiredJobCount = math.select(-1, desiredJobCount, desiredJobCount >= 0);

            _prefabMap = new(initialCapacity, Allocator);

            TransformAccessArray.Allocate(initialCapacity, desiredJobCount, out _transformArray);
            _positions = new(initialCapacity, Allocator);
            _rotations = new(initialCapacity, Allocator);
            _scales = new(initialCapacity, Allocator);

            _unusedGameObjectIdsMap = new(1, Allocator);
            _unusedTransformIdsMap = new(1, Allocator);
            _unusedTransformIndicesMap = new(1, Allocator);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _prefabMap.IsCreated;
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
                _prefabMap.Dispose();
                _transformArray.Dispose();
                _positions.Dispose();
                _rotations.Dispose();
                _scales.Dispose();

                foreach (var (_, list) in _unusedGameObjectIdsMap)
                {
                    list.Dispose();
                }

                foreach (var (_, list) in _unusedTransformIdsMap)
                {
                    list.Dispose();
                }

                foreach (var (_, list) in _unusedTransformIndicesMap)
                {
                    list.Dispose();
                }

                _unusedGameObjectIdsMap.Dispose();
                _unusedTransformIdsMap.Dispose();
                _unusedTransformIndicesMap.Dispose();
            }

            _prefabMap = default;
            _transformArray = default;
            _positions = default;
            _rotations = default;
            _scales = default;
            _unusedGameObjectIdsMap = default;
            _unusedTransformIdsMap = default;
            _unusedTransformIndicesMap = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddPrefab(TKey key, NativePrefabInfo prefab)
        {
            if (_prefabMap.TryAdd(key, prefab) == false)
            {
                return false;
            }

            _unusedGameObjectIdsMap.Add(key, new NativeList<GameObjectId>(_initialCapacity, Allocator));
            _unusedTransformIdsMap.Add(key, new NativeList<TransformId>(_initialCapacity, Allocator));
            _unusedTransformIndicesMap.Add(key, new NativeList<int>(_initialCapacity, Allocator));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPrefab(TKey key, out NativePrefabInfo prefab)
            => _prefabMap.TryGetValue(key, out prefab);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(TKey key, int amount)
            => Prepool(key, amount, NativeReturningOptions.Everything, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Prepool(TKey key, int amount, out NativeRentingError error)
            => Prepool(key, amount, NativeReturningOptions.Everything, out error);

        public bool Prepool(TKey key, int amount, NativeReturningOptions options, out NativeRentingError error)
        {
            if (amount < 1)
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeRentingError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            PrepoolInternal(
                  key
                , prefab
                , amount
                , options
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(TKey key, int amount, NativeList<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(key, amount, result, options, out _);

        public bool Rent(
              TKey key
            , int amount
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeRentingError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            result.ResizeUninitialized(amount);
            RentInternal(
                  key
                , prefab
                , result.AsArray().Slice()
                , options
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(TKey key, in NativeArray<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(key, result, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(
              TKey key
            , in NativeArray<GameObjectInfo> result
            , NativeRentingOptions options
            , out NativeRentingError error
        )
        {
            return Rent(key, result.Slice(), options, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(TKey key, in NativeSlice<GameObjectInfo> result, NativeRentingOptions options)
            => Rent(key, result, options, out _);

        public bool Rent(
              TKey key
            , in NativeSlice<GameObjectInfo> result
            , NativeRentingOptions options
            , out NativeRentingError error
        )
        {
            var amount = result.Length;

            if (BurstHint.Unlikely(amount < 1))
            {
                error = NativeRentingError.AmountMustBeGreaterThanZero;
                return false;
            }

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeRentingError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            RentInternal(
                  key
                , prefab
                , result
                , options
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(
              TKey key
            , NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            return Rent(key, gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Rent(
              TKey key
            , NativeArray<GameObjectId> gameObjectIds
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeRentingError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            RentInternal(
                  key
                , prefab
                , gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Rent(
              TKey key
            , Span<GameObjectId> gameObjectIds
            , Span<TransformId> transformIds
            , Span<int> arrayIndices
            , NativeRentingOptions options
        )
        {
            return Rent(key, gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Rent(
              TKey key
            , Span<GameObjectId> gameObjectIds
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeRentingError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            RentInternal(
                  key
                , prefab
                , NativeArray.CreateFrom(gameObjectIds, Allocator.Temp)
                , NativeArray.CreateFrom(transformIds, Allocator.Temp)
                , NativeArray.CreateFrom(arrayIndices, Allocator.Temp)
                , options
                , default
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeRentingError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(TKey key, NativeList<GameObjectInfo> items, NativeReturningOptions options)
            => Return(key, items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , NativeList<GameObjectInfo> items
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            return Return(key, items.AsArray().Slice(), options, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(TKey key, NativeArray<GameObjectInfo> items, NativeReturningOptions options)
            => Return(key, items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , NativeArray<GameObjectInfo> items
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            return Return(key, items.Slice(), options, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(TKey key, ReadOnlySpan<GameObjectInfo> items, NativeReturningOptions options)
            => Return(key, items, options, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , ReadOnlySpan<GameObjectInfo> items
            , NativeReturningOptions options
            , out NativeReturningError error
        )
        {
            return Return(key, NativeArray.CreateFrom(items, Allocator.Temp).Slice(), options, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(TKey key, in NativeSlice<GameObjectInfo> items, NativeReturningOptions options)
            => Return(key, items, options, out _);

        public bool Return(
              TKey key
            , in NativeSlice<GameObjectInfo> items
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
                  key
                , gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , out error
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , NativeArray<GameObjectId> gameObjectIds
            , NativeArray<TransformId> transformIds
            , NativeArray<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(key, gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              TKey key
            , NativeArray<GameObjectId> gameObjectIds
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeReturningError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            ReturnInternal(
                  prefab
                , gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , gameObjectIds
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeReturningError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , ReadOnlySpan<GameObjectId> gameObjectIds
            , ReadOnlySpan<TransformId> transformIds
            , ReadOnlySpan<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(key, gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              TKey key
            , ReadOnlySpan<GameObjectId> gameObjectIds
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeReturningError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            var returnedGameObjectIds = NativeArray.CreateFast<GameObjectId>(amount, Allocator.Temp);
            var returnedTransformIds = NativeArray.CreateFast<TransformId>(amount, Allocator.Temp);
            var returnedArrayIndices = NativeArray.CreateFast<int>(amount, Allocator.Temp);

            gameObjectIds.CopyTo(returnedGameObjectIds.AsSpan());
            transformIds.CopyTo(returnedTransformIds.AsSpan());
            arrayIndices.CopyTo(returnedArrayIndices.AsSpan());

            ReturnInternal(
                  prefab
                , returnedGameObjectIds
                , returnedTransformIds
                , returnedArrayIndices
                , options
                , returnedGameObjectIds
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeReturningError.None;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(
              TKey key
            , in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeReturningOptions options
        )
        {
            return Return(key, gameObjectIds, transformIds, arrayIndices, options, out _);
        }

        public bool Return(
              TKey key
            , in NativeSlice<GameObjectId> gameObjectIds
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

            if (_prefabMap.TryGetValue(key, out var prefab) == false)
            {
                error = NativeReturningError.NoPrefabAssociatedWithProvidedKey;
                return false;
            }

            _unusedGameObjectIdsMap.TryGetValue(key, out var unusedGameObjectIds);
            _unusedTransformIdsMap.TryGetValue(key, out var unusedTransformIds);
            _unusedTransformIndicesMap.TryGetValue(key, out var unusedTransformIndices);

            ReturnInternal(
                  prefab
                , gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , default
                , unusedGameObjectIds
                , unusedTransformIds
                , unusedTransformIndices
            );

            error = NativeReturningError.None;
            return true;
        }

        private void IncreaseCapacityBy(TKey key, int amount)
        {
            var capacity = _transformArray.length + amount;

            if (_transformArray.capacity < capacity)
            {
                _transformArray.capacity = capacity;
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

            IncreaseCapacity(_unusedGameObjectIdsMap, key, amount);
            IncreaseCapacity(_unusedTransformIdsMap, key, amount);
            IncreaseCapacity(_unusedTransformIndicesMap, key, amount);

            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void IncreaseCapacity<T>(NativeHashMap<TKey, NativeList<T>> map, TKey key, int amount)
                where T : unmanaged
            {
                if (map.TryGetValue(key, out var list))
                {
                    var capacity = list.Length + amount;

                    if (list.Capacity < capacity)
                    {
                        list.Capacity = capacity;
                    }
                }
            }
        }

        private void RentInternal(
              TKey key
            , in NativePrefabInfo prefab
            , in NativeSlice<GameObjectInfo> result
            , NativeRentingOptions options
            , NativeList<GameObjectId> _unusedGameObjectIds
            , NativeList<TransformId> _unusedTransformIds
            , NativeList<int> _unusedTransformIndices
        )
        {
            var slice = result.Slice();
            var gameObjectIds = slice.SliceWithStride<GameObjectId>(GameObjectInfo.OFFSET_GAMEOBJECT_ID);
            var transformIds = slice.SliceWithStride<TransformId>(GameObjectInfo.OFFSET_TRANSFORM_ID);
            var arrayIndices = slice.SliceWithStride<int>(GameObjectInfo.OFFSET_TRANSFORM_ARRAY_INDEX);

            RentInternal(
                  key
                , prefab
                , gameObjectIds
                , transformIds
                , arrayIndices
                , options
                , default
                , _unusedGameObjectIds
                , _unusedTransformIds
                , _unusedTransformIndices
            );
        }

        private void PrepoolInternal(
              TKey key
            , in NativePrefabInfo prefab
            , int amount
            , NativeReturningOptions options
            , NativeList<GameObjectId> _unusedGameObjectIds
            , NativeList<TransformId> _unusedTransformIds
            , NativeList<int> _unusedTransformIndices
        )
        {
            if (amount < 1)
            {
                return;
            }

            IncreaseCapacityBy(key, amount);

            var gameObjectIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);
            var transformIds = NativeArray.CreateFast<int>(amount, Allocator.Temp);

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

            for (var i = 0; i < amount; i++)
            {
                var transformId = transformIds[i];
                var arrayIndex = transformArray.length;

                transformArray.Add((EntityId)transformId);
                _unusedTransformIndices.Add(arrayIndex);
            }

            if (options.Contains(NativeReturningOptions.MoveToScene)
                && prefab.ReturnScene.TryGetValue(out var scene)
            )
            {
                SceneManager.MoveGameObjectsToScene(gameObjectIds.Reinterpret<EntityId>(), scene);
            }
        }

        private void RentInternal(
              TKey key
            , in NativePrefabInfo prefab
            , in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeRentingOptions options
            , NativeArray<GameObjectId> gameObjectIdsToMove
            , NativeList<GameObjectId> _unusedGameObjectIds
            , NativeList<TransformId> _unusedTransformIds
            , NativeList<int> _unusedTransformIndices
        )
        {
            var amount = gameObjectIds.Length;

            PrepoolInternal(
                  key
                , prefab
                , amount - _unusedGameObjectIds.Length
                , NativeReturningOptions.Everything
                , _unusedGameObjectIds
                , _unusedTransformIds
                , _unusedTransformIndices
            );

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
                || prefab.RentScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove.Reinterpret<EntityId>(), scene);
        }

        private void ReturnInternal(
              in NativePrefabInfo prefab
            , in NativeSlice<GameObjectId> gameObjectIds
            , in NativeSlice<TransformId> transformIds
            , in NativeSlice<int> arrayIndices
            , NativeReturningOptions options
            , NativeArray<GameObjectId> gameObjectIdsToMove
            , NativeList<GameObjectId> _unusedGameObjectIds
            , NativeList<TransformId> _unusedTransformIds
            , NativeList<int> _unusedTransformIndices
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
                || prefab.ReturnScene.TryGetValue(out var scene) == false
            )
            {
                return;
            }

            SceneManager.MoveGameObjectsToScene(gameObjectIdsToMove.Reinterpret<EntityId>(), scene);
        }
    }
}

#endif
