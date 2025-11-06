#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections;
using System.Collections.Generic;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class EmptySubscriptions : ICollection<ISubscription>
    {
        public static readonly EmptySubscriptions Default = new();

        private EmptySubscriptions() { }

        public int Count => 0;

        public bool IsReadOnly => true;

        public void Add(ISubscription item) { }

        public void Clear() { }

        public bool Contains(ISubscription item) => false;

        public void CopyTo(ISubscription[] array, int arrayIndex) { }

        public bool Remove(ISubscription item) => true;

        public Enumerator GetEnumerator()
            => new();

        IEnumerator<ISubscription> IEnumerable<ISubscription>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public readonly struct Enumerator : IEnumerator<ISubscription>
        {
            public ISubscription Current => default;

            public void Dispose() { }

            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => Current;
        }
    }
}

#endif
