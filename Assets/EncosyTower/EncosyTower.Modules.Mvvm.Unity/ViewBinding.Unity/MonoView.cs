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
    /// Represents a collection of <see cref="MonoBinder"/>.
    /// </summary>
    public partial class MonoView : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        internal MonoViewSettings _settings = new();

        [SerializeField]
        [SerializeReference]
        [HideInInspector]
        internal IBindingContext _context = new UnityObjectBindingContext();

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

        public bool Initialized { get; private set; }

        protected async void Awake()
        {
            var settings = _settings;

            if (settings.initializeOn != InitializationMethod.Awake)
            {
                return;
            }

            if (settings.initializeAsync)
            {
                await InitializeAsync(settings.startListeningOnInitialized, destroyCancellationToken);
            }
            else
            {
                Initialize(settings.startListeningOnInitialized);
            }
        }

        protected async void Start()
        {
            var settings = _settings;

            if (settings.initializeOn != InitializationMethod.Start)
            {
                return;
            }

            if (settings.initializeAsync)
            {
                await InitializeAsync(settings.startListeningOnInitialized, destroyCancellationToken);
            }
            else
            {
                Initialize(settings.startListeningOnInitialized);
            }
        }

        public void Initialize(bool alsoStartListening)
        {
            if (_context == null || _context.TryGetContext(out var context) == false)
            {
                ErrorFoundNoContext(this);
                return;
            }

            InitializeInternal(context, alsoStartListening);
            Initialized = true;
        }

        private void InitializeInternal(IObservableObject context, bool alsoStartListening)
        {
            var binders = Binders.AsReadOnlySpan();
            var bindersLength = binders.Length;

            for (var i = 0; i < bindersLength; i++)
            {
                binders[i].Initialize(context, alsoStartListening, this);
            }
        }

        public void StartListening()
        {
            if (Initialized == false)
            {
                ErrorNotInitialized(this);
                return;
            }

            var binders = Binders.AsReadOnlySpan();
            var bindersLength = binders.Length;

            for (var i = 0; i < bindersLength; i++)
            {
                binders[i].StartListening(this);
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorFoundNoContext(UnityEngine.Object context)
        {
            DevLoggerAPI.LogError(context, $"MonoView has no context that implements IObservableObject");
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorNotInitialized(UnityEngine.Object context)
        {
            DevLoggerAPI.LogError(context, $"MonoView must be initialized");
        }

        protected sealed class BinderList : StatelessList<BinderBuffer, MonoBinder>
        {
            public BinderList(MonoView view) : base(new(view)) { }
        }

        protected sealed class BinderBuffer : BufferBase<MonoBinder>
        {
            private readonly MonoView _view;

            public BinderBuffer(MonoView view)
            {
                _view = view;
                Count = Buffer.Length;
            }

            public override ref MonoBinder[] Buffer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _view._presetBinders;
            }
        }
    }
}
