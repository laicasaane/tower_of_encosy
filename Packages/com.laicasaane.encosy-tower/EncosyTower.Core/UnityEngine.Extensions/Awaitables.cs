#if UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Debugging;
using EncosyTower.Logging;
using UnityEngine;
using UnityEngine.Pool;

namespace EncosyTower.UnityExtensions
{
    /// <summary>
    /// <see cref="WhenAll"/> APIs are implemented based on
    /// <see href="https://discussions.unity.com/t/awaitables-whenall/1551331/21"/>.
    /// <br/>
    /// <see cref="GetCompleted"/> APIs are implemented based on
    /// <see href="https://discussions.unity.com/t/awaitable-equivalent-of-task-completedtask/1546128/4"/>.
    /// </summary>
    public static class Awaitables
    {
        /// <summary>
        /// Gets an <see cref="Awaitable"/> that has already completed successfully.
        /// </summary>
        public static Awaitable GetCompleted()
            => Completed.Awaitable;

        /// <summary>
        /// Gets an <see cref="Awaitable{T}"/> that has already completed successfully.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>()
            => Completed<T>.Awaitable(default);

        /// <summary>
        /// Gets an <see cref="Awaitable{T}"/> that has already completed successfully.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> GetCompleted<T>(T value)
            => Completed<T>.Awaitable(value);

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// the <paramref name="condition"/> returns true.
        /// </summary>
        public static async Awaitable WaitUntil(
              [NotNull] Func<bool> condition
            , CancellationToken token = default
        )
        {
            while (condition() == false)
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// the <paramref name="condition"/> returns true.
        /// </summary>
        public static async Awaitable WaitUntil<TState>(
              [NotNull] TState state
            , [NotNull] Func<TState, bool> condition
            , CancellationToken token = default
        )
        {
            while (condition(state) == false)
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// the <paramref name="condition"/> returns false.
        /// </summary>
        public static async Awaitable WaitWhile(
              [NotNull] Func<bool> condition
            , CancellationToken token = default
        )
        {
            while (condition())
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// the <paramref name="condition"/> returns false.
        /// </summary>
        public static async Awaitable WaitWhile<TState>(
              [NotNull] TState state
            , [NotNull] Func<TState, bool> condition
            , CancellationToken token = default
        )
        {
            while (condition(state))
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable WhenAll([NotNull] params Awaitable[] awaitables)
        {
            Option<Exception> exceptionOpt = Option.None;

            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(awaitables.Length);

                await CaptureExceptionsAsync(awaitables, exceptions);

                if (exceptions.Count > 0)
                {
                    exceptionOpt = CreateAggregateException(exceptions);
                }
            }

            if (exceptionOpt.TryGetValue(out var ex))
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable WhenAll([NotNull] IEnumerable<Awaitable> awaitables)
        {
            Option<Exception> exceptionOpt = Option.None;

            using (ListPool<Exception>.Get(out var exceptions))
            {
                if (awaitables.TryGetCountFast(out var count))
                {
                    exceptions.IncreaseCapacityTo(count);
                }

                await CaptureExceptionsAsync(awaitables, exceptions);

                if (exceptions.Count > 0)
                {
                    exceptionOpt = CreateAggregateException(exceptions);
                }
            }

            if (exceptionOpt.TryGetValue(out var ex))
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<T[]> WhenAll<T>([NotNull] params Awaitable<T>[] awaitables)
        {
            Result<T[], Exception> result = default;

            using (ListPool<T>.Get(out var results))
            using (ListPool<Exception>.Get(out var exceptions))
            {
                results.IncreaseCapacityTo(awaitables.Length);
                exceptions.IncreaseCapacityTo(awaitables.Length);

                await CaptureExceptionsAsync(awaitables, results, exceptions);

                if (exceptions.Count > 0)
                {
                    result = CreateAggregateException(exceptions);
                }
                else
                {
                    result = results.ToArray();
                }
            }

            if (result.TryGetError(out var ex))
            {
                throw ex;
            }

            return result.GetValueOrDefault(Array.Empty<T>());
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<T[]> WhenAll<T>([NotNull] IEnumerable<Awaitable<T>> awaitables)
        {
            Result<T[], Exception> result = default;

            using (ListPool<T>.Get(out var results))
            using (ListPool<Exception>.Get(out var exceptions))
            {
                if (awaitables.TryGetCountFast(out var count))
                {
                    results.IncreaseCapacityTo(count);
                    exceptions.IncreaseCapacityTo(count);
                }

                await CaptureExceptionsAsync(awaitables, results, exceptions);

                if (exceptions.Count > 0)
                {
                    result = CreateAggregateException(exceptions);
                }
                else
                {
                    result = results.ToArray();
                }
            }

            if (result.TryGetError(out var ex))
            {
                throw ex;
            }

            return result.GetValueOrDefault(Array.Empty<T>());
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable WhenAll(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable awaitable2
        )
        {
            using (ListPool<Awaitable>.Get(out var awaitables))
            {
                awaitables.IncreaseCapacityTo(2);
                awaitables.Add(awaitable1);
                awaitables.Add(awaitable2);

                await WhenAll(awaitables);
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable WhenAll(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable awaitable2
            , [NotNull] Awaitable awaitable3
        )
        {
            using (ListPool<Awaitable>.Get(out var awaitables))
            {
                awaitables.IncreaseCapacityTo(3);
                awaitables.Add(awaitable1);
                awaitables.Add(awaitable2);
                awaitables.Add(awaitable3);

                await WhenAll(awaitables);
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable WhenAll(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable awaitable2
            , [NotNull] Awaitable awaitable3
            , [NotNull] Awaitable awaitable4
        )
        {
            using (ListPool<Awaitable>.Get(out var awaitables))
            {
                awaitables.IncreaseCapacityTo(4);
                awaitables.Add(awaitable1);
                awaitables.Add(awaitable2);
                awaitables.Add(awaitable3);
                awaitables.Add(awaitable4);

                await WhenAll(awaitables);
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> WhenAll<T>(
              [NotNull] Awaitable awaitable1
            , [NotNull] Awaitable<T> awaitable2
        )
        {
            return WhenAll(awaitable2, awaitable1);
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<T> WhenAll<T>(
              [NotNull] Awaitable<T> awaitable1
            , [NotNull] Awaitable awaitable2
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(2);

                var result = await CaptureExceptionsAsync(awaitable1, exceptions);

                await CaptureExceptionsAsync(awaitable2, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return result.GetValueOrThrow();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2)> WhenAll<T1, T2>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(2);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (result1.GetValueOrThrow(), result2.GetValueOrThrow());
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3)> WhenAll<T1, T2, T3>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(3);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (result1.GetValueOrThrow(), result2.GetValueOrThrow(), result3.GetValueOrThrow());
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(4);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
            , [NotNull] Awaitable<T5> awaitable5
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(5);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);
                var result5 = await CaptureExceptionsAsync(awaitable5, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                    , result5.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
            , [NotNull] Awaitable<T5> awaitable5
            , [NotNull] Awaitable<T6> awaitable6
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(6);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);
                var result5 = await CaptureExceptionsAsync(awaitable5, exceptions);
                var result6 = await CaptureExceptionsAsync(awaitable6, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                    , result5.GetValueOrThrow()
                    , result6.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
            , [NotNull] Awaitable<T5> awaitable5
            , [NotNull] Awaitable<T6> awaitable6
            , [NotNull] Awaitable<T7> awaitable7
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(7);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);
                var result5 = await CaptureExceptionsAsync(awaitable5, exceptions);
                var result6 = await CaptureExceptionsAsync(awaitable6, exceptions);
                var result7 = await CaptureExceptionsAsync(awaitable7, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                    , result5.GetValueOrThrow()
                    , result6.GetValueOrThrow()
                    , result7.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
            , [NotNull] Awaitable<T5> awaitable5
            , [NotNull] Awaitable<T6> awaitable6
            , [NotNull] Awaitable<T7> awaitable7
            , [NotNull] Awaitable<T8> awaitable8
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(8);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);
                var result5 = await CaptureExceptionsAsync(awaitable5, exceptions);
                var result6 = await CaptureExceptionsAsync(awaitable6, exceptions);
                var result7 = await CaptureExceptionsAsync(awaitable7, exceptions);
                var result8 = await CaptureExceptionsAsync(awaitable8, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                    , result5.GetValueOrThrow()
                    , result6.GetValueOrThrow()
                    , result7.GetValueOrThrow()
                    , result8.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        public static async Awaitable<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
              [NotNull] Awaitable<T1> awaitable1
            , [NotNull] Awaitable<T2> awaitable2
            , [NotNull] Awaitable<T3> awaitable3
            , [NotNull] Awaitable<T4> awaitable4
            , [NotNull] Awaitable<T5> awaitable5
            , [NotNull] Awaitable<T6> awaitable6
            , [NotNull] Awaitable<T7> awaitable7
            , [NotNull] Awaitable<T8> awaitable8
            , [NotNull] Awaitable<T9> awaitable9
        )
        {
            using (ListPool<Exception>.Get(out var exceptions))
            {
                exceptions.IncreaseCapacityTo(9);

                var result1 = await CaptureExceptionsAsync(awaitable1, exceptions);
                var result2 = await CaptureExceptionsAsync(awaitable2, exceptions);
                var result3 = await CaptureExceptionsAsync(awaitable3, exceptions);
                var result4 = await CaptureExceptionsAsync(awaitable4, exceptions);
                var result5 = await CaptureExceptionsAsync(awaitable5, exceptions);
                var result6 = await CaptureExceptionsAsync(awaitable6, exceptions);
                var result7 = await CaptureExceptionsAsync(awaitable7, exceptions);
                var result8 = await CaptureExceptionsAsync(awaitable8, exceptions);
                var result9 = await CaptureExceptionsAsync(awaitable9, exceptions);

                if (exceptions.Count > 0)
                {
                    throw CreateAggregateException(exceptions);
                }

                return (
                      result1.GetValueOrThrow()
                    , result2.GetValueOrThrow()
                    , result3.GetValueOrThrow()
                    , result4.GetValueOrThrow()
                    , result5.GetValueOrThrow()
                    , result6.GetValueOrThrow()
                    , result7.GetValueOrThrow()
                    , result8.GetValueOrThrow()
                    , result9.GetValueOrThrow()
                );
            }
        }

        /// <summary>
        /// Dismiss warning on fire-and-forget calls.
        /// </summary>
        public static void Forget([NotNull] Awaitable self)
        {
            var awaiter = self.GetAwaiter();

            if (awaiter.IsCompleted == false)
            {
                awaiter.OnCompleted(HandleLogException);
            }
            else
            {
                HandleLogException();
            }

            void HandleLogException()
            {
                try
                {
                    awaiter.GetResult();
                }
#if __ENCOSY_NO_VALIDATION__
#if UNITY_EDITOR
                catch (Exception ex)
                {
                    StaticDevLogger.LogException(ex);
                }
#else
                catch { }
#endif
#else
                catch (Exception ex)
                {
                    StaticLogger.LogException(ex);
                }
#endif
            }
        }

        /// <summary>
        /// Dismiss warning on fire-and-forget calls.
        /// </summary>
        public static void Forget<T>([NotNull] Awaitable<T> self)
        {
            var awaiter = self.GetAwaiter();

            if (awaiter.IsCompleted == false)
            {
                awaiter.OnCompleted(LogHandleException);
            }
            else
            {
                LogHandleException();
            }

            void LogHandleException()
            {
                try
                {
                    _ = awaiter.GetResult();
                }
#if __ENCOSY_NO_VALIDATION__
#if UNITY_EDITOR
                catch (Exception ex)
                {
                    StaticDevLogger.LogException(ex);
                }
#else
                catch { }
#endif
#else
                catch (Exception ex)
                {
                    StaticLogger.LogException(ex);
                }
#endif
            }
        }

        /// <summary>
        /// Runs an <see cref="Awaitable"/> without awaiting for it.
        /// On completion, rethrows any exception raised during execution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run([NotNull] Awaitable self)
        {
#if !__ENCOSY_NO_VALIDATION__
            Checks.IsTrue(HasContinuation(self) == false, "Awaitable already have a continuation, is it already awaited?");
#endif

            var awaiter = self.GetAwaiter();
            awaiter.OnCompleted(() => awaiter.GetResult());
        }

        /// <summary>
        /// Runs multiple <see cref="Awaitable"/>s without awaiting for any.
        /// On completion, rethrow any exception raised during execution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run([NotNull] IEnumerable<Awaitable> list)
        {
            foreach (var item in list)
            {
                Checks.IsTrue(item != null, "An item of 'list' is null");

                var awaiter = item.GetAwaiter();
                awaiter.OnCompleted(() => awaiter.GetResult());
            }
        }

        /// <summary>
        /// Creates an <see cref="Awaitable"/> that first await the supplied <see cref="Awaitable"/>,
        /// then execute the continuation, once completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WithContinuation([NotNull] Awaitable self, [NotNull] Action continuation)
        {
            if (self.IsCompleted == false)
            {
                return AwaitAndContinue(self, continuation);
            }
            else
            {
                continuation();
                return self;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async Awaitable AwaitAndContinue(Awaitable self, Action continuation)
            {
                await self;
                continuation();
            }
        }

        /// <summary>
        /// Sets a continuation, executed once the <see cref="Awaitable"/> has completed.
        /// </summary>
        /// <remarks>
        /// Note that continuation will be overwritten if <see cref="Awaitable"/> is awaited.
        /// This is an unusual method to use, be sure what you are doing.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ContinueWith([NotNull] Awaitable self, [NotNull] Action continuation)
        {
            if (self.IsCompleted == false)
            {
                var awaiter = self.GetAwaiter();

                awaiter.OnCompleted(() => {
                    continuation();
                    awaiter.GetResult();
                });
            }
            else
            {
                continuation();
            }
        }

        public static Awaitable AsAwaitable(
              [NotNull] Task task
            , bool useCurrentSynchronizationContext = true
        )
        {
            var completionSource = new AwaitableCompletionSource();

            task.ContinueWith(
                  ContinueAction
                , completionSource
                , useCurrentSynchronizationContext
                    ? TaskScheduler.FromCurrentSynchronizationContext()
                    : TaskScheduler.Current
            );

            return completionSource.Awaitable;

            static void ContinueAction(Task task, object state)
            {
                var completionSource = (AwaitableCompletionSource)state;

                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        completionSource.TrySetCanceled();
                        break;

                    case TaskStatus.Faulted:
                        completionSource.TrySetException(task.Exception.InnerException ?? task.Exception);
                        break;

                    case TaskStatus.RanToCompletion:
                        completionSource.TrySetResult();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public static Awaitable<T> AsAwaitable<T>(
              [NotNull] Task<T> task
            , bool useCurrentSynchronizationContext = true
        )
        {
            var completionSource = new AwaitableCompletionSource<T>();

            task.ContinueWith(
                  ContinueAction
                , completionSource
                , useCurrentSynchronizationContext
                    ? TaskScheduler.FromCurrentSynchronizationContext()
                    : TaskScheduler.Current
            );

            return completionSource.Awaitable;

            static void ContinueAction(Task<T> task, object state)
            {
                var completionSource = (AwaitableCompletionSource<T>)state;

                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        completionSource.TrySetCanceled();
                        break;

                    case TaskStatus.Faulted:
                        completionSource.TrySetException(task.Exception.InnerException ?? task.Exception);
                        break;

                    case TaskStatus.RanToCompletion:
                        completionSource.TrySetResult(task.Result);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private static async Awaitable CaptureExceptionsAsync(
              IEnumerable<Awaitable> awaitables
            , List<Exception> exceptions
        )
        {
            foreach (var awaitable in awaitables)
            {
                await CaptureExceptionsAsync(awaitable, exceptions);
            }
        }

        private static async Awaitable CaptureExceptionsAsync<T>(
              IEnumerable<Awaitable<T>> awaitables
            , List<T> results
            , List<Exception> exceptions
        )
        {
            foreach (var awaitable in awaitables)
            {
                var resultOpt = await CaptureExceptionsAsync(awaitable, exceptions);

                if (resultOpt.TryGetValue(out var result))
                {
                    results.Add(result);
                }
            }
        }

        private static async Awaitable CaptureExceptionsAsync(
              Awaitable awaitable
            , List<Exception> exceptions
        )
        {
            try
            {
                await awaitable;
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        private static async Awaitable<Option<T>> CaptureExceptionsAsync<T>(
              Awaitable<T> awaitable
            , List<Exception> exceptions
        )
        {
            try
            {
                return await awaitable;
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
                return Option.None;
            }
        }

        private static Exception CreateAggregateException(List<Exception> exceptions)
        {
            if (exceptions.Any(static e => e is OperationCanceledException) == false)
            {
                return new AggregateException(exceptions).Flatten();
            }

            if (exceptions.All(static e => e is OperationCanceledException))
            {
                return new AggregateException(exceptions[0]);
            }

            return new AggregateException(exceptions.Where(static e => e is not OperationCanceledException)).Flatten();
        }

#if !__ENCOSY_NO_VALIDATION__
        private static readonly FieldInfo s_continuationFieldInfo = typeof(Awaitable)
            .GetField("_continuation", BindingFlags.NonPublic | BindingFlags.Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasContinuation(Awaitable awaitable)
            => s_continuationFieldInfo.GetValue(awaitable) != null;
#endif

        private static class Completed
        {
            private readonly static AwaitableCompletionSource s_source = new();

            public static Awaitable Awaitable
            {
                get
                {
                    s_source.SetResult();
                    var awaitable = s_source.Awaitable;
                    s_source.Reset();
                    return awaitable;
                }
            }
        }

        private static class Completed<T>
        {
            private readonly static AwaitableCompletionSource<T> s_source = new();

            public static Awaitable<T> Awaitable(T value)
            {
                s_source.SetResult(value);
                var awaitable = s_source.Awaitable;
                s_source.Reset();
                return awaitable;
            }
        }
    }
}

#endif
