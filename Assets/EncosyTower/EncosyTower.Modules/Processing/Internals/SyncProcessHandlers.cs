#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Processing.Internals.Sync
{
    internal sealed class ProcessHandler<TRequest> : IProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Action<TRequest> _process;

        static ProcessHandler()
        {
            s_typeId = (TypeId)TypeId<Action<TRequest>>.Value;
        }

        public ProcessHandler(Action<TRequest> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process(TRequest arg)
        {
            _process(arg);
        }
    }

    internal sealed class ProcessHandler<TRequest, TResult> : IProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, TResult> _process;

        static ProcessHandler()
        {
            s_typeId = (TypeId)TypeId<Func<TRequest, TResult>>.Value;
        }

        public ProcessHandler(Func<TRequest, TResult> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Process(TRequest arg)
        {
            return _process(arg);
        }
    }
}

#endif
