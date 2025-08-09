#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Processing.Internals;
using EncosyTower.Types;

namespace EncosyTower.Processing
{
    public readonly struct ProcessRegistry : IDisposable
    {
        private readonly WeakReference<ProcessHandlerMap> _map;
        private readonly Option<TypeId> _id;

        internal ProcessRegistry([NotNull] ProcessHandlerMap map, Option<TypeId> id)
        {
            _map = new WeakReference<ProcessHandlerMap>(map);
            _id = id;
        }

        public void Unregister()
        {
            if (_map != null && _id.TryGetValue(out var id) && _map.TryGetTarget(out var map))
            {
                map.Unregister(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
            => Unregister();
    }
}

#endif
