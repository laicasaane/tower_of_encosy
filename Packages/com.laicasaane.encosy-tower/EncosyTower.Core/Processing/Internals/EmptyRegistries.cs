#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections;
using System.Collections.Generic;

namespace EncosyTower.Processing.Internals
{
    internal sealed class EmptyRegistries : ICollection<ProcessRegistry>
    {
        public static readonly EmptyRegistries Default = new();

        private EmptyRegistries() { }

        public int Count => 0;

        public bool IsReadOnly => true;

        public void Add(ProcessRegistry item) { }

        public void Clear() { }

        public bool Contains(ProcessRegistry item) => false;

        public void CopyTo(ProcessRegistry[] array, int arrayIndex) { }

        public bool Remove(ProcessRegistry item) => true;

        public Enumerator GetEnumerator()
            => new();

        IEnumerator<ProcessRegistry> IEnumerable<ProcessRegistry>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public readonly struct Enumerator : IEnumerator<ProcessRegistry>
        {
            public ProcessRegistry Current => default;

            public void Dispose() { }

            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => Current;
        }
    }
}

#endif
