#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions.Unsafe;
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

        public async UnityTask PublishAsync(TMessage message, PublishingContext context)
        {
            var handlerListList = GetHandlerListList(context.Logger);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                var taskArrayPool = TaskArrayPool;
                handlerListList.GetBufferUnsafe(out var handlerListArray, out var length);

                for (var i = 0; i < length; i++)
                {
                    if (context.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    await PublishAsync(handlerListArray[i], message, context, taskArrayPool);

                    if (context.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                context.Logger.LogException(ex);
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
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            if (handlerList.Count < 1)
            {
                return;
            }

            var tasks = taskArrayPool.Rent(handlerList.Count);

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                GetTasks(handlerList, message, context, tasks);

                await UnityTasks.WhenAll(tasks);
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                context.Logger.LogException(ex);
            }
            finally
#endif
            {
                taskArrayPool.Return(tasks, true);
            }
        }

        private static void GetTasks(
              FasterList<IHandler<TMessage>> handlerList
            , TMessage message
            , PublishingContext context
            , UnityTask[] resultTasks
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
                    resultTasks[i] = handlers[i]?.Handle(message, context) ?? UnityTasks.GetCompleted();
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    resultTasks[i] = UnityTasks.GetCompleted();
                    context.Logger.LogException(ex);
                }
#endif
            }
        }
    }
}

#endif
