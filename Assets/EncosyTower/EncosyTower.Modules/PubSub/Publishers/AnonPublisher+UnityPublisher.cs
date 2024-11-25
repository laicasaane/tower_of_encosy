#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub
{
    partial class AnonPublisher
    {
        /// <summary>
        /// Anonymous Publisher that allows invoking handlers that take no message argument
        /// </summary>
        public readonly partial struct UnityPublisher<TScope>
            where TScope : UnityEngine.Object
        {
            internal readonly MessagePublisher.Publisher<UnityInstanceId<TScope>> _publisher;

            public bool IsValid => _publisher.IsValid;

            public UnityInstanceId<TScope> Scope => _publisher.Scope;

            internal UnityPublisher(MessagePublisher.UnityPublisher<TScope> publisher)
            {
                _publisher = publisher._publisher;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public CachedPublisher<AnonMessage> Cache(ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _publisher.Cache<AnonMessage>(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish(
                  CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _publisher.Publish<AnonMessage>(token, logger, callerInfo);
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsValid)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogError(
                    $"{GetType().Name} must be retrieved via `{nameof(AnonPublisher)}.{nameof(AnonPublisher.UnityScope)}` API"
                );

                return false;
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings();
        }
    }
}

#endif
