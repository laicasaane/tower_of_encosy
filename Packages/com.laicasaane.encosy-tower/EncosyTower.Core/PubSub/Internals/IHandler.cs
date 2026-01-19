#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Common;

namespace EncosyTower.PubSub.Internals
{
    internal interface IHandler<in TMessage> : IDisposable
    {
        DelegateId Id { get; }

#if UNITASK
        Cysharp.Threading.Tasks.UniTask
#else
        UnityEngine.Awaitable
#endif
        Handle(TMessage message, PublishingContext context);
    }
}

#endif
