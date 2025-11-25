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
using EncosyTower.Collections.Unsafe;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public static class StatAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BakeStatComponents<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>(
              IBaker baker
            , Entity entity
            , out StatBaker<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statBaker
        )
            where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
            where TStat : unmanaged, IStat<TValuePair>
            where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
            where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
            where TStatObserver : unmanaged, IStatObserver
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            baker.AddComponent(entity, new StatOwner());

            statBaker = new() {
                _baker = baker,
                _entity = entity,
                _statOwner = default,
                _statBuffer = baker.AddBuffer<TStat>(entity),
                _modifierBuffer = baker.AddBuffer<TStatModifier>(entity),
                _observerBuffer = baker.AddBuffer<TStatObserver>(entity),
            };
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
        }

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
        }

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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreateStatCommon<TValuePair, TStat>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , out TStat newStat
            , out StatHandle statHandle
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            statHandle = new StatHandle {
                index = -1,
                entity = entity,
            };

            newStat = new TStat {
                ValuePair = valuePair,
                ProduceChangeEvents = produceChangeEvents,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatHandle CreateStat<TValuePair, TStat, TStatData, TValuePairComposer>(
              Entity entity
            , TStatData statData
            , bool produceChangeEvents
            , ref DynamicBuffer<TStat> statBuffer
            , TValuePairComposer composer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
            where TStatData : unmanaged, IStatData
            where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
        {
            var value = composer.Compose(statData.IsValuePair, statData.BaseValue, statData.CurrentValue);

            CreateStatCommon(entity, value, produceChangeEvents, out TStat newStat, out var statHandle);

            statHandle.index = statBuffer.Length;
            statBuffer.Add(newStat);

            return statHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatHandle CreateStat<TValuePair, TStat>(
              Entity entity
            , TValuePair valuePair
            , bool produceChangeEvents
            , ref DynamicBuffer<TStat> statBuffer
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            CreateStatCommon(entity, valuePair, produceChangeEvents, out TStat newStat, out var statHandle);

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
            , TValuePairComposer composer
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
                                modifierId = modifierRef.Id,
                                affectedStatHandle = statHandle,
                            }
                        }
                    );
                }
            }

            var statRefBaseValue = statRef.GetBaseValueOrDefault();
            var statRefCurrentValue = statRef.GetCurrentValueOrDefault();

            modifierStack.Apply(statRefBaseValue, ref statRefCurrentValue);
            statRef.ValuePair = composer.Compose(statRef.ValuePair.IsPair, statRefBaseValue, statRefCurrentValue);

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
              ObserverRange observers
            , ReadOnlySpan<TStatObserver> observerBuffer
            , NativeList<TStatObserver> statObserversList
        )
            where TStatObserver : unmanaged, IStatObserver
        {
            var end = observers.startIndex + observers.count;

            for (var i = observers.startIndex; i < end; i++)
            {
                statObserversList.Add(observerBuffer[i]);
            }
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

            if ((uint)observedStatHandle.index >= (uint)statBufferOnObservedStat.Length)
            {
                return;
            }

            var observedStat = statBufferOnObservedStat[observedStatHandle.index];

            // IMPORTANT: observers must be sorted in affected stat order
            var observerRange = observedStat.ObserverRange;
            var observersEndIndex = observerRange.startIndex + observerRange.count;

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
            var valuePair = GetStat<TValuePair, TStat>(statHandle, statBuffer);
            var statData = new TStatData { BaseValue = valuePair.GetBaseValueOrDefault() };

            if (statData.IsValuePair)
            {
                statData.CurrentValue = valuePair.GetCurrentValueOrDefault();
            }

            return statData;
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
            statData = new TStatData();

            if (TryGetStat<TValuePair, TStat>(statHandle, statBuffer, out TStat stat) == false)
            {
                return false;
            }

            statData.BaseValue = stat.GetBaseValueOrDefault();

            if (statData.IsValuePair)
            {
                statData.CurrentValue = stat.GetCurrentValueOrDefault();
            }

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
            statData = new TStatData();

            if (TryGetStat<TValuePair, TStat>(statHandle, lookupStats, out TStat stat) == false)
            {
                return false;
            }

            statData.BaseValue = stat.GetBaseValueOrDefault();

            if (statData.IsValuePair)
            {
                statData.CurrentValue = stat.GetCurrentValueOrDefault();
            }

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
            if ((uint)statHandle.index < (uint)statBuffer.Length)
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
                && TryGetStat<TValuePair, TStat>(statHandle, statBuffer.AsNativeArray().AsReadOnly(), out TStat stat)
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
            if ((uint)statHandle.index < (uint)statBuffer.Length)
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
                return TryGetStat<TValuePair, TStat>(statHandle, statBuffer.AsNativeArray().AsReadOnly(), out stat);
            }

            stat = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref TStat GetStatRefUnsafe<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> statBufferLookup
            , out bool success
            , ref TStat nullResult
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statBufferLookup.TryGetBuffer(statHandle.entity, out var statBuffer)
                && statHandle.index < statBuffer.Length
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
            if (statHandle.index < statBuffer.Length)
            {
                success = true;
                return ref statBuffer.ElementAt(statHandle.index);
            }

            success = false;
            return ref nullResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref TStat GetStatRefWithBufferUnsafe<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> statBufferLookup
            , out DynamicBuffer<TStat> statBuffer
            , out bool success
            , ref TStat nullResult
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            if (statBufferLookup.TryGetBuffer(statHandle.entity, out statBuffer)
                && statHandle.index < statBuffer.Length
            )
            {
                success = true;
                return ref statBuffer.ElementAt(statHandle.index);
            }

            success = false;
            return ref nullResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetModifiersCount<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out int modifiersCount
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            modifiersCount = 0;

            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer) == false
                || (uint)statHandle.index >= (uint)statBuffer.Length
            )
            {
                return false;
            }

            modifiersCount = statBuffer[statHandle.index].ModifierRange.count;
            return true;
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

            if ((uint)statHandle.index >= (uint)statBuffer.Length)
            {
                return false;
            }

            var stat = statBuffer[statHandle.index];
            var statModifierRange = stat.ModifierRange;
            var statModifiersStart = statModifierRange.startIndex;
            var statModifiersCount = statModifierRange.count;
            var statModifiersEnd = statModifiersStart + statModifiersCount;

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

            if ((uint)statHandle.index >= (uint)statBuffer.Length)
            {
                return false;
            }

            var stat = statBuffer[statHandle.index];
            var statObserverRange = stat.ObserverRange;
            var statObserversCount = statObserverRange.count;
            var statObserversStart = statObserverRange.startIndex;
            var statObserversEnd = statObserversStart + statObserversCount;

            for (var i = statObserversStart; i < statObserversEnd; i++)
            {
                observers.Add(observerBuffer[i]);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetObserversCount<TValuePair, TStat>(
              StatHandle statHandle
            , BufferLookup<TStat> lookupStats
            , out int observersCount
        )
            where TValuePair : unmanaged, IStatValuePair
            where TStat : unmanaged, IStat<TValuePair>
        {
            observersCount = 0;

            if (lookupStats.TryGetBuffer(statHandle.entity, out var statBuffer) == false)
            {
                return false;
            }

            if ((uint)statHandle.index >= (uint)statBuffer.Length)
            {
                return false;
            }

            observersCount = statBuffer[statHandle.index].ObserverRange.count;
            return true;
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

            for (var i = 0; i < observerBuffer.Length; i++)
            {
                observers.Add(observerBuffer[i]);
            }

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
                    , observerBuffer.AsNativeArray().AsReadOnly()
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
                    , observerBuffer.AsNativeArray().AsReadOnly()
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
                    , observerBuffer.AsNativeArray().AsReadOnly()
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
                      modifierBuffer.AsNativeArray().AsReadOnly()
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
                TStatModifier modifier = modifierBufferOnEntity[i];
                modifier.AddObservedStatsToList(observerStatHandles);
            }

            for (var i = 0; i < observerStatHandles.Length; i++)
            {
                dependsOnEntities.Add(observerStatHandles[i].entity);
            }
        }
    }
}
