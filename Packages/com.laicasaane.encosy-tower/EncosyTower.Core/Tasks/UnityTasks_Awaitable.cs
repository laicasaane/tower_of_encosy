#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Tasks
{
    static partial class UnityTasks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable GetCompleted()
            => Awaitables.GetCompleted();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>()
            => Awaitables.GetCompleted<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>(T value)
            => Awaitables.GetCompleted(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable NextFrameAsync(CancellationToken token)
            => Awaitable.NextFrameAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WaitUntil(Func<bool> predicate, CancellationToken token)
            => Awaitables.WaitUntil(predicate, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WaitUntil<T>(T state, Func<T, bool> predicate, CancellationToken token)
            => Awaitables.WaitUntil<T>(state, predicate, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WaitWhile<T>(T state, Func<T, bool> predicate, CancellationToken token)
            => Awaitables.WaitWhile<T>(state, predicate, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WhenAll(Awaitable[] tasks)
            => Awaitables.WhenAll(tasks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T[]> WhenAll<T>(Awaitable<T>[] tasks)
            => Awaitables.WhenAll(tasks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Awaitable self)
            => Awaitables.Forget(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this Awaitable<T> self)
            => Awaitables.Forget<T>(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run([NotNull] this Awaitable self)
            => Awaitables.Run(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run([NotNull] this IEnumerable<Awaitable> list)
            => Awaitables.Run(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WithContinuation([NotNull] this Awaitable self, [NotNull] Action continuation)
            => Awaitables.WithContinuation(self, continuation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ContinueWith([NotNull] this Awaitable self, [NotNull] Action continuation)
            => Awaitables.ContinueWith(self, continuation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable AsUnityTask<T>([NotNull] this Awaitable<T> self)
            => await self;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable AsUnityTask(
              [NotNull] this Task task
            , bool useCurrentSynchronizationContext = true
        )
        {
            return Awaitables.AsAwaitable(task, useCurrentSynchronizationContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> AsUnityTask<T>(
              [NotNull] this Task<T> task
            , bool useCurrentSynchronizationContext = true
        )
        {
            return Awaitables.AsAwaitable(task, useCurrentSynchronizationContext);
        }
    }
}

#endif
