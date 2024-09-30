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
    /// <summary>
    /// Represents a collection of <see cref="MonoBinding"/>.
    /// </summary>
    [Serializable]
    public abstract partial class MonoBinder
    {
        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        internal MonoBinding[] _presetBindings;

        private BindingList _bindings;

        protected BindingList Bindings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bindings ??= new(this);
        }

        public abstract void Initialize();

        protected sealed class BindingList : StatelessList<BindingBuffer, MonoBinding>
        {
            public BindingList(MonoBinder binder) : base(new(binder)) { }
        }

        protected sealed class BindingBuffer : BufferBase<MonoBinding>
        {
            private readonly MonoBinder _binder;

            public BindingBuffer(MonoBinder binder)
            {
                _binder = binder;
                Count = Buffer.Length;
            }

            public override ref MonoBinding[] Buffer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _binder._presetBindings;
            }
        }
    }

    /// <summary>
    /// Represents a collection of <see cref="MonoBinding{T}"/>.
    /// </summary>
    /// <typeparam name="T">Must inherit from <see cref="UnityEngine.Object"/>.</typeparam>
    [Serializable]
    public abstract partial class MonoBinder<T> : MonoBinder
        where T : UnityEngine.Object
    {
        [SerializeField]
        [HideInInspector]
        internal T[] _presetTargets;

        private TargetList _targets;

        protected TargetList Targets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _targets ??= new(this);
        }

        public sealed override void Initialize()
        {

        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeNotMatch(UnityEngine.Object value)
        {
            DevLoggerAPI.LogError(value, $"Expected type {typeof(T)}, but received {value.GetType()}");
        }

        protected sealed class TargetList : StatelessList<TargetBuffer, T>
        {
            public TargetList(MonoBinder<T> binder) : base(new(binder)) { }
        }

        protected sealed class TargetBuffer : BufferBase<T>
        {
            private readonly MonoBinder<T> _binder;

            public TargetBuffer(MonoBinder<T> binder)
            {
                _binder = binder;
                Count = Buffer.Length;
            }

            public override ref T[] Buffer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _binder._presetTargets;
            }
        }
    }
}
