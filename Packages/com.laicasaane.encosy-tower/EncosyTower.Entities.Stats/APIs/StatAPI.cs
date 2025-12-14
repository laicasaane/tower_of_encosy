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

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public static class StatAPI
    {
        /// <summary>
        /// Adds these components to the entity.
        /// <list type="bullet">
        /// <item><see cref="StatOwner"/></item>
        /// <item>Buffer of <typeparamref name="TStat"/></item>
        /// <item>Buffer of <typeparamref name="TStatModifier"/></item>
        /// <item>Buffer of <typeparamref name="TStatObserver"/></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// The buffer of <typeparamref name="TStat"/> also contains one element
        /// that acts as a <see cref="None"/> stat.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BakeStatComponents<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>(
              IBaker ibaker
            , Entity entity
            , out StatBaker<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statBaker
            , TValuePairComposer valuePairComposer = default
        )
            where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            ibaker.AddComponent(entity, new StatOwner());

            statBaker = new() {
                _ibaker = ibaker,
                _entity = entity,
                _valuePairComposer = valuePairComposer,
                _statOwner = default,
                _statBuffer = ibaker.AddBuffer<TStat>(entity),
                _modifierBuffer = ibaker.AddBuffer<TStatModifier>(entity),
                _observerBuffer = ibaker.AddBuffer<TStatObserver>(entity),
            };

            statBaker._statBuffer.Add(new TStat());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetStatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>(
              this StatBaker<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> _
            , out StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> accessor
            , TValuePairComposer valuePairComposer = default
        )
            where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            accessor = new() {
                _lookupOwner = default,
                _lookupStats = default,
                _lookupModifiers = default,
                _lookupObservers = default,
                _valuePairComposer = valuePairComposer,
                _nullStat = default,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetStatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>(
              this StatBaker<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> _
            , Allocator allocator
            , out StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
        )
            where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            worldData = new(allocator);
        }

        /// <summary>
        /// Adds these components to the entity.
        /// <list type="bullet">
        /// <item><see cref="StatOwner"/></item>
        /// <item>Buffer of <typeparamref name="TStat"/></item>
        /// <item>Buffer of <typeparamref name="TStatModifier"/></item>
        /// <item>Buffer of <typeparamref name="TStatObserver"/></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// The buffer of <typeparamref name="TStat"/> also contains one element
        /// that acts as a <see cref="None"/> stat.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddStatComponents<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver>(
              Entity entity
            , EntityManager entityManager
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
        {
            var types = new ComponentTypeSet(
                  ComponentType.ReadWrite<StatOwner>()
                , ComponentType.ReadWrite<TStat>()
                , ComponentType.ReadWrite<TStatModifier>()
                , ComponentType.ReadWrite<TStatObserver>()
            );

            entityManager.AddComponent(entity, types);

            var statBuffer = entityManager.GetBuffer<TStat>(entity, false);
            statBuffer.Add(new TStat());
        }

        /// <summary>
        /// Adds these components to the entity.
        /// <list type="bullet">
        /// <item><see cref="StatOwner"/></item>
        /// <item>Buffer of <typeparamref name="TStat"/></item>
        /// <item>Buffer of <typeparamref name="TStatModifier"/></item>
        /// <item>Buffer of <typeparamref name="TStatObserver"/></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// The buffer of <typeparamref name="TStat"/> also contains one element
        /// that acts as a <see cref="None"/> stat.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddStatComponents<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver>(
              Entity entity
            , EntityCommandBuffer ecb
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
        {
            var types = new ComponentTypeSet(
                  ComponentType.ReadWrite<StatOwner>()
                , ComponentType.ReadWrite<TStat>()
                , ComponentType.ReadWrite<TStatModifier>()
                , ComponentType.ReadWrite<TStatObserver>()
            );

            ecb.AddComponent(entity, types);

            var statBuffer = ecb.SetBuffer<TStat>(entity);
            statBuffer.Add(new TStat());
        }

        /// <summary>
        /// Adds these components to the entity.
        /// <list type="bullet">
        /// <item><see cref="StatOwner"/></item>
        /// <item>Buffer of <typeparamref name="TStat"/></item>
        /// <item>Buffer of <typeparamref name="TStatModifier"/></item>
        /// <item>Buffer of <typeparamref name="TStatObserver"/></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// The buffer of <typeparamref name="TStat"/> also contains one element
        /// that acts as a <see cref="None"/> stat.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddStatComponents<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver>(
              Entity entity
            , EntityCommandBuffer.ParallelWriter ecb
            , int sortKey
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
        {
            var types = new ComponentTypeSet(
                  ComponentType.ReadWrite<StatOwner>()
                , ComponentType.ReadWrite<TStat>()
                , ComponentType.ReadWrite<TStatModifier>()
                , ComponentType.ReadWrite<TStatObserver>()
            );

            ecb.AddComponent(sortKey, entity, types);

            var statBuffer = ecb.SetBuffer<TStat>(sortKey, entity);
            statBuffer.Add(new TStat());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreateStatCommon<TValuePair, TStat>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , out TStat newStat
            , out StatHandle statHandle
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            statHandle = new StatHandle {
                entity = entity,
            };

            newStat = new TStat {
                ValuePair = valuePair,
                ProduceChangeEvents = produceChangeEvents,
                UserData = userData,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatHandle<TStatData> CreateStatHandle<TValuePair, TStat, TStatData, TValuePairComposer>(
              Entity entity
            , TStatData statData
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
            , TValuePairComposer valuePairComposer = default
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            var value = valuePairComposer.Compose(statData.IsValuePair, statData.BaseValue, statData.CurrentValue);

            CreateStatCommon(entity, value, produceChangeEvents, userData, out TStat newStat, out var statHandle);

            statHandle.index = statBuffer.Length;
            statBuffer.Add(newStat);

            return (StatHandle<TStatData>)statHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatHandle<TStatData> CreateStatHandle<TValuePair, TStat, TStatData, TValuePairComposer>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
            , out TStatData statData
            , TValuePairComposer valuePairComposer = default
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            statData = MakeStatData<TValuePair, TStatData>(valuePair);
            valuePair = valuePairComposer.Compose(
                  statData.IsValuePair
                , valuePair.GetBaseValueOrDefault()
                , valuePair.GetCurrentValueOrDefault()
            );

            CreateStatCommon(entity, valuePair, produceChangeEvents, userData, out TStat newStat, out var statHandle);

            statHandle.index = statBuffer.Length;
            statBuffer.Add(newStat);

            return (StatHandle<TStatData>)statHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatHandle CreateStatHandle<TValuePair, TStat>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            CreateStatCommon(entity, valuePair, produceChangeEvents, userData, out TStat newStat, out var statHandle);

            statHandle.index = statBuffer.Length;
            statBuffer.Add(newStat);

            return statHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void UpdateSingleStatCommon<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>(
              StatHandle statHandle
            , StatReader<TValuePair, TStat> statsReader
            , ref TStat statRef
            , ref DynamicBuffer<TStatModifier> modifierBuffer
            , ReadOnlySpan<TStatObserver> observerBuffer
            , ref StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> worldData
            , TValuePairComposer valuePairComposer
        )
            where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            ThrowHelper.ThrowIfStatWorldDataIsNotCreated(worldData.IsCreated);

            var initialStat = statRef;

            // Apply Modifiers
            ref var modifierStack = ref worldData._modifierStackRef.ValueAsUnsafeRefRW();
            modifierStack.Reset(statRef);

            var statRefModifierRange = statRef.ModifierRange;
            var statRefModifiersCount = statRefModifierRange.count;
            var statRefModifiersStart = statRefModifierRange.startIndex;
            var statRefModifiersEnd = statRefModifiersStart + statRefModifiersCount;

            for (var i = statRefModifiersStart; i < statRefModifiersEnd; i++)
            {
                ref TStatModifier modifierRef = ref modifierBuffer.ElementAt(i);

                // Modifier is applied by ref, so changes in the modifier struct done during Apply() are saved
                modifierRef.Apply(
                      statsReader
                    , ref modifierStack
                    , out bool addModifierTriggerEvent
                );

                // Handle modifier trigger events
                if (addModifierTriggerEvent)
                {
                    worldData.AddModifierTriggerEvent(
                        new ModifierTriggerEvent<TValuePair, TStat, TStatModifier, TStatModifierStack> {
                            modifier = modifierRef,
                            handle = new StatModifierHandle {
                                affectedStatHandle = statHandle,
                                modifierId = modifierRef.Id,
                            }
                        }
                    );
                }
            }

            var statRefBaseValue = statRef.GetBaseValueOrDefault();
            var statRefCurrentValue = statRef.GetCurrentValueOrDefault();

            modifierStack.Apply(statRefBaseValue, ref statRefCurrentValue);
            statRef.ValuePair = valuePairComposer.Compose(
                  statRef.ValuePair.IsPair
                , statRefBaseValue
                , statRefCurrentValue
            );

            // If the stat value really changed
            if (initialStat.ValuePair.Equals(statRef.ValuePair))
            {
                return;
            }

            // Stat change events
            if (statRef.ProduceChangeEvents)
            {
                worldData.AddStatChangeEvent(new StatChangeEvent<TValuePair> {
                    statHandle = statHandle,
                    prevValue = initialStat.ValuePair,
                    newValue = statRef.ValuePair,
                });
            }

            // Notify Observers (add to update list)
            var statRefObserverRange = statRef.ObserverRange;
            var statRefObserversCount = statRefObserverRange.count;
            var statRefObserversStart = statRefObserverRange.startIndex;
            var statRefObserversEnd = statRefObserversStart + statRefObserversCount;

            for (var i = statRefObserversStart; i < statRefObserversEnd; i++)
            {
                var observerHandle = observerBuffer[i].ObserverHandle;

                if (observerHandle.entity == statHandle.entity)
                {
                    // Same-entity observers will be processed next, while we have all the buffers
                    worldData._tmpSameEntityUpdatedStats.Add(observerHandle);
                }
                else
                {
                    // Other-entity observers will be processed later
                    worldData._tmpGlobalUpdatedStats.Add(observerHandle);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddObserversOfStatToList<TStatObserver>(
              ObserverRange observerRange
            , ReadOnlySpan<TStatObserver> observerBuffer
            , NativeList<TStatObserver> statObservers
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            var startIndex = statObservers.Length;
            var count = observerRange.count;
            var statObserverSpan = statObservers.InsertRangeSpan(startIndex, count);
            observerBuffer.Slice(startIndex, count).CopyTo(statObserverSpan);
        }

        internal static unsafe void AddStatAsObserverOfOtherStat<TValuePair, TStat, TStatObserver>(
              StatHandle observerStatHandle
            , StatHandle observedStatHandle
            , ref DynamicBuffer<TStat> statBufferOnObservedStat
            , ref DynamicBuffer<TStatObserver> observerBufferOnObservedStatEntity
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatObserver : unmanaged, IStatObserver
        {
            Assert.IsTrue(observerStatHandle.entity != Entity.Null);

            if (observedStatHandle.index.IsValidInRange(statBufferOnObservedStat.Length) == false)
            {
                return;
            }

            var observedStat = statBufferOnObservedStat[observedStatHandle.index];

            // IMPORTANT: observers must be sorted in affected stat order
            var observerRange = observedStat.ObserverRange;
            var observersEndIndex = observerRange.ExclusiveEnd;

            observerBufferOnObservedStatEntity.Insert(
                  observersEndIndex
                , new TStatObserver { ObserverHandle = observerStatHandle }
            );

            observerRange.count++;
            observedStat.ObserverRange = observerRange;

            statBufferOnObservedStat[observedStatHandle.index] = observedStat;

            // update next stat start indexes
            for (var n = observedStatHandle.index + 1; n < statBufferOnObservedStat.Length; n++)
            {
                ref var nextStatRef = ref statBufferOnObservedStat.ElementAt(n);

                var nextObserverRange = nextStatRef.ObserverRange;
                nextObserverRange.startIndex++;
                nextStatRef.ObserverRange = nextObserverRange;
            }
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TStatData GetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , ReadOnlySpan<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            var stat = GetStat<TValuePair, TStat>(statHandle, statBuffer);
            return MakeStatData<TValuePair, TStatData>(stat.ValuePair);
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TStatData statData
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            if (TryGetStat<TValuePair, TStat>(statHandle, statBuffer, out TStat stat) == false)
            {
                statData = default;
                return false;
            }

            statData = MakeStatData<TValuePair, TStatData>(stat.ValuePair);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , BufferLookup<TStat> lookupStats
            , out TStatData statData
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            if (TryGetStat<TValuePair, TStat>(statHandle, lookupStats, out TStat stat) == false)
            {
                statData = default;
                return false;
            }

            statData = MakeStatData<TValuePair, TStatData>(stat.ValuePair);
            return true;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValuePair GetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            return statBuffer[statHandle.index].ValuePair;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TValuePair valuePair
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                valuePair = statBuffer[statHandle.index].ValuePair;
                return true;
            }

            valuePair = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out TValuePair valuePair
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer)
                && TryGetStat<TValuePair, TStat>(statHandle, statBuffer.AsNativeArray(), out TStat stat)
            )
            {
                valuePair = stat.ValuePair;
                return true;
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<TValuePair, TStat>(StatHandle statHandle, ReadOnlySpan<TStat> statBuffer)
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            return statHandle.index.IsValidInRange(statBuffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<TValuePair, TStat>(StatHandle statHandle, BufferLookup<TStat> lookupStats)
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return Contains<TValuePair, TStat>(statHandle, statBuffer.AsNativeArray());
            }

            return false;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<TValuePair, TStat>(
              StatHandle statHandle
            , uint userData
            , ReadOnlySpan<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (TryGetStat<TValuePair, TStat>(statHandle, statBuffer, out var stat))
            {
                return stat.UserData == userData;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<TValuePair, TStat>(
              StatHandle statHandle
            , uint userData
            , BufferLookup<TStat> lookupStats
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return Contains<TValuePair, TStat>(statHandle, userData, statBuffer.AsNativeArray());
            }

            return false;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TStat GetStat<TValuePair, TStat>(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            return statBuffer[statHandle.index];
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStat<TValuePair, TStat>(
              StatHandle statHandle
            , ReadOnlySpan<TStat> statBuffer
            , out TStat stat
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                stat = statBuffer[statHandle.index];
                return true;
            }

            stat = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStat<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out TStat stat
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return TryGetStat<TValuePair, TStat>(statHandle, statBuffer.AsNativeArray(), out stat);
            }

            stat = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TStat> GetStats<TValuePair, TStat>(
              Entity entity
            , BufferLookup<TStat> lookupStats
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(entity, out var statBuffer))
            {
                return statBuffer.AsNativeArray().AsReadOnly();
            }

            return default;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , TStatData statData
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            ref var statRef = ref statBuffer.ElementAt(statHandle.index);

            statRef.ProduceChangeEvents = produceChangeEvents;
            statRef.UserData = userData;
            statRef.TrySetCurrentValue(statData.CurrentValue);

            if (statData.IsValuePair)
            {
                statRef.TrySetBaseValue(statData.BaseValue);
            }
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , TStatData statData
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length) == false)
            {
                return false;
            }

            SetStatData<TValuePair, TStat, TStatData>(
                  statHandle
                , statData
                , produceChangeEvents
                , userData
                , ref statBuffer
            );

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStatData<TValuePair, TStat, TStatData>(
              StatHandle<TStatData> statHandle
            , TStatData statData
            , bool produceChangeEvents
            , uint userData
            , BufferLookup<TStat> lookupStats
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return TrySetStatData<TValuePair, TStat, TStatData>(
                      statHandle
                    , statData
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                );
            }

            return false;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            ref var statRef = ref statBuffer.ElementAt(statHandle.index);
            statRef.ProduceChangeEvents = produceChangeEvents;
            statRef.UserData = userData;
            statRef.ValuePair = valuePair;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                SetStatValue<TValuePair, TStat>(
                      statHandle
                    , valuePair
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                );

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStatValue<TValuePair, TStat>(
              StatHandle statHandle
            , TValuePair valuePair
            , bool produceChangeEvents
            , uint userData
            , BufferLookup<TStat> lookupStats
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return TrySetStatValue<TValuePair, TStat>(
                      statHandle
                    , valuePair
                    , produceChangeEvents
                    , userData
                    , ref statBuffer
                );
            }

            return false;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// <br/>
        /// Note: Assumes index is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetStat<TValuePair, TStat>(
              StatHandle statHandle
            , TStat stat
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            statBuffer[statHandle.index] = stat;
        }

        /// <summary>
        /// Note: Assumes the "statBuffer" is on the entity of the statHandle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStat<TValuePair, TStat>(
              StatHandle statHandle
            , TStat stat
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                SetStat<TValuePair, TStat>(statHandle, stat, ref statBuffer);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySetStat<TValuePair, TStat>(
              StatHandle statHandle
            , TStat stat
            , BufferLookup<TStat> lookupStats
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer))
            {
                return TrySetStat<TValuePair, TStat>(statHandle, stat, ref statBuffer);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref TStat GetStatRefUnsafe<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out bool success
            , ref TStat nullResult
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer)
                && statHandle.index.IsValidInRange(statBuffer.Length)
            )
            {
                success = true;
                return ref statBuffer.ElementAt(statHandle.index);
            }

            success = false;
            return ref nullResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref TStat GetStatRefUnsafe<TValuePair, TStat>(
              StatHandle statHandle
            , ref DynamicBuffer<TStat> statBuffer
            , out bool success
            , ref TStat nullResult
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                success = true;
                return ref statBuffer.ElementAt(statHandle.index);
            }

            success = false;
            return ref nullResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref TStat GetStatRefUnsafe<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out DynamicBuffer<TStat> statBuffer
            , out bool success
            , ref TStat nullResult
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (lookupStats.TryGetBuffer(statHandle.entity, out statBuffer)
                && statHandle.index.IsValidInRange(statBuffer.Length)
            )
            {
                success = true;
                return ref statBuffer.ElementAt(statHandle.index);
            }

            success = false;
            return ref nullResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetModifierCount<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out int modifierCount
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            modifierCount = 0;

            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer)
                && statHandle.index.IsValidInRange(statBuffer.Length)
            )
            {
                modifierCount = statBuffer[statHandle.index].ModifierRange.count;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Note: does not clear the supplied list
        /// </summary>
        public static bool TryGetModifiersOfStat<TValuePair, TStat, TStatModifier, TStatModifierStack>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , BufferLookup<TStatModifier> lookupModifiers
            , NativeList<StatModifierRecord<TValuePair, TStat, TStatModifier, TStatModifierStack>> modifiers
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        {
            var entity = statHandle.entity;

            if (lookupStats.TryGetBuffer(entity, out var statBuffer) == false
                || lookupModifiers.TryGetBuffer(entity, out var modifierBuffer) == false
            )
            {
                return false;
            }

            if (statHandle.index.IsValidInRange(statBuffer.Length) == false)
            {
                return false;
            }

            var stat = statBuffer[statHandle.index];
            var statModifierRange = stat.ModifierRange;
            var statModifiersStart = statModifierRange.startIndex;
            var statModifiersEnd = statModifierRange.ExclusiveEnd;

            modifiers.IncreaseCapacityTo(modifiers.Length + statModifierRange.count);

            for (var i = statModifiersStart; i < statModifiersEnd; i++)
            {
                var modifier = modifierBuffer[i];

                modifiers.Add(new StatModifierRecord<TValuePair, TStat, TStatModifier, TStatModifierStack>() {
                    modifier = modifier,
                    handle = new StatModifierHandle(statHandle, modifier.Id),
                });
            }

            return true;
        }

        /// <summary>
        /// Note: does not clear the supplied list
        /// </summary>
        public static bool TryGetObserversOfStat<TValuePair, TStat, TStatObserver>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , BufferLookup<TStatObserver> lookupObservers
            , NativeList<TStatObserver> observers
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatObserver : unmanaged, IStatObserver
        {
            var entity = statHandle.entity;

            if (lookupStats.TryGetBuffer(entity, out var statBuffer) == false
                || lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false
            )
            {
                return false;
            }

            if (statHandle.index.IsValidInRange(statBuffer.Length) == false)
            {
                return false;
            }

            var stat = statBuffer[statHandle.index];
            var (startIndex, count) = stat.ObserverRange;
            var sourceObservers = observerBuffer.AsNativeArray().AsReadOnlySpan();
            var destObservers = observers.InsertRangeSpan(observers.Length, count);
            sourceObservers.Slice(startIndex, count).CopyTo(destObservers);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetObserverCount<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out int observerCount
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            observerCount = 0;

            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer) == false)
            {
                return false;
            }

            if (statHandle.index.IsValidInRange(statBuffer.Length))
            {
                observerCount = statBuffer[statHandle.index].ObserverRange.count;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Note: does not clear the supplied list
        /// Note: useful to store observers before destroying an entity, and then manually update all observers after
        /// destroy. An observers update isn't automatically called when a stats entity is destroyed. (TODO:?)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAllObservers<TStatObserver>(
              Entity entity
            , BufferLookup<TStatObserver> lookupObservers
            , NativeList<TStatObserver> observers
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            if (lookupObservers.TryGetBuffer(entity, out var observerBuffer) == false)
            {
                return false;
            }

            observers.AddRange(observerBuffer.AsNativeArray());
            return true;
        }

        /// <summary>
        /// Returns true if any entity other than the specified one depends on stats present on the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EntityHasAnyOtherDependantStatEntities<TStatObserver>(
              Entity entity
            , BufferLookup<TStatObserver> lookupObservers
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            if (lookupObservers.TryGetBuffer(entity, out var observerBuffer))
            {
                return EntityHasAnyOtherDependantStatEntities<TStatObserver>(
                      entity
                    , observerBuffer.AsNativeArray()
                );
            }

            return false;
        }

        /// <summary>
        /// Returns true if any entity other than the specified one depends on stats present on the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EntityHasAnyOtherDependantStatEntities<TStatObserver>(
              Entity entity
            , ReadOnlySpan<TStatObserver> observerBufferOnEntity
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            for (var i = 0; i < observerBufferOnEntity.Length; i++)
            {
                if (observerBufferOnEntity[i].ObserverHandle.entity != entity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetOtherDependantStatsOfEntity<TStatObserver>(
              Entity entity
            , BufferLookup<TStatObserver> lookupObservers
            , NativeList<StatHandle> dependentStats
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            if (lookupObservers.TryGetBuffer(entity, out var observerBuffer))
            {
                GetOtherDependantStatsOfEntity<TStatObserver>(
                      entity
                    , observerBuffer.AsNativeArray()
                    , dependentStats
                );
            }
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetOtherDependantStatsOfEntity<TStatObserver>(
              Entity entity
            , ReadOnlySpan<TStatObserver> observerBufferOnEntity
            , NativeList<StatHandle> dependentStats
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            dependentStats.IncreaseCapacityTo(dependentStats.Length + observerBufferOnEntity.Length);

            for (var i = 0; i < observerBufferOnEntity.Length; i++)
            {
                var handle = observerBufferOnEntity[i].ObserverHandle;

                if (handle.entity != entity)
                {
                    dependentStats.Add(handle);
                }
            }
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetOtherDependantStatEntitiesOfEntity<TStatObserver>(
              Entity entity
            , BufferLookup<TStatObserver> lookupObservers
            , NativeHashSet<Entity> dependentEntities
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            if (lookupObservers.TryGetBuffer(entity, out var observerBuffer))
            {
                GetOtherDependantStatEntitiesOfEntity<TStatObserver>(
                      entity
                    , observerBuffer.AsNativeArray()
                    , dependentEntities
                );
            }
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetOtherDependantStatEntitiesOfEntity<TStatObserver>(
              Entity entity
            , ReadOnlySpan<TStatObserver> observerBufferOnEntity
            , NativeHashSet<Entity> dependentEntities
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            for (var i = 0; i < observerBufferOnEntity.Length; i++)
            {
                var handle = observerBufferOnEntity[i].ObserverHandle;

                if (handle.entity != entity)
                {
                    dependentEntities.Add(handle.entity);
                }
            }
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetStatEntitiesThatEntityDependsOn<TValuePair, TStat, TStatModifier, TStatModifierStack>(
              Entity entity
            , BufferLookup<TStatModifier> lookupModifiers
            , NativeHashSet<Entity> dependsOnEntities
            , NativeList<StatHandle> observerStatHandles
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        {
            if (lookupModifiers.TryGetBuffer(entity, out var modifierBuffer))
            {
                GetStatEntitiesThatEntityDependsOn<TValuePair, TStat, TStatModifier, TStatModifierStack>(
                      modifierBuffer.AsNativeArray()
                    , dependsOnEntities
                    , observerStatHandles
                );
            }
        }

        /// <summary>
        /// Returns all entities that have stats and depend on stats present on the specified entity.
        /// Excludes the specified entity.
        /// Useful for netcode relevancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetStatEntitiesThatEntityDependsOn<TValuePair, TStat, TStatModifier, TStatModifierStack>(
              ReadOnlySpan<TStatModifier> modifierBufferOnEntity
            , NativeHashSet<Entity> dependsOnEntities
            , NativeList<StatHandle> observerStatHandles
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        {
            observerStatHandles.Clear();

            for (var i = 0; i < modifierBufferOnEntity.Length; i++)
            {
                modifierBufferOnEntity[i].AddObservedStatsToList(observerStatHandles);
            }

            dependsOnEntities.IncreaseCapacityTo(dependsOnEntities.Count + observerStatHandles.Length);

            for (var i = 0; i < observerStatHandles.Length; i++)
            {
                dependsOnEntities.Add(observerStatHandles[i].entity);
            }
        }

        public static TStatData MakeStatData<TValuePair, TStatData>(TValuePair valuePair)
            where TValuePair : unmanaged, IStatValuePair
            where TStatData : unmanaged, IStatData
        {
            var statData = new TStatData();

            ThrowHelper.ThrowIfPairsMismatch(valuePair.Type, statData);

            statData.CurrentValue = valuePair.GetCurrentValueOrDefault();

            if (statData.IsValuePair)
            {
                statData.BaseValue = valuePair.GetBaseValueOrDefault();
            }

            return statData;
        }
    }
}
