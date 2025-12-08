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
using System;

namespace EncosyTower.Entities.Stats
{
    partial struct StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
    {
        public struct ReadOnly
        {
            internal ComponentLookup<StatOwner> _lookupOwner;
            internal BufferLookup<TStat> _lookupStats;
            internal BufferLookup<TStatModifier> _lookupModifiers;
            internal BufferLookup<TStatObserver> _lookupObservers;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(ref SystemState state)
            {
                _lookupOwner = state.GetComponentLookup<StatOwner>(true);
                _lookupStats = state.GetBufferLookup<TStat>(true);
                _lookupModifiers = state.GetBufferLookup<TStatModifier>(true);
                _lookupObservers = state.GetBufferLookup<TStatObserver>(true);
            }

            public void Update(ref SystemState state)
            {
                _lookupOwner.Update(ref state);
                _lookupStats.Update(ref state);
                _lookupModifiers.Update(ref state);
                _lookupObservers.Update(ref state);
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetStatModifier(StatModifierHandle modifierHandle, out TStatModifier statModifier)
            {
                var handle = modifierHandle.affectedStatHandle;
                var entity = handle.entity;

                if (_lookupStats.TryGetBuffer(entity, out var statBuffer)
                    && _lookupModifiers.TryGetBuffer(entity, out var modifierBuffer)
                    && StatAPI.TryGetStat<TValuePair, TStat>(handle, statBuffer.AsNativeArray().AsReadOnly(), out TStat affectedStat)
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
        }
    }
}
