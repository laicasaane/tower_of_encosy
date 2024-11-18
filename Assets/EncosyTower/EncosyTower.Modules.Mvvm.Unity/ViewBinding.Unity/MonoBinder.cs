using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Buffers;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    /// <summary>
    /// Represents a collection of <see cref="MonoBinding"/>.
    /// </summary>
    [Serializable]
    public abstract partial class MonoBinder
    {
        [SerializeField]
        [HideInInspector]
        internal string _subtitle;

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

        public abstract void Initialize(
              IObservableObject context
            , bool alsoStartListening
            , UnityEngine.Object loggingContext
        );

        public abstract void StartListening(UnityEngine.Object loggingContext);

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

        public sealed override void Initialize(
              IObservableObject context
            , bool alsoStartListening
            , UnityEngine.Object loggingContext
        )
        {
            var targets = this.Targets;
            var bindings = this.Bindings.AsReadOnlySpan();
            var bindingsLength = bindings.Length;

            for (var i = 0; i < bindingsLength; i++)
            {
                var binding = bindings[i];

                if (binding == null)
                {
                    ErrorIfBindingMissing(loggingContext, i);
                    continue;
                }

                if (binding is not MonoBinding<T> bindingT)
                {
                    ErrorIfTypeNotMatch(loggingContext, i, binding);
                    continue;
                }

                bindingT.SetTargets(targets);
                bindingT.SetContext(context, alsoStartListening);
            }
        }

        public sealed override void StartListening(UnityEngine.Object loggingContext)
        {
            var bindings = this.Bindings.AsReadOnlySpan();
            var bindingsLength = bindings.Length;

            for (var i = 0; i < bindingsLength; i++)
            {
                var binding = bindings[i];

                if (binding == null)
                {
                    ErrorIfBindingMissing(loggingContext, i);
                    continue;
                }

                if (binding is not MonoBinding<T> bindingT)
                {
                    ErrorIfTypeNotMatch(loggingContext, i, binding);
                    continue;
                }

                if (bindingT.Context == null)
                {
                    ErrorIfContextMissing(loggingContext, i);
                    continue;
                }

                bindingT.StopListening();
                bindingT.StartListening();
                bindingT.RefreshContext();
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfBindingMissing(UnityEngine.Object context, int index)
        {
            DevLoggerAPI.LogError(
                  context
                , $"Expected a MonoBinding<{typeof(T)}>, but received a null at index {index}"
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeNotMatch(UnityEngine.Object context, int index, MonoBinding value)
        {
            DevLoggerAPI.LogError(
                  context
                , $"Expected a MonoBinding<{typeof(T)}>, but received a {value.GetType()} at index {index}"
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfContextMissing(UnityEngine.Object context, int index)
        {
            DevLoggerAPI.LogError(
                  context
                , $"The context of MonoBinding<{typeof(T)}> at index {index} is null"
            );
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
