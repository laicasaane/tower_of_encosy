using System.Runtime.CompilerServices;
using Module.Core.Buffers;
using Module.Core.Collections;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    public class MonoView : MonoBehaviour
    {
        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        internal MonoBinder[] _presetBinders = new MonoBinder[0];

        private BinderList _binders;

        protected BinderList Binders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _binders ??= new(this);
        }
    }

    public sealed class BinderList : StatelessList<BinderBuffer, MonoBinder>
    {
        public BinderList(MonoView view) : base(new(view))
        {
        }
    }

    public sealed class BinderBuffer : BufferBase<MonoBinder>
    {
        private readonly MonoView _view;

        public BinderBuffer(MonoView view)
        {
            _view = view;
        }

        public override ref MonoBinder[] Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _view._presetBinders;
        }
    }
}
