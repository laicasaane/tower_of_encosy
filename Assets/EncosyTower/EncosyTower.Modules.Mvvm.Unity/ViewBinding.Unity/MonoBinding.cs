using System;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Mvvm.ComponentModel;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    /// <summary>
    /// Represents a binding mechanism between a view and a view model.
    /// </summary>
    [Serializable]
    public abstract partial class MonoBinding : IBinder
    {
        public abstract bool IsCommand { get; }

        public void SetContext(IObservableObject context, bool alsoStartListening)
        {
            StopListening();
            Context = context;

            if (alsoStartListening == false)
            {
                return;
            }

            StartListening();
            RefreshContext();
        }
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
        private IAsReadOnlySpan<T> _targets;

        protected ReadOnlySpan<T> Targets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _targets != null ? _targets.AsReadOnlySpan() : Array.Empty<T>();
        }

        public void SetTargets(IAsReadOnlySpan<T> targets)
        {
            OnBeforeSetTargets();

            _targets = targets;

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
    public abstract partial class MonoBindingProperty<T> : MonoBinding<T>, IBinder
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
    public abstract partial class MonoBindingCommand<T> : MonoBinding<T>, IBinder
        where T : UnityEngine.Object
    {
        public sealed override bool IsCommand => true;
    }
}
