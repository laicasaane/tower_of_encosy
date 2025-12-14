// MIT License
//
// Copyright (c) 2023 Philippe St-Amand
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Assertions;
using System;
using EncosyTower.Collections;
using EncosyTower.Common;

namespace EncosyTower.Entities.Stats
{
    public partial struct StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        internal ComponentLookup<StatOwner> _lookupOwner;
        internal BufferLookup<TStat> _lookupStats;
        internal BufferLookup<TStatModifier> _lookupModifiers;
        internal BufferLookup<TStatObserver> _lookupObservers;

        internal TValuePairComposer _valuePairComposer;
        internal TStat _nullStat;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatAccessor(ref SystemState state) : this(ref state, default)
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatAccessor(ref SystemState state, TValuePairComposer valuePairComposer)
        {
            _lookupOwner = state.GetComponentLookup<StatOwner>(false);
            _lookupStats = state.GetBufferLookup<TStat>(false);
            _lookupModifiers = state.GetBufferLookup<TStatModifier>(false);
            _lookupObservers = state.GetBufferLookup<TStatObserver>(false);

            _valuePairComposer = valuePairComposer;
            _nullStat = default;
        }

        public void Update(ref SystemState state)
        {
            _lookupOwner.Update(ref state);
            _lookupStats.Update(ref state);
            _lookupModifiers.Update(ref state);
            _lookupObservers.Update(ref state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateStatHandle<TStatData>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , out StatHandle<TStatData> statHandle
            , out TStatData statData
        )
            where TStatData : unmanaged, IStatData
        {
            if (_lookupStats.TryGetBuffer(entity, out var statBuffer))
            {
                statHandle = StatAPI.CreateStatHandle<TValuePair, TStat, TStatData, TValuePairComposer>(
                      entity
                    , valuePair
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                    , out statData
                    , _valuePairComposer
                );

                return true;
            }

            statData = default;
            statHandle = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateStatHandle<TStatData>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , out StatHandle<TStatData> statHandle
        )
            where TStatData : unmanaged, IStatData
        {
            if (_lookupStats.TryGetBuffer(entity, out var statBuffer))
            {
                statHandle = StatAPI.CreateStatHandle<TValuePair, TStat, TStatData, TValuePairComposer>(
                      entity
                    , valuePair
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                    , out _
                    , _valuePairComposer
                );

                return true;
            }

            statHandle = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateStatHandle<TStatData>(
              Entity entity
            , TStatData statData
            , bool produceChangeEvents
            , uint userData
            , out StatHandle<TStatData> statHandle
        )
            where TStatData : unmanaged, IStatData
        {
            if (_lookupStats.TryGetBuffer(entity, out var statBuffer))
            {
                statHandle = StatAPI.CreateStatHandle<TValuePair, TStat, TStatData, TValuePairComposer>(
                      entity
                    , statData
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                    , _valuePairComposer
                );

                return true;
            }

            statHandle = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateStatHandle(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , out StatHandle statHandle
        )
        {
            if (_lookupStats.TryGetBuffer(entity, out var statBuffer))
            {
                statHandle = StatAPI.CreateStatHandle(
                      entity
                    , valuePair
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                );

                return true;
            }

            statHandle = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatData<TStatData>(StatHandle<TStatData> statHandle, out TStatData statData)
            where TStatData : unmanaged, IStatData
        {
            return StatAPI.TryGetStatData<TValuePair, TStat, TStatData>(statHandle, _lookupStats, out statData);
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatData<TStatData>(
              StatHandle<TStatData> statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TStatData statData
        )
            where TStatData : unmanaged, IStatData
        {
            return StatAPI.TryGetStatData<TValuePair, TStat, TStatData>(statHandle, statBuffer, out statData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStat(StatHandle statHandle, out TStat stat)
        {
            return StatAPI.TryGetStat<TValuePair, TStat>(statHandle, _lookupStats, out stat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<TStat> GetStats(Entity entity)
        {
            return StatAPI.GetStats<TValuePair, TStat>(entity, _lookupStats);
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStat(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TStat stat
        )
        {
            return StatAPI.TryGetStat<TValuePair, TStat>(statHandle, statBuffer, out stat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatValue(StatHandle statHandle, out TValuePair valuePair)
        {
            return StatAPI.TryGetStatValue(statHandle, _lookupStats, out valuePair);
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatValue(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TValuePair valuePair
        )
        {
            return StatAPI.TryGetStatValue(statHandle, statBuffer, out valuePair);
        }

        public bool TrySetStatBaseValue(
              StatHandle statHandle
            , in TValuePair value
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out var statBuffer
                , out bool success
                , ref _nullStat
            );

            var entity = statHandle.entity;

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetBaseValue(value.GetBaseValueOrDefault());

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        public bool TrySetStatBaseValue(
              StatHandle statHandle
            , in TValuePair value
            , ref DynamicBuffer<TStat> statBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = statHandle.entity;
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetBaseValue(value.GetBaseValueOrDefault());

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        public bool TrySetStatCurrentValue(
              StatHandle statHandle
            , in TValuePair value
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out var statBuffer
                , out bool success
                , ref _nullStat
            );

            var entity = statHandle.entity;

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetCurrentValue(value.GetCurrentValueOrDefault());

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        public bool TrySetStatCurrentValue(
              StatHandle statHandle
            , in TValuePair value
            , ref DynamicBuffer<TStat> statBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = statHandle.entity;
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetCurrentValue(value.GetCurrentValueOrDefault());

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        public bool TrySetStatData<TStatData>(
              in StatDataParams<TStatData> statParams
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
            where TStatData : unmanaged, IStatData
        {
            if (statParams.IsCreated == false || statParams.Handle.TryGetValue(out var statHandle) == false)
            {
                return false;
            }

            var entity = statHandle.entity;

            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out var statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            if (statParams.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
            {
                statRef.ProduceChangeEvents = produceChangeEvents;
            }

            if (statParams.UserData.TryGetValue(out var userdata))
            {
                statRef.UserData = userdata;
            }

            if (statParams.StatData.TryGetValue(out var statData)
                && statRef.TrySetValues(statData.BaseValue, statData.CurrentValue)
            )
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return true;
        }

        public bool TrySetStatData(
              in StatValueParams<TValuePair> statParams
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            if (statParams.IsCreated == false || statParams.Handle.TryGetValue(out var statHandle) == false)
            {
                return false;
            }

            var entity = statHandle.entity;

            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out var statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            if (statParams.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
            {
                statRef.ProduceChangeEvents = produceChangeEvents;
            }

            if (statParams.UserData.TryGetValue(out var userdata))
            {
                statRef.UserData = userdata;
            }

            if (statParams.StatValues.TryGetValue(out var statValues)
                && statRef.TrySetValues(statValues.GetBaseValueOrDefault(), statValues.GetCurrentValueOrDefault())
            )
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return true;
        }

        /// <remarks>
        /// <paramref name="paramsForStats"/> and <paramref name="results"/> must be of the same length.
        /// </remarks>
        public void TrySetBaseValueToStats(
              ReadOnlySpan<StatValueParams<TValuePair>> paramsForStats
            , Span<bool> results
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var paramsArrLength = paramsForStats.Length;
            var resultsLength = results.Length;

            if (paramsArrLength != resultsLength)
            {
                results.Fill(false);
                return;
            }

            if (paramsArrLength < 1)
            {
                return;
            }

            var entityParamsIndexMap = new NativeParallelMultiHashMap<Entity, int>(paramsArrLength, Allocator.Temp);
            var uniqueEntities = new NativeHashSet<Entity>(paramsArrLength, Allocator.Temp);

            for (var i = 0; i < paramsArrLength; i++)
            {
                var statParams = paramsForStats[i];

                if (statParams.Handle.TryGetValue(out var handle) && statParams.StatValues.HasValue)
                {
                    entityParamsIndexMap.Add(handle.entity, i);
                    uniqueEntities.Add(handle.entity);
                }
                else
                {
                    results[i] = false;
                }
            }

            var lookupStats = _lookupStats;
            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;

            foreach (var entity in uniqueEntities)
            {
                var indexEnumerator = entityParamsIndexMap.GetValuesForKey(entity);

                if (lookupStats.TryGetBuffer(entity, out var statBuffer) == false
                    || lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                    || lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
                )
                {
                    foreach (var arrIndex in indexEnumerator)
                    {
                        results[arrIndex] = false;
                    }

                    continue;
                }

                foreach (var arrIndex in indexEnumerator)
                {
                    var statParams = paramsForStats[arrIndex];

                    results[arrIndex] = TrySetBaseValues(
                          statParams.Handle.GetValueOrThrow()
                        , statParams.StatValues.GetValueOrThrow()
                        , ref statBuffer
                        , ref modifierBuffer
                        , ref observerBuffer
                        , ref worldData
                    );
                }
            }
        }

        /// <remarks>
        /// <paramref name="paramsForStats"/> and <paramref name="results"/> must be of the same length.
        /// </remarks>
        public void TrySetCurrentValueToStats(
              ReadOnlySpan<StatValueParams<TValuePair>> paramsForStats
            , Span<bool> results
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var paramsArrLength = paramsForStats.Length;
            var resultsLength = results.Length;

            if (paramsArrLength != resultsLength)
            {
                results.Fill(false);
                return;
            }

            if (paramsArrLength < 1)
            {
                return;
            }

            var entityParamsIndexMap = new NativeParallelMultiHashMap<Entity, int>(paramsArrLength, Allocator.Temp);
            var uniqueEntities = new NativeHashSet<Entity>(paramsArrLength, Allocator.Temp);

            for (var i = 0; i < paramsArrLength; i++)
            {
                var statParams = paramsForStats[i];

                if (statParams.Handle.TryGetValue(out var handle) && statParams.StatValues.HasValue)
                {
                    entityParamsIndexMap.Add(handle.entity, i);
                    uniqueEntities.Add(handle.entity);
                }
                else
                {
                    results[i] = false;
                }
            }

            var lookupStats = _lookupStats;
            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;

            foreach (var entity in uniqueEntities)
            {
                var indexEnumerator = entityParamsIndexMap.GetValuesForKey(entity);

                if (lookupStats.TryGetBuffer(entity, out var statBuffer) == false
                    || lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                    || lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
                )
                {
                    foreach (var arrIndex in indexEnumerator)
                    {
                        results[arrIndex] = false;
                    }

                    continue;
                }

                foreach (var arrIndex in indexEnumerator)
                {
                    var statParams = paramsForStats[arrIndex];

                    results[arrIndex] = TrySetCurrentValues(
                          statParams.Handle.GetValueOrThrow()
                        , statParams.StatValues.GetValueOrThrow()
                        , ref statBuffer
                        , ref modifierBuffer
                        , ref observerBuffer
                        , ref worldData
                    );
                }
            }
        }

        /// <remarks>
        /// <paramref name="paramsForStats"/> and <paramref name="results"/> must be of the same length.
        /// </remarks>
        public void TrySetDataToStats(
              ReadOnlySpan<StatValueParams<TValuePair>> paramsForStats
            , Span<bool> results
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var paramsArrLength = paramsForStats.Length;
            var resultsLength = results.Length;

            if (paramsArrLength != resultsLength)
            {
                results.Fill(false);
                return;
            }

            if (paramsArrLength < 1)
            {
                return;
            }

            var entityParamsIndexMap = new NativeParallelMultiHashMap<Entity, int>(paramsArrLength, Allocator.Temp);
            var uniqueEntities = new NativeHashSet<Entity>(paramsArrLength, Allocator.Temp);

            for (var i = 0; i < paramsArrLength; i++)
            {
                var statParams = paramsForStats[i];

                if (statParams.IsCreated && statParams.Handle.TryGetValue(out var handle))
                {
                    entityParamsIndexMap.Add(handle.entity, i);
                    uniqueEntities.Add(handle.entity);
                }
                else
                {
                    results[i] = false;
                }
            }

            var lookupStats = _lookupStats;
            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;

            foreach (var entity in uniqueEntities)
            {
                var indexEnumerator = entityParamsIndexMap.GetValuesForKey(entity);

                if (lookupStats.TryGetBuffer(entity, out var statBuffer) == false
                    || lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                    || lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
                )
                {
                    foreach (var arrIndex in indexEnumerator)
                    {
                        results[arrIndex] = false;
                    }

                    continue;
                }

                foreach (var arrIndex in indexEnumerator)
                {
                    var statParams = paramsForStats[arrIndex];

                    results[arrIndex] = TrySetValues(
                          statParams
                        , ref statBuffer
                        , ref modifierBuffer
                        , ref observerBuffer
                        , ref worldData
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TrySetBaseValues(
              StatHandle statHandle
            , TValuePair value
            , ref DynamicBuffer<TStat> statBuffer
            , ref DynamicBuffer<TStatModifier> modifierBuffer
            , ref DynamicBuffer<TStatObserver> observerBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success && statRef.TrySetBaseValue(value.GetBaseValueOrDefault()))
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TrySetCurrentValues(
              StatHandle statHandle
            , TValuePair value
            , ref DynamicBuffer<TStat> statBuffer
            , ref DynamicBuffer<TStatModifier> modifierBuffer
            , ref DynamicBuffer<TStatObserver> observerBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success && statRef.TrySetCurrentValue(value.GetCurrentValueOrDefault()))
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return success;
        }

        private bool TrySetValues(
              StatValueParams<TValuePair> statParams
            , ref DynamicBuffer<TStat> statBuffer
            , ref DynamicBuffer<TStatModifier> modifierBuffer
            , ref DynamicBuffer<TStatObserver> observerBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statParams.Handle.GetValueOrThrow()
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success && statParams.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
            {
                statRef.ProduceChangeEvents = produceChangeEvents;
            }

            if (success && statParams.UserData.TryGetValue(out var userdata))
            {
                statRef.UserData = userdata;
            }

            if (success && statParams.StatValues.TryGetValue(out var statValues)
                && statRef.TrySetValues(statValues.GetBaseValueOrDefault(), statValues.GetCurrentValueOrDefault())
            )
            {
                UpdateStatRef(
                      statParams.Handle.GetValueOrThrow()
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatData<TStatData>(
              StatHandle<TStatData> statHandle
            , TStatData statData
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
            where TStatData : unmanaged, IStatData
        {
            return TrySetStatValues(
                  statHandle
                , statData.BaseValue
                , statData.CurrentValue
                , ref worldData
            );
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatData<TStatData>(
              StatHandle<TStatData> statHandle
            , TStatData statData
            , ref DynamicBuffer<TStat> statBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
            where TStatData : unmanaged, IStatData
        {
            return TrySetStatValues(
                  statHandle
                , statData.BaseValue
                , statData.CurrentValue
                , ref statBuffer
                , ref worldData
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatValues(
              StatHandle statHandle
            , in TValuePair value
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            return TrySetStatValues(
                  statHandle
                , value.GetBaseValueOrDefault()
                , value.GetCurrentValueOrDefault()
                , ref worldData
            );
        }

        public bool TrySetStatValues(
              StatHandle statHandle
            , in StatVariant baseValue
            , in StatVariant currentValue
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = statHandle.entity;

            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out var statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetValues(baseValue, currentValue);

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatValues(
              StatHandle statHandle
            , in TValuePair value
            , ref DynamicBuffer<TStat> statBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            return TrySetStatValues(
                  statHandle
                , value.GetBaseValueOrDefault()
                , value.GetCurrentValueOrDefault()
                , ref statBuffer
                , ref worldData
            );
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        public bool TrySetStatValues(
              StatHandle statHandle
            , in StatVariant baseValue
            , in StatVariant currentValue
            , ref DynamicBuffer<TStat> statBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = statHandle.entity;

            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            var result = statRef.TrySetValues(baseValue, currentValue);

            if (result)
            {
                UpdateStatRef(
                      statHandle
                    , ref statRef
                    , ref statBuffer
                    , ref modifierBuffer
                    , ref observerBuffer
                    , ref worldData
                );
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatProduceChangeEvents(StatHandle statHandle, bool value)
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out bool success
                , ref _nullStat
            );

            if (success)
            {
                statRef.ProduceChangeEvents = value;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatProduceChangeEvents(StatHandle statHandle, bool value, ref DynamicBuffer<TStat> statBuffer)
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success)
            {
                statRef.ProduceChangeEvents = value;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatUserData(StatHandle statHandle, uint value)
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out bool success
                , ref _nullStat
            );

            if (success)
            {
                statRef.UserData = value;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetStatUserData(StatHandle statHandle, uint value, ref DynamicBuffer<TStat> statBuffer)
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool success
                , ref _nullStat
            );

            if (success)
            {
                statRef.UserData = value;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryUpdateAllStats(
              Entity entity
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            if (_lookupStats.TryGetBuffer(entity, out var statBuffer) == false)
            {
                return;
            }

            var length = statBuffer.Length;

            for (var i = 0; i < length; i++)
            {
                TryUpdateStat(new StatHandle(entity, i), ref worldData);
            }
        }

        /// <summary>
        /// Note: if the stat doesn't exist, it just does nothing (no error).
        /// </summary>
        /// <param name="statHandle"></param>
        public void TryUpdateStat(
              StatHandle statHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var lookupStats = _lookupStats;
            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;
            var valuePairComposer = _valuePairComposer;
            var statReader = new StatReader<TValuePair, TStat>(lookupStats);
            var tmpGlobalUpdatedStats = worldData._tmpGlobalUpdatedStats;
            var tmpSameEntityUpdatedStats = worldData._tmpSameEntityUpdatedStats;
            tmpGlobalUpdatedStats.Clear();
            tmpGlobalUpdatedStats.Add(statHandle);

            for (var i = 0; i < tmpGlobalUpdatedStats.Length; i++)
            {
                var newStatHandle = tmpGlobalUpdatedStats[i];
                var newEntity = newStatHandle.entity;

                ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                      newStatHandle
                    , lookupStats
                    , out var statBuffer
                    , out bool getStatSuccess
                    , ref _nullStat
                );

                if (getStatSuccess == false
                    || lookupModifiers.TryGetBuffer(newEntity, out var modifierBuffer) == false
                    || lookupObservers.TryGetBuffer(newEntity, out var observerBuffer) == false
                )
                {
                    continue;
                }

                tmpSameEntityUpdatedStats.Clear();

                StatAPI.UpdateSingleStatCommon(
                      newStatHandle
                    , statReader
                    , ref statRef
                    , ref modifierBuffer
                    , observerBuffer.AsNativeArray()
                    , ref worldData
                    , valuePairComposer
                );

                // Then update same-entity list

                for (var s = 0; s < tmpSameEntityUpdatedStats.Length; s++)
                {
                    var sameEntityStatHandle = tmpSameEntityUpdatedStats[s];

                    ref TStat sameEntityStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                          sameEntityStatHandle
                        , ref statBuffer
                        , out _
                        , ref _nullStat
                    );

                    StatAPI.UpdateSingleStatCommon(
                          sameEntityStatHandle
                        , statReader
                        , ref sameEntityStatRef
                        , ref modifierBuffer
                        , observerBuffer.AsNativeArray()
                        , ref worldData
                        , valuePairComposer
                    );
                }
            }
        }

        internal void TryUpdateStatAssumeSingleEntity(
              StatHandle statHandle
            , ref DynamicBuffer<TStat> statBuffer
            , ref DynamicBuffer<TStatModifier> modifierBuffer
            , ref DynamicBuffer<TStatObserver> observerBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var statReader = new StatReader<TValuePair, TStat>(statBuffer);
            var tmpGlobalUpdatedStats = worldData._tmpGlobalUpdatedStats;
            var tmpSameEntityUpdatedStats = worldData._tmpSameEntityUpdatedStats;
            var valuePairComposer = _valuePairComposer;

            tmpGlobalUpdatedStats.Clear();
            tmpSameEntityUpdatedStats.Clear();

            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , ref statBuffer
                , out bool getStatSuccess
                , ref _nullStat
            );

            if (getStatSuccess)
            {
                StatAPI.UpdateSingleStatCommon(
                      statHandle
                    , statReader
                    , ref statRef
                    , ref modifierBuffer
                    , observerBuffer.AsNativeArray()
                    , ref worldData
                    , valuePairComposer
                );

                // Then update same-entity list
                for (var s = 0; s < tmpSameEntityUpdatedStats.Length; s++)
                {
                    var sameEntityStatHandle = tmpSameEntityUpdatedStats[s];

                    ref TStat sameEntityStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                          sameEntityStatHandle
                        , ref statBuffer
                        , out _
                        , ref _nullStat
                    );

                    StatAPI.UpdateSingleStatCommon(
                          sameEntityStatHandle
                        , statReader
                        , ref sameEntityStatRef
                        , ref modifierBuffer
                        , observerBuffer.AsNativeArray()
                        , ref worldData
                        , valuePairComposer
                    );
                }
            }

            Assert.IsTrue(tmpGlobalUpdatedStats.Length == 0);
        }

        internal void UpdateStatRef(
              StatHandle statHandle
            , ref TStat initialStatRef
            , ref DynamicBuffer<TStat> initialStatBuffer
            , ref DynamicBuffer<TStatModifier> initialStatModifierBuffer
            , ref DynamicBuffer<TStatObserver> initialStatObserverBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var statReader = new StatReader<TValuePair, TStat>(_lookupStats);
            var tmpGlobalUpdatedStats = worldData._tmpGlobalUpdatedStats;
            var tmpSameEntityUpdatedStats = worldData._tmpSameEntityUpdatedStats;
            var valuePairComposer = _valuePairComposer;

            tmpGlobalUpdatedStats.Clear();
            tmpSameEntityUpdatedStats.Clear();

            // First update the current stat ref
            StatAPI.UpdateSingleStatCommon(
                  statHandle
                , statReader
                , ref initialStatRef
                , ref initialStatModifierBuffer
                , initialStatObserverBuffer.AsNativeArray()
                , ref worldData
                , valuePairComposer
            );

            // Then update same-entity list
            for (var s = 0; s < tmpSameEntityUpdatedStats.Length; s++)
            {
                var sameEntityStatHandle = tmpSameEntityUpdatedStats[s];

                ref TStat sameEntityStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                      sameEntityStatHandle
                    , ref initialStatBuffer
                    , out _
                    , ref _nullStat
                );

                StatAPI.UpdateSingleStatCommon(
                      sameEntityStatHandle
                    , statReader
                    , ref sameEntityStatRef
                    , ref initialStatModifierBuffer
                    , initialStatObserverBuffer.AsNativeArray()
                    , ref worldData
                    , valuePairComposer
                );
            }

            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;

            // Then update following stats
            for (var i = 0; i < tmpGlobalUpdatedStats.Length; i++)
            {
                var newStatHandle = tmpGlobalUpdatedStats[i];
                var newEntity = newStatHandle.entity;

                ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                      newStatHandle
                    , _lookupStats
                    , out var statBuffer
                    , out bool getStatSuccess
                    , ref _nullStat
                );

                if (getStatSuccess == false
                    || lookupModifiers.TryGetBuffer(newEntity, out var modifierBuffer) == false
                    || lookupObservers.TryGetBuffer(newEntity, out var observerBuffer) == false
                )
                {
                    continue;
                }

                tmpSameEntityUpdatedStats.Clear();

                StatAPI.UpdateSingleStatCommon(
                      newStatHandle
                    , statReader
                    , ref statRef
                    , ref modifierBuffer
                    , observerBuffer.AsNativeArray()
                    , ref worldData
                    , valuePairComposer
                );

                // Then update same-entity list
                for (var s = 0; s < tmpSameEntityUpdatedStats.Length; s++)
                {
                    var sameEntityStatHandle = tmpSameEntityUpdatedStats[s];

                    ref TStat sameEntityStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                          sameEntityStatHandle
                        , ref statBuffer
                        , out _
                        , ref _nullStat
                    );

                    StatAPI.UpdateSingleStatCommon(
                          sameEntityStatHandle
                        , statReader
                        , ref sameEntityStatRef
                        , ref modifierBuffer
                        , observerBuffer.AsNativeArray()
                        , ref worldData
                        , valuePairComposer
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetModifierCount(StatHandle statHandle, out int modifierCount)
        {
            return StatAPI.TryGetModifierCount<TValuePair, TStat>(statHandle, _lookupStats, out modifierCount);
        }

        /// <summary>
        /// Note: does not clear the supplied list
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetModifiersOfStat(
              StatHandle statHandle
            , NativeList<StatModifierRecord<TValuePair, TStat, TStatModifier, TStatModifierStack>> modifiers
        )
        {
            return StatAPI.TryGetModifiersOfStat(statHandle, _lookupStats, _lookupModifiers, modifiers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetObserverCount(StatHandle statHandle, out int observerCount)
        {
            return StatAPI.TryGetObserverCount<TValuePair, TStat>(statHandle, _lookupStats, out observerCount);
        }

        /// <summary>
        /// Note: does not clear the supplied list
        /// Note: useful to store observers before destroying an entity, and then manually update all observers after
        /// destroy. An observers update isn't automatically called when a stats entity is destroyed. (TODO:?)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAllObservers(Entity entity, NativeList<TStatObserver> observers)
        {
            return StatAPI.TryGetAllObservers(entity, _lookupObservers, observers);
        }

        public bool TryAddStatModifier(
              StatHandle affectedStatHandle
            , TStatModifier modifier
            , out StatModifierHandle statModifierHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = affectedStatHandle.entity;

            ref TStat affectedStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  affectedStatHandle
                , _lookupStats
                , out var statBuffer
                , out bool getStatSuccess
                , ref _nullStat
            );

            if (getStatSuccess == false
                || _lookupOwner.HasComponent(entity) == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBufferOnAffectedEntity) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBufferOnAffectedEntity) == false
            )
            {
                statModifierHandle = default;
                return false;
            }

            ref StatOwner affectStatOwnerRef = ref _lookupOwner.GetRefRW(entity).ValueRW;

            return TryAddStatModifier(
                  affectedStatHandle
                , modifier
                , ref affectStatOwnerRef
                , ref affectedStatRef
                , ref statBuffer
                , ref modifierBufferOnAffectedEntity
                , ref observerBufferOnAffectedEntity
                , out statModifierHandle
                , ref worldData
                , false
                , true
            );
        }

        public bool TryAddStatModifiersBatch(
              StatHandle affectedStatHandle
            , ReadOnlySpan<TStatModifier> modifiers
            , NativeList<StatModifierHandle> statModifierHandles
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            var entity = affectedStatHandle.entity;

            ref TStat affectedStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  affectedStatHandle
                , _lookupStats
                , out var statBuffer
                , out bool getStatSuccess
                , ref _nullStat
            );

            if (getStatSuccess == false
                || _lookupOwner.HasComponent(entity) == false
                || _lookupModifiers.TryGetBuffer(entity, out var modifierBufferOnAffectedEntity) == false
                || _lookupObservers.TryGetBuffer(entity, out var observerBufferOnAffectedEntity) == false
            )
            {
                return false;
            }

            ref StatOwner affectStatOwnerRef = ref _lookupOwner.GetRefRW(entity).ValueRW;
            bool success = true;

            statModifierHandles.IncreaseCapacityTo(statModifierHandles.Length + modifiers.Length);

            for (var i = 0; i < modifiers.Length; i++)
            {
                var result = TryAddStatModifier(
                      affectedStatHandle
                    , modifiers[i]
                    , ref affectStatOwnerRef
                    , ref affectedStatRef
                    , ref statBuffer
                    , ref modifierBufferOnAffectedEntity
                    , ref observerBufferOnAffectedEntity
                    , out StatModifierHandle statModifierHandle
                    , ref worldData
                    , false
                    , false
                );

                if (result)
                {
                    statModifierHandles.Add(statModifierHandle);
                }

                success &= result;
            }

            UpdateStatRef(
                  affectedStatHandle
                , ref affectedStatRef
                , ref statBuffer
                , ref modifierBufferOnAffectedEntity
                , ref observerBufferOnAffectedEntity
                , ref worldData
            );

            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryAddStatModifierSingleEntity(
              StatHandle affectedStatHandle
            , TStatModifier modifier
            , ref StatOwner affectStatOwnerRef
            , ref DynamicBuffer<TStat> statBufferOnAffectedEntity
            , ref DynamicBuffer<TStatModifier> modifierBufferOnAffectedEntity
            , ref DynamicBuffer<TStatObserver> observerBufferOnAffectedEntity
            , out StatModifierHandle statModifierHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat affectedStatRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  affectedStatHandle
                , ref statBufferOnAffectedEntity
                , out bool getStatSuccess
                , ref _nullStat
            );

            if (getStatSuccess)
            {
                return TryAddStatModifier(
                      affectedStatHandle
                    , modifier
                    , ref affectStatOwnerRef
                    , ref affectedStatRef
                    , ref statBufferOnAffectedEntity
                    , ref modifierBufferOnAffectedEntity
                    , ref observerBufferOnAffectedEntity
                    , out statModifierHandle
                    , ref worldData
                    , true
                    , true
                );
            }

            statModifierHandle = default;
            return false;
        }

        internal unsafe bool TryAddStatModifier(
              StatHandle affectedStatHandle
            , TStatModifier modifier
            , ref StatOwner affectStatOwnerRef
            , ref TStat affectedStatRef
            , ref DynamicBuffer<TStat> statBufferOnAffectedEntity
            , ref DynamicBuffer<TStatModifier> modifierBufferOnAffectedEntity
            , ref DynamicBuffer<TStatObserver> observerBufferOnAffectedEntity
            , out StatModifierHandle statModifierHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
            , bool isGuaranteedSingleEntity
            , bool recalculateStat
        )
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var tmpModifierObservedStats = worldData._tmpModifierObservedStats;
            var tmpStatObservers = worldData._tmpStatObservers;
            var lookupStats = _lookupStats;
            var lookupObservers = _lookupObservers;

            // Ensure lists are created and cleared
            tmpModifierObservedStats.Clear();
            tmpStatObservers.Clear();

            // Increment modifier Id (local to entity)
            affectStatOwnerRef.modifierIdCounter++;
            modifier.Id = affectStatOwnerRef.modifierIdCounter;

            statModifierHandle = new StatModifierHandle {
                affectedStatHandle = affectedStatHandle,
                modifierId = modifier.Id,
            };

            // Get observed stats of modifier
            modifier.AddObservedStatsToList(tmpModifierObservedStats);

            var modifierCanBeAdded = true;

            // If the modifier observes any stats, handle infinite observer loops detection
            if (tmpModifierObservedStats.Length > 0)
            {
                // Make sure the modifier wouldn't make the stat observe itself (would cause infinite loop)
                for (var k = 0; k < tmpModifierObservedStats.Length; k++)
                {
                    var modifierObservedStatHandle = tmpModifierObservedStats[k];

                    if (affectedStatHandle == modifierObservedStatHandle)
                    {
                        modifierCanBeAdded = false;
                        break;
                    }
                }

                // Don't allow infinite observer loops.
                // Follow the chain of stats that would react to this stat's changes if the modifier was added (follow the
                // observers chain). If we end up finding this stat anywhere in the chain, it would cause an infinite loop.
                // TODO: an alternative would be to configure a max stats update chain length and early exit an update
                // if over limit
                if (modifierCanBeAdded)
                {
                    // Start by adding the affected stat's observers
                    StatAPI.AddObserversOfStatToList<TStatObserver>(
                          affectedStatRef.ObserverRange
                        , observerBufferOnAffectedEntity.AsNativeArray()
                        , tmpStatObservers
                    );

                    // TODO: make sure this verification loop can't possibly end up being infinite either.
                    // It could be infinite if we haven't guaranteed loop detection for other modifier adds...
                    for (var i = 0; i < tmpStatObservers.Length; i++)
                    {
                        var iteratedObserverStatHandle = tmpStatObservers[i].ObserverHandle;
                        var iteratedEntity = iteratedObserverStatHandle.entity;

                        // If we find the affected stat down the chain of stats that it observes,
                        // it would create an infinite loop. Prevent adding modifier.
                        if (iteratedObserverStatHandle == affectedStatHandle)
                        {
                            modifierCanBeAdded = false;
                            break;
                        }

                        // Add the affected stat to the observers chain list if the iterated observer is
                        // an observed stat of the modifier. Because if we proceed with adding the modifier, the
                        // affected stat would be added as an observer of all modifier observed stats
                        for (var k = 0; k < tmpModifierObservedStats.Length; k++)
                        {
                            var modifierObservedStatHandle = tmpModifierObservedStats[k];

                            if (iteratedObserverStatHandle == modifierObservedStatHandle)
                            {
                                tmpStatObservers.Add(new TStatObserver {
                                    ObserverHandle = affectedStatHandle,
                                });
                            }
                        }

                        // Update buffers so they represent the ones on the observer entity
                        if (isGuaranteedSingleEntity)
                        {
                            if (StatAPI.TryGetStat<TValuePair, TStat>(
                                  iteratedObserverStatHandle
                                , statBufferOnAffectedEntity.AsNativeArray()
                                , out TStat observerStat
                            ))
                            {
                                StatAPI.AddObserversOfStatToList(
                                      observerStat.ObserverRange
                                    , observerBufferOnAffectedEntity.AsNativeArray()
                                    , tmpStatObservers
                                );
                            }
                        }
                        else if (lookupStats.TryGetBuffer(iteratedEntity, out var observerStatBuffer)
                            && lookupObservers.TryGetBuffer(iteratedEntity, out var observerStatObserverBuffer)
                            && StatAPI.TryGetStat<TValuePair, TStat>(
                                  iteratedObserverStatHandle
                                , observerStatBuffer.AsNativeArray()
                                , out TStat observerStat
                            )
                        )
                        {
                            StatAPI.AddObserversOfStatToList(
                                  observerStat.ObserverRange
                                , observerStatObserverBuffer.AsNativeArray()
                                , tmpStatObservers
                            );
                        }
                    }
                }
            }

            if (modifierCanBeAdded == false)
            {
                statModifierHandle = default;
                return false;
            }

            // Add modifier by inserting it at the end of the stat's modifiers sub-list
            {
                var modifierRange = affectedStatRef.ModifierRange;

                // IMPORTANT: modifiers must be sorted in affected stat order
                modifierBufferOnAffectedEntity.Insert(modifierRange.ExclusiveEnd, modifier);

                modifierRange.count++;
                affectedStatRef.ModifierRange = modifierRange;

                // update next stat start indexes
                for (var n = affectedStatHandle.index + 1; n < statBufferOnAffectedEntity.Length; n++)
                {
                    ref TStat nextStatRef = ref statBufferOnAffectedEntity.ElementAt(n);

                    var nextModifierRange = nextStatRef.ModifierRange;
                    nextModifierRange.startIndex++;
                    nextStatRef.ModifierRange = modifierRange;
                }
            }

            // Add affected stat as observer of all observed stats
            for (var i = 0; i < tmpModifierObservedStats.Length; i++)
            {
                var observedStatHandle = tmpModifierObservedStats[i];
                var observedEntity = observedStatHandle.entity;

                // Update buffers so they represent the ones on the observer entity
                if (isGuaranteedSingleEntity)
                {
                    StatAPI.AddStatAsObserverOfOtherStat<TValuePair, TStat, TStatObserver>(
                          affectedStatHandle
                        , observedStatHandle
                        , ref statBufferOnAffectedEntity
                        , ref observerBufferOnAffectedEntity
                    );
                }
                else if (lookupStats.TryGetBuffer(observedEntity, out var observedStatBuffer)
                    && lookupObservers.TryGetBuffer(observedEntity, out var observedStatObserverBuffer)
                )
                {
                    StatAPI.AddStatAsObserverOfOtherStat<TValuePair, TStat, TStatObserver>(
                          affectedStatHandle
                        , observedStatHandle
                        , ref observedStatBuffer
                        , ref observedStatObserverBuffer
                    );
                }
            }

            // Update stat following modifier add
            if (recalculateStat)
            {
                if (isGuaranteedSingleEntity)
                {
                    TryUpdateStatAssumeSingleEntity(
                          affectedStatHandle
                        , ref statBufferOnAffectedEntity
                        , ref modifierBufferOnAffectedEntity
                        , ref observerBufferOnAffectedEntity
                        , ref worldData
                    );
                }
                else
                {
                    TryUpdateStat(affectedStatHandle, ref worldData);
                }
            }

            return true;
        }

        public bool TryGetStatModifier(StatModifierHandle modifierHandle, out TStatModifier statModifier)
        {
            var handle = modifierHandle.affectedStatHandle;
            var entity = handle.entity;

            if (_lookupStats.TryGetBuffer(entity, out var statBuffer)
                && _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer)
                && StatAPI.TryGetStat<TValuePair, TStat>(
                      handle
                    , statBuffer.AsNativeArray()
                    , out TStat affectedStat
                )
                && affectedStat.ModifierRange.count > 0
            )
            {
                var modifierRange = affectedStat.ModifierRange;
                var end = modifierRange.ExclusiveEnd;

                for (var i = modifierRange.startIndex; i < end; i++)
                {
                    TStatModifier modifier = modifierBuffer[i];

                    if (modifier.Id == modifierHandle.modifierId)
                    {
                        statModifier = modifier;
                        return true;
                    }
                }
            }

            statModifier = default;
            return false;
        }

        public unsafe bool TryRemoveStatModifier(
              StatModifierHandle modifierHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var lookupStats = _lookupStats;
            var lookupModifiers = _lookupModifiers;
            var lookupObservers = _lookupObservers;
            var affectedHandle = modifierHandle.affectedStatHandle;
            var affectedEntity = affectedHandle.entity;

            if (lookupStats.TryGetBuffer(affectedEntity, out var statBufferOnAffectedEntity) == false
                || lookupModifiers.TryGetBuffer(affectedEntity, out var modifierBufferOnAffectedEntity) == false
                || affectedHandle.index.IsValidInRange(statBufferOnAffectedEntity.Length) == false
            )
            {
                return false;
            }

            var tmpModifierObservedStats = worldData._tmpModifierObservedStats;
            ref TStat affectedStatRef = ref statBufferOnAffectedEntity.ElementAt(affectedHandle.index);
            var affectedModifierRange = affectedStatRef.ModifierRange;

            for (var i = affectedModifierRange.startIndex; i < affectedModifierRange.ExclusiveEnd; i++)
            {
                TStatModifier modifier = modifierBufferOnAffectedEntity[i];

                if (modifier.Id != modifierHandle.modifierId)
                {
                    continue;
                }

                // Remove modifier
                modifierBufferOnAffectedEntity.RemoveAt(i);

                // Update count
                affectedModifierRange.count--;
                affectedStatRef.ModifierRange = affectedModifierRange;

                Assert.IsTrue(affectedStatRef.ModifierRange.count >= 0);

                // Update start indexes of next stats
                for (var n = affectedHandle.index + 1; n < statBufferOnAffectedEntity.Length; n++)
                {
                    ref TStat nextStatRef = ref statBufferOnAffectedEntity.ElementAt(n);

                    var nextModifierRange = nextStatRef.ModifierRange;
                    nextModifierRange.startIndex--;
                    nextStatRef.ModifierRange = nextModifierRange;
                }

                // Remove the modifier's affected stat as an observer of the modifier's observed stats
                tmpModifierObservedStats.Clear();
                modifier.AddObservedStatsToList(tmpModifierObservedStats);

                for (var n = 0; n < tmpModifierObservedStats.Length; n++)
                {
                    var observedStatHandle = tmpModifierObservedStats[n];
                    var observedEntity = observedStatHandle.entity;

                    if (lookupStats.TryGetBuffer(observedEntity, out var statBufferOnObservedEntity) == false
                        || lookupObservers.TryGetBuffer(observedEntity, out var observerBufferOnObservedEntity) == false
                        || observedStatHandle.index.IsValidInRange(statBufferOnObservedEntity.Length) == false
                    )
                    {
                        continue;
                    }

                    ref TStat observedStatRef = ref statBufferOnObservedEntity.ElementAt(observedStatHandle.index);

                    var observerRange = observedStatRef.ObserverRange;

                    for (var k = observerRange.startIndex; k < observerRange.ExclusiveEnd; k++)
                    {
                        var observerOfObservedStat = observerBufferOnObservedEntity[k];

                        if (observerOfObservedStat.ObserverHandle != modifierHandle.affectedStatHandle)
                        {
                            continue;
                        }

                        // Remove
                        observerBufferOnObservedEntity.RemoveAt(k);

                        // Update counts
                        observerRange.count--;
                        observedStatRef.ObserverRange = observerRange;

                        Assert.IsTrue(observedStatRef.ObserverRange.count >= 0);

                        // Update start indexes of next stats
                        for (var t = observedStatHandle.index + 1; t < statBufferOnObservedEntity.Length; t++)
                        {
                            ref TStat nextStatRef = ref statBufferOnObservedEntity.ElementAt(t);

                            var nextObserverRange = nextStatRef.ObserverRange;
                            nextObserverRange.startIndex--;
                            nextStatRef.ObserverRange = nextObserverRange;
                        }

                        // Break so we don't remove all observer instances of this stat
                        break;
                    }
                }

                // Stat update following modifier remove
                TryUpdateStat(modifierHandle.affectedStatHandle, ref worldData);

                return true;
            }

            return false;
        }

        public bool TryRemoveModifiersOfStat(
              StatHandle statHandle
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
        {
            ref TStat statRef = ref StatAPI.GetStatRefUnsafe<TValuePair, TStat>(
                  statHandle
                , _lookupStats
                , out _
                , out bool getStatSuccess
                , ref _nullStat
            );

            if (getStatSuccess == false
                || _lookupModifiers.TryGetBuffer(statHandle.entity, out var modifierBuffer) == false
            )
            {
                return false;
            }

            var modifiers = modifierBuffer.AsNativeArray().AsReadOnlySpan();

            while (statRef.ModifierRange.count > 0)
            {
                TStatModifier modifier = modifiers[statRef.ModifierRange.startIndex];

                var handle = new StatModifierHandle {
                    affectedStatHandle = statHandle,
                    modifierId = modifier.Id,
                };

                TryRemoveStatModifier(handle, ref worldData);
            }

            return true;
        }
    }
}
