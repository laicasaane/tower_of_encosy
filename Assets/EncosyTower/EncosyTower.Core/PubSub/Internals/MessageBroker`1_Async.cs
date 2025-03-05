#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial class MessageBroker<TMessage>
    {
        private ArrayPool<UnityTask> _taskArrayPool;

        public ArrayPool<UnityTask> TaskArrayPool
        {
            get => _taskArrayPool ?? ArrayPool<UnityTask>.Shared;
            set => _taskArrayPool = value ?? throw new ArgumentNullException(nameof(value));
        }

        public async UnityTask PublishAsync(
              TMessage message
            , PublishingContext context
            , CancellationToken token
            , ILogger logger
        )
        {
            var handlerListList = GetHandlerListList(logger);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                handlerListList.GetBufferUnsafe(out var handlerListArray, out var length);

                for (var i = 0; i < length; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await PublishAsync(handlerListArray[i], message, context, token, TaskArrayPool, logger);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                logger?.LogException(ex);
            }
            finally
#endif
            {
                Dispose(handlerListList);
            }
        }

        private static async UnityTask PublishAsync(
              FasterList<IHandler<TMessage>> handlerList
            , TMessage message
            , PublishingContext context
            , CancellationToken token
            , ArrayPool<UnityTask> taskArrayPool
            , ILogger logger
        )
        {
            if (handlerList.Count < 1)
            {
                return;
            }

            var tasks = taskArrayPool.Rent(handlerList.Count);
            ToTasks(handlerList, message, context, token, logger, tasks);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                await UnityTasks.WhenAll(tasks);
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                logger?.LogException(ex);
            }
            finally
#endif
            {
                taskArrayPool.Return(tasks);
            }

            static void ToTasks(
                  FasterList<IHandler<TMessage>> handlerList
                , TMessage message
                , PublishingContext context
                , CancellationToken token
                , ILogger logger
                , UnityTask[] result
            )
            {
                var handlers = handlerList.AsReadOnlySpan();
                var handlersLength = handlerList.Count;

                for (var i = 0; i < handlersLength; i++)
                {
#if __ENCOSY_VALIDATION__
                    try
#endif
                    {
                        result[i] = handlers[i]?.Handle(message, context, token) ?? UnityTasks.GetCompleted();
                    }
#if __ENCOSY_VALIDATION__
                    catch (Exception ex)
                    {
                        result[i] = UnityTasks.GetCompleted();
                        logger?.LogException(ex);
                    }
#endif
                }
            }
        }
    }
}

#endif
