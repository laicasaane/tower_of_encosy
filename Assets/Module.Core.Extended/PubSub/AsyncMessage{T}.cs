#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using Module.Core.PubSub;

namespace Module.Core.Extended.PubSub
{
    public readonly struct AsyncMessage<T> : IMessage
        where T : struct, IMessage
    {
        public readonly T Message;

        public AsyncMessage(in T message)
        {
            Message = message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncMessage<T>(in T message)
            => new(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T(in AsyncMessage<T> message)
            => message.Message;
    }

    public static class AsyncMessageExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncMessage<T> AsAsync<T>(this T message) where T : struct, IMessage
            => new(message);
    }
}

#endif
