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
using Unity.Jobs;

namespace EncosyTower.Entities.Stats
{
    public struct StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> : IDisposable, INativeDisposable
        where TValuePair : unmanaged, IStatValuePair
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
    {
        internal NativeList<StatChangeEvent<TValuePair>> _statChangeEvents;
        internal NativeList<ModifierTriggerEvent<TValuePair, TStat, TStatModifier, TStatModifierStack>> _modifierTriggerEvents;

        internal NativeList<StatHandle> _tmpModifierObservedStats;
        internal NativeList<TStatObserver> _tmpStatObservers;
        internal NativeList<StatHandle> _tmpGlobalUpdatedStats;
        internal NativeList<StatHandle> _tmpSameEntityUpdatedStats;

        internal NativeReference<TStatModifierStack> _modifierStackRef;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatWorldData(AllocatorManager.AllocatorHandle allocator)
        {
            _statChangeEvents = new(allocator);
            _modifierTriggerEvents = new(allocator);

            _tmpModifierObservedStats = new(allocator);
            _tmpStatObservers = new(allocator);
            _tmpGlobalUpdatedStats = new(allocator);
            _tmpSameEntityUpdatedStats = new(allocator);

            _modifierStackRef = new(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatWorldData(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            _statChangeEvents = new(initialCapacity, allocator);
            _modifierTriggerEvents = new(initialCapacity, allocator);

            _tmpModifierObservedStats = new(initialCapacity, allocator);
            _tmpStatObservers = new(initialCapacity, allocator);
            _tmpGlobalUpdatedStats = new(initialCapacity, allocator);
            _tmpSameEntityUpdatedStats = new(initialCapacity, allocator);

            _modifierStackRef = new(allocator);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _statChangeEvents.IsCreated;
        }

        public void SetStatModifierStack(in TStatModifierStack stack)
        {
            _modifierStackRef.Value = stack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearStatChangeEvents()
        {
            _statChangeEvents.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<StatChangeEvent<TValuePair>> GetStatChangeEvents(
            AllocatorManager.AllocatorHandle allocator
        )
        {
            return _statChangeEvents.ToArray(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void GetStatChangeEvents(NativeList<StatChangeEvent<TValuePair>> result)
        {
            result.AddRange(_statChangeEvents.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddStatChangeEvent(in StatChangeEvent<TValuePair> evt)
        {
            _statChangeEvents.Add(evt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearModifierTriggerEvents()
        {
            _modifierTriggerEvents.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<ModifierTriggerEvent<TValuePair, TStat, TStatModifier, TStatModifierStack>>
            GetModifierTriggerEvents(AllocatorManager.AllocatorHandle allocator)
        {
            return _modifierTriggerEvents.ToArray(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void GetModifierTriggerEvents(
            NativeList<ModifierTriggerEvent<TValuePair, TStat, TStatModifier, TStatModifierStack>> result
        )
        {
            result.AddRange(_modifierTriggerEvents.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddModifierTriggerEvent(
            in ModifierTriggerEvent<TValuePair, TStat, TStatModifier, TStatModifierStack> evt
        )
        {
            _modifierTriggerEvents.Add(evt);
        }

        public void Clear()
        {
            ClearStatChangeEvents();
            ClearModifierTriggerEvents();

            _tmpModifierObservedStats.Clear();
            _tmpStatObservers.Clear();
            _tmpGlobalUpdatedStats.Clear();
            _tmpSameEntityUpdatedStats.Clear();

            _modifierStackRef.Value = default;
        }

        public void Dispose()
        {
            if (IsCreated == false)
            {
                return;
            }

            _statChangeEvents.Dispose();
            _modifierTriggerEvents.Dispose();
            _tmpModifierObservedStats.Dispose();
            _tmpStatObservers.Dispose();
            _tmpGlobalUpdatedStats.Dispose();
            _tmpSameEntityUpdatedStats.Dispose();
            _modifierStackRef.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (IsCreated == false)
            {
                return inputDeps;
            }

            var handles = new NativeArray<JobHandle>(7, Allocator.Temp);
            handles[0] = _statChangeEvents.Dispose(inputDeps);
            handles[1] = _modifierTriggerEvents.Dispose(inputDeps);
            handles[2] = _tmpModifierObservedStats.Dispose(inputDeps);
            handles[3] = _tmpStatObservers.Dispose(inputDeps);
            handles[4] = _tmpGlobalUpdatedStats.Dispose(inputDeps);
            handles[5] = _tmpSameEntityUpdatedStats.Dispose(inputDeps);
            handles[6] = _modifierStackRef.Dispose(inputDeps);

            return JobHandle.CombineDependencies(handles);
        }
    }
}
