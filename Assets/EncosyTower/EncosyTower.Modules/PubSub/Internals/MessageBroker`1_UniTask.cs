#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Collections.Unsafe;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub.Internals
{
    partial class MessageBroker<TMessage>
    {
        private CappedArrayPool<UniTask> _taskArrayPool;

        public CappedArrayPool<UniTask> TaskArrayPool
        {
            get => _taskArrayPool ?? CappedArrayPool<UniTask>.Shared8Limit;
            set => _taskArrayPool = value ?? throw new ArgumentNullException(nameof(value));
        }

        public async UniTask PublishAsync(
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

        private static async UniTask PublishAsync(
              FasterList<IHandler<TMessage>> handlerList
            , TMessage message
            , PublishingContext context
            , CancellationToken token
            , CappedArrayPool<UniTask> taskArrayPool
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
                await UniTask.WhenAll(tasks);
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
                , UniTask[] result
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
                        result[i] = handlers[i]?.Handle(message, context, token) ?? UniTask.CompletedTask;
                    }
#if __ENCOSY_VALIDATION__
                    catch (Exception ex)
                    {
                        result[i] = UniTask.CompletedTask;
                        logger?.LogException(ex);
                    }
#endif
                }
            }
        }
    }
}

#endif
