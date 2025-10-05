using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Logging;
using EncosyTower.Mvvm.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Mvvm.ViewBinding.Components
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

        [SerializeField, SerializeReference, HideInInspector]
        [FormerlySerializedAs("_presetBindings")]
        internal List<MonoBinding> _bindings = new();

        protected ReadOnlyList<MonoBinding> Bindings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bindings ??= new();
        }

        public abstract void Initialize(
              IObservableObject context
            , bool alsoStartListening
            , UnityEngine.Object loggingContext
        );

        public abstract void StartListening(UnityEngine.Object loggingContext);
    }

    /// <summary>
    /// Represents a collection of <see cref="MonoBinding{T}"/>.
    /// </summary>
    /// <typeparam name="T">Must inherit from <see cref="UnityEngine.Object"/>.</typeparam>
    [Serializable]
    public abstract partial class MonoBinder<T> : MonoBinder
        where T : UnityEngine.Object
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_presetTargets")]
        internal List<T> _targets;

        protected ReadOnlyList<T> Targets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _targets ??= new();
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

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfBindingMissing(UnityEngine.Object context, int index)
        {
            StaticDevLogger.LogError(
                  context
                , $"Expected a MonoBinding<{typeof(T)}>, but received a null at index {index}"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfTypeNotMatch(UnityEngine.Object context, int index, MonoBinding value)
        {
            StaticDevLogger.LogError(
                  context
                , $"Expected a MonoBinding<{typeof(T)}>, but received a {value.GetType()} at index {index}"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorIfContextMissing(UnityEngine.Object context, int index)
        {
            StaticDevLogger.LogError(
                  context
                , $"The context of MonoBinding<{typeof(T)}> at index {index} is null"
            );
        }
    }
}
