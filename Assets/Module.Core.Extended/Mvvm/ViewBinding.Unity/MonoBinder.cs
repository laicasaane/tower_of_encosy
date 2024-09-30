using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Module.Core.Buffers;
using Module.Core.Collections;
using Module.Core.Logging;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    [Serializable]
    public abstract partial class MonoBinder
    {
        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        internal MonoBinding[] _presetBindings;

        public abstract void Initialize();
    }

    [Serializable]
    public abstract partial class MonoBinder<T> : MonoBinder
        where T : UnityEngine.Object
    {
        [SerializeField]
        [HideInInspector]
        internal T[] _presetTargets;

        private TargetList<T> _targets;
        private BindingList<T> _bindings;

        protected TargetList<T> Targets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _targets ??= new(this);
        }

        protected BindingList<T> Bindings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bindings ??= new(this);
        }

        public sealed override void Initialize()
        {

        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeNotMatch(UnityEngine.Object value)
        {
            DevLoggerAPI.LogError(value, $"Expected type {typeof(T)}, but received {value.GetType()}");
        }
    }

    public sealed class TargetList<T> : StatelessList<TargetBuffer<T>, T>
        where T : UnityEngine.Object
    {
        public TargetList(MonoBinder<T> binder) : base(new(binder))
        {
        }
    }

    public sealed class TargetBuffer<T> : BufferBase<T>
        where T : UnityEngine.Object
    {
        private readonly MonoBinder<T> _binder;

        public TargetBuffer(MonoBinder<T> binder)
        {
            _binder = binder;
        }

        public override ref T[] Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _binder._presetTargets;
        }
    }

    public sealed class BindingList<T> : StatelessList<BindingBuffer<T>, MonoBinding>
        where T : UnityEngine.Object
    {
        public BindingList(MonoBinder<T> binder) : base(new(binder))
        {
        }
    }

    public sealed class BindingBuffer<T> : BufferBase<MonoBinding>
        where T : UnityEngine.Object
    {
        private readonly MonoBinder<T> _binder;

        public BindingBuffer(MonoBinder<T> binder)
        {
            _binder = binder;
        }

        public override ref MonoBinding[] Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _binder._presetBindings;
        }
    }
}
