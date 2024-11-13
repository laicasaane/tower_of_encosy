#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal interface IHandler<in TMessage> : IDisposable
    {
        DelegateId Id { get; }

#if UNITASK
        Cysharp.Threading.Tasks.UniTask Handle(TMessage message, PublishingContext context, CancellationToken token);
#else
        UnityEngine.Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token);
#endif
    }
}

#endif
