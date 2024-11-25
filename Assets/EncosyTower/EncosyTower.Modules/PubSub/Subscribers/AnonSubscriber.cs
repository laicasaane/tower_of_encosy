#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.PubSub
{
    /// <summary>
    /// Anonymous Subscriber allows registering handlers that take no message argument
    /// </summary>
    public partial class AnonSubscriber
    {
        private readonly MessageSubscriber _subscriber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<GlobalScope> Global()
        {
            return new(_subscriber.Global());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<TScope> Scope<TScope>()
            where TScope : struct
        {
            return new(_subscriber.Scope(default(TScope)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<TScope> Scope<TScope>([NotNull] TScope scope)
        {
            return new(_subscriber.Scope(scope));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitySubscriber<TScope> UnityScope<TScope>([NotNull] TScope scope)
            where TScope : UnityEngine.Object
        {
            return new(_subscriber.UnityScope(scope));
        }
    }
}

#endif
