#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using Module.Core.Processing.Internals;
using Module.Core.Vaults;

namespace Module.Core.Processing
{
    public sealed class Processor : IDisposable
    {
        private readonly SingletonVault<ProcessHandlerMapBase> _maps = new();

        public void Dispose()
        {
            _maps.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessHub<GlobalScope> Global()
            => Scope(default(GlobalScope));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessHub<TScope> Scope<TScope>()
            where TScope : struct
            => Scope<TScope>(default);

        public ProcessHub<TScope> Scope<TScope>(TScope scope)
        {
            _maps.TryGetOrAdd(out ProcessHandlerMap<TScope> map);
            return new(scope, map.Scope(scope));
        }
    }
}

#endif
