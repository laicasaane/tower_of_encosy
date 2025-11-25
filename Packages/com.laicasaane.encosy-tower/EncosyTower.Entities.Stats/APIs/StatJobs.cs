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

namespace EncosyTower.Entities.Stats
{
    using System;
    using Unity.Collections;

    /// <summary>
    /// Useful for making fast stat changes, potentially in parallel,
    /// and then deferring the stats update to a later single-thread job
    /// NOTE: clears the list.
    /// </summary>
    public struct DeferredUpdateStatListJob<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        public StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statAccessor;
        public StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> statWorldData;
        public NativeList<StatHandle> statsToUpdate;

        public void Execute()
        {
            for (int i = 0; i < statsToUpdate.Length; i++)
            {
                statAccessor.TryUpdateStat(statsToUpdate[i], ref statWorldData);
            }

            statsToUpdate.Clear();
        }
    }

    /// <summary>
    /// Useful for making fast stat changes, potentially in parallel,
    /// and then deferring the stats update to a later single-thread job.
    /// NOTE: clears the queue.
    /// </summary>
    public struct DeferredUpdateStatQueueJob<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        public StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statAccessor;
        public StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> statWorldData;
        public NativeQueue<StatHandle> statsToUpdate;

        public void Execute()
        {
            while (statsToUpdate.TryDequeue(out StatHandle statHandle))
            {
                statAccessor.TryUpdateStat(statHandle, ref statWorldData);
            }

            statsToUpdate.Clear();
        }
    }

    /// <summary>
    /// Useful for making fast stat changes, potentially in parallel,
    /// and then deferring the stats update to a later single-thread job.
    /// NOTE: you must dispose the stream afterwards.
    /// </summary>
    public struct DeferredUpdateStatStreamJob<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        public StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statAccessor;
        public StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> statWorldData;
        public NativeStream.Reader statsToUpdate;

        public void Execute()
        {
            for (int i = 0; i < statsToUpdate.ForEachCount; i++)
            {
                statsToUpdate.BeginForEachIndex(i);

                while (statsToUpdate.RemainingItemCount > 0)
                {
                    StatHandle statHandle = statsToUpdate.Read<StatHandle>();
                    statAccessor.TryUpdateStat(statHandle, ref statWorldData);
                }

                statsToUpdate.EndForEachIndex();
            }
        }
    }
}

#if LATIOS_FRAMEWORK

namespace EncosyTower.Entities.Stats
{
    using System;
    using Latios.Unsafe;

    /// <summary>
    /// Useful for making fast stat changes, potentially in parallel,
    /// and then deferring the stats update to a later single-thread job.
    /// NOTE: you must dispose the stream afterwards.
    /// </summary>
    public struct DeferredUpdateStatUnsafeBlockListJob<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer>
        where TValuePair : unmanaged, IStatValuePair, IEquatable<TValuePair>
        where TStat : unmanaged, IStat<TValuePair>
        where TStatModifier : unmanaged, IStatModifier<TValuePair, TStat, TStatModifierStack>
        where TStatModifierStack : unmanaged, IStatModifierStack<TValuePair, TStat>
        where TStatObserver : unmanaged, IStatObserver
        where TValuePairComposer : unmanaged, IStatValuePairComposer<TValuePair>
    {
        public StatAccessor<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer> statAccessor;
        public StatWorldData<TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver> statWorldData;
        public UnsafeParallelBlockList<StatHandle> statsToUpdate;

        public void Execute()
        {
            if (statsToUpdate.Count() < 1)
            {
                return;
            }

            foreach (var statHandle in statsToUpdate)
            {
                statAccessor.TryUpdateStat(statHandle, ref statWorldData);
            }
        }
    }
}

#endif
