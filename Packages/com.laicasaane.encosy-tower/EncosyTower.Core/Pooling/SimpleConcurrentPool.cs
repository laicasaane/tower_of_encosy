using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EncosyTower.Pooling
{
    public class SimpleConcurrentPool<T> : IDisposable
        where T : class
    {
        private readonly ConcurrentQueue<T> _items = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnRent;
        private readonly Action<T> _actionOnReturn;

        private T _fastItem;

        public SimpleConcurrentPool(
              [NotNull] Func<T> createFunc
            , Action<T> actionOnRent = null
            , Action<T> actionOnReturn = null
        )
        {
            _createFunc = createFunc;
            _actionOnRent = actionOnRent;
            _actionOnReturn = actionOnReturn;
        }

        public void Dispose()
        {
            _items.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Rent()
        {
            var value = _fastItem;

            if (value != null && Interlocked.CompareExchange(ref _fastItem, null, value) == value)
            {
                _actionOnRent?.Invoke(value);
                return value;
            }

            if (_items.TryDequeue(out value))
            {
                _actionOnRent?.Invoke(value);
                return value;
            }

            return _createFunc();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T value)
        {
            if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, value, null) != null)
            {
                _actionOnReturn?.Invoke(value);
                _items.Enqueue(value);
            }
        }
    }
}
