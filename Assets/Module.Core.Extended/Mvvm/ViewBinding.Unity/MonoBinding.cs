using System;
using System.Runtime.CompilerServices;
using Module.Core.Collections;
using Module.Core.Mvvm.ViewBinding;
using Unity.Collections.LowLevel.Unsafe;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    /// <summary>
    /// Represents a binding mechanism between a view and a view model.
    /// </summary>
    [Serializable]
    public abstract partial class MonoBinding : IBinder
    {
        public IBindingContext Context { get; protected set; }

        public abstract bool IsCommand { get; }

        public abstract void SetTargets(FasterList<UnityEngine.Object> targets);
    }

    /// <summary>
    /// Represents a binding mechanism between a target view
    /// of type <typeparamref name="T"/> and a view model.
    /// </summary>
    /// <typeparam name="T">Must inherit from <see cref="UnityEngine.Object"/>.</typeparam>
    [Serializable]
    public abstract partial class MonoBinding<T> : MonoBinding, IBinder
        where T : UnityEngine.Object
    {
        private FasterList<T> _targets;

        protected ReadOnlySpan<T> Targets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _targets != null ? _targets.AsReadOnlySpan() : default;
        }

        public sealed override void SetTargets(FasterList<UnityEngine.Object> targets)
        {
            OnBeforeSetTargets();

            _targets = UnsafeUtility.As<FasterList<UnityEngine.Object>, FasterList<T>>(ref targets);

            OnAfterSetTargets();
        }

        protected virtual void OnBeforeSetTargets() { }

        protected virtual void OnAfterSetTargets() { }
    }

    /// <summary>
    /// Represents a single binding property between a view
    /// of type <typeparamref name="T"/> and a view model.
    /// </summary>
    /// <remarks>
    /// By design, any class inheriting from this class
    /// should not have more than 1 binding property.
    /// </remarks>
    [Serializable]
    public abstract partial class MonoPropertyBinding<T> : MonoBinding<T>, IBinder
        where T : UnityEngine.Object
    {
        public sealed override bool IsCommand => false;
    }

    /// <summary>
    /// Represents a single binding command between a view
    /// of type <typeparamref name="T"/> and a view model.
    /// </summary>
    /// <remarks>
    /// By design, any class inheriting from this class
    /// should not have more than 1 binding command.
    /// </remarks>
    [Serializable]
    public abstract partial class MonoCommandBinding<T> : MonoBinding<T>, IBinder
        where T : UnityEngine.Object
    {
        public sealed override bool IsCommand => true;
    }
}
