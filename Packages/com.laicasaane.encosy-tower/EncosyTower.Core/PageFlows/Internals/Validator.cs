#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    internal static partial class Validator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetLogCurrentlyInTransition(IPageFlow flow)
            => $"{flow.GetType()} is currently in transition.";

        public static async UnityTaskBool ValidateTransitionAsync(
              [NotNull] IPageFlow flow
            , [NotNull] ILogger logger
            , PageAsyncOperation asyncOperation
            , CancellationToken token
        )
        {
            if (flow.IsInTransition == false)
            {
                return true;
            }

            switch (asyncOperation)
            {
                case PageAsyncOperation.Sequential:
                {
                    await UnityTasks.WaitWhile(flow, static state => state.IsInTransition, token);
                    return !token.IsCancellationRequested;
                }

                case PageAsyncOperation.Drop:
                {
                    return false;
                }

                case PageAsyncOperation.DropError:
                {
                    logger.LogError(GetLogCurrentlyInTransition(flow));
                    return false;
                }

                default:
                {
                    throw new InvalidOperationException(GetLogCurrentlyInTransition(flow));
                }
            }
        }
    }
}

#endif
