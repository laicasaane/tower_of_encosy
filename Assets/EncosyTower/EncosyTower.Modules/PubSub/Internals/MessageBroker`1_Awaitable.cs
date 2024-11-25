#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Threading;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Collections.Unsafe;
using UnityEngine;

namespace EncosyTower.Modules.PubSub.Internals
{
    partial class MessageBroker<TMessage>
    {
        private AwaitableCompletionSource _completionSource;
        private CappedArrayPool<Awaitable> _taskArrayPool;

        internal CappedArrayPool<Awaitable> TaskArrayPool
        {
            get => _taskArrayPool ?? CappedArrayPool<Awaitable>.Shared8Limit;
            set => _taskArrayPool = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal AwaitableCompletionSource CompletionSource
        {
            get
            {
                if (_completionSource == null)
                {
                    _completionSource = new();
                    _completionSource.SetResult();
                }

                return _completionSource;
            }
        }

        public async Awaitable PublishAsync(
              TMessage message
            , PublishingContext context
            , CancellationToken token
            , EncosyTower.Modules.Logging.ILogger logger
        )
        {
            var handlersArray = GetHandlerListList(logger);
            var completionSource = CompletionSource;

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                handlersArray.GetBufferUnsafe(out var handlers, out var length);

                for (var i = 0; i < length; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await PublishAsync(handlers[i], message, context, token, TaskArrayPool, completionSource, logger);

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
                Dispose(handlersArray);
            }
        }

        private static async Awaitable PublishAsync(
              FasterList<IHandler<TMessage>> handlerList
            , TMessage message
            , PublishingContext context
            , CancellationToken token
            , CappedArrayPool<Awaitable> taskArrayPool
            , AwaitableCompletionSource completionSource
            , EncosyTower.Modules.Logging.ILogger logger
        )
        {
            if (handlerList.Count < 1)
            {
                return;
            }

            var tasks = taskArrayPool.Rent(handlerList.Count);
            ToTasks(handlerList, message, context, token, logger, tasks, completionSource);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                await Awaitables.WhenAll(tasks);
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
                , EncosyTower.Modules.Logging.ILogger logger
                , Awaitable[] result
                , AwaitableCompletionSource completionSource
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
                        result[i] = handlers[i]?.Handle(message, context, token) ?? completionSource.Awaitable;
                    }
#if __ENCOSY_VALIDATION__
                    catch (Exception ex)
                    {
                        result[i] = completionSource.Awaitable;
                        logger?.LogException(ex);
                    }
#endif
                }
            }
        }
    }
}

#endif
