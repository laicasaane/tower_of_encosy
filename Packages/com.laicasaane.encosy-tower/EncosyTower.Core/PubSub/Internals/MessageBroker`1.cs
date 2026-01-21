#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions.Unsafe;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class MessageBroker<TMessage> : IMessageBroker
    {
        private readonly FasterList<int> _ordering = new(1);
        private readonly Dictionary<int, ArrayMap<DelegateId, IHandler<TMessage>>> _orderToHandlerMap = new(1);

        private ArrayPool<UnityTask> _taskArrayPool;
        private long _refCount;

        public ArrayPool<UnityTask> TaskArrayPool
        {
            get => _taskArrayPool;
            set => _taskArrayPool = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool HasHandlers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ordering.Count > 0;
        }

        public bool IsCached => _refCount > 0;

        public Subscription<TMessage> Subscribe([NotNull] IHandler<TMessage> handler, int order, ILogger logger)
        {
            lock (_orderToHandlerMap)
            {
#if __ENCOSY_VALIDATION__
                try
#endif
                {
                    var orderToHandlerMap = _orderToHandlerMap;
                    var ordering = _ordering;

                    if (ordering.Contains(order) == false)
                    {
                        ordering.Add(order);
                        ordering.Sort(Comparer<int>.Default);
                    }

                    if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                    {
                        handlerMap = new ArrayMap<DelegateId, IHandler<TMessage>>();
                        orderToHandlerMap.Add(order, handlerMap);
                    }

                    if (handlerMap.TryAdd(handler.Id, handler))
                    {
                        return new Subscription<TMessage>(this, handler, order);
                    }
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    logger?.LogException(ex);
                }
#endif

                return Subscription<TMessage>.None;
            }
        }

        public void Dispose()
        {
            lock (_orderToHandlerMap)
            {
                var orderToHandlerMap = _orderToHandlerMap;

                foreach (var kvp in orderToHandlerMap)
                {
                    kvp.Value?.Dispose();
                }

                orderToHandlerMap.Clear();
            }
        }

        public void RemoveHandler(DelegateId id, int order)
        {
            lock (_orderToHandlerMap)
            {
                if (_orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                {
                    _ordering.Remove(order);
                    return;
                }

                handlerMap.Remove(id);

                if (handlerMap.Count < 1)
                {
                    _orderToHandlerMap.Remove(order);
                    _ordering.Remove(order);
                }
            }
        }

        public void OnCache()
        {
            checked
            {
                _refCount++;
            }
        }

        public void OnUncache()
        {
            _refCount = Math.Max(0, _refCount - 1);
        }

        /// <summary>
        /// Remove empty handler groups to optimize performance.
        /// </summary>
        public void Compress(ILogger logger)
        {
            lock (_orderToHandlerMap)
            {
#if __ENCOSY_VALIDATION__
                try
#endif
                {
                    var orderToHandlerMap = _orderToHandlerMap;
                    var ordering = _ordering;
                    var orders = ordering.AsSpan();

                    for (var i = orders.Length - 1; i >= 0; i--)
                    {
                        var order = orders[i];

                        if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false)
                        {
                            continue;
                        }

                        if (handlerMap.Count > 0)
                        {
                            continue;
                        }

                        orderToHandlerMap.Remove(order);
                        ordering.RemoveAt(i);
                    }
                }
#if __ENCOSY_VALIDATION__
                catch (Exception ex)
                {
                    logger?.LogException(ex);
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask> TryPublishAsync(TMessage message, PublishingContext context)
        {
            return HasHandlers ? PublishAsync(message, context) : Option.None;
        }

        private async UnityTask PublishAsync(TMessage message, PublishingContext context)
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

        private FasterList<FasterList<IHandler<TMessage>>> GetHandlerListList(ILogger logger)
        {
            var orderToHandlerMap = _orderToHandlerMap;
            var orders = _ordering.AsSpan();
            var handlerListList = GetHandlerListList();

#if __ENCOSY_VALIDATION__
            try
#endif
            {
                for (var i = orders.Length - 1; i >= 0; i--)
                {
                    var order = orders[i];

                    if (orderToHandlerMap.TryGetValue(order, out var handlerMap) == false
                        || handlerMap.Count < 1
                    )
                    {
                        continue;
                    }

                    var handlerArray = handlerMap.GetValues();
                    var handlerList = GetHandlerList();
                    var handlerSpan = handlerList.AddReplicateNoInit(handlerArray.Length);
                    handlerArray.CopyTo(handlerSpan);
                    handlerListList.Add(handlerList);
                }
            }
#if __ENCOSY_VALIDATION__
            catch (Exception ex)
            {
                logger?.LogException(ex);
            }
#endif

            return handlerListList;
        }

        private static void Dispose(FasterList<FasterList<IHandler<TMessage>>> handlersList)
        {
            if (handlersList == null)
            {
                return;
            }

            var handlersArray = handlersList.AsSpan();
            var handlersArrayLength = handlersArray.Length;

            for (var i = 0; i < handlersArrayLength; i++)
            {
                ref var handlers = ref handlersArray[i];

                if (handlers == null) continue;

                Release(handlers);

                handlers = default;
            }

            Release(handlersList);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FasterList<FasterList<IHandler<TMessage>>> GetHandlerListList()
            => FasterListPool<FasterList<IHandler<TMessage>>>.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Release(FasterList<FasterList<IHandler<TMessage>>> item)
            => FasterListPool<FasterList<IHandler<TMessage>>>.Release(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FasterList<IHandler<TMessage>> GetHandlerList()
            => FasterListPool<IHandler<TMessage>>.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Release(FasterList<IHandler<TMessage>> item)
            => FasterListPool<IHandler<TMessage>>.Release(item);
    }
}

#endif
