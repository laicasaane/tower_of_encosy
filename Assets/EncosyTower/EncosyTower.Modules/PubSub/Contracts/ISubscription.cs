#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.PubSub
{
    public interface ISubscription : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unsubscribe()
            => Dispose();
    }
}

#endif
