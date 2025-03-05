#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        public static Awaitable NextFrameAsync(CancellationToken token)
            => Awaitable.NextFrameAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WaitUntil<T>(T state, Func<T, bool> predicate, CancellationToken token)
            => Awaitables.WaitUntil<T>(state, predicate, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WhenAll(Awaitable[] tasks)
            => Awaitables.WhenAll(tasks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Awaitable self)
            => Awaitables.Forget(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this Awaitable<T> self)
            => Awaitables.Forget<T>(self);
    }
}

#endif
