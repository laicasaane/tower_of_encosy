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
using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public struct StatBaker<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        internal IBaker _baker;
        internal Entity _entity;

        internal StatOwner _statOwner;
        internal DynamicBuffer<TStat> _statBuffer;
        internal DynamicBuffer<TStatModifier> _modifierBuffer;
        internal DynamicBuffer<TStatObserver> _observerBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle CreateStat<TStatData>(
              TStatData statData
            , bool produceChangeEvents
            , TValuePairComposer valuePairComposer = default
        )
            where TStatData : unmanaged, IStatData
        {
            return StatAPI.CreateStat<TValuePair, TStat, TStatData, TValuePairComposer>(
                  _entity
                , statData
                , produceChangeEvents
                , ref _statBuffer
                , valuePairComposer
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatHandle CreateStat(TValuePair valuePair, bool produceChangeEvents)
        {
            return StatAPI.CreateStat(_entity, valuePair, produceChangeEvents, ref _statBuffer);
        }

        public bool TryAddStatModifier(
              StatHandle affectedStatHandle
            , TStatModifier modifier
            , out StatModifierHandle statModifierHandle
            , TValuePairComposer valuePairComposer = default
        )
        {
            var entity = _entity;

            // Cancel if the affected stat is not on this entity
            if (affectedStatHandle.entity != entity)
            {
                statModifierHandle = default;
                return false;
            }

            // Cancel if the modifier involves stats of any other entity
            var tmpObservedStatHandles = new NativeList<StatHandle>(Allocator.Temp);
            modifier.AddObservedStatsToList(tmpObservedStatHandles);

            for (var i = 0; i < tmpObservedStatHandles.Length; ++i)
            {
                if (tmpObservedStatHandles[i].entity != entity)
                {
                    statModifierHandle = default;
                    return false;
                }
            }

            this.GetStatWorldData(Allocator.Temp, out var worldData);
            this.GetStatAccessor(out var accessor, valuePairComposer);

            var success = accessor.TryAddStatModifierSingleEntity(
                  affectedStatHandle
                , modifier
                , ref _statOwner
                , ref _statBuffer
                , ref _modifierBuffer
                , ref _observerBuffer
                , out statModifierHandle
                , ref worldData
            );

            _baker.SetComponent(_entity, _statOwner);

            return success;
        }
    }
}
