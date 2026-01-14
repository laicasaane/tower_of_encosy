#if UNITASK

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Tasks
{
    static partial class UnityTasks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask GetCompleted()
            => UniTask.CompletedTask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<T> GetCompleted<T>()
            => UniTask.FromResult<T>(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<T> GetCompleted<T>(T value)
            => UniTask.FromResult(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask NextFrameAsync(CancellationToken token)
            => UniTask.NextFrame(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntil(Func<bool> predicate, CancellationToken token)
            => UniTask.WaitUntil(predicate, cancellationToken: token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntil<T>(T state, Func<T, bool> predicate, CancellationToken token)
            => UniTask.WaitUntil<T>(state, predicate, cancellationToken: token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitWhile<T>(T state, Func<T, bool> predicate, CancellationToken token)
            => UniTask.WaitWhile<T>(state, predicate, cancellationToken: token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WhenAll(UniTask[] tasks)
            => UniTask.WhenAll(tasks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<T[]> WhenAll<T>(UniTask<T>[] tasks)
            => UniTask.WhenAll(tasks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this UniTask self)
            => UniTaskExtensions.Forget(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget<T>(this UniTask<T> self)
            => UniTaskExtensions.Forget<T>(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask AsUnityTask<T>(this UniTask<T> self)
            => self;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask AsUnityTask(
              [NotNull] this Task task
            , bool useCurrentSynchronizationContext = true
        )
        {
            await task.AsUniTask(useCurrentSynchronizationContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<T> AsUnityTask<T>(
              [NotNull] this Task<T> task
            , bool useCurrentSynchronizationContext = true
        )
        {
            return await task.AsUniTask(useCurrentSynchronizationContext);
        }
    }
}

#endif
