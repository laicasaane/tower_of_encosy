#if UNITY_6000_0_OR_NEWER

#if !DEBUG && !ENABLE_UNITY_COLLECTIONS_CHECKS && !UNITY_DOTS_DEBUG
#define DISABLE_CHECKS
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;
using UnityEngine;

namespace EncosyTower.Modules
{
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
            => Completed<T>.Awaitable;

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// the <paramref name="condition"/> returns true.
        /// </summary>
        public static async Awaitable WaitUntil(
              Func<bool> condition
            , CancellationToken token = default
        )
        {
            Checks.IsTrue(condition != null);

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
              TState state
            , Func<TState, bool> condition
            , CancellationToken token = default
        )
        {
            Checks.IsTrue(condition != null);

            while (condition(state) == false)
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// all of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WhenAll(params Awaitable[] awaitables)
        {
            if (awaitables == null || awaitables.Length < 1)
            {
                return GetCompleted();
            }

            if (awaitables.Length == 1)
            {
                Checks.IsTrue(awaitables[0] != null);
                return awaitables[0];
            }

            return AwaitAll(awaitables);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async Awaitable AwaitAll(Awaitable[] awaitables)
            {
                for (var i = 0; i < awaitables.Length; i++)
                {
                    Checks.IsTrue(awaitables[i] != null);
                    await awaitables[i];
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="Awaitable"/> that will complete when
        /// any of the supplied <see cref="Awaitable"/> have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WhenAny(params Awaitable[] awaitables)
        {
            if (awaitables == null || awaitables.Length < 1)
            {
                return GetCompleted();
            }

            if (awaitables.Length == 1)
            {
                Checks.IsTrue(awaitables[0] != null);
                return awaitables[0];
            }

            return AwaitAny(awaitables);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async Awaitable AwaitAny(Awaitable[] awaitables)
            {
                var awaited = new AwaitableCompletionSource();

                for (var i = 0; i < awaitables.Length; i++)
                {
                    Checks.IsTrue(awaitables[i] != null);
                    Run(WaitCompletion(awaitables[i], awaited));
                }

                if (awaited != null)
                {
                    await awaited.Awaitable;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async Awaitable WaitCompletion(Awaitable awaitable, AwaitableCompletionSource awaited)
            {
                await awaitable;
                awaited?.SetResult();
            }
        }

        /// <summary>
        /// Dismiss warning on fire-and-forget calls.
        /// </summary>
        public static void Forget(this Awaitable self)
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
#if DISABLE_CHECKS
#if UNITY_EDITOR
                catch (Exception ex)
                {
                    DevLoggerAPI.LogException(ex);
                }
#else
                catch { }
#endif
#else
                catch (Exception ex)
                {
                    RuntimeLoggerAPI.LogException(ex);
                }
#endif
            }
        }

        /// <summary>
        /// Dismiss warning on fire-and-forget calls.
        /// </summary>
        public static void Forget<TResult>(this Awaitable<TResult> self)
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
#if DISABLE_CHECKS
#if UNITY_EDITOR
                catch (Exception ex)
                {
                    DevLoggerAPI.LogException(ex);
                }
#else
                catch { }
#endif
#else
                catch (Exception ex)
                {
                    RuntimeLoggerAPI.LogException(ex);
                }
#endif
            }
        }

        /// <summary>
        /// Runs an <see cref="Awaitable"/> without awaiting for it.
        /// On completion, rethrows any exception raised during execution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run(this Awaitable self)
        {
#if !DISABLE_CHECKS
            Checks.IsTrue(self != null);
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
        public static void Run(IEnumerable<Awaitable> list)
        {
            Checks.IsTrue(list != null);

            foreach (var item in list)
            {
                Checks.IsTrue(item != null);

                var awaiter = item.GetAwaiter();
                awaiter.OnCompleted(() => awaiter.GetResult());
            }
        }

        /// <summary>
        /// Creates an <see cref="Awaitable"/> that first await the supplied <see cref="Awaitable"/>,
        /// then execute the continuation, once completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable WithContinuation(this Awaitable self, Action continuation)
        {
            Checks.IsTrue(self != null);
            Checks.IsTrue(continuation != null);

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
        public static void ContinueWith(this Awaitable self, Action continuation)
        {
            Checks.IsTrue(self != null);
            Checks.IsTrue(continuation != null);

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

#if !DISABLE_CHECKS
        private static readonly FieldInfo s_continuationFieldInfo = typeof(Awaitable)
            .GetField("_continuation", BindingFlags.NonPublic | BindingFlags.Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasContinuation(Awaitable awaitable)
            => s_continuationFieldInfo.GetValue(awaitable) != null;
#endif

        private static class Completed
        {
            private readonly static AwaitableCompletionSource s_source;

            public static Awaitable Awaitable
            {
                get
                {
                    // https://discussions.unity.com/t/awaitable-equivalent-of-task-completedtask/1546128/4
                    s_source.SetResult();
                    var awaitable = s_source.Awaitable;
                    s_source.Reset();
                    return awaitable;
                }
            }
        }

        private static class Completed<T>
        {
            private readonly static AwaitableCompletionSource<T> s_source;

            public static Awaitable<T> Awaitable
            {
                get
                {
                    s_source.SetResult(default);
                    var awaitable = s_source.Awaitable;
                    s_source.Reset();
                    return awaitable;
                }
            }
        }
    }
}

#endif
