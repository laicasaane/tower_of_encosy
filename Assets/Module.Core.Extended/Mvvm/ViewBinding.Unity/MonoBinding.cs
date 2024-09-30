using System;
using System.Runtime.CompilerServices;
using Module.Core.Collections;
using Module.Core.Mvvm.ViewBinding;
using Unity.Collections.LowLevel.Unsafe;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    [Serializable]
    public abstract partial class MonoBinding : IBinder
    {
        public IBindingContext Context { get; protected set; }

        public abstract bool IsCommand { get; }

        public abstract void SetTargets(FasterList<UnityEngine.Object> targets);
    }

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

    [Serializable]
    public abstract partial class MonoPropertyBinding<T> : MonoBinding<T>, IBinder
        where T : UnityEngine.Object
    {
        public sealed override bool IsCommand => false;
    }

    [Serializable]
    public abstract partial class MonoCommandBinding<T> : MonoBinding<T>, IBinder
        where T : UnityEngine.Object
    {
        public sealed override bool IsCommand => true;
    }
}
