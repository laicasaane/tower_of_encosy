using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Logging;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.ViewBinding.Contexts;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskObservable = Cysharp.Threading.Tasks.UniTask<IObservableObject>;
#elif UNITY_6000_0_OR_NEWER
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskObservable = UnityEngine.Awaitable<IObservableObject>;
#endif

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

        [SerializeField, SerializeReference, HideInInspector]
        [FormerlySerializedAs("_presetBinders")]
        internal List<MonoBinder> _binders;

        protected ListFast<MonoBinder>.ReadOnly Binders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _binders ??= new();
        }

        public bool IsInitialized { get; private set; }

        protected async void Awake()
        {
            var settings = _settings;

            if (settings.initializeOn != InitializationMethod.Awake)
            {
                return;
            }

            if (settings.initializeAsync)
            {
                await InitializeAsync(settings.startAfterInitialization, destroyCancellationToken);
            }
            else
            {
                Initialize(settings.startAfterInitialization);
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
                await InitializeAsync(settings.startAfterInitialization, destroyCancellationToken);
            }
            else
            {
                Initialize(settings.startAfterInitialization);
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
            IsInitialized = true;
        }

        public async UnityTask InitializeAsync(bool alsoStartListening, CancellationToken token = default)
        {
            var context = await GetContextAsync(token);
            InitializeInternal(context, alsoStartListening);
            IsInitialized = true;
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
            if (IsInitialized == false)
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

        private async UnityTaskObservable GetContextAsync(CancellationToken token)
        {
            await UnityTasks.WaitUntil(
                  this
                , static state => state._context != null && state._context.TryGetContext(out _)
                , token
            );

            if (token.IsCancellationRequested)
            {
                return null;
            }

            return _context.TryGetContext(out var context) ? context : null;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorFoundNoContext(UnityEngine.Object context)
        {
            StaticDevLogger.LogError(context, $"MonoView has no context that implements IObservableObject");
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected static void ErrorNotInitialized(UnityEngine.Object context)
        {
            StaticDevLogger.LogError(context, $"MonoView must be initialized");
        }
    }
}
