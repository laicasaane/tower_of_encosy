#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Processing.Internals
{
    internal abstract class ProcessHandlerMapBase { }

    internal sealed class ProcessHandlerMap<TScope> : ProcessHandlerMapBase
    {
        private readonly Dictionary<TScope, ProcessHandlerMap> _map = new();

        public ProcessHandlerMap Scope(TScope scope)
        {
            if (_map.TryGetValue(scope, out var handler) == false)
            {
                _map[scope] = handler = new ProcessHandlerMap();
            }

            return handler;
        }
    }

    internal sealed class ProcessHandlerMap : ProcessHandlerMapBase
    {
        private readonly Dictionary<TypeId, IProcessHandler> _map = new();

        public Option<TypeId> Register(IProcessHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (_map.ContainsKey(handler.Id))
            {
                LogIfExist(handler);
                return default;
            }

            var id = handler.Id;

            _map[id] = handler;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister(TypeId id)
        {
            return _map.Remove(id);
        }

        public bool TryGet(TypeId id, out IProcessHandler handler)
        {
            if (_map.TryGetValue(id, out var result)
                && result is not null
            )
            {
                handler = result;
                return true;
            }

            handler = default;
            return false;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void LogIfExist(IProcessHandler handler)
        {
            Logging.DevLoggerAPI.LogWarning(
                $"A process handler of type {handler.Id.ToType()} has already been registered."
            );
        }
    }
}

#endif
